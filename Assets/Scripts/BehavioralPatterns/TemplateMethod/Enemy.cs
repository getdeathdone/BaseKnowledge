using UnityEngine;

namespace BehavioralPatterns.TemplateMethod
{
    public abstract class Enemy : MonoBehaviour
    {
        public void PerformAction()
        {
            Spawn();
            Attack();
            Die();
        }

        protected abstract void Spawn();
        protected abstract void Attack();
        protected abstract void Die();
    }

    public class Goblin : Enemy
    {
        protected override void Spawn()
        {
            Debug.Log("Goblin spawns.");
        }

        protected override void Attack()
        {
            Debug.Log("Goblin attacks.");
        }

        protected override void Die()
        {
            Debug.Log("Goblin dies.");
        }
    }

    public class Orc : Enemy
    {
        protected override void Spawn()
        {
            Debug.Log("Orc spawns.");
        }

        protected override void Attack()
        {
            Debug.Log("Orc attacks.");
        }

        protected override void Die()
        {
            Debug.Log("Orc dies.");
        }
    }
}