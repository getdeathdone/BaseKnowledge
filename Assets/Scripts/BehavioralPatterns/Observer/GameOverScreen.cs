using UnityEngine;

namespace BehavioralPatterns.Observer
{
    public class GameOverScreen : MonoBehaviour
    {
        public void OnPlayerDeath()
        {
            Debug.Log("Game Over screen activated.");
        }
    }
}