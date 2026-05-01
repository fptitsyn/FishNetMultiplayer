using FishNet.Object;
using Network;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sprintMultiplier;
        [SerializeField] private float jumpForce;
        [SerializeField] private float distanceToGround;

        private InputAction _walkAction;
        private InputAction _sprintAction;
        private InputAction _jumpAction;

        private const float Gravity = -9.81f;

        private CharacterController _controller;
        private bool _grounded;
        private Vector3 _velocity;
        private float _moveSpeed;

        private Camera _camera;
    
        private PlayerNetwork _network;

        private void Start()
        {
            _walkAction = InputSystem.actions.FindAction("Move");
            _sprintAction = InputSystem.actions.FindAction("Sprint");
            _jumpAction = InputSystem.actions.FindAction("Jump");
    
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Walk();
        }

        [ServerRpc(RequireOwnership = false)]
        private void Walk()
        {
            if (!Owner.IsLocalClient || !_network.IsAlive.Value)
            {
                return;
            }
        
            Vector2 moveValue = _walkAction.ReadValue<Vector2>();
            if (_sprintAction.IsPressed() && moveValue is { y: > 0, x: 0 })
            {
                _moveSpeed = sprintMultiplier * walkSpeed;
            }
            else
            {
                _moveSpeed = walkSpeed;
            }
    
            if (_controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
    
            Vector3 move = transform.right * moveValue.x + transform.forward * moveValue.y;
        
            _controller.Move(move * (_moveSpeed * Time.deltaTime));
        
            _grounded = Physics.Raycast(transform.position, Vector3.down, distanceToGround);
        
            if (_jumpAction.IsPressed() && _grounded)
            {
                _grounded = false;
                _velocity.y = jumpForce;
            }
        
            _velocity.y += Gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        
            Vector3 cameraForward = _camera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 horizontalMove = new Vector3(move.x, 0f, move.z);

            Vector3 lookDirection = horizontalMove;

            if (lookDirection.sqrMagnitude < 0.01f)
            {
                lookDirection = cameraForward;
            }

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
            }
        }
    
        public override void OnStartNetwork()
        {
            if (!Owner.IsLocalClient) return;
        
            _camera = Camera.main;
            _network = GetComponent<PlayerNetwork>();
        }
    }
}
