using UnityEngine;

namespace StructuralPatterns.Adapter
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var player = new PlayerMovement();
            IEnemyMovement enemy = new EnemyMovementAdapter(player);

            enemy.MoveEnemy();
        }
    }
}