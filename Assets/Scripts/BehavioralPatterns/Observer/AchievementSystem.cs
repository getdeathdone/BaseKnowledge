using UnityEngine;

namespace BehavioralPatterns.Observer
{
    public class AchievementSystem : MonoBehaviour
    {
        public void OnPlayerDeath()
        {
            Debug.Log("Achievement Unlocked: First Death!");
        }
    }
}