using OcentraAI.LLMGames.Events;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class PlayerInfoButton : Button3DSimple
    {
       [SerializeField] protected bool ShowShowSubTab = true;

        protected override void OnButton3DSimpleClick(Button3DSimpleClickEvent e)
        {
            if (ReferenceEquals(e.Button3DSimple, this))
            {
               
                EventBus.Instance.Publish(new ShowSubTabEvent(ShowShowSubTab));
                base.OnButton3DSimpleClick(e);
            }
           
        }
    }
}