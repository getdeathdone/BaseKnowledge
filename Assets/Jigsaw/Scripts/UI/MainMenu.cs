using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jigsaw.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            
            Screen.orientation = ScreenOrientation.LandscapeRight; 
            Screen.autorotateToLandscapeLeft = true; 
            Screen.autorotateToLandscapeRight = true; 
            Screen.autorotateToPortrait = false; 
            Screen.autorotateToPortraitUpsideDown = false; 
            Screen.orientation = ScreenOrientation.AutoRotation; 
        }

        public void PlayOffline()
        {
            Debug.Log("Загрузка одиночной игры (Сцена 1)...");
            SceneManager.LoadScene(1);
        }

        public void PlayOnline()
        {
            Debug.Log("Загрузка онлайн игры (Сцена 2)...");
            SceneManager.LoadScene(2);
        }

        public void ExitGame()
        {
            Debug.Log("Выход из игры...");
        
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
