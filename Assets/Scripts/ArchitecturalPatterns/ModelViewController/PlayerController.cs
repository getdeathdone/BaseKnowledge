using UnityEngine;

namespace ArchitecturalPatterns.ModelViewController
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerModel playerModel;
        private PlayerView playerView;

        private void Start()
        {
            playerModel = new PlayerModel { Health = 100, Score = 0 };
            playerView = GetComponent<PlayerView>();
            UpdateView();
        }

        public void TakeDamage(int damage)
        {
            playerModel.Health -= damage;
            UpdateView();
        }

        public void AddScore(int points)
        {
            playerModel.Score += points;
            UpdateView();
        }

        private void UpdateView()
        {
            playerView.UpdateHealth(playerModel.Health);
            playerView.UpdateScore(playerModel.Score);
        }
    }
}