using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]
    public class LobbyItemHolder : ElementGameHolder<LobbyHolderUI, LobbyType>
    {
        public Transform BlockerPanel;
        public bool BlockerPanelEnabled = true;

        
        public override void Init()
        {
            BlockerPanel = transform.FindChildRecursively(nameof(BlockerPanel));
            if (BlockerPanel != null)
            {
                BlockerPanel.gameObject.SetActive(BlockerPanelEnabled);
            }
            base.Init();
        }

        protected override async UniTask OnButton3DSimpleClick(Button3DSimpleClickEvent e)
        {

            await base.OnButton3DSimpleClick(e);
            if (FilterContext != null)
            {
                BlockerPanelEnabled = false;

                if (BlockerPanel != null)
                {
                    BlockerPanel.gameObject.SetActive(false);
                }
                
            }
           

        }
    }
}