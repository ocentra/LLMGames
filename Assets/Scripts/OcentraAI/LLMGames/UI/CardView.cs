using UnityEngine;
using Sirenix.OdinInspector;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;

#if UNITY_EDITOR
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using UnityEditor;
#endif

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]
    public class CardView : MonoBehaviour
    {

        [SerializeField, InlineProperty, BoxGroup(nameof(MainCardState)), HideLabel] protected CardMaterialState MainCardState;
        [SerializeField, InlineProperty, BoxGroup(nameof(HighlightCardState)), HideLabel] protected CardMaterialState HighlightCardState;
        [SerializeField, InlineProperty, BoxGroup(nameof(BackCardState)), HideLabel] protected CardMaterialState BackCardState;

        [HideLabel, BoxGroup(nameof(MeshRenderer))] public MeshRenderer MainCard;
        [HideLabel, BoxGroup(nameof(MeshRenderer))] public MeshRenderer BackCard;
        [HideLabel, BoxGroup(nameof(MeshRenderer))] public MeshRenderer HighlightCard;

        [SerializeField] protected Material MainCardRuntimeMaterial;
        [SerializeField] protected Material HighlightCardRuntimeMaterial;

        [SerializeField, ColorUsage(true, true), ReadOnly]
        public Color OriginalHighlightColor;

        [SerializeField, ColorUsage(true, true), ReadOnly]
        public Color OriginalMainCardColor;

        [Header("Card Settings")]
        [SerializeField, ColorUsage(true, true)] protected Color HighlightColor = Color.white;

        [SerializeField, ColorUsage(true, true)] protected Color MainCardColor = Color.white;

        [ShowInInspector] public Card Card { get; private set; }

        [ShowInInspector] protected bool IsCardHighlighted;


        private void Awake()
        {
            ApplyAllMaterials(true);
            UpdateCardView();
        }

        private void OnDisable()
        {

            ApplyAllMaterials(false);
        }

        private void OnDestroy()
        {
            DisposeAllStates();
        }

        private void OnValidate()
        {



        }

        [Button]
        protected virtual void Init()
        {
            MainCard = transform.FindChildRecursively<MeshRenderer>(nameof(MainCard), true);
            BackCard = transform.FindChildRecursively<MeshRenderer>(nameof(BackCard), true);
            HighlightCard = transform.FindChildRecursively<MeshRenderer>(nameof(HighlightCard), true);


            MainCardState = new CardMaterialState(MainCard);
            HighlightCardState = new CardMaterialState(HighlightCard);
            BackCardState = new CardMaterialState(BackCard);

            MainCardRuntimeMaterial = MainCardState.RuntimeMaterials[0];
            HighlightCardRuntimeMaterial = HighlightCardState.RuntimeMaterials[0];

            OriginalHighlightColor = HighlightCardState.OriginalColor;
            OriginalMainCardColor = MainCardState.OriginalColor;

            UpdateCardView();
            ApplyAllMaterials(true);

            EditorUtility.SetDirty(this);
        }


        protected virtual void ApplyAllMaterials(bool useRuntime)
        {
            MainCardState.ApplyMaterials(useRuntime);
            HighlightCardState.ApplyMaterials(useRuntime);
            BackCardState.ApplyMaterials(useRuntime);
        }

        protected virtual void DisposeAllStates()
        {
            MainCardState.Dispose();
            HighlightCardState.Dispose();
            BackCardState.Dispose();
        }

        public virtual void SetCard(Card newCard)
        {
            Card = newCard;
            UpdateCardView();
        }

        public virtual void UpdateCardView()
        {

            if (Card != null && Card.Texture2D != null && MainCardRuntimeMaterial != null)
            {
                MainCardRuntimeMaterial.mainTexture = Card.Texture2D;
                if (MainCard != null)
                {
                    MainCard.gameObject.SetActive(true);
                }
                if (BackCard != null)
                {
                    BackCard.gameObject.SetActive(false);
                }

                if (MainCardRuntimeMaterial.HasProperty("_BaseColor"))
                {
                    MainCardRuntimeMaterial.SetColor("_BaseColor", MainCardColor);
                }
            }
            else
            {
                ShowBackside();
            }
        }

        public virtual void ShowBackside()
        {
            if (MainCard != null)
            {
                MainCard.gameObject.SetActive(false);
            }
            if (BackCard != null)
            {
                BackCard.gameObject.SetActive(true);
            }
        }

        public virtual void SetHighlight(bool set, Color? glowColor = null)
        {


            if (HighlightCard == null || HighlightCardRuntimeMaterial == null)
            {
                Debug.LogWarning("Highlight card components are not properly initialized.");
                return;
            }

            HighlightCard.gameObject.SetActive(set);
            Material material = HighlightCardRuntimeMaterial;

            if (set && material != null && material.HasProperty("_BaseColor"))
            {
                Color colorToSet = glowColor.HasValue ? glowColor.Value : HighlightColor;
                material.SetColor("_BaseColor", colorToSet);
                material.EnableKeyword("_EMISSION");
            }
            else if (material != null)
            {
                material.DisableKeyword("_EMISSION");
            }

            IsCardHighlighted = set;
        }

        public virtual void SetInteractable(bool show = false)
        {

            if (MainCard != null)
            {
                MainCard.gameObject.SetActive(show);
            }

            if (BackCard != null)
            {
                BackCard.gameObject.SetActive(!show);
            }

            if (HighlightCard != null)
            {
                HighlightCard.gameObject.SetActive(IsCardHighlighted);
            }


        }



#if UNITY_EDITOR



        private bool mainCardPreview = false;

        [Button]
        public virtual void SetMainCardPreview()
        {
            ApplyAllMaterials(true);
            mainCardPreview = !mainCardPreview;
            SetCard(mainCardPreview ? Deck.Instance.GetRandomCard() : null);

        }

        private bool showHighlightPreview = false;

        [Button]
        public virtual void SetHighlightPreview()
        {
            ApplyAllMaterials(true);
            showHighlightPreview = !showHighlightPreview;

            SetHighlight(showHighlightPreview);


        }


#endif

        public virtual void ResetCardView()
        {

            Card = null;
            ShowBackside();
        }
    }
}