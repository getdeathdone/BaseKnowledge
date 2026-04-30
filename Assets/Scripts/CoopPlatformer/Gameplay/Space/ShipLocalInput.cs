using UnityEngine;
using UnityEngine.EventSystems;

namespace CoopPlatformer.Gameplay.Space
{
    public readonly struct ShipInputSnapshot
    {
        public readonly Vector2 Move;
        public readonly Vector2 Aim;
        public readonly bool Fire;

        public ShipInputSnapshot(Vector2 move, Vector2 aim, bool fire)
        {
            Move = move;
            Aim = aim;
            Fire = fire;
        }
    }

    public class ShipLocalInput : MonoBehaviour
    {
        private const int UnassignedFingerId = int.MinValue;

        private bool _fireButtonQueued;
        private bool _isFireButtonPressed;
        private int _fireButtonFingerId = UnassignedFingerId;
        private ShipConfig _config;

        private void Awake()
        {
            _config = GetComponent<ShipConfig>();
        }

        public ShipInputSnapshot Read(Camera gameplayCamera, Vector3 shipPosition, Vector2 currentAim)
        {
            bool pointerOverUi = IsPointerOverUi();

            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 aim = currentAim;
            bool fire = _fireButtonQueued || Input.GetKeyDown(KeyCode.Space);

            if (TryGetGameplayTouch(out Touch gameplayTouch))
            {
                if (gameplayCamera != null)
                {
                    Vector3 worldPos = gameplayCamera.ScreenToWorldPoint(new Vector3(gameplayTouch.position.x, gameplayTouch.position.y, 10f));
                    worldPos.z = 0f;

                    Vector2 toTouch = (Vector2)(worldPos - shipPosition);
                    float distance = toTouch.magnitude;
                    if (distance > _config.TouchMoveDeadZone)
                    {
                        Vector2 direction = toTouch / distance;
                        float throttle = Mathf.Clamp01((distance - _config.TouchMoveDeadZone) / (_config.TouchMoveMaxDistance - _config.TouchMoveDeadZone));
                        move = direction * throttle;
                        aim = toTouch.normalized;
                    }
                }
            }
            // Fallback for mouse/keyboard on non-mobile platforms
            else if (Input.touchCount == 0 && move.sqrMagnitude < 0.01f && gameplayCamera != null && !pointerOverUi && !_isFireButtonPressed && !Application.isMobilePlatform)
            {
                Vector3 mouseWorld = gameplayCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
                mouseWorld.z = 0f;
                aim = (mouseWorld - shipPosition).normalized;
            }

            _fireButtonQueued = false;
            return new ShipInputSnapshot(move, aim, fire);
        }

        public void QueueFireButtonShot()
        {
            _fireButtonQueued = true;
        }

        public void SetFireButtonPressed(bool isPressed, int pointerId = UnassignedFingerId)
        {
            _isFireButtonPressed = isPressed;
            _fireButtonFingerId = isPressed ? pointerId : UnassignedFingerId;
        }

        private bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    {
                        return true;
                    }
                }

                return false;
            }

            return EventSystem.current.IsPointerOverGameObject();
        }

        private bool TryGetGameplayTouch(out Touch gameplayTouch)
        {
            gameplayTouch = default;

            if (_isFireButtonPressed && Input.touchCount == 1)
            {
                return false;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    continue;
                }

                if (touch.fingerId == _fireButtonFingerId)
                {
                    continue;
                }

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    continue;
                }

                gameplayTouch = touch;
                return true;
            }

            return false;
        }
    }
}
