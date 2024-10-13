using UnityEngine;

namespace BehavioralPatterns.Observer
{
    public class GameManager : MonoBehaviour
    {
        public Player player;
        public AchievementSystem achievementSystem;
        public GameOverScreen gameOverScreen;

        private void Start()
        {
            player.OnPlayerDeath += achievementSystem.OnPlayerDeath;
            player.OnPlayerDeath += gameOverScreen.OnPlayerDeath;
        }

        private void OnDestroy()
        {
            player.OnPlayerDeath -= achievementSystem.OnPlayerDeath;
            player.OnPlayerDeath -= gameOverScreen.OnPlayerDeath;
        }
    }
}