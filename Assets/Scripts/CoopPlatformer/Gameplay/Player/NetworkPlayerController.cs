using Unity.Netcode;
using UnityEngine;

namespace CoopPlatformer.Gameplay.Player
{
    /// <summary>
    /// Synchronized 2D Player Controller.
    /// Senior Tip: Local authority for movement (responsiveness) + Server-side triggers for logic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkPlayerController : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 7f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        public override void OnNetworkSpawn()
        {
            _rb = GetComponent<Rigidbody2D>();
            
            // Only enable movement and camera follow for the local player
            if (IsOwner)
            {
                SetupCameraFollow();
            }
            else
            {
                _rb.bodyType = RigidbodyType2D.Kinematic; // Disable physics for remote players to avoid jitter
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

            _moveInput.x = Input.GetAxisRaw("Horizontal");
            
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                JumpServerRpc();
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            _rb.velocity = new Vector2(_moveInput.x * _moveSpeed, _rb.velocity.y);
        }

        [ServerRpc]
        private void JumpServerRpc()
        {
            // Senior Tip: Server-side validation (e.g., check cooldown or energy)
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        private bool IsGrounded()
        {
            // Simple ground check logic
            return Mathf.Abs(_rb.velocity.y) < 0.01f;
        }
    }
}
