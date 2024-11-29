using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;
using static System.String;

namespace OcentraAI.LLMGames.Manager.Authentication
{
    [RequireComponent(typeof(UnityServicesManager))]
    public class AuthenticationManager : ManagerBase<AuthenticationManager>
    {
        public bool UseAnonymousSignInFromScene = false; // only used in scene for quick test
        private bool IsGuest { get; set; } = false;
        public bool SignInWithUnityComplete { get; set; } = false;
        [ShowInInspector] private AuthPlayerData authPlayerData { get;  set; } = null;
        public bool IsSigningIn { get; private set; }
        public bool IsLoggedIn { get; set; }

        public AuthPlayerData GetAuthPlayerData() => authPlayerData;

        protected override async UniTask InitializeAsync()
        {
            if (Application.isPlaying)
            {
                try
                {
                    await UnityServicesManager.Instance.WaitForInitializationAsync();
                }
                catch (Exception e)
                {
                    LogError($"Error during Unity Services initialization: {e.Message}", this);
                }
            }

            await base.InitializeAsync();
        }

        protected override async void Start()
        {
            if (UseAnonymousSignInFromScene)
            {
                try
                {
                    await SignInAnonymouslyAsync();
                }
                catch (Exception e)
                {
                    LogError($"Anonymous sign-in failed: {e.Message}", this);
                }
            }
        }


        public override async void SubscribeToEvents()
        {
            await UniTask.WaitUntil(() => UnityServices.State == ServicesInitializationState.Initialized);

            base.SubscribeToEvents();

            PlayerAccountService.Instance.SignedIn += OnPlayerAccountServiceSignedIn;
            PlayerAccountService.Instance.SignInFailed += OnPlayerAccountServiceSignInFailed;
            PlayerAccountService.Instance.SignedOut += OnPlayerAccountServiceSignedOut;


            AuthenticationService.Instance.SignedIn += OnAuthenticationServiceSignedIn;
            AuthenticationService.Instance.SignInFailed += OnAuthenticationServiceSignInFailed;
            AuthenticationService.Instance.SignedOut += OnAuthenticationServiceSignedOut;
            AuthenticationService.Instance.Expired += OnAuthenticationServiceExpired;

            EventBus.Instance.Subscribe<SignInWithUserPasswordEvent>(OnSignInWithUserPassword);
            EventBus.Instance.Subscribe<SignInCachedUserEvent>(OnSignInCachedUserEvent);
            EventBus.Instance.Subscribe<SignInWithUnityEvent>(OnSignInWithUnity);
            EventBus.Instance.Subscribe<SignInWithGoogleEvent>(OnSignInWithGoogle);
            EventBus.Instance.Subscribe<SignInWithFacebookEvent>(OnSignInWithFacebook);
            EventBus.Instance.Subscribe<SignInWithSteamEvent>(OnSignInWithSteam);
            EventBus.Instance.Subscribe<SignInAsGuestEvent>(OnSignInAsGuest);

            EventBus.Instance.Subscribe<CreateAccountEvent>(OnCreateAccount);
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();

            PlayerAccountService.Instance.SignedIn -= OnPlayerAccountServiceSignedIn;
            PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountServiceSignInFailed;
            PlayerAccountService.Instance.SignedOut -= OnPlayerAccountServiceSignedOut;

            AuthenticationService.Instance.SignedIn -= OnAuthenticationServiceSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnAuthenticationServiceSignInFailed;
            AuthenticationService.Instance.SignedOut -= OnAuthenticationServiceSignedOut;
            AuthenticationService.Instance.Expired -= OnAuthenticationServiceExpired;

            EventBus.Instance.Unsubscribe<SignInWithUserPasswordEvent>(OnSignInWithUserPassword);
            EventBus.Instance.Unsubscribe<SignInWithUnityEvent>(OnSignInWithUnity);
            EventBus.Instance.Unsubscribe<SignInWithGoogleEvent>(OnSignInWithGoogle);
            EventBus.Instance.Unsubscribe<SignInWithFacebookEvent>(OnSignInWithFacebook);
            EventBus.Instance.Unsubscribe<SignInWithSteamEvent>(OnSignInWithSteam);
            EventBus.Instance.Unsubscribe<SignInAsGuestEvent>(OnSignInAsGuest);
            EventBus.Instance.Unsubscribe<SignInCachedUserEvent>(OnSignInCachedUserEvent);

            EventBus.Instance.Unsubscribe<CreateAccountEvent>(OnCreateAccount);
        }

        private void OnPlayerAccountServiceSignedIn()
        {
            SignInWithUnityComplete = true;
        }

        private void OnPlayerAccountServiceSignInFailed(RequestFailedException obj)
        {
            SignInWithUnityComplete = false;
        }

        private void OnPlayerAccountServiceSignedOut()
        {
            SignInWithUnityComplete = false;
        }


        private async void OnAuthenticationServiceSignedIn()
        {
  
            Log($"PlayerID: {AuthenticationService.Instance.PlayerId}", null);
            Log($"Access Token: {AuthenticationService.Instance.AccessToken}", null);



            (bool success, AuthPlayerData playerData) = await GetOrCreatePlayerDataAndUpdate(IsGuest, AuthenticationService.Instance.PlayerId);
           
            if (success)
            {
                authPlayerData = playerData;

                if (!IsGuest && !UseAnonymousSignInFromScene)
                {
                    await UnityServicesManager.Instance.SavePlayerDataToCloud(playerData);
                }


                if (!IsNullOrEmpty(playerData.PlayerName))
                {
                    IsLoggedIn = true;

                    if (!UseAnonymousSignInFromScene)
                    {
                        await PublishAuthResult(AuthResult.Authenticated(playerData));
                    }
                    else
                    {
                       await EventBus.Instance.PublishAsync(new StartMainGameEvent(playerData));
                    }
                  
                   
                }
                else
                {

                    if (!UseAnonymousSignInFromScene)
                    {
                        await PublishAuthResult(AuthResult.Failure("PlayerName is Null or empty"));
                    }
                  
                }

               
            }
            else
            {
                if (!UseAnonymousSignInFromScene)
                {
                    await PublishAuthResult(AuthResult.Failure("AuthPlayerData could not be retrieved @ OnAuthenticationServiceSignedIn Method "));
                }

               
            }


        }

        private async UniTask<(bool success, AuthPlayerData playerData)> GetOrCreatePlayerDataAndUpdate(bool isGuest, string playerId)
        {
            try
            {


                AuthPlayerData data;
                if (!isGuest)
                {
                    (bool success, AuthPlayerData playerData) = await UnityServicesManager.Instance.TryGetPlayerDataFromCloud(playerId);

                    string userName = playerData?.PlayerName ?? Empty;
                    string email = playerData?.Email ?? Empty;

                    if (playerData == null || IsNullOrEmpty(email) || userName.Contains("guest", StringComparison.OrdinalIgnoreCase))
                    {
                        RequestAdditionalUserInfoEvent additionalUserInfoEvent = new RequestAdditionalUserInfoEvent(false);
                        await EventBus.Instance.PublishAsync(additionalUserInfoEvent);
                        (userName, email) = await additionalUserInfoEvent.CompletionSource.Task;
                    }


                    if (success && playerData != null)
                    {
                        data = playerData;
                        data.PlayerName = userName;
                        data.Email = email;
                    }
                    else
                    {
                        data = new AuthPlayerData { PlayerID = playerId, PlayerName = userName, Email = email };
                    }

                   

                    return (true, data);
                }
                else
                {
                    if (UseAnonymousSignInFromScene)
                    {
                        data = new AuthPlayerData { PlayerID = playerId, PlayerName = "userName" };
                        return (true, data);
                    }
                    else
                    {
                        RequestAdditionalUserInfoEvent additionalUserInfoEvent = new RequestAdditionalUserInfoEvent(true);
                        await EventBus.Instance.PublishAsync(additionalUserInfoEvent);
                        (string userName, string _) = await additionalUserInfoEvent.CompletionSource.Task;
                        data = new AuthPlayerData { PlayerID = playerId, PlayerName = userName };

                        return (true, data);
                    }

                }

               
            }
            catch (Exception e)
            {
                LogError($"GetOrCreatePlayerDataAndUpdate: {e}", this);
                await PublishAuthResult(AuthResult.Failure($"Failed to get or create player data: {e.Message}"));
                return (false, null);
            }
        }

        private void OnAuthenticationServiceSignInFailed(RequestFailedException err)
        {
            IsLoggedIn = false;
            authPlayerData = null;
            PublishAuthResult(AuthResult.Failure($"Sign-in failed: {err.ErrorCode} \n {err.Message}")).Forget();
        }

        private void OnAuthenticationServiceSignedOut()
        {
            IsLoggedIn = false;
            authPlayerData = null;
            PublishAuthResult(AuthResult.Success("Player signed out.")).Forget();
        }

        private void OnAuthenticationServiceExpired()
        {
            IsLoggedIn = false;
            authPlayerData = null;
            PublishAuthResult(AuthResult.Failure("Player session could not be refreshed and expired.")).Forget();
        }

        private async void OnSignInWithUserPassword(SignInWithUserPasswordEvent e)
        {
            AuthResult result = await PerformAuthenticationAsync(AuthenticationService.Instance.SignInWithUsernamePasswordAsync, e.Username, e.Password);
            e.CompletionSource.TrySetResult(result);
        }

        private async void OnCreateAccount(CreateAccountEvent e)
        {
            AuthResult result = await PerformAuthenticationAsync(AuthenticationService.Instance.SignUpWithUsernamePasswordAsync, e.Username, e.Password);
            e.CompletionSource.TrySetResult(result);
        }


        private async void OnSignInWithUnity(SignInWithUnityEvent e)
        {
            AuthResult result = await SignInWithUnityAsync();
            e.CompletionSource.TrySetResult(result);
        }

        private void OnSignInWithGoogle(SignInWithGoogleEvent e)
        {
            AuthResult authResult = AuthResult.Failure("SignInWithGoogle Not Implemented Yet");
            e.CompletionSource.TrySetResult(authResult);
        }

        private void OnSignInWithFacebook(SignInWithFacebookEvent e)
        {
            AuthResult authResult = AuthResult.Failure("SignInWithFacebook Not Implemented Yet");
            e.CompletionSource.TrySetResult(authResult);
        }

        private void OnSignInWithSteam(SignInWithSteamEvent e)
        {
            AuthResult authResult = AuthResult.Failure("SignInWithSteam Not Implemented Yet");
            e.CompletionSource.TrySetResult(authResult);
        }

        private async void OnSignInAsGuest(SignInAsGuestEvent e)
        {
            await SignInAnonymouslyAsync();
            AuthResult authResult = AuthResult.Success("guest Sign In");
            e.CompletionSource.TrySetResult(authResult);
        }
        

        public async UniTask<AuthResult> PerformAuthenticationAsync(Func<string, string, Task> authOperation, string username,
            string password, int maxRetries = 2)
        {
            if (IsSigningIn)
            {
                return AuthResult.Pending();
            }
            IsSigningIn = true;
            int retryCount = 0;

            while (retryCount <= maxRetries)
            {
                try
                {
                    await PublishAuthResult(AuthResult.Pending($"Attempting authentication (Attempt {retryCount + 1}/{maxRetries + 1})"));
                    await authOperation(username, password).AsUniTask();
                    return AuthResult.Success("Authentication successful");
                }
                catch (AuthenticationException ex)
                {
                    if (retryCount == maxRetries)
                    {
                        IsSigningIn = false;
                        return HandleAuthenticationException(ex);
                    }
                }
                catch (RequestFailedException ex) when (IsNetworkError(ex))
                {
                    if (CheckInternetConnection() && retryCount < maxRetries)
                    {
                        retryCount++;
                        continue;
                    }
                    IsSigningIn = false;
                    return AuthResult.Failure($"Network error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    IsSigningIn = false;
                    return AuthResult.Failure($"Error: {ex.Message}");
                }
                retryCount++;
            }
            IsSigningIn = false;
            return AuthResult.Failure("Max retries reached");
        }

        private async UniTask PublishAuthResult(AuthResult result)
        {
            await EventBus.Instance.PublishAsync(new AuthenticationStatusEvent(result));

        }
        

        public async UniTask<AuthResult> StartSignInUsingUnityAsync()
        {
            try
            {
                SignInWithUnityComplete = false;
                await PlayerAccountService.Instance.StartSignInAsync().AsUniTask();
                await UniTask.WaitUntil(() => SignInWithUnityComplete && PlayerAccountService.Instance.AccessToken != null);
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken).AsUniTask();
                return AuthResult.Success();
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                return AuthResult.Failure(ex.Message);
            }
        }




        private async UniTask<AuthResult> SignInWithUnityAsync()
        {
            if (IsSigningIn)
            {
                return AuthResult.Pending("Sign-in already in progress.");
            }

            IsSigningIn = true;
            EventBus.Instance.Publish(new UpdateUIInteractabilityEvent(false));

            try
            {

                AuthResult authResult = await StartSignInUsingUnityAsync();
                return AuthResult.Success("SignIn with Unity is successful.");
            }
            catch (AuthenticationException ex)
            {
                string message = $"SignInWithUnityAsync: AuthenticationException: {ex.ErrorCode} \n {ex.Message}";
                return AuthResult.Failure(message);
            }
            catch (RequestFailedException ex)
            {
                string message = $"SignInWithUnityAsync: RequestFailedException: {ex.ErrorCode} \n {ex.Message}";
                return AuthResult.Failure(message);
            }
            finally
            {
                IsSigningIn = false;
                EventBus.Instance.Publish(new UpdateUIInteractabilityEvent(true));
            }
        }


        private async UniTask<bool> LinkWithUnityAsync()
        {
            try
            {
                await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken).AsUniTask();
                await PublishAuthResult(AuthResult.Failure($"LinkWithUnity Success"));
                return true;
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                string message = $"LinkWithUnityAsync: AccountAlreadyLinked :  \n   {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
                return false;
            }
            catch (AuthenticationException ex)
            {
                string message = $"LinkWithUnityAsync: AuthenticationException :  \n   {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
                return false;
            }
            catch (RequestFailedException ex)
            {
                string message = $"LinkWithUnityAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
                return false;
            }
        }


        private async UniTask SignInAnonymouslyAsync()
        {
            try
            {
                IsGuest = true;
                AuthenticationService.Instance.ClearSessionToken();

                SignInOptions signInOptions = new SignInOptions { CreateAccount = true };

                await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOptions).AsUniTask();
                Log("Sign in anonymously succeeded!", null);
                Log($"PlayerID: {AuthenticationService.Instance.PlayerId}", null);
            }
            catch (AuthenticationException ex)
            {
                string message = $"SignInAnonymouslyAsync: AuthenticationException :  \n    {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
            }
            catch (RequestFailedException ex)
            {
                string message = $"SignInAnonymouslyAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
            }
        }


        private async UniTask AddUsernamePasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password).AsUniTask();
                string message = "Username and password added.";
                await PublishAuthResult(AuthResult.Success(message));
            }
            catch (AuthenticationException ex)
            {

                string message = $"AddUsernamePasswordAsync: AuthenticationException :  \n {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
            }
            catch (RequestFailedException ex)
            {

                string message = $"AddUsernamePasswordAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
            }
        }




        private async void OnSignInCachedUserEvent(SignInCachedUserEvent e)
        {
            try
            {
                string accessToken = AuthenticationService.Instance.AccessToken;
                if (accessToken != null)
                {
                    if (!await LinkWithUnityAsync())
                    {
                        RequestUserCredentialsEvent userCredentialsEvent = new RequestUserCredentialsEvent();
                        await EventBus.Instance.PublishAsync(userCredentialsEvent);
                        UserCredentials credentials = await userCredentialsEvent.CompletionSource.Task;

                        if (credentials.IsValid)
                        {
                            await AddUsernamePasswordAsync(credentials.Username, credentials.Password);
                            e.CompletionSource.TrySetResult(AuthResult.Success("Cached user signed in with username and password."));
                        }
                        else
                        {
                            SignInOptions signInOptions = new SignInOptions { CreateAccount = true };
                            await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOptions).AsUniTask();
                            e.CompletionSource.TrySetResult(AuthResult.Success("Signed in anonymously."));
                        }
                    }
                    else
                    {
                        e.CompletionSource.TrySetResult(AuthResult.Success("Linked with Unity successfully."));
                    }
                }
                else
                {
                    e.CompletionSource.TrySetResult(AuthResult.Failure("No access token available."));
                }
            }
            catch (AuthenticationException ex)
            {
                string message = $"SignInCachedUserAsync: AuthenticationException: {ex.ErrorCode} \n {ex.Message}";
                e.CompletionSource.TrySetResult(AuthResult.Failure(message));
            }
            catch (RequestFailedException ex)
            {
                string message = $"SignInCachedUserAsync: RequestFailedException: {ex.ErrorCode} \n {ex.Message}";
                e.CompletionSource.TrySetResult(AuthResult.Failure(message));
            }
        }

        private bool IsNetworkError(RequestFailedException ex)
        {
            HashSet<int> networkErrorCodes = new HashSet<int>
            {
                401, // Unauthorized
                403, // Forbidden
                404, // Not Found
                408, // Request Timeout
                500, // Internal Server Error
                503, // Service Unavailable
                504 // Gateway Timeout
            };

            return networkErrorCodes.Contains(ex.ErrorCode);
        }

        private bool CheckInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private AuthResult HandleAuthenticationException(AuthenticationException ex)
        {
            if (ex.ErrorCode == AuthenticationErrorCodes.ClientInvalidProfile)
            {
                return AuthResult.Failure($"Account does not exist. Please create a new user. AuthenticationException: {ex}");
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.InvalidParameters)
            {
                return AuthResult.Failure($"Invalid username or password. AuthenticationException : {ex} ");
            }
            else
            {
                return AuthResult.Failure($"Authentication error: {ex.ErrorCode} \n {ex.Message}");
            }
        }

        private async UniTask UpdatePlayerNameAsync(string username)
        {
            try
            {
                string playerName = await AuthenticationService.Instance.UpdatePlayerNameAsync(username).AsUniTask();
                string message = $"Username Updated to :{playerName}";
                await PublishAuthResult(AuthResult.Success(message));
            }
            catch (AuthenticationException ex)
            {
                string message = $"UpdatePlayerNameAsync: AuthenticationException :  \n {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
            }
            catch (RequestFailedException ex)
            {
                string message = $"UpdatePlayerNameAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}";
                await PublishAuthResult(AuthResult.Failure(message));
            }
        }

        protected override async UniTask<bool> ApplicationWantsToQuit()
        {
            if (!IsLoggedIn || authPlayerData == null) {
               
                return true;
            }

            await UnityServicesManager.Instance.SignOut(authPlayerData);
            authPlayerData = null;
            return false;
        }


       
    }
}