using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement
{
    public class PlatformerPlayerMovement : MonoBehaviour
    {
        [Header("Movement Variables")]
        [Tooltip("How fast the player moves")]
        [SerializeField] private float moveSpeed = 2f;
        [Tooltip("How much upward force is applied on jumping")]
        [SerializeField] private float jumpForce = 2f;
        [Tooltip("How much directional force is applied on dashing")]
        [SerializeField] private float dashForce = 50f;

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
        }

        private void FixedUpdate()
        {
            Movement(moveAction.ReadValue<Vector2>());
        }

        private void Update()
        {
            GroundChecking();
            Jump(jumpAction);
            Dash(dashAction, moveAction.ReadValue<Vector2>());
        }

        private void Movement(Vector2 moveCoords)
        {
            rbref.linearVelocity = new Vector2(moveCoords.x * moveSpeed, rbref.linearVelocity.y);
        }

        private void Jump(InputAction action)
        {
            if (action.triggered && isGrounded == true)
            {
                rbref.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        private void Dash(InputAction action, Vector2 moveDirection)
        {
            if (action.triggered)
            {
                if (doDebugging)
                {
                    Debug.Log("Dash triggered");
                }
                //rbref.AddForce(moveDirection * dashForce, ForceMode2D.Impulse);
                rbref.linearVelocity = new Vector2(moveDirection.x * dashForce, rbref.linearVelocity.y);
            }
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
                isGrounded = true;
                if (doDebugging)
                {
                    Debug.Log("Player is grounded");
                }
            }
            else
            {
                isGrounded = false;
                if (doDebugging)
                {
                    Debug.Log("Player is not grounded");
                }
            }
        }
    }
}