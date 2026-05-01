using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jigsaw.Scripts.UI
{
    /// <summary>
    /// Скрипт для главного меню игры.
    /// Обрабатывает нажатия кнопок: "Играть", "Играть Онлайн" и "Выход".
    /// </summary>
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

        // Метод для кнопки "Играть" (Обычная игра)
        // Загружает сцену под номером 1
        public void PlayOffline()
        {
            Debug.Log("Загрузка одиночной игры (Сцена 1)...");
            SceneManager.LoadScene(1);
        }

        // Метод для кнопки "Играть Онлайн" (Мультиплеер)
        // Загружает сцену под номером 2
        public void PlayOnline()
        {
            Debug.Log("Загрузка онлайн игры (Сцена 2)...");
            SceneManager.LoadScene(2);
        }

        // Метод для кнопки "Выход"
        // Закрывает приложение
        public void ExitGame()
        {
            Debug.Log("Выход из игры...");
        
            // В редакторе Unity Application.Quit() не работает, поэтому добавляем это для удобства
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
