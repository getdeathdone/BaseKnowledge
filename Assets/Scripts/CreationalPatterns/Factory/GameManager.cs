using UnityEngine;

namespace CreationalPatterns.Factory
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var vampire = EnemyFactory.CreateEnemy("Vampire");
            vampire.Attack();
            var zombie = EnemyFactory.CreateEnemy("Zombie");
            zombie.Attack();

            EnemyFactory trollFactory = new VampireFactory();
            var troll = trollFactory.CreateEnemy();
            troll.Attack();
            EnemyFactory orcFactory = new ZombieFactory();
            var orc = orcFactory.CreateEnemy();
            orc.Attack();
        }
    }
}