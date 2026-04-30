using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace CoopPlatformer.Gameplay.Player
{
    
    
    
    
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkPlayerController : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 7f;

        private Rigidbody2D _rb;
        private Vector2 _serverMoveInput;
        private Vector2 _lastSentMoveInput = new Vector2(float.NaN, float.NaN);
        private bool _jumpRequested;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SetupCameraFollow();
            }

            if (IsServer)
            {
                _rb.simulated = true;
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }
            else
            {
                _rb.simulated = false;
                _rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        private void SetupCameraFollow()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return;

            if (!mainCamera.TryGetComponent<CameraFollow>(out var follow))
            {
                follow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }

            follow.SetTarget(transform);
        }

        private void Update()
        {
            if (!IsOwner) return;

            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
            bool jumpPressed = Input.GetButtonDown("Jump");

            if (moveInput != _lastSentMoveInput || jumpPressed)
            {
                SubmitInputServerRpc(moveInput, jumpPressed);
                _lastSentMoveInput = moveInput;
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;

            _rb.velocity = new Vector2(_serverMoveInput.x * _moveSpeed, _rb.velocity.y);

            if (_jumpRequested && IsGrounded())
            {
                _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            }

            _jumpRequested = false;
        }

        [ServerRpc]
        private void SubmitInputServerRpc(Vector2 moveInput, bool jumpPressed)
        {
            _serverMoveInput = Vector2.ClampMagnitude(moveInput, 1f);
            _jumpRequested |= jumpPressed;
        }

        private bool IsGrounded()
        {
            
            return Mathf.Abs(_rb.velocity.y) < 0.01f;
        }
    }
}
