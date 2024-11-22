using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Screens3D;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.Screens
{
    [ExecuteAlways]
    public class LobbyScreen : UI3DScreen<LobbyScreen>
    {
        [SerializeField] private  List<UI3DScreen> AllLobbyScreens { get; set; }= new List<UI3DScreen>();
        [SerializeField] private int maxAttempts = 10;
        [SerializeField] private int delayMs = 100;

        #region Event Subscriptions
        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventBus.Instance.Subscribe<ShowScreenEvent>(OnShowScreen);
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            EventBus.Instance.Unsubscribe<ShowScreenEvent>(OnShowScreen);
        }

        private void OnShowScreen(ShowScreenEvent showScreenEvent)
        {
            foreach (UI3DScreen uiScreen in AllLobbyScreens)
            {
                if (uiScreen.ScreenName == ScreenName)
                {
                    uiScreen.ShowScreen();
                }
                else
                {
                    uiScreen.HideScreen();
                }
            }
        }
        #endregion


        protected override void Init(bool startEnabled)
        {
            SetMainPanelForScreen<LobbyListScreen>();
            SetMainPanelForScreen<LobbyCreationScreen>();
            SetMainPanelForScreen<JoinedLobbyScreen>();
            SetMainPanelForScreen<InputPasswordScreen>();
            SetMainPanelForScreen<MatchmakerScreen>();
            base.Init(StartEnabled);
        }
        

        private async void SetMainPanelForScreen<T>() where T : UI3DScreen<T>
        {
            T screen = UI3DScreen<T>.Instance;
            int attempts = 0;

            while (screen == null && attempts < maxAttempts)
            {
                attempts++;
                screen = UI3DScreen<T>.Instance;
               
                if (screen == null)
                {
                    Debug.LogWarning($"{typeof(T).Name} instance is null, retrying attempt {attempts}/{maxAttempts}...");
                    await UniTask.Delay(delayMs);
                    screen = await UI3DScreen<T>.GetInstanceAsync();
                }

            }

            if (screen == null)
            {
                Debug.LogError($"{typeof(T).Name} instance is still null after {maxAttempts} attempts.");
                return;
            }

            await screen.WaitForInitializationAsync();

            if (!AllLobbyScreens.Contains(screen))
            {
                AllLobbyScreens.Add(screen);
            }

            RegisteredScreens.TryAdd(typeof(T),screen);
        }
    }
}