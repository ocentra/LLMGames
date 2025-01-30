using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;

namespace OcentraAI.LLMGames.UI
{
    public class LobbyInfoKeyValueRow : MonoBehaviourBase<LobbyInfoKeyValueRow>
    {
        [ShowInInspector,Required] protected TextMeshPro KeyText;
        [ShowInInspector, Required] protected TextMeshPro ValueText;
        [ShowInInspector, Required] protected TextMeshPro IconText;

        public void SetData((string key, string value, string icon) keyValuePair)
        {
            KeyText.text = keyValuePair.key;   
            ValueText.text = keyValuePair.value;
            IconText.text = keyValuePair.icon;
        }


        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnValidate()
        {
            Init();
            base.OnValidate();
           
        }

        protected void Init()
        {
            KeyText = transform.FindChildRecursively<TextMeshPro>(nameof(KeyText));
            ValueText = transform.FindChildRecursively<TextMeshPro>(nameof(ValueText));
            IconText = transform.FindChildRecursively<TextMeshPro>(nameof(IconText));
        }


    }
}