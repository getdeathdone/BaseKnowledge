using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace CoopPlatformer.Infrastructure
{
    /// <summary>
    /// Manages Unity Relay allocations and transport configuration.
    /// Senior Tip: Use ServerEndpoints to explicitly choose between 'udp' and 'dtls' (secure) connections.
    /// </summary>
    public static class RelayProvider
    {
        private const string ConnectionType = "udp"; // Use UDP for better compatibility during development

        public static async UniTask<string> CreateRelayHostAsync(int maxConnections = 4)
        {
            try
            {
                Debug.Log("[Relay] Requesting allocation in europe-central2...");
                
                // Senior Tip: Sometimes manual region selection bypasses QoS bugs in older SDK versions
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, "europe-central2");
                
                if (allocation == null)
                {
                    Debug.LogError("[Relay] Allocation failed: Server returned null.");
                    return null;
                }

                Debug.Log($"[Relay] Allocation successful (ID: {allocation.AllocationId}). Requesting Join Code...");
                
                // Small delay to let the server "catch up"
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                if (string.IsNullOrEmpty(joinCode))
                {
                    Debug.LogError("[Relay] Failed to get Join Code: Server returned empty string.");
                    return null;
                }

                var endpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == ConnectionType);
                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();

                utp.SetHostRelayData(
                    endpoint.Host,
                    (ushort)endpoint.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    ConnectionType == "dtls"
                );

                Debug.Log($"[Relay] Host created via {ConnectionType}. Join Code: {joinCode}");
                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"[Relay] Relay Service Exception: {e.Reason} (Error Code: {e.ErrorCode})");
                Debug.LogError($"[Relay] Message: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Relay] Failed to create allocation: {e.Message}");
                return null;
            }
        }

        public static async UniTask<bool> JoinRelayAsync(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var endpoint = joinAllocation.ServerEndpoints.First(e => e.ConnectionType == ConnectionType);

                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();

                utp.SetClientRelayData(
                    endpoint.Host,
                    (ushort)endpoint.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData,
                    ConnectionType == "dtls"
                );

                Debug.Log($"[Relay] Joined allocation via {ConnectionType} with code: {joinCode}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Relay] Failed to join allocation: {e.Message}");
                return false;
            }
        }
    }
}
