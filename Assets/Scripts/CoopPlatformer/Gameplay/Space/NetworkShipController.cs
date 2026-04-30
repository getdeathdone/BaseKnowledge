using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using CoopPlatformer.Gameplay.Environment;
using System.Collections.Generic;

namespace CoopPlatformer.Gameplay.Space
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkShipController : NetworkBehaviour
    {
        private const int UnassignedFingerId = int.MinValue;
        private const float InputSendInterval = 1f / 30f;
        private const float MoveSendThreshold = 0.04f;
        private const float AimSendThreshold = 0.02f;
        private const float TouchMoveDeadZone = 0.6f;
        private const float TouchMoveMaxDistance = 4f;

        [Header("Movement Settings")]
        private const float MoveSpeed = 12f;
        private const float Acceleration = 24f;
        private const float Deceleration = 8f;
        private const float  RotationSpeed = 600f;
        
        private float _arenaRadius;

        [Header("References")]
        [SerializeField] private Transform _muzzle;

        private readonly NetworkVariable<int> _health = new(5);
        private readonly NetworkVariable<float> _networkArenaRadius = new(25f);
        private Rigidbody2D _rb;
        private Camera _cam;
        
        // Input state synced from client to server
        private Vector2 _inputMove;
        private Vector2 _inputAim;
        private bool _isFiringRequested;
        private bool _fireButtonQueued;
        private bool _isFireButtonPressed;
        private int _fireButtonFingerId = UnassignedFingerId;
        private Vector2 _lastSentMove;
        private Vector2 _lastSentAim = Vector2.up;
        private float _nextInputSendTime;
        
        private float _lastFireTime;

        private static readonly HashSet<NetworkShipController> ActiveShips = new HashSet<NetworkShipController>();

        public int CurrentHealth => _health.Value;
        public int MaxHealth => 5;
        public static IReadOnlyCollection<NetworkShipController> Ships => ActiveShips;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _cam = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            ActiveShips.Add(this);
            _networkArenaRadius.OnValueChanged += OnArenaRadiusChanged;

            if (IsServer)
            {
                _health.Value = 5;
                _networkArenaRadius.Value = ResolveArenaRadius();
                _rb.simulated = true;
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = 0f;
                _rb.drag = 0f;
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

            ApplyArenaRadius(_networkArenaRadius.Value);
        }

        public override void OnNetworkDespawn()
        {
            ActiveShips.Remove(this);
            _networkArenaRadius.OnValueChanged -= OnArenaRadiusChanged;
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

            bool pointerOverUi = Input.touchCount > 0 ? false : IsPointerOverUi();

            // 1. Gather Input (Keyboard/Mouse)
            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 aim = transform.up;
            bool firing = _fireButtonQueued || Input.GetKeyDown(KeyCode.Space);

            // 2. Touch Input (Override if touching outside gameplay UI)
            if (TryGetGameplayTouch(out Touch gameplayTouch))
            {
                if (_cam != null)
                {
                    Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(gameplayTouch.position.x, gameplayTouch.position.y, 10f));
                    worldPos.z = 0f;
                    
                    Vector2 toTouch = (Vector2)(worldPos - transform.position);
                    
                    float distance = toTouch.magnitude;
                    if (distance > TouchMoveDeadZone)
                    {
                        Vector2 direction = toTouch / distance;
                        float throttle = Mathf.Clamp01((distance - TouchMoveDeadZone) / (TouchMoveMaxDistance - TouchMoveDeadZone));
                        move = direction * throttle;
                        aim = toTouch.normalized;
                    }
                }
            }
            else if (move.sqrMagnitude < 0.01f && _cam != null && !pointerOverUi && !_isFireButtonPressed)
            {
                Vector3 mouseWorld = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
                mouseWorld.z = 0f;
                aim = (mouseWorld - transform.position).normalized;
            }

            // 3. Send to Server
            if (ShouldSendInput(move, aim, firing))
            {
                move = QuantizeVector(move, 0.02f);
                aim = QuantizeVector(aim, 0.02f);

                UpdateInputServerRpc(move, aim, firing);
                _lastSentMove = move;
                _lastSentAim = aim;
                _nextInputSendTime = Time.unscaledTime + InputSendInterval;
            }
            _fireButtonQueued = false;
            
            // 4. Local Camera Fallback
            if (_cam != null && _cam.TryGetComponent<Gameplay.Player.CameraFollow>(out var follow))
            {
                if (follow.Target == null) follow.SetTarget(transform);
            }
        }

        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void UpdateInputServerRpc(Vector2 move, Vector2 aim, bool firing)
        {
            _inputMove = Vector2.ClampMagnitude(move, 1f);
            if (aim.sqrMagnitude > 0.1f) _inputAim = aim.normalized;
            _isFiringRequested |= firing;
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;

            ApplyMovement();

            // Apply Rotation
            if (_inputAim.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(_inputAim.y, _inputAim.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = _rb.rotation;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, RotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(newAngle);
            }

            // Shooting
            if (_isFiringRequested)
            {
                TryFire();
                _isFiringRequested = false;
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
            NetworkProjectile projectilePrefab = GameplayPrefabRegistry.Instance.ProjectilePrefab;
            if (projectilePrefab == null) return;

            // Check alignment
            float angle = Vector2.Angle(transform.up, _inputAim);
            if (angle > 20f) return; // Wait until rotated

            _lastFireTime = Time.time;
            
            Vector3 spawnPos = _muzzle != null ? _muzzle.position : transform.position + transform.up * 0.8f;
            var proj = Instantiate(projectilePrefab, spawnPos, transform.rotation);
            proj.gameObject.SetActive(true);
            proj.Initialize(transform.up, OwnerClientId);
            NetworkObject projectileNetworkObject = proj.GetComponent<NetworkObject>();
            if (projectileNetworkObject == null)
            {
                Debug.LogError("[NetworkShipController] Projectile is missing NetworkObject.");
                Destroy(proj.gameObject);
                return;
            }

            projectileNetworkObject.Spawn(true);
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

        public void QueueFireButtonShot()
        {
            if (!IsOwner) return;
            _fireButtonQueued = true;
        }

        public void SetFireButtonPressed(bool isPressed, int pointerId = UnassignedFingerId)
        {
            if (!IsOwner) return;
            _isFireButtonPressed = isPressed;
            _fireButtonFingerId = isPressed ? pointerId : UnassignedFingerId;
        }

        private void ApplyMovement()
        {
            Vector2 targetVelocity = _inputMove.sqrMagnitude > 0.01f
                ? _inputMove * MoveSpeed
                : Vector2.zero;

            float rate = _inputMove.sqrMagnitude > 0.01f ? Acceleration : Deceleration;
            _rb.velocity = Vector2.MoveTowards(_rb.velocity, targetVelocity, rate * Time.fixedDeltaTime);
        }

        private float ResolveArenaRadius()
        {
            ArenaSpawner arenaSpawner = FindObjectOfType<ArenaSpawner>();
            return arenaSpawner != null ? arenaSpawner.CurrentRadius : _arenaRadius;
        }

        private void OnArenaRadiusChanged(float previousValue, float newValue)
        {
            ApplyArenaRadius(newValue);
        }

        private void ApplyArenaRadius(float radius)
        {
            if (radius > 0f)
            {
                _arenaRadius = radius;
            }
        }

        private bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    {
                        return true;
                    }
                }

                return false;
            }

            return EventSystem.current.IsPointerOverGameObject();
        }

        private bool TryGetGameplayTouch(out Touch gameplayTouch)
        {
            gameplayTouch = default;

            if (_isFireButtonPressed && Input.touchCount == 1)
            {
                return false;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId == _fireButtonFingerId)
                {
                    continue;
                }

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    continue;
                }

                gameplayTouch = touch;
                return true;
            }

            return false;
        }

        private bool ShouldSendInput(Vector2 move, Vector2 aim, bool firing)
        {
            if (firing)
            {
                return true;
            }

            if (Time.unscaledTime >= _nextInputSendTime)
            {
                return true;
            }

            if (move != _lastSentMove)
            {
                return Vector2.Distance(move, _lastSentMove) > MoveSendThreshold;
            }

            return Vector2.Distance(aim, _lastSentAim) > AimSendThreshold;
        }

        private static Vector2 QuantizeVector(Vector2 value, float step)
        {
            if (step <= 0f)
            {
                return value;
            }

            return new Vector2(
                Mathf.Round(value.x / step) * step,
                Mathf.Round(value.y / step) * step);
        }
    }
}
