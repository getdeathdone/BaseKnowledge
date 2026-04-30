using UnityEngine;

namespace CoopPlatformer.Gameplay.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private float _smoothTime = 0.12f;

        [SerializeField] private Vector2 _offset = new(0f, 0f);
        [SerializeField] private float _zDepth = -10f;

        private Vector3 _velocity;


        public Transform Target { get; private set; }

        private void LateUpdate()
        {
            if (Target == null) return;

            var desiredPosition = new Vector3(Target.position.x + _offset.x, Target.position.y + _offset.y, _zDepth);
            var smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothTime);
            transform.position = smoothedPosition;
        }

        public void SetTarget(Transform target)
        {
            Target = target;


            if (Target != null)
                transform.position = new Vector3(Target.position.x + _offset.x, Target.position.y + _offset.y, _zDepth);
        }
    }
}