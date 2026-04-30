using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace CoopPlatformer.Infrastructure
{
    /// <summary>
    /// Handles Lobby lifecycle and Relay integration.
    /// Senior Tip: Implement heartbeats and cleanup to ensure lobby health.
    /// </summary>
    public static class LobbyProvider
    {
        private const string RelayKey = "RelayJoinCode";
        private static Lobby _currentLobby;
        private static int _heartbeatGeneration;

        public static async UniTask<Lobby> CreateLobbyAsync(string lobbyName, string relayJoinCode, int maxPlayers = 4)
        {
            try
            {
                var options = new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { RelayKey, new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                    }
                };

                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                
                _heartbeatGeneration++;
                _ = SendHeartbeatAsync(_currentLobby.Id, _heartbeatGeneration);
                
                Debug.Log($"[Lobby] Created: {_currentLobby.Name} ({_currentLobby.Id})");
                return _currentLobby;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Create failed: {e.Message}");
                return null;
            }
        }

        public static async UniTask<string> JoinLobbyByCodeAsync(string lobbyCode)
        {
            try
            {
                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
                return _currentLobby.Data[RelayKey].Value;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Join failed: {e.Message}");
                return null;
            }
        }

        public static async UniTask<string> JoinLobbyByIdAsync(string lobbyId)
        {
            try
            {
                _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
                return _currentLobby.Data[RelayKey].Value;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Join by ID failed: {e.Message}");
                return null;
            }
        }

        public static async UniTask<List<Lobby>> QueryPublicLobbiesAsync(int maxResults = 10)
        {
            try
            {
                var options = new QueryLobbiesOptions
                {
                    Count = maxResults,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "0")
                    },
                    Order = new List<QueryOrder>
                    {
                        new QueryOrder(false, QueryOrder.FieldOptions.Created)
                    }
                };

                var response = await LobbyService.Instance.QueryLobbiesAsync(options);
                return response.Results;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Query failed: {e.Message}");
                return new List<Lobby>();
            }
        }

        private static async UniTaskVoid SendHeartbeatAsync(string lobbyId, int generation)
        {
            while (_currentLobby != null && _currentLobby.Id == lobbyId && generation == _heartbeatGeneration)
            {
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                    await UniTask.Delay(TimeSpan.FromSeconds(15));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Lobby] Heartbeat failed: {e.Message}");
                    break;
                }
            }
        }

        public static async UniTask LeaveLobbyAsync()
        {
            if (_currentLobby == null) return;

            try
            {
                _heartbeatGeneration++;
                string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, playerId);
                _currentLobby = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Leave failed: {e.Message}");
            }
        }

        public static async UniTask DeleteLobbyAsync()
        {
            if (_currentLobby == null) return;

            try
            {
                _heartbeatGeneration++;
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Delete failed: {e.Message}");
            }
            finally
            {
                _currentLobby = null;
            }
        }
    }
}
