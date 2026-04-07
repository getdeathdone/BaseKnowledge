using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TestLogic
{
    public class UltimateNetworkService : MonoBehaviour
    {
        private const int MaxRetries = 3;
        private const int BaseRetryDelayMs = 2000;
        private const string OfflineQueueKey = "OfflineDataQueue";

        private async void Start()
        {
            var cts = this.GetCancellationTokenOnDestroy();

            var regData = new PostData { title = "User123", body = "Registration", userId = 1 };
            string json = JsonUtility.ToJson(regData);

            var createdPost = await SendCriticalRequestAsync<PostData>(ApiConfig.Posts, UnityWebRequest.kHttpVerbPOST, json, cts);
            
            if (createdPost != null)
                Debug.Log($"<color=blue>POST Success:</color> Created ID: {createdPost.id} | Title: {createdPost.title} | Body: {createdPost.body} | UserId: {createdPost.userId}");

        }

        public async UniTask<T> SendCriticalRequestAsync<T>(string url, string method, string json,
            CancellationToken ct) where T : class
        {
            string idempotencyKey = Guid.NewGuid().ToString();
            int attempt = 0;

            while (attempt < MaxRetries)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    await UniTask.Delay(BaseRetryDelayMs, cancellationToken: ct);
                    attempt++;
                    continue;
                }

                using var request = new UnityWebRequest(url, method);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 15;

                if (!string.IsNullOrEmpty(json))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                request.SetRequestHeader("X-Idempotency-Key", idempotencyKey);

                try
                {
                    await request.SendWebRequest().WithCancellation(ct);

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        return JsonUtility.FromJson<T>(request.downloadHandler.text);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                }

                attempt++;

                if (attempt < MaxRetries)
                {
                    if (!await IsServerReachable(ct))
                    {
                        await UniTask.Delay(BaseRetryDelayMs, cancellationToken: ct);
                    }

                    int delay = BaseRetryDelayMs * (int)Mathf.Pow(2, attempt - 1);
                    await UniTask.Delay(delay, cancellationToken: ct);
                }
            }

            SaveToOfflineQueue(url, json);
            return null;
        }

        private async UniTask<bool> IsServerReachable(CancellationToken ct)
        {
            using var ping = UnityWebRequest.Head(ApiConfig.BaseUrl);
            ping.timeout = 5;
            try
            {
                await ping.SendWebRequest().WithCancellation(ct);
                return ping.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
        }

        private void SaveToOfflineQueue(string url, string json)
        {
            string failedRequest = $"{url}|{json}";
            string currentQueue = PlayerPrefs.GetString(OfflineQueueKey, "");
            PlayerPrefs.SetString(OfflineQueueKey, currentQueue + ";" + failedRequest);
            PlayerPrefs.Save();
            Debug.LogWarning("Request saved to offline queue.");
        }
    }
}