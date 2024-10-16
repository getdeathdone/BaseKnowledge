using UnityEngine;

namespace StructuralPatterns.Adapter
{
    public interface IEnemyMovement
    {
        void MoveEnemy();
    }

    public class PlayerMovement
    {
        public void MovePlayer(float speed)
        {
            Debug.Log("Moving player at speed: " + speed);
        }
    }

    public class EnemyMovementAdapter : IEnemyMovement
    {
        private readonly PlayerMovement _playerMovement;

        public EnemyMovementAdapter(PlayerMovement playerMovement)
        {
            _playerMovement = playerMovement;
        }

        public void MoveEnemy()
        {
            _playerMovement.MovePlayer(5.0f);
        }
    }
}