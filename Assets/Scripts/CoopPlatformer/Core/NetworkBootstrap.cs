using Cysharp.Threading.Tasks;
using CoopPlatformer.Infrastructure;
using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using UnityEngine;

namespace CoopPlatformer.Core
{
    /// <summary>
    /// Entry point for the network session. Orchestrates Auth -> Relay -> Lobby -> NGO.
    /// Senior Tip: Use a state machine for connection management to handle edge cases (timeout, failure).
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private string _lobbyName = "CoopRoom";

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
            string relayCode = await RelayProvider.CreateRelayHostAsync();
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

            NetworkManager.Singleton.StartHost();
            Debug.Log($"[Bootstrap] Host started successfully. Lobby Code: {lobby.LobbyCode}");
            StatusChanged?.Invoke($"Host ready. Lobby Code: {lobby.LobbyCode}");
            return true;
        }

        public async UniTask<bool> StartServerOnly()
        {
            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync())
            {
                StatusChanged?.Invoke("Sign-in failed.");
                return false;
            }

            StatusChanged?.Invoke("Creating relay...");
            string relayCode = await RelayProvider.CreateRelayHostAsync();
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

            NetworkManager.Singleton.StartServer();
            Debug.Log($"[Bootstrap] Server started successfully. Lobby Code: {lobby.LobbyCode}");
            StatusChanged?.Invoke($"Server ready. Lobby Code: {lobby.LobbyCode}");
            return true;
        }

        public async UniTask<bool> JoinRoom(string lobbyCode)
        {
            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync())
            {
                StatusChanged?.Invoke("Sign-in failed.");
                return false;
            }

            StatusChanged?.Invoke("Joining lobby...");
            string relayCode = await LobbyProvider.JoinLobbyByCodeAsync(lobbyCode);
            if (string.IsNullOrEmpty(relayCode))
            {
                StatusChanged?.Invoke("Lobby join failed.");
                return false;
            }

            if (await RelayProvider.JoinRelayAsync(relayCode))
            {
                NetworkManager.Singleton.StartClient();
                Debug.Log("[Bootstrap] Client started successfully.");
                StatusChanged?.Invoke("Connected to host.");
                return true;
            }

            StatusChanged?.Invoke("Relay join failed.");
            return false;
        }

        public async UniTask<List<Lobby>> QueryRooms()
        {
            StatusChanged?.Invoke("Refreshing rooms...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync())
            {
                StatusChanged?.Invoke("Sign-in failed.");
                return new List<Lobby>();
            }

            var lobbies = await LobbyProvider.QueryPublicLobbiesAsync();
            StatusChanged?.Invoke(lobbies.Count > 0 ? $"Found {lobbies.Count} room(s)." : "No public rooms found.");
            return lobbies;
        }

        public async UniTask<bool> JoinRoomById(string lobbyId)
        {
            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync())
            {
                StatusChanged?.Invoke("Sign-in failed.");
                return false;
            }

            StatusChanged?.Invoke("Joining room...");
            string relayCode = await LobbyProvider.JoinLobbyByIdAsync(lobbyId);
            if (string.IsNullOrEmpty(relayCode))
            {
                StatusChanged?.Invoke("Room join failed.");
                return false;
            }

            if (await RelayProvider.JoinRelayAsync(relayCode))
            {
                NetworkManager.Singleton.StartClient();
                Debug.Log("[Bootstrap] Client started successfully.");
                StatusChanged?.Invoke("Connected to host.");
                return true;
            }

            StatusChanged?.Invoke("Relay join failed.");
            return false;
        }
    }
}
