using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    public class CardView : MonoBehaviour
    {
        [ShowInInspector ,Required]
        public Image CardImage { get; private set; }
        [ShowInInspector] public Image HighlightImage { get; private set; }
        [ShowInInspector] public Card Card { get; private set; }
        [ShowInInspector] private Transform Parent { get; set; }
        [ShowInInspector] private Button Button { get; set; }

        void OnValidate()
        {
            Init();
        }

        void Start()
        {
            Init();
        }

        public void Init()
        {
            if (CardImage == null)
            {
                CardImage = GetComponent<Image>();
            }

            if (Button == null)
            {
                Button = GetComponent<Button>();
            }

            if (HighlightImage == null)
            {
                HighlightImage = transform.FindChildRecursively<Image>(nameof(HighlightImage));
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
            if (HighlightImage != null)
            {
                HighlightImage.enabled = false;
            }
            CardImage.sprite = Deck.Instance.BackCard.Sprite;
        }

        public void UpdateCardView()
        {
            if (CardImage == null)
            {
                CardImage = GetComponent<Image>();
            }

            if (Card != null && Card.Sprite != null && CardImage != null)
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
            if (Parent != null)
            {
                Parent.gameObject.SetActive(value);
            }
        }

        public void Hide()
        {
            GameObject sourceObject = gameObject;
            if (Parent != null)
            {
                sourceObject= Parent.gameObject;
            }

            sourceObject.GetComponentsInChildren<Image>().ForEach(i=> i.color = Color.clear) ;

            sourceObject.GetComponentsInChildren<TextMeshProUGUI>().ForEach(i => i.enabled = false);

        }

        public void ResetCardView()
        {
            Card = null;
            ShowBackside();
        }

        public void SetHighlight(bool set)
        {
            if (HighlightImage != null)
            {
                HighlightImage.enabled = set;
            }

            if (Button != null)
            {
                Button.enabled = set;
            }
        }
    }
}
