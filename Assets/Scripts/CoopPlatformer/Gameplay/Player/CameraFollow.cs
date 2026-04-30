using UnityEngine;

namespace CoopPlatformer.Gameplay.Player
{
    /// <summary>
    /// Smoothly follows a target transform.
    /// Senior Tip: Use LateUpdate for camera movement to ensure the target has already moved in Update/FixedUpdate.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _smoothSpeed = 0.125f;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 0f); // Reset offset for space shooter by default
        [SerializeField] private float _zDepth = -10f;

        private Transform _target;

        // Public property to check current target from other scripts
        public Transform Target => _target;

        public void SetTarget(Transform target)
        {
            _target = target;
            
            // Snap to target immediately on first set to avoid cinematic "slide-in" from origin
            if (_target != null)
            {
                transform.position = new Vector3(_target.position.x + _offset.x, _target.position.y + _offset.y, _zDepth);
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = new Vector3(_target.position.x + _offset.x, _target.position.y + _offset.y, _zDepth);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
