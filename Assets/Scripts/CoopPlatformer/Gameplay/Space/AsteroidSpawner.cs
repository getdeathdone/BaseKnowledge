using Unity.Netcode;
using UnityEngine;
using CoopPlatformer.Gameplay.Environment;

namespace CoopPlatformer.Gameplay.Space
{
    public class AsteroidSpawner : NetworkBehaviour
    {
        [SerializeField] private float _arenaRadius = 18f;
        [SerializeField] private float _spawnInterval = 1.8f;
        [SerializeField] private int _maxAsteroids = 12;
        [SerializeField] private float _minSpeed = 1.5f;
        [SerializeField] private float _maxSpeed = 3.5f;

        private float _nextSpawnTime;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                ArenaSpawner arenaSpawner = FindObjectOfType<ArenaSpawner>();
                if (arenaSpawner != null)
                {
                    _arenaRadius = arenaSpawner.CurrentRadius;
                }
            }
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (Time.time < _nextSpawnTime)
            {
                return;
            }

            if (AsteroidController.ActiveCount >= _maxAsteroids)
            {
                _nextSpawnTime = Time.time + 0.5f;
                return;
            }

            SpawnAsteroid();
            _nextSpawnTime = Time.time + _spawnInterval;
        }

        private void SpawnAsteroid()
        {
            AsteroidController asteroidPrefab = GameplayPrefabRegistry.Instance.AsteroidPrefab;
            if (asteroidPrefab == null)
            {
                return;
            }

            var angle = Random.Range(0f, Mathf.PI * 2f);
            var spawnPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _arenaRadius;
            var driftTarget = Random.insideUnitCircle * (_arenaRadius * 0.35f);
            var velocity = (driftTarget - spawnPosition).normalized * Random.Range(_minSpeed, _maxSpeed);
            var angularVelocity = Random.Range(-35f, 35f);

            var asteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
            if (asteroid == null)
            {
                Debug.LogError("[AsteroidSpawner] Failed to instantiate asteroid prefab.");
                return;
            }

            asteroid.gameObject.SetActive(true);
            asteroid.Initialize(velocity, angularVelocity);

            var networkObject = asteroid.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[AsteroidSpawner] Asteroid instance is missing NetworkObject.");
                Destroy(asteroid.gameObject);
                return;
            }

            networkObject.Spawn(true);
        }
    }
}
