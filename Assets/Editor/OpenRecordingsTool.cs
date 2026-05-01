using UnityEngine;
using UnityEditor;
using System.IO;

public class OpenRecordingsTool
{
    // Добавляем пункт в верхнее меню: вкладка "Tools" -> "Open Recordings Folder"
    // Хоткей: Ctrl + Shift + O (на Windows) или Cmd + Shift + O (на Mac)
    [MenuItem("Tools/Open Recordings Folder %#o")]
    public static void OpenFolder()
    {
        // 1. Указываем тот же путь, что и в скрипте скриншотов
        string folderPath = Path.Combine(Application.dataPath, "..", "Recordings");

        // 2. Проверяем, существует ли папка
        if (Directory.Exists(folderPath))
        {
            // 3. Открываем папку. 
            // EditorUtility.RevealInFinder кроссплатформенно открывает Проводник (Windows) или Finder (Mac)
            EditorUtility.RevealInFinder(folderPath);
            Debug.Log("📂 Папка со скриншотами открыта.");
        }
        else
        {
            // Если папки нет (например, скриншоты еще не делали), выводим предупреждение
            Debug.LogWarning("⚠️ Папка Recordings еще не создана. Сначала сделайте скриншот!");
        }
    }
}