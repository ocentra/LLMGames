using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class GameModeUI : Button3DSimple
    {

        [SerializeField, ShowInInspector, ValueDropdown(nameof(GetAvailableGameModeType)), PropertyOrder(-1)]
        [Tooltip("The GameModeType this button represents.")]
        private int id = 0;

      

        public IEnumerable<ValueDropdownItem<int>> GetAvailableGameModeType()
        {
            List<ValueDropdownItem<int>> dropdownItems = new List<ValueDropdownItem<int>>();

            foreach (GameModeType genre in GameModeType.GetAll())
            {
                dropdownItems.Add(new ValueDropdownItem<int>(genre.Name, genre.Id));
            }

            return dropdownItems;
        }
       
        [ShowInInspector, ReadOnly, PropertyOrder(-1)]
        public GameModeType GameModeType { get => GameModeType.FromId(id); set => id = value.Id; }

        [ShowInInspector, ReadOnly, PropertyOrder(-1)]
        public GameMode GameMode { get; set; }

        [ShowInInspector, TextArea(5, 15),RichText, PropertyOrder(-1)]
        protected string Info ;
        

        protected override void Init()
        {
            if (GameModeType != null)
            {
                Info = GameModeType.Name;

                if (GameModeManager.Instance.TryGetGameMode(GameModeType.GenreId, out GameMode gameMode))
                {
                    GameMode = gameMode;
                }
            }


            if (GameMode != null)
            {
                Info = GameMode.GameDescription.Player;
                Info += $"{Environment.NewLine}{Environment.NewLine}";
                Info += GameMode.GameRules.Player;
            }
            base.Init();
        }

        protected override void SetButtonsOnThisGroup()
        {
            if (Parent == null)
            {
                return;
            }

            ButtonsOnThisGroup = new List<Button3DSimple>();

            GameModeUI[] componentsInChildren = Parent.GetComponentsInChildren<GameModeUI>();

            foreach (GameModeUI component in componentsInChildren)
            {
                if (component.GameModeType != null && GameModeType != null &&
                    component.GameModeType.GameGenre == GameModeType.GameGenre)
                {
                    if (!ButtonsOnThisGroup.Contains(component))
                    {
                        ButtonsOnThisGroup.Add(component);
                    }

                }
            }
        }

        protected override void OnButton3DSimpleClick(Button3DSimpleClickEvent e)
        {
            if (ReferenceEquals(e.Button3DSimple, this))
            {
                EventBus.Instance.Publish(new ArcadeInfoEvent(Info));
            }

            base.OnButton3DSimpleClick(e);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Init();
        }


    }
}