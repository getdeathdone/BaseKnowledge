using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkShipController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 12f;
        [SerializeField] private float _rotationSpeed = 600f;
        [SerializeField] private float _arenaRadius = 25f;

        [Header("References")]
        [SerializeField] private NetworkProjectile _projectilePrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _visual;

        private readonly NetworkVariable<int> _health = new(5);
        private Rigidbody2D _rb;
        private Camera _cam;
        
        // Input state synced from client to server
        private Vector2 _inputMove;
        private Vector2 _inputAim;
        private bool _isFiringRequested;
        
        private float _lastFireTime;

        public int CurrentHealth => _health.Value;
        public int MaxHealth => 5;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _cam = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _health.Value = 5;
                _rb.simulated = true;
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = 0f;
                _rb.drag = 2f;
                _rb.angularDrag = 5f;
                
                transform.position = SpawnPointResolver.GetSpawnPosition(OwnerClientId);
            }
            else
            {
                // Clients don't run physics for remote or even local ships (server is authoritative)
                _rb.simulated = false;
                _rb.bodyType = RigidbodyType2D.Kinematic;
            }

            if (IsOwner)
            {
                SetupCamera();
            }

            if (_visual != null) _visual.gameObject.SetActive(true);
        }

        private void SetupCamera()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam != null)
            {
                if (!_cam.TryGetComponent<Gameplay.Player.CameraFollow>(out var follow))
                {
                    follow = _cam.gameObject.AddComponent<Gameplay.Player.CameraFollow>();
                }
                follow.SetTarget(transform);
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            // 1. Gather Input (Keyboard/Mouse)
            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 aim = transform.up;
            bool firing = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

            // 2. Touch Input (Override if touching)
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (_cam != null)
                {
                    Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, 10f));
                    worldPos.z = 0f;
                    
                    Vector2 toTouch = (Vector2)(worldPos - transform.position);
                    
                    // Move and Aim towards touch point
                    if (toTouch.magnitude > 0.5f)
                    {
                        move = toTouch.normalized;
                        aim = toTouch.normalized;
                    }
                    firing = true; // Auto-fire while touching
                }
            }
            else if (move.sqrMagnitude < 0.01f && _cam != null)
            {
                // If not moving via keyboard, look at mouse
                Vector3 mouseWorld = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
                mouseWorld.z = 0f;
                aim = (mouseWorld - transform.position).normalized;
            }

            // 3. Send to Server
            UpdateInputServerRpc(move, aim, firing);
            
            // 4. Local Camera Fallback
            if (_cam != null && _cam.TryGetComponent<Gameplay.Player.CameraFollow>(out var follow))
            {
                if (follow.Target == null) follow.SetTarget(transform);
            }
        }

        [ServerRpc]
        private void UpdateInputServerRpc(Vector2 move, Vector2 aim, bool firing)
        {
            _inputMove = Vector2.ClampMagnitude(move, 1f);
            if (aim.sqrMagnitude > 0.1f) _inputAim = aim.normalized;
            _isFiringRequested = firing;
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;

            // Apply Movement
            if (_inputMove.sqrMagnitude > 0.01f)
            {
                _rb.AddForce(_inputMove * _moveSpeed * 10f); // Use force for smoother feel with drag
            }

            // Apply Rotation
            if (_inputAim.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(_inputAim.y, _inputAim.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = _rb.rotation;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _rotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(newAngle);
            }

            // Shooting
            if (_isFiringRequested)
            {
                TryFire();
            }

            // Constraints
            if (_rb.position.magnitude > _arenaRadius)
            {
                _rb.position = _rb.position.normalized * _arenaRadius;
                _rb.velocity *= 0.5f;
            }
        }

        private void TryFire()
        {
            if (Time.time - _lastFireTime < 0.2f) return;
            if (_projectilePrefab == null) return;

            // Check alignment
            float angle = Vector2.Angle(transform.up, _inputAim);
            if (angle > 20f) return; // Wait until rotated

            _lastFireTime = Time.time;
            
            Vector3 spawnPos = _muzzle != null ? _muzzle.position : transform.position + transform.up * 0.8f;
            var proj = Instantiate(_projectilePrefab, spawnPos, transform.rotation);
            proj.gameObject.SetActive(true);
            proj.Initialize(transform.up, OwnerClientId);
            proj.GetComponent<NetworkObject>().Spawn(true);
        }

        public void TakeDamage(int amount)
        {
            if (!IsServer) return;
            _health.Value -= amount;
            if (_health.Value <= 0)
            {
                _health.Value = 5;
                _rb.position = SpawnPointResolver.GetSpawnPosition(OwnerClientId);
                _rb.velocity = Vector2.zero;
            }
        }
    }
}
