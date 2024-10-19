using UnityEngine;

namespace BehavioralPatterns.State
{
    public interface IPlayerState
    {
        void Handle(PlayerStateContext context);
    }

    public class IdleState : IPlayerState
    {
        public void Handle(PlayerStateContext context)
        {
            Debug.Log("Player is Idle.");
            if (Input.GetKeyDown(KeyCode.Space)) context.SetState(new JumpState());
        }
    }

    public class JumpState : IPlayerState
    {
        public void Handle(PlayerStateContext context)
        {
            Debug.Log("Player is Jumping.");
            if (Input.GetKeyDown(KeyCode.S)) context.SetState(new IdleState());
        }
    }
}