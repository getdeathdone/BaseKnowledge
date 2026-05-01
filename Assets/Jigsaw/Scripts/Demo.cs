using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Для использования списка

namespace Jigsaw.Scripts
{
    public class Demo : MonoBehaviour
    {
        public List<Texture> images; // Лист текстур
        public GameObject puzzle;

        public Button guiMenuNextButton;
        public Button guiMenuPiecesButton;
        public Button guiMenuRestartButton;

        private DemoJigsawPuzzle jigsawPuzzle;
        private int puzzleImageIndex; // Индекс текущей текстуры
        private int sizeMode = 3;

        public static int PUZZLE_Number
        {
            get => PlayerPrefs.GetInt(nameof(PUZZLE_Number), 0);
            set => PlayerPrefs.SetInt(nameof(PUZZLE_Number), value);
        }

        private void Awake()
        {
            guiMenuNextButton.onClick.AddListener(Puzzle);
            guiMenuRestartButton.onClick.AddListener(Restart);
            guiMenuPiecesButton.onClick.AddListener(Pieces);
        }

        private void Start()
        {
            if (puzzle == null) return;

            jigsawPuzzle = puzzle.GetComponent<DemoJigsawPuzzle_mac_ios>();

            if (jigsawPuzzle == null) return;

            SetSize();
            PuzzleNumber(PUZZLE_Number);
        }

        private void OnGUI()
        {
            if (jigsawPuzzle.solved)
            {
                // Если пазл решен, выводим статистику
                GUI.skin.box.fontSize = 24;
                GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                GUI.Box(new Rect(Screen.width - 320, 20, 300, 100),
                    new GUIContent("- SOLVED -\n" + jigsawPuzzle.moves + " moves\n" + DispTime()));
            }
        }

        private void Puzzle()
        {
            PuzzleNumber(puzzleImageIndex++);
        }

        private void PuzzleNumber(int number)
        {
            // Увеличиваем индекс изображения
            puzzleImageIndex = number;

            // Если индекс превышает количество изображений в списке, сбрасываем его в 0
            if (puzzleImageIndex >= images.Count) puzzleImageIndex = 0;

            // Получаем текущее изображение из списка и устанавливаем его в пазл
            jigsawPuzzle.image = images[puzzleImageIndex];
            puzzle.transform.localScale = new Vector3(8.25f, 5.15625f, 0.738487959f);

            // Применяем масштаб для текущего изображения
            /*switch (puzzleImageIndex)
        {
          case 0:
            puzzle.transform.localScale = new Vector3(8, 5, puzzle.transform.localScale.z);
            break;
          case 1:
            puzzle.transform.localScale = new Vector3(6, 6, puzzle.transform.localScale.z);
            break;
          case 2:
            puzzle.transform.localScale = new Vector3(5, 7, puzzle.transform.localScale.z);
            break;
          // Добавьте другие случаи, если необходимо для большего количества текстур
        }*/

            SetSize();

            Restart();
        }

        private void SetSize()
        {
            switch (sizeMode)
            {
                case 1:
                    jigsawPuzzle.size = new Vector2(3, 2);
                    break;
                case 2:
                    jigsawPuzzle.size = new Vector2(4, 3);
                    break;
                case 3:
                    jigsawPuzzle.size = new Vector2(6, 4);
                    break;
                case 4:
                    jigsawPuzzle.size = new Vector2(8, 6);
                    break;
                case 5:
                    jigsawPuzzle.size = new Vector2(12, 8);
                    break;
                case 6:
                    jigsawPuzzle.size = new Vector2(25, 15);
                    break;
            }


            /*switch (puzzleImageIndex)
        {
          case 0:
            switch (sizeMode)
            {
              case 1:
                jigsawPuzzle.size = new Vector2(3, 2);
                break;
              case 2:
                jigsawPuzzle.size = new Vector2(4, 3);
                break;
              case 3:
                jigsawPuzzle.size = new Vector2(6, 4);
                break;
              case 4:
                jigsawPuzzle.size = new Vector2(8, 6);
                break;
              case 5:
                jigsawPuzzle.size = new Vector2(12, 8);
                break;
              case 6:
                jigsawPuzzle.size = new Vector2(25, 15);
                break;
            }

            break;
          case 1:
            switch (sizeMode)
            {
              case 1:
                jigsawPuzzle.size = new Vector2(3, 3);
                break;
              case 2:
                jigsawPuzzle.size = new Vector2(4, 4);
                break;
              case 3:
                jigsawPuzzle.size = new Vector2(6, 6);
                break;
              case 4:
                jigsawPuzzle.size = new Vector2(8, 8);
                break;
              case 5:
                jigsawPuzzle.size = new Vector2(12, 12);
                break;
              case 6:
                jigsawPuzzle.size = new Vector2(25, 25);
                break;
            }

            break;
          case 2:
            switch (sizeMode)
            {
              case 1:
                jigsawPuzzle.size = new Vector2(2, 3);
                break;
              case 2:
                jigsawPuzzle.size = new Vector2(3, 4);
                break;
              case 3:
                jigsawPuzzle.size = new Vector2(4, 6);
                break;
              case 4:
                jigsawPuzzle.size = new Vector2(6, 8);
                break;
              case 5:
                jigsawPuzzle.size = new Vector2(8, 12);
                break;
              case 6:
                jigsawPuzzle.size = new Vector2(25, 25);
                break;
            }

            break;
        }*/
        }

        private void Pieces()
        {
            // Логика изменения размеров пазла
            sizeMode++;

            if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) &&
                sizeMode == 6)
                sizeMode = 1;
            else if (sizeMode == 7) sizeMode = 1;

            SetSize();
        }

        private void Restart()
        {
            // Случайное начальное положение верхней левой части пазла
            var topLeft = "" + ((int)Mathf.Floor(Random.value * 5) + 1) + ((int)Mathf.Floor(Random.value * 5) + 1);

            while (jigsawPuzzle.topLeftPiece == topLeft)
                topLeft = "" + ((int)Mathf.Floor(Random.value * 5) + 1) + ((int)Mathf.Floor(Random.value * 5) + 1);

            // Устанавливаем верхнюю левую часть пазла, чтобы перезапуск был принудительным
            jigsawPuzzle.topLeftPiece = topLeft;
        }

        private string DispTime()
        {
            if (jigsawPuzzle.time < 60) return string.Format("{0:0} seconds", jigsawPuzzle.time);

            return string.Format("{0:0.0} minutes", jigsawPuzzle.time / 60);
        }
    }
}