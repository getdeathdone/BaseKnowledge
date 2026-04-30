using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections.Generic;

namespace CoopPlatformer.Gameplay.Space
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class AsteroidController : NetworkBehaviour
    {
        [SerializeField] private int _health = 1;

        private static readonly HashSet<AsteroidController> ActiveAsteroids = new HashSet<AsteroidController>();
        private Rigidbody2D _rigidbody2D;
        private Vector2 _velocity;
        public static int ActiveCount => ActiveAsteroids.Count;

        private void Awake()
        {
            EnsureComponents();
        }

        public void Initialize(Vector2 velocity, float angularVelocity)
        {
            _velocity = velocity;
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            if (_rigidbody2D != null)
            {
                _rigidbody2D.angularVelocity = angularVelocity;
            }
        }

        public override void OnNetworkSpawn()
        {
            EnsureComponents();
            if (IsSpawnTemplate())
            {
                return;
            }

            ActiveAsteroids.Add(this);
            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (!IsServer)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.simulated = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            ActiveAsteroids.Remove(this);
        }

        private void FixedUpdate()
        {
            if (!IsServer || IsSpawnTemplate())
            {
                return;
            }

            EnsureComponents();
            _rigidbody2D.velocity = _velocity;
        }

        public void TakeHit(int damage)
        {
            if (!IsServer)
            {
                return;
            }

            _health -= damage;
            if (_health <= 0)
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

            var ship = other.GetComponent<NetworkShipController>();
            if (ship == null)
            {
                return;
            }

            ship.TakeDamage(1);
            Despawn();
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
