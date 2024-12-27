using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
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
    public class AuthenticationManager : MonoBehaviourBase<AuthenticationManager>
    {


        public bool UseAnonymousSignInFromScene = false;
        private bool IsGuest { get; set; } = false;
        public bool SignInWithUnityComplete { get; set; } = false;
        [ShowInInspector] private IAuthPlayerData AuthPlayerData { get; set; } = null;
        public bool IsSigningIn { get; private set; }
        public bool IsLoggedIn { get; set; }

        IUnityServicesManager UnityServicesManager { get; set; }

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
                    GameLoggerScriptable.Instance.LogError($"Anonymous sign-in failed: {e.Message}", this);
                }
            }
        }

        public override async UniTask InitializeAsync()
        {
            if (Application.isPlaying)
            {
                try
                {
                    UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>> completionSource = new UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>>();
                    await EventBus.Instance.PublishAsync(new WaitForInitializationEvent(completionSource, GetType(), typeof(IUnityServicesManager), 10));
                    IOperationResult<IMonoBehaviourBase> operationResult = await completionSource.Task;
                    if (operationResult.IsSuccess && operationResult.Value is IUnityServicesManager manager)
                    {
                        UnityServicesManager = manager;
                    }
                    else
                    {
                        Debug.LogError("Failed to initialize UnityServicesManager. AuthenticationManager cannot proceed.");
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize AuthenticationManager: {ex.Message}");
                    return;
                }
            }
            await base.InitializeAsync();
        }





        public override async void SubscribeToEvents()
        {
            await UniTask.WaitUntil(() => UnityServices.State == ServicesInitializationState.Initialized);


            PlayerAccountService.Instance.SignedIn += OnPlayerAccountServiceSignedIn;
            PlayerAccountService.Instance.SignInFailed += OnPlayerAccountServiceSignInFailed;
            PlayerAccountService.Instance.SignedOut += OnPlayerAccountServiceSignedOut;


            AuthenticationService.Instance.SignedIn += OnAuthenticationServiceSignedIn;
            AuthenticationService.Instance.SignInFailed += OnAuthenticationServiceSignInFailed;
            AuthenticationService.Instance.SignedOut += OnAuthenticationServiceSignedOut;
            AuthenticationService.Instance.Expired += OnAuthenticationServiceExpired;

            EventRegistrar.Subscribe<SignInWithUserPasswordEvent>(OnSignInWithUserPassword);
            EventRegistrar.Subscribe<SignInCachedUserEvent>(OnSignInCachedUserEvent);
            EventRegistrar.Subscribe<SignInWithUnityEvent>(OnSignInWithUnity);
            EventRegistrar.Subscribe<SignInWithGoogleEvent>(OnSignInWithGoogle);
            EventRegistrar.Subscribe<SignInWithFacebookEvent>(OnSignInWithFacebook);
            EventRegistrar.Subscribe<SignInWithSteamEvent>(OnSignInWithSteam);
            EventRegistrar.Subscribe<SignInAsGuestEvent>(OnSignInAsGuest);
            EventRegistrar.Subscribe<CreateAccountEvent>(OnCreateAccount);
        }

        public override void UnsubscribeFromEvents()
        {

            PlayerAccountService.Instance.SignedIn -= OnPlayerAccountServiceSignedIn;
            PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountServiceSignInFailed;
            PlayerAccountService.Instance.SignedOut -= OnPlayerAccountServiceSignedOut;

            AuthenticationService.Instance.SignedIn -= OnAuthenticationServiceSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnAuthenticationServiceSignInFailed;
            AuthenticationService.Instance.SignedOut -= OnAuthenticationServiceSignedOut;
            AuthenticationService.Instance.Expired -= OnAuthenticationServiceExpired;

            EventRegistrar.UnsubscribeAll();
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

            Log($"PlayerID: {AuthenticationService.Instance.PlayerId}", this);
            Log($"Access Token: {AuthenticationService.Instance.AccessToken}", this);



            (bool success, IAuthPlayerData playerData) = await GetOrCreatePlayerDataAndUpdate(IsGuest, AuthenticationService.Instance.PlayerId);

            if (success)
            {
                AuthPlayerData = playerData as AuthPlayerData;

                if (!IsGuest && !UseAnonymousSignInFromScene)
                {
                    await EventBus.Instance.PublishAsync(new SavePlayerDataToCloudEvent(playerData));
                }


                if (!IsNullOrEmpty(playerData.PlayerName))
                {
                    IsLoggedIn = true;

                    if (!UseAnonymousSignInFromScene)
                    {
                        await PublishAuthResult(AuthResult.Authenticated(AuthPlayerData));
                    }
                    else
                    {
                        await EventBus.Instance.PublishAsync(new StartMainGameEvent(AuthPlayerData));
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

        private async UniTask<(bool success, IAuthPlayerData playerData)> GetOrCreatePlayerDataAndUpdate(bool isGuest, string playerId)
        {
            try
            {


                IAuthPlayerData data;
                if (!isGuest)
                {
                    UniTaskCompletionSource<(bool success, IAuthPlayerData playerData)> uniTaskCompletionSource = new UniTaskCompletionSource<(bool success, IAuthPlayerData playerData)>();
                    await EventBus.Instance.PublishAsync(new RequestPlayerDataFromCloudEvent(uniTaskCompletionSource, playerId));

                    (bool success, IAuthPlayerData playerData) = await uniTaskCompletionSource.Task;

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
                        data.Update(userName, email);
                    }
                    else
                    {
                        data = new AuthPlayerData(playerId, userName, email);
                    }



                    return (true, data);
                }
                else
                {
                    if (UseAnonymousSignInFromScene)
                    {
                        data = new AuthPlayerData(playerId, "userName");
                        return (true, data);
                    }
                    else
                    {
                        RequestAdditionalUserInfoEvent additionalUserInfoEvent = new RequestAdditionalUserInfoEvent(true);
                        await EventBus.Instance.PublishAsync(additionalUserInfoEvent);
                        (string userName, string _) = await additionalUserInfoEvent.CompletionSource.Task;
                        data = new AuthPlayerData(playerId, userName);

                        return (true, data);
                    }

                }


            }
            catch (Exception e)
            {
                GameLoggerScriptable.Instance.LogError($"GetOrCreatePlayerDataAndUpdate: {e}", this);
                await PublishAuthResult(AuthResult.Failure($"Failed to get or create player data: {e.Message}"));
                return (false, null);
            }
        }

        private void OnAuthenticationServiceSignInFailed(RequestFailedException err)
        {
            IsLoggedIn = false;
            AuthPlayerData = null;
            PublishAuthResult(AuthResult.Failure($"Sign-in failed: {err.ErrorCode} \n {err.Message}")).Forget();
        }

        private void OnAuthenticationServiceSignedOut()
        {
            IsLoggedIn = false;
            AuthPlayerData = null;
            PublishAuthResult(AuthResult.Success("Player signed out.")).Forget();
        }

        private void OnAuthenticationServiceExpired()
        {
            IsLoggedIn = false;
            AuthPlayerData = null;
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
                GameLoggerScriptable.Instance.Log("Sign in anonymously succeeded!", this);
                GameLoggerScriptable.Instance.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}", this);
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

        public override async UniTask<bool> ApplicationWantsToQuit()
        {
            if (!IsLoggedIn || AuthPlayerData == null)
            {

                return true;
            }
            await EventBus.Instance.PublishAsync(new AuthenticationSignOutEvent(AuthPlayerData));
            AuthPlayerData = null;
            return false;
        }



    }
}