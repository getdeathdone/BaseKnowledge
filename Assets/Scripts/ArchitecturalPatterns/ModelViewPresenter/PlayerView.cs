using UnityEngine;
using UnityEngine.UI;

namespace ArchitecturalPatterns.ModelViewPresenter
{
    public class PlayerView : MonoBehaviour, IPlayerView
    {
        public Text healthText;
        public Text scoreText;

        public void UpdateHealth(int health)
        {
            healthText.text = "Health: " + health;
        }

        public void UpdateScore(int score)
        {
            scoreText.text = "Score: " + score;
        }
    }
}