using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeCardBrag
{
    public class CardView : MonoBehaviour
    {
        [ShowInInspector] 
        public Image CardImage { get; private set; }
        
        [ShowInInspector]
        public Card Card { get; private set; }
        
        [ShowInInspector]
        private Transform Parent { get; set; }
        


        void OnValidate()
        {
            Init();

        }
        void Start()
        {
            Init();
        }

        private void Init()
        {
            if (CardImage == null )
            {
                CardImage = GetComponent<Image>();
            }

            Parent = transform.parent;


            UpdateCardView();
        }

        public void SetCard(Card newCard)
        {
            Card = newCard;
        }

        public void ShowBackside()
        {
            CardImage.sprite = Deck.Instance.BackCard.Sprite;
        }

        public void UpdateCardView()
        {

            if (Card != null && Card.Sprite != null )
            {
                CardImage.sprite = Card.Sprite;
            }
            else
            {
                ShowBackside();
            }
        }

        public void SetActive(bool value)
        {
            
            gameObject.SetActive(value);
            Parent.gameObject.SetActive(value);
        }



        public void ResetCardView()
        {
            Card = null;
            ShowBackside();
        }
    }
}