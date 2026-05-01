using System.Collections.Generic;
using Jigsaw.Scripts.Core;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Jigsaw.Scripts.Networking
{
    public class JigsawNetworkSync : NetworkBehaviour
    {
        public NetworkVariable<int> ImageIndex = new();
        public NetworkVariable<Vector2> PuzzleSize = new(new Vector2(5, 5));
        public NetworkVariable<FixedString64Bytes> TopLeftPiece = new("11");
        private Game.Game _game;
        private readonly Dictionary<string, ulong> _lockedPieces = new();

        private readonly Dictionary<string, GameObject> _pieceNameToObj = new();
        private JigsawPuzzle _puzzle;

        private void Awake()
        {
            _puzzle = GetComponent<JigsawPuzzle>();
            _game = FindObjectOfType<Game.Game>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                ImageIndex.Value = Game.Game.PUZZLE_Number;
                PuzzleSize.Value = _puzzle.size;
                TopLeftPiece.Value = _puzzle.topLeftPiece;

                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
            else
            {
                ApplyNetworkSettings();
            }

            ImageIndex.OnValueChanged += (oldVal, newVal) => ApplyNetworkSettings();
            PuzzleSize.OnValueChanged += (oldVal, newVal) => ApplyNetworkSettings();
            TopLeftPiece.OnValueChanged += (oldVal, newVal) => ApplyNetworkSettings();

            // Hide UI buttons for clients
            if (!IsHost)
                if (_game != null)
                {
                    if (_game.guiMenuNextButton != null) _game.guiMenuNextButton.gameObject.SetActive(false);
                    if (_game.guiMenuPiecesButton != null) _game.guiMenuPiecesButton.gameObject.SetActive(false);
                    if (_game.guiMenuRestartButton != null) _game.guiMenuRestartButton.gameObject.SetActive(false);
                }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;
            // Batch sync could be implemented here if needed
        }

        private void ApplyNetworkSettings()
        {
            if (IsHost) return;

            // Sync image
            if (_game != null && ImageIndex.Value < _game.images.Count) _puzzle.image = _game.images[ImageIndex.Value];

            _puzzle.size = PuzzleSize.Value;
            _puzzle.topLeftPiece = TopLeftPiece.Value.ToString();
            _puzzle.Restart();
        }

        public void UpdateSettings(int imageIndex, Vector2 size, string topLeft)
        {
            if (!IsServer) return;
            ImageIndex.Value = imageIndex;
            PuzzleSize.Value = size;
            TopLeftPiece.Value = topLeft;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDragServerRpc(string pieceName, ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
            if (!_lockedPieces.ContainsKey(pieceName) || _lockedPieces[pieceName] == 0)
            {
                _lockedPieces[pieceName] = clientId;
                OnDragStartedClientRpc(pieceName, clientId);
            }
        }

        [ClientRpc]
        private void OnDragStartedClientRpc(string pieceName, ulong ownerId)
        {
            _lockedPieces[pieceName] = ownerId;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePiecePositionServerRpc(string pieceName, Vector3 position, ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
            if (_lockedPieces.ContainsKey(pieceName) && _lockedPieces[pieceName] == clientId)
                UpdatePiecePositionClientRpc(pieceName, position, clientId);
        }

        [ClientRpc]
        private void UpdatePiecePositionClientRpc(string pieceName, Vector3 position, ulong ownerId)
        {
            if (ownerId == NetworkManager.Singleton.LocalClientId) return;

            var piece = GetPieceByName(pieceName);
            if (piece != null) piece.transform.position = position;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReleaseDragServerRpc(string pieceName, Vector3 position, bool isPlaced,
            ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
            if (_lockedPieces.ContainsKey(pieceName) && _lockedPieces[pieceName] == clientId)
            {
                _lockedPieces[pieceName] = 0;
                OnDragReleasedClientRpc(pieceName, position, isPlaced);
            }
        }

        [ClientRpc]
        private void OnDragReleasedClientRpc(string pieceName, Vector3 position, bool isPlaced)
        {
            _lockedPieces[pieceName] = 0;
            var piece = GetPieceByName(pieceName);
            if (piece != null)
            {
                piece.transform.position = position;
                if (isPlaced)
                {
                    var puzzleContainer = GameObject.Find("puzzleContainer");
                    if (puzzleContainer != null) piece.transform.parent = puzzleContainer.transform;
                }
            }
        }

        private GameObject GetPieceByName(string pieceName)
        {
            if (_pieceNameToObj.TryGetValue(pieceName, out var obj) && obj != null) return obj;

            var piecesContainer = GameObject.Find("piecesContainer");
            if (piecesContainer != null)
                foreach (Transform child in piecesContainer.transform)
                    if (child.name == pieceName)
                    {
                        _pieceNameToObj[pieceName] = child.gameObject;
                        return child.gameObject;
                    }

            var puzzleContainer = GameObject.Find("puzzleContainer");
            if (puzzleContainer != null)
                foreach (Transform child in puzzleContainer.transform)
                    if (child.name == pieceName)
                    {
                        _pieceNameToObj[pieceName] = child.gameObject;
                        return child.gameObject;
                    }

            return null;
        }

        public bool IsPieceLocked(string pieceName)
        {
            return _lockedPieces.ContainsKey(pieceName) && _lockedPieces[pieceName] != 0 &&
                   _lockedPieces[pieceName] != NetworkManager.Singleton.LocalClientId;
        }
    }
}