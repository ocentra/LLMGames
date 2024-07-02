using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThreeCardBrag.GameScreen;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

namespace ThreeCardBrag.Authentication
{
    public class AuthenticationManager : MonoBehaviour
    {
        public static AuthenticationManager Instance { get; private set; }


        [ShowInInspector]
        public PlayerData PlayerData { get; private set; } = null;
        public bool IsSigningIn { get; private set; }
        public bool IsLoggedIn { get; set; }

        public string PlayerName { get; private set; }

        public event Action AuthenticationCompleted;

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


        void Start()
        {
            try
            {

                SetAuthenticationEvents();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            AuthenticationCompleted += LoginScreen.Instance.OnAuthenticationCompleted;
            PlayerAccountService.Instance.SignedIn += SignInWithUnityAsync;
        }


        private void SetAuthenticationEvents()
        {
            AuthenticationService.Instance.SignedIn += async () =>
            {

                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");


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
                        LoginScreen.Instance.ShowErrorMessage($"PlayerName is Null or empty");
                    }
                }
                else
                {
                    LoginScreen.Instance.ShowErrorMessage($"PlayerData could not be");
                }

                // Invoke the event for main thread to respond
                AuthenticationCompleted?.Invoke();

            };

            AuthenticationService.Instance.SignInFailed += (err) =>
            {
                IsLoggedIn = false;
                PlayerData = null;
                // Debug.LogError(err.Message);
                LoginScreen.Instance.ShowErrorMessage($"{err.ErrorCode} \n {err.Message}");
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                IsLoggedIn = false;
                PlayerData = null;
                // Debug.Log("Player signed out.");
                LoginScreen.Instance.ShowErrorMessage("Player signed out. ");
            };

            AuthenticationService.Instance.Expired += () =>
            {
                IsLoggedIn = false;
                PlayerData = null;
                // Debug.Log("Player session could not be refreshed and expired.");
                LoginScreen.Instance.ShowErrorMessage("Player session could not be refreshed and expired. ");


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
            LoginScreen.Instance.UpdateButtonInteractability(false);

            try
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                // Debug.Log("SignIn is successful.");
            }
            catch (AuthenticationException ex)
            {

                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"SignInWithUnityAsync: AuthenticationException :  \n   {ex.ErrorCode} \n {ex.Message}");
            }
            catch (RequestFailedException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"SignInWithUnityAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}");
            }
            finally
            {
                IsSigningIn = false;
                LoginScreen.Instance.UpdateButtonInteractability(true);
            }
        }

        private async Task<bool> LinkWithUnityAsync()
        {
            try
            {
                await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                // Debug.Log("Link is successful.");
                return true;
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                // Debug.LogError("This user is already linked with another account. Log in instead.");
                LoginScreen.Instance.ShowErrorMessage($"LinkWithUnityAsync: AccountAlreadyLinked :  \n   {ex.ErrorCode} \n {ex.Message}");
                return false;
            }

            catch (AuthenticationException ex)
            {

                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"LinkWithUnityAsync: AuthenticationException :  \n   {ex.ErrorCode} \n {ex.Message}");
                return false;
            }
            catch (RequestFailedException ex)
            {

                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"LinkWithUnityAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}");
                return false;
            }
        }

        private async Task SignInAnonymouslyAsync()
        {
            // todo should we create or not an annon account ? having account does gives analitics but would be hard to track annon user
            try
            {
                SignInOptions signInOptions = new SignInOptions
                {
                    CreateAccount = false
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOptions);
                // Debug.Log("Sign in anonymously succeeded!");

                // Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

                await AddUsernamePasswordAsync("annon", "password");


            }
            catch (AuthenticationException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"SignInAnonymouslyAsync: AuthenticationException :  \n    {ex.ErrorCode} \n {ex.Message}");
            }
            catch (RequestFailedException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"SignInAnonymouslyAsync: RequestFailedException :  \n  {ex.ErrorCode} \n {ex.Message}");
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
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"AddUsernamePasswordAsync: AuthenticationException :  \n {ex.ErrorCode} \n {ex.Message}");
            }
            catch (RequestFailedException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"AddUsernamePasswordAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}");
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
            //todo need work on this 
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
                        if (LoginScreen.Instance.ValidateUserAndPassword())
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
                            Debug.Log("Sign in anonymously succeeded!");

                            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

                        }

                    }
                }

            }
            catch (AuthenticationException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"SignInCachedUserAsync: AuthenticationException :  \n    {ex.ErrorCode} \n {ex.Message}");
            }
            catch (RequestFailedException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"SignInCachedUserAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}");
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
                // Debug.Log("Username  added.");
            }
            catch (AuthenticationException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"UpdatePlayerNameAsync: AuthenticationException :  \n {ex.ErrorCode} \n {ex.Message}");
            }
            catch (RequestFailedException ex)
            {
                // Debug.LogException(ex);
                LoginScreen.Instance.ShowErrorMessage($"UpdatePlayerNameAsync: RequestFailedException :  \n {ex.ErrorCode} \n {ex.Message}");
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
