using System;
using UnityEngine;

namespace ArchitecturalPatterns.ModelViewViewModel
{
    public class PlayerViewModel : MonoBehaviour
    {
        private PlayerModel playerModel;

        public int Health => playerModel.Health;
        public int Score => playerModel.Score;

        private void Start()
        {
            playerModel = new PlayerModel { Health = 100, Score = 0 };
        }

        public event Action OnHealthChanged;
        public event Action OnScoreChanged;

        public void TakeDamage(int damage)
        {
            playerModel.Health -= damage;
            OnHealthChanged?.Invoke();
        }

        public void AddScore(int points)
        {
            playerModel.Score += points;
            OnScoreChanged?.Invoke();
        }
    }
}