using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class NetworkProjectile : NetworkBehaviour
    {
        [SerializeField] private float _speed = 18f;
        [SerializeField] private float _lifeTime = 2.5f;
        [SerializeField] private int _damage = 1;

        private Rigidbody2D _rigidbody2D;
        private Vector2 _direction = Vector2.up;
        private ulong _ownerClientId;
        private float _spawnTime;

        private void Awake()
        {
            EnsureComponents();
        }

        public void Initialize(Vector2 direction, ulong ownerClientId)
        {
            _direction = direction.normalized;
            _ownerClientId = ownerClientId;
        }

        public override void OnNetworkSpawn()
        {
            EnsureComponents();
            if (IsSpawnTemplate())
            {
                return;
            }

            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
            _spawnTime = Time.time;

            if (!IsServer)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.simulated = false;
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer || IsSpawnTemplate())
            {
                return;
            }

            EnsureComponents();
            if (_rigidbody2D == null)
            {
                return;
            }

            _rigidbody2D.velocity = _direction * _speed;
        }

        private void Update()
        {
            if (!IsServer || IsSpawnTemplate())
            {
                return;
            }

            if (Time.time - _spawnTime >= _lifeTime)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer)
            {
                return;
            }

            var asteroid = other.GetComponent<AsteroidController>();
            if (asteroid != null)
            {
                asteroid.TakeHit(_damage);
                Despawn();
                return;
            }

            var ship = other.GetComponent<NetworkShipController>();
            if (ship != null && ship.OwnerClientId != _ownerClientId)
            {
                ship.TakeDamage(_damage);
                Despawn();
            }
        }

        private void Despawn()
        {
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(true);
            }
            else
            {
                Destroy(gameObject);
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
        }
    }
}
