using UnityEngine;

namespace BehavioralPatterns.State
{
    public class PlayerStateContext : MonoBehaviour
    {
        private IPlayerState _state;

        private void Start()
        {
            SetState(new IdleState());
        }

        private void Update()
        {
            _state?.Handle(this);
        }

        public void SetState(IPlayerState state)
        {
            _state = state;
        }
    }
}