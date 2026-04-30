using System;
using System.Collections.Generic;
using CoopPlatformer.Core.Configuration;
using CoopPlatformer.Infrastructure;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace CoopPlatformer.Core
{
    public class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private string _lobbyName = "CoopRoom";
        private bool _createdLobbyAsHost;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void OnDestroy()
        {
            CleanupSessionAsync().Forget();
        }

        private void OnApplicationQuit()
        {
            CleanupSessionAsync().Forget();
        }

        public event Action<string> StatusChanged;

        public async UniTask<bool> StartHost()
        {
            if (!ConfigureNetworkPrefabs())
            {
                StatusChanged?.Invoke("Network prefabs are not configured.");
                return false;
            }

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
            Debug.Log($"[Bootstrap] Host started successfully. Lobby Code: {lobby.LobbyCode}");
            StatusChanged?.Invoke($"Host ready. Lobby Code: {lobby.LobbyCode}");
            return true;
        }

        public async UniTask<bool> StartServerOnly()
        {
            if (!ConfigureNetworkPrefabs())
            {
                StatusChanged?.Invoke("Network prefabs are not configured.");
                return false;
            }

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

            if (!NetworkManager.Singleton.StartServer())
            {
                await LobbyProvider.DeleteLobbyAsync();
                StatusChanged?.Invoke("Server start failed.");
                return false;
            }

            _createdLobbyAsHost = true;
            Debug.Log($"[Bootstrap] Server started successfully. Lobby Code: {lobby.LobbyCode}");
            StatusChanged?.Invoke($"Server ready. Lobby Code: {lobby.LobbyCode}");
            return true;
        }

        public async UniTask<bool> JoinRoom(string lobbyCode)
        {
            if (!ConfigureNetworkPrefabs())
            {
                StatusChanged?.Invoke("Network prefabs are not configured.");
                return false;
            }

            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync())
            {
                StatusChanged?.Invoke("Sign-in failed.");
                return false;
            }

            StatusChanged?.Invoke("Joining lobby...");
            var relayCode = await LobbyProvider.JoinLobbyByCodeAsync(lobbyCode);
            if (string.IsNullOrEmpty(relayCode))
            {
                StatusChanged?.Invoke("Lobby join failed.");
                return false;
            }

            if (await RelayProvider.JoinRelayAsync(relayCode))
            {
                if (!NetworkManager.Singleton.StartClient())
                {
                    await LobbyProvider.LeaveLobbyAsync();
                    StatusChanged?.Invoke("Client start failed.");
                    return false;
                }

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
            if (!ConfigureNetworkPrefabs())
            {
                StatusChanged?.Invoke("Network prefabs are not configured.");
                return false;
            }

            StatusChanged?.Invoke("Signing in...");
            if (!await AuthenticationProvider.InitializeAndSignInAsync())
            {
                StatusChanged?.Invoke("Sign-in failed.");
                return false;
            }

            StatusChanged?.Invoke("Joining room...");
            var relayCode = await LobbyProvider.JoinLobbyByIdAsync(lobbyId);
            if (string.IsNullOrEmpty(relayCode))
            {
                StatusChanged?.Invoke("Room join failed.");
                return false;
            }

            if (await RelayProvider.JoinRelayAsync(relayCode))
            {
                if (!NetworkManager.Singleton.StartClient())
                {
                    await LobbyProvider.LeaveLobbyAsync();
                    StatusChanged?.Invoke("Client start failed.");
                    return false;
                }

                Debug.Log("[Bootstrap] Client started successfully.");
                StatusChanged?.Invoke("Connected to host.");
                return true;
            }

            StatusChanged?.Invoke("Relay join failed.");
            return false;
        }

        private async UniTaskVoid CleanupSessionAsync()
        {
            if (!Application.isPlaying) return;

            try
            {
                if (_createdLobbyAsHost)
                {
                    await LobbyProvider.DeleteLobbyAsync();
                    _createdLobbyAsHost = false;
                }
                else
                {
                    await LobbyProvider.LeaveLobbyAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bootstrap] Cleanup failed: {e.Message}");
            }
        }

        private bool ConfigureNetworkPrefabs()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[Bootstrap] NetworkManager.Singleton is missing.");
                return false;
            }

            var registry = GameplayPrefabRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("[Bootstrap] GameplayPrefabRegistry is not configured.");
                return false;
            }

            if (registry.ShipPrefab != null) networkManager.NetworkConfig.PlayerPrefab = registry.ShipPrefab.gameObject;


            AddPrefabIfNotExists(networkManager, registry.ShipPrefab?.gameObject);
            AddPrefabIfNotExists(networkManager, registry.ProjectilePrefab?.gameObject);
            AddPrefabIfNotExists(networkManager, registry.AsteroidPrefab?.gameObject);

            return true;
        }

        private void AddPrefabIfNotExists(NetworkManager manager, GameObject prefab)
        {
            if (prefab == null) return;

            var prefabsList = manager.NetworkConfig.Prefabs.Prefabs;
            foreach (var networkPrefab in prefabsList)
                if (networkPrefab.Prefab == prefab)
                    return;

            manager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = prefab });
        }
    }
}