using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement
{
    public class PlatformerPlayerMovement : MonoBehaviour
    {
        [Header("Movement Variables")]
        [Tooltip("How fast the player moves")]
        [SerializeField] private float moveSpeed = 10f;
        [Tooltip("Amount of acceleration applied to the player")]
        [SerializeField] private float acceleration = 10f;
        [Tooltip("Amount of decceleration applied to the player")]
        [SerializeField] private float decceleration = 15f;
        [SerializeField] private float velPower = 0.9f;
        [Tooltip("Amount of friction applied to player to stop movement")]
        [SerializeField] private float frictionAmount = 0.2f;
        [Tooltip("How much upward force is applied on jumping")]
        [SerializeField] private float jumpForce = 10f;
        [Tooltip("Amount of buffer time a player is given after they've left a surface to jump")]
        [SerializeField] private float jumpCoyoteTime;
        [SerializeField] private float jumpBufferTime;
        [Tooltip("Controls how much the player ascends on a jump cut")]
        [SerializeField] private float jumpCutMultiplier = 0.25f;
        [Tooltip("How much gravity should increase when the player stops moving vertically")]
        [SerializeField] private float fallGravityMultiplier = 1.75f;
        private float lastGroundedTime;
        private float lastJumpTime;
        private bool isJumping;
        [Tooltip("How much directional force is applied on dashing")]
        [SerializeField] private float dashForce = 10f;
        [Tooltip("The amount of dashes a player has")]
        [SerializeField] private int maxDashes = 1;
        private int currentDashAmount;
        [SerializeField] private bool isDashing;

        [Space(2)]
        [Header("Ground Check Variables")]
        [Tooltip("If the player is on a ground layer")]
        [SerializeField] private bool isGrounded;
        [Tooltip("The Y offset of the overlap box checking for ground")]
        [SerializeField] private float groundCheckBoxYOffset = 0.5f;
        [Tooltip("The size of the box scale on the X axis")]
        [SerializeField] private float groundCheckBoxXSize = 1f;
        [Tooltip("The size of the box scale on the Y axis")]
        [SerializeField] private float groundCheckBoxYSize = 1f;
        [Tooltip("What layers should make the player \"Grounded\"")]
        [SerializeField] private LayerMask groundCheckLayerMask;
        private GameObject debugSprite;

        [Space(2)]
        [Header("Debugging")]
        [Tooltip("Turns on/off debugging")]
        [SerializeField] private bool doDebugging = false;

        private Rigidbody2D rbref;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dashAction;

        private void Start()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
            if (moveAction == null)
            {
                moveAction = playerInput.actions["Move"];
            }
            if (jumpAction == null)
            {
                jumpAction = playerInput.actions["Jump"];
            }
            if (dashAction == null)
            {
                dashAction = playerInput.actions["Dash"];
            }

            if (rbref == null)
            {
                rbref = GetComponent<Rigidbody2D>();
                if (rbref == null)
                {
                    rbref = transform.AddComponent<Rigidbody2D>();
                }
                rbref.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            if (debugSprite == null)
            {
                debugSprite = GameObject.Find("GroundCheckDebug");
                if (!doDebugging)
                {
                    debugSprite.SetActive(false);
                }
            }
            else if (!doDebugging)
            {
                debugSprite.SetActive(false);
            }

            isGrounded = true;
            isDashing = false;
            currentDashAmount = maxDashes;
        }

        private void FixedUpdate()
        {
            Movement(moveAction.ReadValue<Vector2>());
            ArtificialFriction(moveAction.ReadValue<Vector2>());
        }

        private void Update()
        {
            GroundChecking();
            Jump(jumpAction);
            Dash(dashAction, moveAction.ReadValue<Vector2>());
            if (jumpAction.WasReleasedThisFrame())
            {
                JumpCut();
                lastGroundedTime = 0f;
                lastJumpTime = 0f;
                isJumping = false;
            }
            FaceDirection(moveAction.ReadValue<Vector2>());
            JumpGravity();
        }

        private void Movement(Vector2 moveCoords)
        {
            if (!isDashing)
            {
                //rbref.linearVelocity = new Vector2(moveCoords.x * moveSpeed, rbref.linearVelocity.y);
                float targetSpeed = moveCoords.x * moveSpeed;
                float speedDiff = targetSpeed - rbref.linearVelocity.x;
                float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
                float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velPower) * Mathf.Sign(speedDiff);

                rbref.AddForce(movement * Vector2.right);
            }
        }

        private void ArtificialFriction(Vector2 moveInput)
        {
            if (lastGroundedTime > 0 && Mathf.Abs(moveInput.x) < 0.01f)
            {
                float amount = Mathf.Min(Mathf.Abs(rbref.linearVelocity.x), Mathf.Abs(frictionAmount));
                amount *= Mathf.Sign(rbref.linearVelocity.x);
                rbref.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
            }
        }

        private void Jump(InputAction action)
        {
            if (action.triggered)
            {
                lastJumpTime = jumpBufferTime;
                if (lastGroundedTime > 0 && lastJumpTime > 0 && !isJumping)
                {
                    rbref.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    isJumping = true;
                }
            }
        }

        private void JumpCut()
        {
            if (rbref.linearVelocity.y > 0 && isJumping)
            {
                rbref.AddForce(Vector2.down * rbref.linearVelocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
                isJumping = false;
            }
        }

        private void JumpGravity()
        {
            int gravityScale = 1;
            if (rbref.linearVelocityY < 0)
            {
                rbref.gravityScale = gravityScale * fallGravityMultiplier;
            }
            else
            {
                rbref.gravityScale = gravityScale;
            }
        }

        private void Dash(InputAction action, Vector2 moveDirection)
        {
            if (action.triggered && isDashing == false && currentDashAmount > 0)
            {
                if (doDebugging)
                {
                    Debug.Log("Dash triggered");
                }
                isDashing = true;
                currentDashAmount -= 1;
                rbref.AddForce(moveDirection * dashForce, ForceMode2D.Impulse);
                Invoke("DashCooldown", 0.25f);
            }
            if (isGrounded)
            {
                currentDashAmount = maxDashes;
            }
        }

        private void DashCooldown()
        {
            isDashing = false;
        }

        private void GroundChecking()
        {
            Vector2 rbrefPositionVector = new Vector2(rbref.position.x, rbref.position.y - groundCheckBoxYOffset);

            if (doDebugging)
            {
                debugSprite.SetActive(true);
                debugSprite.transform.position = rbrefPositionVector;
                debugSprite.transform.localScale = new Vector2(groundCheckBoxXSize, groundCheckBoxYSize);
            }
            else
            {
                debugSprite.SetActive(false);
            }

            Collider2D results = Physics2D.OverlapBox(rbrefPositionVector, new Vector2(groundCheckBoxXSize, groundCheckBoxYSize), 0f, groundCheckLayerMask);

            if (results)
            {
                lastGroundedTime = jumpCoyoteTime;
                isGrounded = true;
                if (doDebugging)
                {
                    Debug.Log("Player is grounded");
                }
            }
            else
            {
                isGrounded = false;
                lastGroundedTime -= Time.deltaTime;
                if (doDebugging)
                {
                    Debug.Log("Player is not grounded");
                }
            }
        }

        private void FaceDirection(Vector2 moveInput)
        {
           if (moveInput.x > 0)
            {
                transform.eulerAngles = new Vector3(0,0,0);
            } 
            else if (moveInput.x < 0)
            {
                transform.eulerAngles = new Vector3(0,180,0);
            }
        }
    }
}