using UnityEngine;

namespace CoopPlatformer.Gameplay.Player
{
    
    
    
    
    public class CameraFollow : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _smoothTime = 0.12f;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 0f); 
        [SerializeField] private float _zDepth = -10f;

        private Transform _target;
        private Vector3 _velocity;

        
        public Transform Target => _target;

        public void SetTarget(Transform target)
        {
            _target = target;
            
            
            if (_target != null)
            {
                transform.position = new Vector3(_target.position.x + _offset.x, _target.position.y + _offset.y, _zDepth);
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = new Vector3(_target.position.x + _offset.x, _target.position.y + _offset.y, _zDepth);
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothTime);
            transform.position = smoothedPosition;
        }
    }
}
