using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OcentraAI.LLMGames.Screens;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

namespace OcentraAI.LLMGames.Authentication
{
    [RequireComponent(typeof(UnityServicesManager))]
    public class AuthenticationManager : MonoBehaviour
    {
        public static AuthenticationManager Instance { get; private set; }


        [ShowInInspector]
        public PlayerData PlayerData { get; private set; } = null;
        public bool IsSigningIn { get; private set; }
        public bool IsLoggedIn { get; set; }

        public string PlayerName { get; private set; }

        public event Action AuthenticationCompleted;

        public bool UseAnonymousSignIn  = false;

        async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            await UnityServices.InitializeAsync();
        }


        async void Start()
        {
            try
            {

                SetAuthenticationEvents();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (LoginScreen.Instance !=null)
            {
                AuthenticationCompleted += LoginScreen.Instance.OnAuthenticationCompleted;

            }
            else
            {
                AuthenticationCompleted += OnAuthenticationCompleted;
            }
            
            PlayerAccountService.Instance.SignedIn += SignInWithUnityAsync;

            if (UseAnonymousSignIn)
            {
               await SignInAnonymouslyAsync();
            }
        }

        public void OnAuthenticationCompleted()
        {
            
           IsLoggedIn = true;
        }

        private void SetAuthenticationEvents()
        {
            AuthenticationService.Instance.SignedIn += async () =>
            {
                //Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                //Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

                (bool success, PlayerData playerData) = await GetOrCreatePlayerDataAndUpdate();

                if (success)
                {
                    PlayerData = playerData;

                    await UnityServicesManager.Instance.SavePlayerDataToCloud(PlayerData);

                    if (!string.IsNullOrEmpty(PlayerData.PlayerName))
                    {
                        await UpdatePlayerNameAsync(PlayerData.PlayerName);
                    }
                    else
                    {
                        if (LoginScreen.Instance != null)
                        {
                            LoginScreen.Instance.ShowErrorMessage("PlayerName is Null or empty");
                        }
                        else
                        {
                            Debug.LogError("PlayerName is Null or empty");
                        }
                    }
                }
                else
                {
                    if (LoginScreen.Instance != null)
                    {
                        LoginScreen.Instance.ShowErrorMessage("PlayerData could not be retrieved");
                    }
                    else
                    {
                        Debug.LogError("PlayerData could not be retrieved");
                    }
                }

                // Invoke the event for main thread to respond
                AuthenticationCompleted?.Invoke();
            };

            AuthenticationService.Instance.SignInFailed += (err) =>
            {
                IsLoggedIn = false;
                PlayerData = null;
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage($"{err.ErrorCode} \n {err.Message}");
                }
                else
                {
                    Debug.LogError($"{err.ErrorCode} \n {err.Message}");
                }
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                IsLoggedIn = false;
                PlayerData = null;
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage("Player signed out.");
                }
                else
                {
                    Debug.LogError("Player signed out.");
                }
            };

            AuthenticationService.Instance.Expired += () =>
            {
                IsLoggedIn = false;
                PlayerData = null;
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage("Player session could not be refreshed and expired.");
                }
                else
                {
                    Debug.LogError("Player session could not be refreshed and expired.");
                }
            };
        }

        public async Task PerformAuthenticationAsync(Func<string, string, Task> authOperation, string username, string password, Action<string> onError, int maxRetries = 2)
        {
            if (IsSigningIn) return;

            IsSigningIn = true;

            int retryCount = 0;
            while (retryCount <= maxRetries)
            {
                try
                {
                    await authOperation(username, password);
                    break;
                }
                catch (AuthenticationException ex)
                {
                    HandleAuthenticationException(ex, onError);
                    break;
                }
                catch (RequestFailedException ex) when (IsNetworkError(ex))
                {
                    if (CheckInternetConnection())
                    {
                        retryCount++;
                        continue;
                    }
                    onError($"Network error: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    onError($"Error: {ex.Message}");
                    break;
                }
            }

            IsSigningIn = false;
        }

        public async Task StartSignInAsync(Action<string> onError)
        {
            try
            {
                await PlayerAccountService.Instance.StartSignInAsync();
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                onError(ex.Message);
            }
        }

        public async Task StartSignInAsync()
        {
            if (PlayerAccountService.Instance.IsSignedIn)
            {
                SignInWithUnityAsync();
                return;
            }

            try
            {
                await PlayerAccountService.Instance.StartSignInAsync();
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);

            }
        }

        private async void SignInWithUnityAsync()
        {
            if (IsSigningIn) return;

            IsSigningIn = true;
            if (LoginScreen.Instance != null)
            {
                LoginScreen.Instance.UpdateButtonInteractability(false);
            }

            try
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("SignIn is successful.");
            }
            catch (AuthenticationException ex)
            {
                var message = $"SignInWithUnityAsync: AuthenticationException :  \n   {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
            catch (RequestFailedException ex)
            {
                var message = $"SignInWithUnityAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
            finally
            {
                IsSigningIn = false;
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.UpdateButtonInteractability(true);
                }
            }
        }


        private async Task<bool> LinkWithUnityAsync()
        {
            try
            {
                await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Link is successful.");
                return true;
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                var message = $"LinkWithUnityAsync: AccountAlreadyLinked :  \n   {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogError("This user is already linked with another account. Log in instead.");
                }
                return false;
            }
            catch (AuthenticationException ex)
            {
                var message = $"LinkWithUnityAsync: AuthenticationException :  \n   {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
                return false;
            }
            catch (RequestFailedException ex)
            {
                var message = $"LinkWithUnityAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
                return false;
            }
        }


        private async Task SignInAnonymouslyAsync()
        {
            try
            {
                SignInOptions signInOptions = new SignInOptions
                {
                    CreateAccount = true
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOptions);
                //Debug.Log("Sign in anonymously succeeded!");
                //Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (AuthenticationException ex)
            {
                var message = $"SignInAnonymouslyAsync: AuthenticationException :  \n    {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
            catch (RequestFailedException ex)
            {
                var message = $"SignInAnonymouslyAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
        }


        private async Task AddUsernamePasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
                // Debug.Log("Username and password added.");
            }
            catch (AuthenticationException ex)
            {
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage($"AddUsernamePasswordAsync: AuthenticationException :  \n {ex.ErrorCode} \n {ex.Message}");

                }
                else
                {
                    Debug.LogException(ex);

                }
            }
            catch (RequestFailedException ex)
            {
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage($"AddUsernamePasswordAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}");

                }
                else
                {
                    Debug.LogException(ex);

                }

            }
        }

        private async Task<(bool success, PlayerData playerData)> GetOrCreatePlayerDataAndUpdate()
        {
            try
            {
                PlayerData data = default;

                (bool success, PlayerData playerData) = await UnityServicesManager.Instance.TryGetPlayerDataFromCloud(AuthenticationService.Instance.PlayerId);

                if (success)
                {
                    data = playerData;
                    data.PlayerID = AuthenticationService.Instance.PlayerId;
                    if (string.IsNullOrEmpty(data.PlayerName))
                    {
                        var (userName, email) = await LoginScreen.Instance.ShowUserNameAndEmailUI();
                        data.PlayerName = userName;
                    }
                }
                else
                {
                    string userName = "Guest";

                    data = new PlayerData
                    {
                        PlayerID = AuthenticationService.Instance.PlayerId,
                        PlayerName = userName,
                    };
                }

                PlayerName = data.PlayerName;

                return (true, data);
            }
            catch (Exception e)
            {
                Debug.LogError($" GetOrCreatePlayerDataAndUpdate {e}");
            }

            return (false, null);
        }

        private async Task SignInCachedUserAsync()
        {
            if (!AuthenticationService.Instance.SessionTokenExists)
            {
                return;
            }

            try
            {
                string accessToken = AuthenticationService.Instance.AccessToken;
                if (accessToken != null)
                {
                    if (!await LinkWithUnityAsync())
                    {
                        if (LoginScreen.Instance != null && LoginScreen.Instance.ValidateUserAndPassword())
                        {
                            await AddUsernamePasswordAsync(LoginScreen.Instance.UserNameInputField.text, LoginScreen.Instance.PasswordInputField.text);
                        }
                        else
                        {
                            SignInOptions signInOptions = new SignInOptions
                            {
                                CreateAccount = true
                            };

                            await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOptions);
                            //Debug.Log("Sign in anonymously succeeded!");
                            //Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                        }
                    }
                }
            }
            catch (AuthenticationException ex)
            {
                var message = $"SignInCachedUserAsync: AuthenticationException :  \n    {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
            catch (RequestFailedException ex)
            {
                var message = $"SignInCachedUserAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
        }

        private bool IsNetworkError(RequestFailedException ex)
        {
            var networkErrorCodes = new HashSet<int>
            {
                401, // Unauthorized
                403, // Forbidden
                404, // Not Found
                408, // Request Timeout
                500, // Internal Server Error
                503, // Service Unavailable
                504  // Gateway Timeout
            };

            return networkErrorCodes.Contains(ex.ErrorCode);
        }

        private bool CheckInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void HandleAuthenticationException(AuthenticationException ex, Action<string> onError)
        {
            if (ex.ErrorCode == AuthenticationErrorCodes.ClientInvalidProfile)
            {
                onError("Account does not exist. Please create a new user.");
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.InvalidParameters)
            {
                onError("Invalid username or password.");
            }
            else
            {
                onError($"Authentication error: {ex.ErrorCode} \n {ex.Message}");
            }
        }

        private async Task UpdatePlayerNameAsync(string username)
        {
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(username);
                //Debug.Log("Username added.");
            }
            catch (AuthenticationException ex)
            {
                var message = $"UpdatePlayerNameAsync: AuthenticationException :  \n {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
            catch (RequestFailedException ex)
            {
                var message = $"UpdatePlayerNameAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}";
                if (LoginScreen.Instance != null)
                {
                    LoginScreen.Instance.ShowErrorMessage(message);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
        }


        private bool OnApplicationWantsToQuit()
        {
            if (!IsLoggedIn || PlayerData == null)
            {
                return true;
            }

            UnityServicesManager.Instance.OnApplicationWantsToQuit(PlayerData);
            PlayerData = null;
            return false;
        }
    }
}
