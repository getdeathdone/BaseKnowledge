using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class NetworkShipController : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _rotationSpeed = 540f;
        [SerializeField] private float _arenaRadius = 16f;

        [Header("Combat")]
        [SerializeField] private NetworkProjectile _projectilePrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _fireCooldown = 0.2f;
        [SerializeField] private int _maxHealth = 5;

        private readonly NetworkVariable<int> _health = new(5);
        private Rigidbody2D _rigidbody2D;
        private Camera _mainCamera;
        private Vector2 _serverMoveInput;
        private Vector2 _serverAimDirection = Vector2.up;
        private float _lastFireTime;

        public int CurrentHealth => _health.Value;
        public int MaxHealth => _maxHealth;

        private void Awake()
        {
            EnsureComponents();
        }

        public override void OnNetworkSpawn()
        {
            EnsureComponents();
            if (IsSpawnTemplate())
            {
                return;
            }

            EnableVisuals();
            _muzzle = transform.Find("Muzzle");
            _mainCamera = Camera.main;

            if (IsServer)
            {
                _health.Value = _maxHealth;
                transform.position = SpawnPointResolver.GetSpawnPosition(OwnerClientId);
            }

            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.drag = 4f;
            _rigidbody2D.angularDrag = 8f;
            _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (!IsServer)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.simulated = false;
            }
        }

        private void Update()
        {
            if (IsSpawnTemplate() || !IsOwner)
            {
                return;
            }

            var moveInput = ReadMoveInput();
            var aimDirection = ReadAimDirection();
            SubmitInputServerRpc(moveInput, aimDirection);

            if (ShouldFire())
            {
                RequestFireServerRpc(aimDirection);
            }
        }

        private void FixedUpdate()
        {
            if (IsSpawnTemplate() || !IsServer)
            {
                return;
            }

            EnsureComponents();
            _rigidbody2D.velocity = _serverMoveInput * _moveSpeed;

            if (_serverAimDirection.sqrMagnitude > 0.001f)
            {
                var targetAngle = Mathf.Atan2(_serverAimDirection.y, _serverAimDirection.x) * Mathf.Rad2Deg - 90f;
                var nextAngle = Mathf.MoveTowardsAngle(_rigidbody2D.rotation, targetAngle, _rotationSpeed * Time.fixedDeltaTime);
                _rigidbody2D.MoveRotation(nextAngle);
            }

            ClampInsideArena();
        }

        [ServerRpc]
        private void SubmitInputServerRpc(Vector2 moveInput, Vector2 aimDirection)
        {
            _serverMoveInput = moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;
            if (aimDirection.sqrMagnitude > 0.001f)
            {
                _serverAimDirection = aimDirection.normalized;
            }
        }

        [ServerRpc]
        private void RequestFireServerRpc(Vector2 aimDirection)
        {
            if (_projectilePrefab == null)
            {
                return;
            }

            if (Time.time - _lastFireTime < _fireCooldown)
            {
                return;
            }

            Vector2 direction = aimDirection.sqrMagnitude > 0.001f ? aimDirection.normalized : (Vector2)transform.up;
            Vector3 spawnPosition = _muzzle != null
                ? _muzzle.position
                : transform.position + new Vector3(direction.x, direction.y, 0f) * 0.9f;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            var projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.Euler(0f, 0f, angle));
            if (projectile == null)
            {
                return;
            }

            projectile.gameObject.SetActive(true);
            projectile.Initialize(direction, OwnerClientId);

            var networkObject = projectile.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Destroy(projectile.gameObject);
                return;
            }

            networkObject.Spawn(true);
            _lastFireTime = Time.time;
        }

        public void TakeDamage(int damage)
        {
            if (!IsServer)
            {
                return;
            }

            _health.Value = Mathf.Max(0, _health.Value - damage);
            if (_health.Value == 0)
            {
                _health.Value = _maxHealth;
                transform.position = SpawnPointResolver.GetSpawnPosition(OwnerClientId);
                _rigidbody2D.velocity = Vector2.zero;
                _serverMoveInput = Vector2.zero;
                _serverAimDirection = Vector2.up;
            }
        }

        private Vector2 ReadMoveInput()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (_mainCamera != null)
                {
                    Vector3 world = _mainCamera.ScreenToWorldPoint(touch.position);
                    Vector2 delta = new Vector2(world.x - transform.position.x, world.y - transform.position.y);
                    return delta.sqrMagnitude > 0.25f ? delta.normalized : Vector2.zero;
                }
            }

            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private Vector2 ReadAimDirection()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (_mainCamera != null)
                {
                    Vector3 world = _mainCamera.ScreenToWorldPoint(touch.position);
                    Vector2 delta = new Vector2(world.x - transform.position.x, world.y - transform.position.y);
                    if (delta.sqrMagnitude > 0.01f)
                    {
                        return delta.normalized;
                    }
                }
            }

            if (_mainCamera != null)
            {
                Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 delta = new Vector2(mouseWorld.x - transform.position.x, mouseWorld.y - transform.position.y);
                if (delta.sqrMagnitude > 0.01f)
                {
                    return delta.normalized;
                }
            }

            return transform.up;
        }

        private bool ShouldFire()
        {
            if (Input.touchCount > 0)
            {
                return true;
            }

            return Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
        }

        private void ClampInsideArena()
        {
            var position = _rigidbody2D.position;
            if (position.magnitude <= _arenaRadius)
            {
                return;
            }

            _rigidbody2D.position = position.normalized * _arenaRadius;
            _rigidbody2D.velocity = Vector2.zero;
        }

        private void EnableVisuals()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }
        }

        private bool IsSpawnTemplate()
        {
            return transform.parent != null && transform.parent.name == "GeneratedPrefabs";
        }

        private void EnsureComponents()
        {
            if (_rigidbody2D == null)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }
    }
}
