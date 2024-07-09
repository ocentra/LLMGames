using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using OcentraAI.LLMGames.Extensions;


namespace OcentraAI.LLMGames.Screens
{
    public class SettingsScreen : UIScreen<SettingsScreen>
    {
        private  string MusicVolume =>"MusicVolume";
        private string SFXVolume => "SFXVolume";
        private string LLMModel => "LLMModel";

        [ShowInInspector, Required]
        public GameObject GeneralSettingsTab { get; private set; }

        [ShowInInspector, Required]
        public GameObject LLMSettingsTab { get; private set; }

        [ShowInInspector, Required]
        public GameObject SkinSettingsTab { get; private set; }

        [ShowInInspector, Required]
        public GameObject CardSettingsTab { get; private set; }

        [ShowInInspector, Required]
        public List<Button> TabButtons { get; private set; } = new List<Button>();

        [ShowInInspector, Required]
        public Slider MusicVolumeSlider { get; private set; }

        [ShowInInspector, Required]
        public Slider SFXVolumeSlider { get; private set; }

        [ShowInInspector, Required]
        public Toggle FullscreenToggle { get; private set; }

        [ShowInInspector, Required]
        public TMP_Dropdown ResolutionDropdown { get; private set; }

        [ShowInInspector, Required]
        public TMP_Dropdown LLMModelDropdown { get; private set; }

        [ShowInInspector, Required]
        public Button ApplyButton { get; private set; }

        [ShowInInspector, Required]
        public Button BackButton { get; private set; }


        [ShowInInspector, Required]
        public Button GeneralTabButton { get; private set; }
        [ShowInInspector, Required]
        public Button LLMTabButton { get; private set; }
        [ShowInInspector, Required]
        public Button SkinTabButton { get; private set; }
        [ShowInInspector, Required]
        public Button CardTabButton { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            InitReferences();
        }

        void OnValidate()
        {
            InitReferences();
        }

        private void InitReferences()
        {
            GeneralSettingsTab = transform.FindChildRecursively<Transform>(nameof(GeneralSettingsTab)).gameObject;
            LLMSettingsTab = transform.FindChildRecursively<Transform>(nameof(LLMSettingsTab)).gameObject;
            SkinSettingsTab = transform.FindChildRecursively<Transform>(nameof(SkinSettingsTab)).gameObject;
            CardSettingsTab = transform.FindChildRecursively<Transform>(nameof(CardSettingsTab)).gameObject;
            
            MusicVolumeSlider = transform.FindChildRecursively<Slider>(nameof(MusicVolumeSlider));
            SFXVolumeSlider = transform.FindChildRecursively<Slider>(nameof(SFXVolumeSlider));
            
            FullscreenToggle = transform.FindChildRecursively<Toggle>(nameof(FullscreenToggle));
            
            ResolutionDropdown = transform.FindChildRecursively<TMP_Dropdown>(nameof(ResolutionDropdown));
            LLMModelDropdown = transform.FindChildRecursively<TMP_Dropdown>(nameof(LLMModelDropdown));
            
            ApplyButton = transform.FindChildRecursively<Button>(nameof(ApplyButton));
            BackButton = transform.FindChildRecursively<Button>(nameof(BackButton));
            
            GeneralTabButton = transform.FindChildRecursively<Button>(nameof(GeneralTabButton));
            LLMTabButton = transform.FindChildRecursively<Button>(nameof(LLMTabButton));
            SkinTabButton = transform.FindChildRecursively<Button>(nameof(SkinTabButton));
            CardTabButton = transform.FindChildRecursively<Button>(nameof(CardTabButton));

            TabButtons = new List<Button>
            {
                GeneralTabButton,LLMTabButton,SkinTabButton,CardTabButton
      
            };
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializeSettingsScreen();
        }

        private void InitializeSettingsScreen()
        {
            LoadCurrentSettings();
            SetupTabButtons();
            ApplyButton.onClick.AddListener(ApplySettings);
            BackButton.onClick.AddListener(GoBack);
        }

        private void SetupTabButtons()
        {
            for (int i = 0; i < TabButtons.Count; i++)
            {
                int index = i;
                TabButtons[i].onClick.AddListener(() => SwitchTab(index));
            }
            SwitchTab(0); // Start with the general settings tab
        }

        private void SwitchTab(int tabIndex)
        {
            GeneralSettingsTab.SetActive(tabIndex == 0);
            LLMSettingsTab.SetActive(tabIndex == 1);
            SkinSettingsTab.SetActive(tabIndex == 2);
            CardSettingsTab.SetActive(tabIndex == 3);
        }

        private void LoadCurrentSettings()
        {
            
            // Todo implement actual settings loading
            MusicVolumeSlider.value = PlayerPrefs.GetFloat(MusicVolume, 1f);
            SFXVolumeSlider.value = PlayerPrefs.GetFloat(SFXVolume, 1f);
            FullscreenToggle.isOn = Screen.fullScreen;
            PopulateResolutionDropdown();
            PopulateLLMModelDropdown();
            // todo Load skin and card settings
        }

        private void PopulateResolutionDropdown()
        {
            Resolution[] resolutions = Screen.resolutions;
            ResolutionDropdown.ClearOptions();
            List<string> options = new List<string>();

            foreach (Resolution resolution in resolutions)
            {
                string option = resolution.width + " x " + resolution.height;
                options.Add(option);
            }

            ResolutionDropdown.AddOptions(options);
            ResolutionDropdown.value = options.IndexOf(Screen.currentResolution.width + " x " + Screen.currentResolution.height);
            ResolutionDropdown.RefreshShownValue();
        }


        private void PopulateLLMModelDropdown()
        {
            LLMModelDropdown.ClearOptions();
            LLMModelDropdown.AddOptions(new List<string> { "GPT-3.5", "GPT-4", "GPT4-O","Claude","Local" });
        }

        private void ApplySettings()
        {
            PlaySelectionSound();
            PlayerPrefs.SetFloat(MusicVolume, MusicVolumeSlider.value);
            PlayerPrefs.SetFloat(SFXVolume, SFXVolumeSlider.value);
            Screen.fullScreen = FullscreenToggle.isOn;
            Resolution resolution = Screen.resolutions[ResolutionDropdown.value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetString(LLMModel, LLMModelDropdown.options[LLMModelDropdown.value].text);
            PlayerPrefs.Save();
        }

   
    }
}
