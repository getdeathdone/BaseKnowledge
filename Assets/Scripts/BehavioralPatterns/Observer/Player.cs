using System;
using UnityEngine;

namespace BehavioralPatterns.Observer
{
    public class Player : MonoBehaviour
    {
        public event Action OnPlayerDeath;

        public void Die()
        {
            OnPlayerDeath?.Invoke();
            Debug.Log("Player has died.");
        }
    }
}