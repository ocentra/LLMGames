using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class TopCardView : CardView
{
    [ShowInInspector,Required] protected Button3D Button3D;

    void OnValidate()
    {
        transform.FindChildWithComponent(ref Button3D, nameof(Button3D));
    }

    public override void SetActive(bool show = false)
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif
        base.SetActive(show);
        if (IsValidObject(Button3D))
        {
            Button3D.SetInteractable(show);
        }
    }

    protected override void Init()
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif
        base.Init();
        transform.FindChildWithComponent(ref Button3D, nameof(Button3D));
    }

    public override void SetHighlight(bool set, Color? glowColor = null)
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif
        base.SetHighlight(set, glowColor);
        if (IsValidObject(Button3D) && IsValidObject(HighlightCard))
        {
            Button3D.SetInteractable(HighlightCard.enabled);
        }
    }
}