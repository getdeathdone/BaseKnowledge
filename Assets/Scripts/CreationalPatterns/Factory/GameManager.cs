using UnityEngine;

namespace CreationalPatterns.Factory
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var zombie = EnemyFactory.CreateEnemy("Zombie");
            zombie.Attack();

            var vampire = EnemyFactory.CreateEnemy("Vampire");
            vampire.Attack();
        }
    }
}