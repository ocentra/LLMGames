using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class ArcadeGameGenre : Button3DSimple
    {

        [SerializeField, ShowInInspector, ValueDropdown(nameof(GetAvailableDecisions)), LabelText(nameof(GameGenre)), PropertyOrder(-1)]
        [Tooltip("The SelectedGameGenre this button represents.")]
        private int id = 0;

        public IEnumerable<ValueDropdownItem<int>> GetAvailableDecisions()
        {
            List<ValueDropdownItem<int>> dropdownItems = new List<ValueDropdownItem<int>>();

            foreach (GameGenre genre in GameGenre.GetAll())
            {
                dropdownItems.Add(new ValueDropdownItem<int>(genre.Name, genre.Id));
            }

            return dropdownItems;
        }

        public GameGenre GameGenre { get => GameGenre.FromId(id); set => id = value.Id; }

        [SerializeField, ShowInInspector, TextArea, RichText, PropertyOrder(-1)]
        protected string Info = "Info About This";

        protected override void Init()
        {
            if (GameGenre != null)
            {
                Info = GameGenre.Name;

                if (GameModeManager.Instance.GameInfo != null && GameModeManager.Instance.GameInfo.TryGetValue(GameGenre, out Info info))
                {
                    Info = info.Value;
                }
            }



            base.Init();
        }
        protected override void SetButtonsOnThisGroup()
        {
            if (Parent != null)
            {
                ButtonsOnThisGroup = new List<Button3DSimple>();

                ArcadeGameGenre[] componentsInChildren = Parent.GetComponentsInChildren<ArcadeGameGenre>();
                foreach (ArcadeGameGenre component in componentsInChildren)
                {
                    if (component.GameGenre != null)
                    {
                        ButtonsOnThisGroup.Add(component);
                    }
                }

            }

        }

        protected override async void OnButton3DSimpleClick(Button3DSimpleClickEvent e)
        {
            if (ReferenceEquals(e.Button3DSimple, this))
            {
                await EventBus.Instance.PublishAsync(new ArcadeInfoEvent(Info));
            }
            base.OnButton3DSimpleClick(e);
        }
    }
}