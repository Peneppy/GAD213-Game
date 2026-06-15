using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement
{
    public class PlatformerPlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float jumpForce = 2f;

        private Rigidbody2D rbref;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;

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

            if (rbref == null)
            {
                rbref = GetComponent<Rigidbody2D>();
                if (rbref == null)
                {
                    rbref = transform.AddComponent<Rigidbody2D>();
                }
                rbref.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        private void FixedUpdate()
        {
            Movement(moveAction.ReadValue<Vector2>());
        }

        private void Update()
        {
            Jump(jumpAction);
        }
        private void Movement(Vector2 moveCoords)
        {
            rbref.linearVelocity = new Vector2(moveCoords.x * moveSpeed, rbref.linearVelocity.y);

        }

        private void Jump(InputAction action)
        {
            if (action.triggered)
            {
                rbref.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }
}