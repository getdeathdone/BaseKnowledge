using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShipMotor : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private ShipConfig _config;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _config = GetComponent<ShipConfig>();
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void ConfigureForServer()
        {
            _rb.simulated = true;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 0f;
            _rb.drag = 0f;
            _rb.angularDrag = 5f;
        }

        public void ConfigureForClient()
        {
            _rb.simulated = false;
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        public void ApplyMovement(Vector2 moveInput)
        {
            Vector2 targetVelocity = moveInput.sqrMagnitude > 0.01f
                ? moveInput * _config.MoveSpeed
                : Vector2.zero;

            float rate = moveInput.sqrMagnitude > 0.01f ? _config.Acceleration : _config.Deceleration;
            _rb.velocity = Vector2.MoveTowards(_rb.velocity, targetVelocity, rate * Time.fixedDeltaTime);
        }

        public void ApplyRotation(Vector2 aimInput)
        {
            if (aimInput.sqrMagnitude <= 0.01f)
            {
                return;
            }

            float targetAngle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg - 90f;
            float newAngle = Mathf.MoveTowardsAngle(_rb.rotation, targetAngle, _config.RotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(newAngle);
        }

        public void ClampToRadius(float radius)
        {
            if (_rb.position.magnitude <= radius)
            {
                return;
            }

            _rb.position = _rb.position.normalized * radius;
            _rb.velocity *= 0.5f;
        }

        public void ResetTo(Vector2 position)
        {
            _rb.position = position;
            _rb.velocity = Vector2.zero;
        }
    }
}
