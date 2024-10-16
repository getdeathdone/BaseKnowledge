using UnityEngine;

namespace BehavioralPatterns.Command
{
    public interface ICommand
    {
        void Execute();
    }

    public class MoveCommand : ICommand
    {
        private readonly Vector3 _direction;
        private readonly GameObject _player;

        public MoveCommand(GameObject player, Vector3 direction)
        {
            _player = player;
            _direction = direction;
        }

        public void Execute()
        {
            _player.transform.Translate(_direction);
        }
    }
}