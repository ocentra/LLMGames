using UnityEngine;

namespace ThreeCardBrag
{
    public class HumanPlayer : Player
    {


        public override void SeeHand()
        {
            base.SeeHand();
            GameManager.Instance.UIController.UpdateHumanPlayerHandDisplay();
        }

        public override void ShowHand(bool isRoundEnd=false)
        {
            base.ShowHand(isRoundEnd);
            GameManager.Instance.UIController.UpdateHumanPlayerHandDisplay(isRoundEnd);
        }
        
    }
}