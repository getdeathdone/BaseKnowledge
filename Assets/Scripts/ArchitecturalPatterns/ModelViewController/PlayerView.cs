using UnityEngine;
using UnityEngine.UI;

namespace ArchitecturalPatterns.ModelViewController
{
    public class PlayerView : MonoBehaviour
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