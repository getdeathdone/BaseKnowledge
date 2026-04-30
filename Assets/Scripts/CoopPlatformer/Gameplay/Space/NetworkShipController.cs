using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using CoopPlatformer.Gameplay.Environment;
using System.Collections.Generic;

namespace CoopPlatformer.Gameplay.Space
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(ShipConfig))]
    [RequireComponent(typeof(ShipLocalInput))]
    [RequireComponent(typeof(ShipMotor))]
    [RequireComponent(typeof(ShipCombat))]
    public class NetworkShipController : NetworkBehaviour
    {
        private float _arenaRadius;

        private readonly NetworkVariable<int> _health = new(5);
        private readonly NetworkVariable<float> _networkArenaRadius = new(25f);
        private Camera _cam;
        
        // Input state synced from client to server
        private Vector2 _inputMove;
        private Vector2 _inputAim;
        private bool _isFiringRequested;
        private Vector2 _lastSentMove;
        private Vector2 _lastSentAim = Vector2.up;
        private float _nextInputSendTime;
        private ShipLocalInput _localInput;
        private ShipMotor _motor;
        private ShipCombat _combat;
        private ShipConfig _config;

        private static readonly HashSet<NetworkShipController> ActiveShips = new HashSet<NetworkShipController>();

        public int CurrentHealth => _health.Value;
        public int MaxHealth => _config.MaxHealth;
        public static IReadOnlyCollection<NetworkShipController> Ships => ActiveShips;

        private void Awake()
        {
            _config = GetComponent<ShipConfig>();
            _localInput = GetComponent<ShipLocalInput>();
            _motor = GetComponent<ShipMotor>();
            _combat = GetComponent<ShipCombat>();
            _cam = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            ActiveShips.Add(this);
            _networkArenaRadius.OnValueChanged += OnArenaRadiusChanged;

            if (IsServer)
            {
                _health.Value = _config.MaxHealth;
                _networkArenaRadius.Value = ResolveArenaRadius();
                _motor.ConfigureForServer();
                transform.position = SpawnPointResolver.GetSpawnPosition(OwnerClientId);
            }
            else
            {
                _motor.ConfigureForClient();
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

            ShipInputSnapshot input = _localInput.Read(_cam, transform.position, transform.up);
            Vector2 move = input.Move;
            Vector2 aim = input.Aim;
            bool firing = input.Fire;

            // 3. Send to Server
            if (ShouldSendInput(move, aim, firing))
            {
                move = QuantizeVector(move, 0.02f);
                aim = QuantizeVector(aim, 0.02f);

                UpdateInputServerRpc(move, aim, firing);
                _lastSentMove = move;
                _lastSentAim = aim;
                _nextInputSendTime = Time.unscaledTime + _config.InputSendInterval;
            }
            
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

            _motor.ApplyMovement(_inputMove);
            _motor.ApplyRotation(_inputAim);

            if (_isFiringRequested)
            {
                _combat.TryFire(_inputAim, OwnerClientId);
                _isFiringRequested = false;
            }

            _motor.ClampToRadius(_arenaRadius);
        }

        public void TakeDamage(int amount, Vector3 hitPosition)
        {
            if (!IsServer) return;
            ShowCollisionExplosionClientRpc(hitPosition);
            _health.Value -= amount;
            if (_health.Value <= 0)
            {
                _health.Value = _config.MaxHealth;
                _motor.ResetTo(SpawnPointResolver.GetSpawnPosition(OwnerClientId));
            }
        }

        public void TakeDamage(int amount)
        {
            if (!IsServer) return;
            _health.Value -= amount;
            if (_health.Value <= 0)
            {
                _health.Value = _config.MaxHealth;
                _motor.ResetTo(SpawnPointResolver.GetSpawnPosition(OwnerClientId));
            }
        }

        [ClientRpc]
        private void ShowCollisionExplosionClientRpc(Vector3 hitPosition)
        {
            GameObject explosion = Instantiate(_config.CollisionExplosionPrefab, hitPosition, Quaternion.identity);
            explosion.SetActive(true);
        }

        public void QueueFireButtonShot()
        {
            if (!IsOwner) return;
            _localInput.QueueFireButtonShot();
        }

        public void SetFireButtonPressed(bool isPressed, int pointerId = int.MinValue)
        {
            if (!IsOwner) return;
            _localInput.SetFireButtonPressed(isPressed, pointerId);
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
                return Vector2.Distance(move, _lastSentMove) > _config.MoveSendThreshold;
            }

            return Vector2.Distance(aim, _lastSentAim) > _config.AimSendThreshold;
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
