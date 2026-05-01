using UnityEngine;
using UnityEditor;
using System;
using System.IO; // Обязательно добавляем для работы с папками (Directory и Path)

public class ScreenshotTool
{
    // Добавляем пункт в верхнее меню: вкладка "Tools" -> "Take Screenshot"
    // Хоткей: Ctrl + Shift + S (на Windows) или Cmd + Shift + S (на Mac)
    [MenuItem("Tools/Take Screenshot %#s")]
    public static void Capture()
    {
        // 1. Определяем путь к папке Screenshots в корне проекта (на уровень выше Assets)
        string folderPath = Path.Combine(Application.dataPath, "..", "Recordings");

        // 2. Проверяем, существует ли папка. Если нет — создаем её
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 3. Генерируем уникальное имя файла
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"Screenshot_{timestamp}.png";

        // 4. Склеиваем путь к папке и имя файла
        string fullPath = Path.Combine(folderPath, filename);

        // 5. Делаем скриншот по полному пути
        ScreenCapture.CaptureScreenshot(fullPath);

        // 6. Выводим кликабельное сообщение в консоль
        Debug.Log($"📸 Скриншот успешно сохранен: {fullPath}");
    }
}