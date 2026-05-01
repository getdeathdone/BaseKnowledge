using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Jigsaw.Networking
{
    public class JigsawLobbyUI : MonoBehaviour
    {
        [SerializeField] private JigsawNetworkBootstrap _bootstrap;

        [Header("UI Elements")] 
        [SerializeField] private TMP_InputField _joinCodeInput;

        [SerializeField] private Button _hostOnlyButton;
        [SerializeField] private Button _clientButton; 
        [SerializeField] private Button _hostClientButton;
        [SerializeField] private Button _refreshButton;
        
        [SerializeField] private GameObject _mainPanel; 
        [SerializeField] private GameObject _uiPanel; 
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private RectTransform _roomListRoot;
        [SerializeField] private TMP_Text _roomListEmptyLabel;

        private void Start()
        {
            if (_hostOnlyButton != null) _hostOnlyButton.onClick.AddListener(() => OnHostOnlyClicked().Forget());
            if (_clientButton != null) _clientButton.onClick.AddListener(() => OnJoinByCodeClicked().Forget());
            if (_hostClientButton != null) _hostClientButton.onClick.AddListener(() => OnHostClientClicked().Forget());
            if (_refreshButton != null) _refreshButton.onClick.AddListener(() => OnRefreshClicked().Forget());

            if (_bootstrap != null) _bootstrap.StatusChanged += UpdateStatus;

            UpdateStatus("Выберите режим или введите код комнаты.");
            ClearRoomList();
            OnRefreshClicked().Forget();
        }

        private void OnDestroy()
        {
            if (_bootstrap != null) _bootstrap.StatusChanged -= UpdateStatus;
        }

        private async UniTaskVoid OnHostOnlyClicked()
        {
            SetLoading(true);
            if (await _bootstrap.StartServerOnly()) _mainPanel.SetActive(false);
            SetLoading(false);
        }

        private async UniTaskVoid OnHostClientClicked()
        {
            SetLoading(true);
            if (await _bootstrap.StartHost()) _mainPanel.SetActive(false);
            SetLoading(false);
        }

        private async UniTaskVoid OnJoinByCodeClicked()
        {
            string code = _joinCodeInput.text.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(code)) return;

            SetLoading(true);
            if (await _bootstrap.JoinRoom(code))
            {
                _mainPanel.SetActive(false);
                _uiPanel.SetActive(false);
            };
            SetLoading(false);
        }

        private async UniTaskVoid OnRefreshClicked()
        {
            SetLoading(true);
            var rooms = await _bootstrap.QueryRooms();
            RenderRoomList(rooms);
            SetLoading(false);
        }

        private void ClearRoomList()
        {
            if (_roomListRoot == null) return;
            foreach (Transform child in _roomListRoot) Destroy(child.gameObject);
            if (_roomListEmptyLabel != null) _roomListEmptyLabel.gameObject.SetActive(true);
        }

        private void RenderRoomList(List<Lobby> rooms)
        {
            ClearRoomList();
            if (_roomListRoot == null) return;

            if (rooms == null || rooms.Count == 0)
            {
                if (_roomListEmptyLabel != null) _roomListEmptyLabel.text = "Публичных комнат не найдено.";
                return;
            }

            if (_roomListEmptyLabel != null) _roomListEmptyLabel.gameObject.SetActive(false);

            foreach (var room in rooms)
            {
                CreateRoomEntry(room);
            }
        }

        private void CreateRoomEntry(Lobby room)
        {
            GameObject entry = new GameObject($"Room_{room.Name}", typeof(RectTransform), typeof(Image));
            entry.transform.SetParent(_roomListRoot, false);
            
            var image = entry.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var layout = entry.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.spacing = 10;
            layout.childControlWidth = true;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(entry.transform, false);
            var text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = $"{room.Name} ({room.Players.Count}/{room.MaxPlayers})";
            text.fontSize = 18;

            GameObject btnObj = new GameObject("JoinBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(entry.transform, false);
            btnObj.GetComponent<Image>().color = Color.green;
            var btn = btnObj.GetComponent<Button>();
            
            GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btnText = btnTextObj.GetComponent<TextMeshProUGUI>();
            btnText.text = "Join";
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.black;
            btnText.fontSize = 18;

            btn.onClick.AddListener(() => OnJoinByIdClicked(room.Id).Forget());
        }

        private async UniTaskVoid OnJoinByIdClicked(string lobbyId)
        {
            SetLoading(true);
            if (await _bootstrap.JoinRoomById(lobbyId))
            {
                _mainPanel.SetActive(false);
                _uiPanel.SetActive(false);
            }
            SetLoading(false);
        }

        private void SetLoading(bool isLoading)
        {
            if (_loadingOverlay != null) _loadingOverlay.SetActive(isLoading);
            _hostOnlyButton.interactable = !isLoading;
            _hostClientButton.interactable = !isLoading;
            if (_refreshButton != null) _refreshButton.interactable = !isLoading;
        }

        private void UpdateStatus(string message)
        {
            if (_statusLabel != null) _statusLabel.text = message;
        }
    }
}
