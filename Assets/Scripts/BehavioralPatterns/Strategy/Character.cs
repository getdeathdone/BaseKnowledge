using UnityEngine;

namespace BehavioralPatterns.Strategy
{
    public interface IAttackStrategy
    {
        void Attack();
    }

    public class MeleeAttack : IAttackStrategy
    {
        public void Attack()
        {
            Debug.Log("Performing a melee attack.");
        }
    }

    public class RangedAttack : IAttackStrategy
    {
        public void Attack()
        {
            Debug.Log("Performing a ranged attack.");
        }
    }

    public class Character : MonoBehaviour
    {
        private IAttackStrategy attackStrategy;

        public void SetAttackStrategy(IAttackStrategy strategy)
        {
            attackStrategy = strategy;
        }

        public void PerformAttack()
        {
            attackStrategy.Attack();
        }
    }
}