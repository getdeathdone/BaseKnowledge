using UnityEngine;

namespace BehavioralPatterns.Mediator
{
    public class Enemy : MonoBehaviour
    {
        public int health = 100;
        private Mediator _mediator;

        private void Start()
        {
            _mediator = FindObjectOfType<Mediator>();
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0) _mediator.Notify(gameObject, "EnemyDefeated");
        }
    }
}