using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkService : MonoBehaviour
{
    private async void Start()
    {
        var cts = this.GetCancellationTokenOnDestroy();

        // GET Example
        var todo = await SendRequestAsync<TodoResponse>(
            ApiConfig.Todos, 
            UnityWebRequest.kHttpVerbGET, 
            null, 
            cts
        );
        
        if (todo != null) 
            Debug.Log($"<color=green>GET Success:</color> Task: {todo.title} | ID: {todo.id} | Completed: {todo.completed}");

        // POST Example
        var newPost = new PostData { title = "Hello", body = "World", userId = 1 };
        string jsonPayload = JsonUtility.ToJson(newPost);

        var createdPost = await SendRequestAsync<PostData>(
            ApiConfig.Posts, 
            UnityWebRequest.kHttpVerbPOST, 
            jsonPayload, 
            cts
        );
        
        if (createdPost != null) 
            Debug.Log($"<color=blue>POST Success:</color> Created ID: {createdPost.id} | Title: {createdPost.title} | Body: {createdPost.body} | UserId: {createdPost.userId}");
    }

    public async UniTask<T> SendRequestAsync<T>(string url, string method, string jsonBody = null, CancellationToken ct = default) 
        where T : class
    {
        using UnityWebRequest request = new UnityWebRequest(url, method);
        request.downloadHandler = new DownloadHandlerBuffer();

        if (!string.IsNullOrEmpty(jsonBody))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
        }

        request.timeout = 15;

        try 
        {
            await request.SendWebRequest().WithCancellation(ct);

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<T>(request.downloadHandler.text);
            }
            
            Debug.LogError($"Request Failed: {request.responseCode} - {request.error}");
        }
        catch (OperationCanceledException) { }
        catch (Exception e) 
        { 
            Debug.LogError($"Critical Network Error: {e.Message}"); 
        }

        return null;
    }
}

public static class ApiConfig
{
    public const string BaseUrl = "https://jsonplaceholder.typicode.com";
    public const string Posts = BaseUrl + "/posts";
    public const string Todos = BaseUrl + "/todos/1";
}

[Serializable]
public class TodoResponse
{
    public int id;
    public string title;
    public bool completed;
}

[Serializable]
public class PostData
{
    public int id;
    public string title;
    public string body;
    public int userId;
}