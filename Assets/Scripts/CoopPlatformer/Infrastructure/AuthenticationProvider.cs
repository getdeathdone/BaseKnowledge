using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace CoopPlatformer.Infrastructure
{
    /// <summary>
    /// Handles Unity Services initialization and anonymous authentication.
    /// Senior Tip: Use async/await (UniTask) for non-blocking initialization.
    /// </summary>
    public static class AuthenticationProvider
    {
        public static bool IsAuthenticated => AuthenticationService.Instance.IsSignedIn;

        public static async UniTask<bool> InitializeAndSignInAsync()
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    await UnityServices.InitializeAsync();
                }

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    return true;
                }

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[Auth] Signed in as: {AuthenticationService.Instance.PlayerId}");
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Auth] Initialization failed: {e.Message}");
                if (e.InnerException != null)
                {
                    Debug.LogError($"[Auth] Inner Exception: {e.InnerException.Message}");
                }
                Debug.LogException(e); // Logs the full stack trace and details
                return false;
            }
        }
    }
}
