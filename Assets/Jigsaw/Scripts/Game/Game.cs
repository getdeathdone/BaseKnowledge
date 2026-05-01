using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jigsaw.Scripts.Game
{
    public class Game : MonoBehaviour
    {
        public List<Texture> images; 
        public GameObject puzzle;

        public Button guiMenuNextButton;
        public Button guiMenuPiecesButton;
        public Button guiMenuRestartButton;

        private GameJigsawPuzzle jigsawPuzzle;
        private int puzzleImageIndex; 
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

            jigsawPuzzle = puzzle.GetComponent<GameJigsawPuzzleMacIOS>();

            if (jigsawPuzzle == null) return;

            SetSize();
            PuzzleNumber(PUZZLE_Number);
        }

        private void OnGUI()
        {
            if (jigsawPuzzle.solved)
            {
                GUI.skin.box.fontSize = 24;
                GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                GUI.Box(new Rect(Screen.width - 320, 20, 300, 100),
                    new GUIContent("- SOLVED -\n" + jigsawPuzzle.moves + " moves\n" + DispTime()));
            }
        }

        private void Puzzle()
        {
            if (Unity.Netcode.NetworkManager.Singleton != null && !Unity.Netcode.NetworkManager.Singleton.IsHost) return;
            PuzzleNumber(puzzleImageIndex + 1);
        }

        public void UpdateNetworkSettings()
        {
            if (puzzle == null) return;
            var networkSync = puzzle.GetComponent<Networking.JigsawNetworkSync>();
            if (networkSync != null && networkSync.IsServer)
            {
                networkSync.UpdateSettings(puzzleImageIndex, jigsawPuzzle.size, jigsawPuzzle.topLeftPiece);
            }
        }

        private void PuzzleNumber(int number)
        {
            puzzleImageIndex = number;

            if (puzzleImageIndex >= images.Count) puzzleImageIndex = 0;

            jigsawPuzzle.image = images[puzzleImageIndex];
            puzzle.transform.localScale = new Vector3(8.25f, 5.15625f, 0.738487959f);

            SetSize();
            Restart();
            UpdateNetworkSettings();
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
        }

        private void Pieces()
        {
            if (Unity.Netcode.NetworkManager.Singleton != null && !Unity.Netcode.NetworkManager.Singleton.IsHost) return;

            sizeMode++;

            if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) &&
                sizeMode == 6)
                sizeMode = 1;
            else if (sizeMode == 7) sizeMode = 1;

            SetSize();
            UpdateNetworkSettings();
        }

        private void Restart()
        {
            if (Unity.Netcode.NetworkManager.Singleton != null && !Unity.Netcode.NetworkManager.Singleton.IsHost && Unity.Netcode.NetworkManager.Singleton.IsConnectedClient) return;

            int newImageIndex = Random.Range(0, images.Count);
            while (newImageIndex == puzzleImageIndex && images.Count > 1)
            {
                newImageIndex = Random.Range(0, images.Count);
            }
            
            puzzleImageIndex = newImageIndex;
            jigsawPuzzle.image = images[puzzleImageIndex];

            var topLeft = "" + ((int)Mathf.Floor(Random.value * 5) + 1) + ((int)Mathf.Floor(Random.value * 5) + 1);

            while (jigsawPuzzle.topLeftPiece == topLeft)
                topLeft = "" + ((int)Mathf.Floor(Random.value * 5) + 1) + ((int)Mathf.Floor(Random.value * 5) + 1);

            jigsawPuzzle.topLeftPiece = topLeft;
            
            UpdateNetworkSettings();
        }

        private string DispTime()
        {
            if (jigsawPuzzle.time < 60) return string.Format("{0:0} seconds", jigsawPuzzle.time);

            return string.Format("{0:0.0} minutes", jigsawPuzzle.time / 60);
        }
    }
}