using System;
using System.Collections.Generic;
using CoopPlatformer.Infrastructure;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Jigsaw.Networking
{
    public class JigsawNetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private string _lobbyName = "JigsawPuzzleRoom";
        private bool _createdLobbyAsHost;

        public event Action<string> StatusChanged;

        public async UniTask<bool> StartHost()
        {
            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync())
            {
                StatusChanged?.Invoke("Sign-in failed.");
                return false;
            }

            StatusChanged?.Invoke("Creating relay...");
            var relayCode = await RelayProvider.CreateRelayHostAsync();
            if (string.IsNullOrEmpty(relayCode))
            {
                StatusChanged?.Invoke("Relay creation failed.");
                return false;
            }

            StatusChanged?.Invoke("Creating lobby...");
            var lobby = await LobbyProvider.CreateLobbyAsync(_lobbyName, relayCode);
            if (lobby == null)
            {
                StatusChanged?.Invoke("Lobby creation failed.");
                return false;
            }

            if (!NetworkManager.Singleton.StartHost())
            {
                await LobbyProvider.DeleteLobbyAsync();
                StatusChanged?.Invoke("Host start failed.");
                return false;
            }

            _createdLobbyAsHost = true;
            SetLandscapeOrientation();
            StatusChanged?.Invoke($"Host ready. Code: {lobby.LobbyCode}");
            return true;
        }

        public async UniTask<bool> StartServerOnly()
        {
            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync()) return false;

            StatusChanged?.Invoke("Creating relay...");
            var relayCode = await RelayProvider.CreateRelayHostAsync();
            if (string.IsNullOrEmpty(relayCode)) return false;

            StatusChanged?.Invoke("Creating lobby...");
            var lobby = await LobbyProvider.CreateLobbyAsync(_lobbyName, relayCode);
            if (lobby == null) return false;

            if (!NetworkManager.Singleton.StartServer())
            {
                await LobbyProvider.DeleteLobbyAsync();
                return false;
            }

            _createdLobbyAsHost = true;
            SetLandscapeOrientation();
            StatusChanged?.Invoke($"Server ready. Code: {lobby.LobbyCode}");
            return true;
        }

        public async UniTask<bool> JoinRoom(string lobbyCode)
        {
            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync()) return false;

            StatusChanged?.Invoke("Joining lobby...");
            var relayCode = await LobbyProvider.JoinLobbyByCodeAsync(lobbyCode);
            if (string.IsNullOrEmpty(relayCode))
            {
                StatusChanged?.Invoke("Lobby not found.");
                return false;
            }

            if (await RelayProvider.JoinRelayAsync(relayCode))
            {
                if (!NetworkManager.Singleton.StartClient())
                {
                    await LobbyProvider.LeaveLobbyAsync();
                    return false;
                }
                SetLandscapeOrientation();
                StatusChanged?.Invoke("Connected.");
                return true;
            }
            return false;
        }

        public async UniTask<List<Lobby>> QueryRooms()
        {
            StatusChanged?.Invoke("Refreshing...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync()) return new List<Lobby>();
            return await LobbyProvider.QueryPublicLobbiesAsync();
        }

        public async UniTask<bool> JoinRoomById(string lobbyId)
        {
            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync()) return false;

            StatusChanged?.Invoke("Joining...");
            var relayCode = await LobbyProvider.JoinLobbyByIdAsync(lobbyId);
            if (string.IsNullOrEmpty(relayCode)) return false;

            if (await RelayProvider.JoinRelayAsync(relayCode))
            {
                if (!NetworkManager.Singleton.StartClient())
                {
                    await LobbyProvider.LeaveLobbyAsync();
                    return false;
                }
                SetLandscapeOrientation();
                return true;
            }
            return false;
        }

        private void SetLandscapeOrientation()
        {
            Debug.Log("Setting orientation to Landscape...");
            Screen.orientation = ScreenOrientation.LandscapeRight; 
            Screen.autorotateToLandscapeLeft = true; 
            Screen.autorotateToLandscapeRight = true; 
            Screen.autorotateToPortrait = false; 
            Screen.autorotateToPortraitUpsideDown = false; 
            Screen.orientation = ScreenOrientation.AutoRotation; 
        }

        private void OnDestroy()
        {
            CleanupSessionAsync().Forget();
        }

        private async UniTaskVoid CleanupSessionAsync()
        {
            if (!Application.isPlaying) return;
            if (_createdLobbyAsHost) await LobbyProvider.DeleteLobbyAsync();
            else await LobbyProvider.LeaveLobbyAsync();
        }
    }
}
