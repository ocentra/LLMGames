using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.UI;
using UnityEngine;

[ExecuteAlways]
public class TopCardView : CardView
{
    [SerializeField] private PlayerDecisionButton playerDecisionButton;

    public PlayerDecisionButton PlayerDecisionButton
    {
        get => playerDecisionButton;
        set => playerDecisionButton = value;
    }

    void OnValidate()
    {
        transform.FindChildWithComponent(ref playerDecisionButton, nameof(Button3D));
    }
    public override void SetInteractable(bool show = false)
    {
        if (MainCard != null)
        {
            MainCard.gameObject.SetActive(Card != null);
        }

        if (BackCard != null)
        {
            BackCard.gameObject.SetActive(Card == null);
        }

        if (HighlightCard != null)
        {
            HighlightCard.gameObject.SetActive(IsCardHighlighted);
            playerDecisionButton.SetInteractable(IsCardHighlighted && show);
        }


    }

    protected override void Init()
    {
        base.Init();
        transform.FindChildWithComponent(ref playerDecisionButton, nameof(Button3D));
    }

    public override void SetHighlight(bool set, Color? glowColor = null)
    {
        base.SetHighlight(set, glowColor);
        playerDecisionButton.SetInteractable(HighlightCard.enabled && set);
    }
}