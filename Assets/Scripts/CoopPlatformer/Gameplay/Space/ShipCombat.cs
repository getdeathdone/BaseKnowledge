using CoopPlatformer.Core.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    public class ShipCombat : MonoBehaviour
    {
        [SerializeField] private Transform _muzzle;
        private ShipConfig _config;

        private float _lastFireTime;

        private void Awake()
        {
            _config = GetComponent<ShipConfig>();
        }

        public bool TryFire(Vector2 aim, ulong ownerClientId)
        {
            if (Time.time - _lastFireTime < _config.FireInterval) return false;

            var projectilePrefab = GameplayPrefabRegistry.Instance.ProjectilePrefab;
            if (projectilePrefab == null) return false;

            var angle = Vector2.Angle(transform.up, aim);
            if (angle > _config.MaxFireAngle) return false;

            _lastFireTime = Time.time;

            var spawnPosition = _muzzle != null ? _muzzle.position : transform.position + transform.up * 0.8f;
            var projectile = Instantiate(projectilePrefab, spawnPosition, transform.rotation);
            projectile.gameObject.SetActive(true);
            projectile.Initialize(transform.up, ownerClientId);
            projectile.GetComponent<NetworkObject>().Spawn(true);
            return true;
        }
    }
}