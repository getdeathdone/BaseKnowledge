using CoopPlatformer.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CoopPlatformer.UI
{
    
    
    
    
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap _bootstrap;
        
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField _joinCodeInput;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _hostClientButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private RectTransform _roomListRoot;
        [SerializeField] private TMP_Text _roomListEmptyLabel;

        private void Start()
        {
            _hostButton.onClick.AddListener(() => OnHostOnlyClicked().Forget());
            _clientButton.onClick.AddListener(() => OnClientClicked().Forget());
            _hostClientButton.onClick.AddListener(() => OnHostClientClicked().Forget());
            if (_refreshButton != null)
            {
                _refreshButton.onClick.AddListener(() => OnRefreshClicked().Forget());
            }
            if (_bootstrap != null)
            {
                _bootstrap.StatusChanged += HandleStatusChanged;
            }

            HandleStatusChanged("Host creates a room. Join uses Lobby Code.");
            ClearRoomList();
        }

        private void OnDestroy()
        {
            if (_bootstrap != null)
            {
                _bootstrap.StatusChanged -= HandleStatusChanged;
            }
        }

        private async UniTaskVoid OnHostOnlyClicked()
        {
            SetLoading(true);
            try
            {
                if (await _bootstrap.StartServerOnly())
                {
                    gameObject.SetActive(false);
                }
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async UniTaskVoid OnHostClientClicked()
        {
            SetLoading(true);
            try
            {
                if (await _bootstrap.StartHost())
                {
                    gameObject.SetActive(false);
                }
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async UniTaskVoid OnClientClicked()
        {
            string code = _joinCodeInput.text.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(code)) return;

            SetLoading(true);
            try
            {
                if (await _bootstrap.JoinRoom(code))
                {
                    gameObject.SetActive(false);
                }
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool isLoading)
        {
            _loadingOverlay.SetActive(isLoading);
            _hostButton.interactable = !isLoading;
            _clientButton.interactable = !isLoading;
            _hostClientButton.interactable = !isLoading;
            if (_refreshButton != null)
            {
                _refreshButton.interactable = !isLoading;
            }
        }

        private void HandleStatusChanged(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }
        }

        private async UniTaskVoid OnRefreshClicked()
        {
            SetLoading(true);
            try
            {
                var rooms = await _bootstrap.QueryRooms();
                RenderRoomList(rooms);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void ClearRoomList()
        {
            if (_roomListRoot == null)
            {
                return;
            }

            for (var i = _roomListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_roomListRoot.GetChild(i).gameObject);
            }

            if (_roomListEmptyLabel != null)
            {
                _roomListEmptyLabel.gameObject.SetActive(true);
                _roomListEmptyLabel.text = "No rooms loaded.";
            }
        }

        private void RenderRoomList(List<Lobby> rooms)
        {
            ClearRoomList();
            if (_roomListRoot == null)
            {
                return;
            }

            if (rooms == null || rooms.Count == 0)
            {
                if (_roomListEmptyLabel != null)
                {
                    _roomListEmptyLabel.text = "No public rooms found.";
                }
                return;
            }

            if (_roomListEmptyLabel != null)
            {
                _roomListEmptyLabel.gameObject.SetActive(false);
            }

            foreach (var room in rooms)
            {
                CreateRoomEntry(room);
            }
        }

        private void CreateRoomEntry(Lobby room)
        {
            var row = new GameObject($"Room_{room.Id}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(_roomListRoot, false);

            var rowImage = row.GetComponent<Image>();
            rowImage.color = new Color(0.1f, 0.14f, 0.2f, 0.92f);

            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.spacing = 10f;
            rowLayout.padding = new RectOffset(12, 12, 8, 8);

            var rowFitter = row.AddComponent<ContentSizeFitter>();
            rowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var info = new GameObject("Info", typeof(RectTransform), typeof(TextMeshProUGUI));
            info.transform.SetParent(row.transform, false);
            var infoText = info.GetComponent<TextMeshProUGUI>();
            infoText.fontSize = 17f;
            infoText.color = Color.white;
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.text = $"{room.Name}  {room.Players.Count}/{room.MaxPlayers}";

            var infoLayout = info.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1f;
            infoLayout.minWidth = 180f;

            var joinButton = CreateActionButton("Join", new Color(0.95f, 0.45f, 0.22f), row.transform, new Vector2(72f, 34f));
            joinButton.onClick.AddListener(() => JoinRoomFromList(room.Id).Forget());
        }

        private Button CreateActionButton(string label, Color color, Transform parent, Vector2 size)
        {
            var buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = color;

            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var labelText = labelObject.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 16f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;

            return buttonObject.GetComponent<Button>();
        }

        private async UniTaskVoid JoinRoomFromList(string lobbyId)
        {
            SetLoading(true);
            try
            {
                if (await _bootstrap.JoinRoomById(lobbyId))
                {
                    gameObject.SetActive(false);
                }
            }
            finally
            {
                SetLoading(false);
            }
        }
    }
}
