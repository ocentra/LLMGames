using Animancer;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Screens3D;

namespace OcentraAI.LLMGames.Screens
{
    public class ArcadeScreen : UI3DScreen<ArcadeScreen>
    {


        #region Event Subscriptions
        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventRegistrar.Subscribe<ShowScreenEvent>(OnShowScreen);
        }



        private void OnShowScreen(ShowScreenEvent showScreenEvent)
        {
           
        }
        #endregion


        protected override void Init(bool startEnabled)
        {

            base.Init(StartEnabled);
        }


    }
}