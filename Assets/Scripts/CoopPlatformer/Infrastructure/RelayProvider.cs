using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using UnityEngine;

namespace CoopPlatformer.Infrastructure
{
    public static class RelayProvider
    {
        private const string ConnectionType = "udp";

        public static async UniTask<string> CreateRelayHostAsync(int maxConnections = 4, string region = null)
        {
            try
            {
                Debug.Log(string.IsNullOrEmpty(region)
                    ? "[Relay] Requesting allocation with automatic region."
                    : $"[Relay] Requesting allocation in {region}.");

                var allocation = string.IsNullOrEmpty(region)
                    ? await RelayService.Instance.CreateAllocationAsync(maxConnections)
                    : await RelayService.Instance.CreateAllocationAsync(maxConnections, region);

                if (allocation == null)
                {
                    Debug.LogError("[Relay] Allocation failed: Server returned null.");
                    return null;
                }

                Debug.Log($"[Relay] Allocation successful (ID: {allocation.AllocationId}). Requesting Join Code...");

                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                if (string.IsNullOrEmpty(joinCode))
                {
                    Debug.LogError("[Relay] Failed to get Join Code: Server returned empty string.");
                    return null;
                }

                var endpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == ConnectionType);
                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (utp == null)
                {
                    Debug.LogError("[Relay] UnityTransport component is missing on NetworkManager.");
                    return null;
                }

                utp.SetHostRelayData(
                    endpoint.Host,
                    (ushort)endpoint.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
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
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var endpoint = joinAllocation.ServerEndpoints.First(e => e.ConnectionType == ConnectionType);

                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (utp == null)
                {
                    Debug.LogError("[Relay] UnityTransport component is missing on NetworkManager.");
                    return false;
                }

                utp.SetClientRelayData(
                    endpoint.Host,
                    (ushort)endpoint.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
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