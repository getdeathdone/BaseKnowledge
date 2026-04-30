using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    public class ShipConfig : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 12f;
        [SerializeField] private float _acceleration = 24f;
        [SerializeField] private float _deceleration = 8f;
        [SerializeField] private float _rotationSpeed = 600f;

        [Header("Combat")]
        [SerializeField] private int _maxHealth = 5;
        [SerializeField] private float _fireInterval = 0.2f;
        [SerializeField] private float _maxFireAngle = 20f;
        [SerializeField] private GameObject _collisionExplosionPrefab;

        [Header("Input")]
        [SerializeField] private float _touchMoveDeadZone = 0.6f;
        [SerializeField] private float _touchMoveMaxDistance = 4f;

        [Header("Networking")]
        [SerializeField] private float _inputSendInterval = 1f / 30f;
        [SerializeField] private float _moveSendThreshold = 0.04f;
        [SerializeField] private float _aimSendThreshold = 0.02f;

        public float MoveSpeed => _moveSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float RotationSpeed => _rotationSpeed;
        public int MaxHealth => _maxHealth;
        public float FireInterval => _fireInterval;
        public float MaxFireAngle => _maxFireAngle;
        public GameObject CollisionExplosionPrefab => _collisionExplosionPrefab;
        public float TouchMoveDeadZone => _touchMoveDeadZone;
        public float TouchMoveMaxDistance => _touchMoveMaxDistance;
        public float InputSendInterval => _inputSendInterval;
        public float MoveSendThreshold => _moveSendThreshold;
        public float AimSendThreshold => _aimSendThreshold;
    }
}
