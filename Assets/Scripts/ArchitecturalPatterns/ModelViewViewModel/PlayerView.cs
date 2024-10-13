using UnityEngine;
using UnityEngine.UI;

namespace ArchitecturalPatterns.ModelViewViewModel
{
    public class PlayerView : MonoBehaviour
    {
        public Text healthText;
        public Text scoreText;

        private PlayerViewModel viewModel;

        private void Start()
        {
            viewModel = GetComponent<PlayerViewModel>();

            viewModel.OnHealthChanged += UpdateHealth;
            viewModel.OnScoreChanged += UpdateScore;
        }

        private void UpdateHealth()
        {
            healthText.text = "Health: " + viewModel.Health;
        }

        private void UpdateScore()
        {
            scoreText.text = "Score: " + viewModel.Score;
        }
    }
}