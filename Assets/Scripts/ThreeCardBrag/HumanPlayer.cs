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

        public override void ShowHand()
        {
            base.ShowHand();
            GameController.Instance.UIController.UpdateHumanPlayerHandDisplay();
        }
        
    }
}