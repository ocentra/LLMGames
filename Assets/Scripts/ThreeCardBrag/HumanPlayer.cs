using UnityEngine;

namespace ThreeCardBrag
{
    public class HumanPlayer : Player
    {


        public override void SeeHand()
        {
            base.SeeHand();
            GameController.Instance.UIController.UpdateHumanPlayerHandDisplay();
        }

        public override void ShowHand(bool isRoundEnd=false)
        {
            base.ShowHand(isRoundEnd);
            GameController.Instance.UIController.UpdateHumanPlayerHandDisplay(isRoundEnd);
        }
        
    }
}