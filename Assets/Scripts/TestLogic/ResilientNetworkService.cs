using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TestLogic
{
    public class ResilientNetworkService : MonoBehaviour
    {
        private const int MaxRetries = 3;
        private const int BaseRetryDelayMs = 2000;

        private async void Start()
        {
            var cts = this.GetCancellationTokenOnDestroy();
            
            Debug.Log("Starting GET request...");
            var todo = await SendRequestWithRetryAsync<TodoResponse>(
                ApiConfig.Todos,
                UnityWebRequest.kHttpVerbGET,
                null,
                cts
            );

            if (todo != null)
                Debug.Log($"<color=green>GET Success:</color> Task: {todo.title} | ID: {todo.id} | Completed: {todo.completed}");
            
            Debug.Log("Starting POST request...");
            var newPost = new PostData 
            { 
                title = "Test POST request", 
                body = "Resilient Networking", 
                userId = 1 
            };
    
            string jsonPayload = JsonUtility.ToJson(newPost);

            var createdPost = await SendRequestWithRetryAsync<PostData>(
                ApiConfig.Posts,
                UnityWebRequest.kHttpVerbPOST,
                jsonPayload,
                cts
            );

            if (createdPost != null)
                Debug.Log($"<color=blue>POST Success:</color> Created ID: {createdPost.id} | Title: {createdPost.title} | Body: {createdPost.body} | UserId: {createdPost.userId}");
        }
        
        public async UniTask<T> SendRequestWithRetryAsync<T>(string url, string method, string jsonBody = null,
            CancellationToken ct = default) where T : class
        {
            var attempt = 0;

            while (attempt < MaxRetries)
            {
                var result = await SendRequestAsync<T>(url, method, jsonBody, ct);

                if (result != null) return result;

                attempt++;

                if (attempt < MaxRetries)
                {
                    if (!await IsServerReachable(ct))
                        Debug.LogWarning("Network unreachable. Waiting for connection...");
                    
                    var delay = BaseRetryDelayMs * (int)Mathf.Pow(2, attempt - 1); // 2s, 4s, 8s
                    Debug.LogWarning($"Attempt {attempt} failed. Retrying in {delay}ms...");
                    await UniTask.Delay(delay, cancellationToken: ct);
                }
            }

            Debug.LogError("All retry attempts failed.");
            return null;
        }

        
        public async UniTask<T> SendRequestAsync<T>(string url, string method, string jsonBody = null,
            CancellationToken ct = default) where T : class
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("No internet connection detected on device.");
                return null;
            }

            using var request = new UnityWebRequest(url, method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 15;

            if (!string.IsNullOrEmpty(jsonBody))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            try
            {
                await request.SendWebRequest().WithCancellation(ct);

                if (request.result == UnityWebRequest.Result.Success)
                    return JsonUtility.FromJson<T>(request.downloadHandler.text);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError($"Network Error: {e.Message}");
            }

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
    }
}