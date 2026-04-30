using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    public class GameplayPrefabRegistry : MonoBehaviour
    {
        [Header("Network Prefabs")]
        [SerializeField] private NetworkShipController _shipPrefab;
        [SerializeField] private NetworkProjectile _projectilePrefab;
        [SerializeField] private AsteroidController _asteroidPrefab;

        public static GameplayPrefabRegistry Instance { get; private set; }

        public NetworkShipController ShipPrefab => _shipPrefab;
        public NetworkProjectile ProjectilePrefab => _projectilePrefab;
        public AsteroidController AsteroidPrefab => _asteroidPrefab;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
