using JetBrains.Annotations;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.UI
{
    public class LeftPanelCardView : MonoBehaviour
    {
        public RawImage CardImage;
        public RawImage BackCard;

        private void OnValidate()
        {

            CardImage = transform.FindChildRecursively<RawImage>(nameof(CardImage), true);
            BackCard = transform.FindChildRecursively<RawImage>(nameof(BackCard), true);
            SetCard(null);

        }

        void Awake()
        {
            CardImage = transform.FindChildRecursively<RawImage>(nameof(CardImage), true);
            BackCard = transform.FindChildRecursively<RawImage>(nameof(BackCard), true);
            SetCard(null);
        }


        public void SetCard([CanBeNull] Card card)
        {
            if (card != null)
            {
                CardImage.texture = card.Texture2D;
                CardImage.gameObject.SetActive(true);
                BackCard.gameObject.SetActive(false);
            }
            else
            {
                CardImage.gameObject.SetActive(false);
                BackCard.gameObject.SetActive(true);
            }
        }

    }
}