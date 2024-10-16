using UnityEngine;

namespace StructuralPatterns.Facade
{
    public class AudioManager
    {
        public void PlaySound()
        {
            Debug.Log("Playing sound");
        }
    }

    public class EnemyManager
    {
        public void SpawnEnemies()
        {
            Debug.Log("Spawning enemies");
        }
    }

    public class UIManager
    {
        public void ShowUI()
        {
            Debug.Log("Showing UI");
        }
    }

    public class GameFacade
    {
        private readonly AudioManager _audioManager;
        private readonly EnemyManager _enemyManager;
        private readonly UIManager _uiManager;

        public GameFacade()
        {
            _audioManager = new AudioManager();
            _enemyManager = new EnemyManager();
            _uiManager = new UIManager();
        }

        public void StartGame()
        {
            _audioManager.PlaySound();
            _enemyManager.SpawnEnemies();
            _uiManager.ShowUI();
        }
    }
}