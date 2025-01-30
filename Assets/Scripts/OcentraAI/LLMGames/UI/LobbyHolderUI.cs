using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]
    public class LobbyHolderUI : Button3DSimple
    {

        [SerializeField, ShowInInspector, ValueDropdown(nameof(GetAvailableLobbyTypes)), FoldoutGroup("LobbyHolderUI", false), PropertyOrder(-1)]
        [Tooltip("The LobbyType this button represents.")]
        private int lobbyID = 0;


        [ShowInInspector, ReadOnly, PropertyOrder(-1)]
        public LobbyType LobbyType
        {
            get => LobbyType.FromId(lobbyID);
            set => lobbyID = value?.Id ?? 0;
        }

        public IEnumerable<ValueDropdownItem<int>> GetAvailableLobbyTypes()
        {
            List<ValueDropdownItem<int>> dropdownItems = new List<ValueDropdownItem<int>>();

            foreach (LobbyType lobby in LobbyType.GetAll())
            {
                dropdownItems.Add(new ValueDropdownItem<int>(
                    $"{lobby.Name} [{lobby.HostingMethod.Name}]",
                    lobby.Id
                ));
            }

            return dropdownItems;
        }

        [ShowInInspector, FoldoutGroup("LobbyHolderUI", false), PropertyOrder(-1)]
        public GameMode GameMode { get; set; }

        [ShowInInspector, TextArea(5, 15), RichText, FoldoutGroup("LobbyHolderUI", false), PropertyOrder(-1)]
        public string Info;

        [SerializeField, Tooltip("Editable list of key-value pairs for lobby info.")]
        public List<LobbyInfoEntry> LobbyInfoEntries = new List<LobbyInfoEntry>();

        [ShowInInspector, FoldoutGroup("LobbyHolderUI/HolderMaterial Info", false), PropertyOrder(-1)]
        public List<HolderStatusEntry> HolderStatusList = new List<HolderStatusEntry>();

        [SerializeField, FoldoutGroup("LobbyHolderUI/HolderMaterial Info", false), OnValueChanged(nameof(UpdateLobbyStatusInfoState))]
        public bool LobbyActive = false;

        [SerializeField, FoldoutGroup("LobbyHolderUI/HolderMaterial Info", false), OnValueChanged(nameof(UpdateLobbyStatusInfoState))]
        public bool SlotAvailable = false;

        protected Renderer LobbyTypeFrame;
        protected Renderer PlayerCountFrame;
        protected List<Renderer> TargetRenderers { get; set; }
        protected override void Init()
        {
            if (LobbyType != null && LobbyType != LobbyType.None)
            {
                if (LobbyInfoEntries == null || LobbyInfoEntries.Count == 0)
                {
                    LobbyInfoEntries = new List<LobbyInfoEntry>
                    {
                        new LobbyInfoEntry("Hosting", "🏛",value: LobbyType.HostingMethod.Name),
                        new LobbyInfoEntry("LLM Source", "🧠",value: LobbyType.LLMSource.Name),
                        new LobbyInfoEntry("Requires Desktop", "🖥", LobbyType.RequiresDesktop),
                        new LobbyInfoEntry("Player Creatable", "🎮", LobbyType.IsPlayerCreatable),
                        new LobbyInfoEntry("Desktop", "💻", LobbyType.SupportsDesktop),
                        new LobbyInfoEntry("Mobile", "📱", LobbyType.SupportsMobile),
                        new LobbyInfoEntry("Web", "🌐", LobbyType.SupportsWeb),
                        new LobbyInfoEntry("VR", "🕶", LobbyType.SupportsVR),
                        new LobbyInfoEntry("AR", "📱🕶", LobbyType.SupportsAR)

                    };

                }
            }


            if (HolderStatusList == null)
            {
                HolderStatusList = new List<HolderStatusEntry>();
            }

            if (TargetRenderers == null)
            {
                TargetRenderers = new List<Renderer>();
            }

            AddRendererToList(nameof(LobbyTypeFrame), ref LobbyTypeFrame);
            AddRendererToList(nameof(PlayerCountFrame), ref PlayerCountFrame);

            foreach (Renderer targetRenderer in TargetRenderers)
            {
                if (!HolderStatusList.Exists(entry => entry.Renderer == targetRenderer))
                {
                    HolderStatusList.Add(new HolderStatusEntry { Renderer = targetRenderer });
                }
            }

            foreach (HolderStatusEntry entry in HolderStatusList)
            {
                InitializeLobbyStatus(entry.Renderer, entry.StatusInfoList);
            }


            UpdateLobbyStatusInfoState();


            base.Init();
        }



        protected void InitializeLobbyStatus(Renderer targetRenderer, List<LobbyStatusInfo> materialInfos)
        {
            if (targetRenderer == null) return;

            Material[] sharedMaterials = targetRenderer.sharedMaterials;


            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material mat = sharedMaterials[i];
                if (mat == null) continue;
                string statusTypeName = string.Equals(targetRenderer.name, LobbyTypeFrame.name, StringComparison.CurrentCultureIgnoreCase) ? nameof(LobbyActive) : nameof(SlotAvailable);

                LobbyStatusInfo existingInfo = null;
                foreach (LobbyStatusInfo info in materialInfos)
                {
                    if (info.Material == mat)
                    {
                        existingInfo = info;
                        break;
                    }
                }

                if (existingInfo == null)
                {
                    LobbyStatusInfo newInfo = new LobbyStatusInfo(mat, i, statusTypeName)
                    {
                        MaterialIndex = i,
                        MaterialName = mat.name
                    };
                    materialInfos.Add(newInfo);
                }
                else
                {
                    existingInfo.MaterialIndex = i;
                    existingInfo.MaterialName = mat.name;
                    existingInfo.StatusTypeName = statusTypeName;
                }
            }

            for (int index = 0; index < materialInfos.Count; index++)
            {
                LobbyStatusInfo info = materialInfos[index];
                if (info.PropertyBlock == null)
                {
                    info.PropertyBlock = new MaterialPropertyBlock();
                }
            }
        }

        private void AddRendererToList(string rendererName, ref Renderer targetRenderer)
        {
            if (targetRenderer == null)
            {
                targetRenderer = transform.FindChildRecursively<Renderer>(rendererName, true);

            }

            if (targetRenderer != null && !TargetRenderers.Contains(targetRenderer))
            {
                TargetRenderers.Add(targetRenderer);
            }
        }
        public void UpdateLobbyStatusInfoState()
        {
            foreach (HolderStatusEntry entry in HolderStatusList)
            {
                foreach (LobbyStatusInfo info in entry.StatusInfoList)
                {
                    if (info.PropertyBlock == null)
                    {
                        info.PropertyBlock = new MaterialPropertyBlock();
                    }

                    if (string.Equals(info.StatusTypeName, nameof(SlotAvailable), StringComparison.CurrentCultureIgnoreCase))
                    {
                        info.PropertyBlock.SetColor(BaseColor, SlotAvailable ? info.ActiveColor : info.InActiveColor);
                        info.PropertyBlock.SetColor(EmissionColor, SlotAvailable ? Color.clear : info.InActiveColor);
                    }
                    else if (string.Equals(info.StatusTypeName, nameof(LobbyActive), StringComparison.CurrentCultureIgnoreCase))
                    {
                        info.PropertyBlock.SetColor(BaseColor, LobbyActive ? info.ActiveColor : info.InActiveColor);
                        info.PropertyBlock.SetColor(EmissionColor, LobbyActive ? Color.clear : info.InActiveColor);
                    }

                    entry.Renderer.SetPropertyBlock(info.PropertyBlock, info.MaterialIndex);
                }
            }
        }


        protected override void SetButtonsOnThisGroup()
        {
            if (Parent == null)
            {
                return;
            }

            ButtonsOnThisGroup = new List<Button3DSimple>();

            LobbyHolderUI[] componentsInChildren = Parent.GetComponentsInChildren<LobbyHolderUI>();

            for (int index = 0; index < componentsInChildren.Length; index++)
            {
                LobbyHolderUI component = componentsInChildren[index];
                if (!ButtonsOnThisGroup.Contains(component))
                {
                    ButtonsOnThisGroup.Add(component);
                }
            }
        }

        protected override async void OnButton3DSimpleClick(Button3DSimpleClickEvent e)
        {
            IButton3DSimple button3DSimple = e.Button3DSimple;

            if (ReferenceEquals(button3DSimple, this))
            {
                await EventBus.Instance.PublishAsync(new ShowSubTabEvent(false));
                await EventBus.Instance.PublishAsync(new LobbyInfoEvent(button3DSimple));
            }

            GameModeUI gameModeUI = button3DSimple as GameModeUI;
            if (gameModeUI != null)
            {
                GameMode = gameModeUI.GameMode;
                if (GameMode != null)
                {
                    ButtonText.text = GameMode.GameName;
                }

            }


            base.OnButton3DSimpleClick(e);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (HolderStatusList != null)
            {
                HolderStatusList.Clear();
            }

            if (TargetRenderers != null)
            {
                TargetRenderers.Clear();
            }

            Init();
        }


    }
}