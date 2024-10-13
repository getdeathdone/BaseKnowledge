using UnityEngine;

namespace BehavioralPatterns.Strategy
{
    public class GameManager : MonoBehaviour
    {
        public Character characterPrefab;

        private void Start()
        {
            var character = Instantiate(characterPrefab);
            character.SetAttackStrategy(new MeleeAttack());
            character.PerformAttack();

            character.SetAttackStrategy(new RangedAttack());
            character.PerformAttack();
        }
    }
}