using UnityEngine;

namespace BehavioralPatterns.Mediator
{
    public class Mediator : MonoBehaviour
    {
        public void Notify(GameObject sender, string eventMessage)
        {
            if (eventMessage == "PlayerHit")
            {
                Debug.Log("Player hit the enemy!");
                FindObjectOfType<Enemy>().TakeDamage(10);
            }
            else if (eventMessage == "EnemyDefeated")
            {
                Debug.Log("Enemy defeated by player!");
            }
        }
    }
}