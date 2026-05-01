using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Атрибут заставляет скрипт выполняться при запуске редактора и после компиляции
[InitializeOnLoad]
public class OpenFirstSceneOnLoad
{
    // Ключ для проверки, открывали ли мы уже сцену в этой сессии
    private const string SessionKey = "HasOpenedFirstScene_SessionFlag";

    // Статический конструктор вызывается Unity автоматически
    static OpenFirstSceneOnLoad()
    {
        // Подписываемся на обновление редактора, чтобы дождаться полной загрузки Unity
        EditorApplication.update += RunOnceOnStartup;
    }

    private static void RunOnceOnStartup()
    {
        // Сразу отписываемся, чтобы метод сработал ровно один раз
        EditorApplication.update -= RunOnceOnStartup;

        // Если в текущей сессии мы уже открывали сцену, ничего не делаем
        if (SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        // Запоминаем, что мы выполнили авто-открытие (чтобы при компиляции кода оно не повторилось)
        SessionState.SetBool(SessionKey, true);

        // Получаем все сцены из File -> Build Settings
        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

        if (buildScenes.Length > 0)
        {
            // Берем путь к самой первой сцене (индекс 0)
            string firstScenePath = buildScenes[0].path;

            if (!string.IsNullOrEmpty(firstScenePath))
            {
                // Открываем сцену
                EditorSceneManager.OpenScene(firstScenePath, OpenSceneMode.Single);
                Debug.Log($"<b>[Auto-Load]</b> Проект запущен! Автоматически открыта сцена: {firstScenePath}");
            }
            else
            {
                Debug.LogWarning("[Auto-Load] Первая сцена в Build Settings пуста или удалена.");
            }
        }
        else
        {
            Debug.LogWarning("[Auto-Load] В Build Settings (File -> Build Settings) нет ни одной сцены!");
        }
    }
}