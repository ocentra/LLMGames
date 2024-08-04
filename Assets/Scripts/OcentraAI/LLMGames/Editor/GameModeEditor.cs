using OcentraAI.LLMGames.GameModes.Rules;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [CustomEditor(typeof(GameMode), true)]
    public class GameModeEditor : OdinEditor
    {
        [OdinSerialize] public List<BaseBonusRule> BaseBonusRulesTemplate { get; set; } = new List<BaseBonusRule>();
        [OdinSerialize] public Dictionary<string, bool> RuleSelectionState { get; set; } = new Dictionary<string, bool>();
        [OdinSerialize] public Dictionary<string, bool> RuleCompatibilityState { get; set; } = new Dictionary<string, bool>();
        [OdinSerialize] public GameMode GameMode { get; set; }

        [FolderPath(AbsolutePath = true), ReadOnly]
        [OdinSerialize] public string RulesTemplatePath = "Assets/Resources/GameMode";

        [OdinSerialize] public Vector2 scrollPosition;
        [OdinSerialize] public float Height { get; set; } = 100;

        protected override void OnEnable()
        {
            base.OnEnable();
            GameMode = (GameMode)target;
            LoadRuleTemplates();
            SyncSelectedRules();
        }

        private void LoadRuleTemplates()
        {
            BaseBonusRulesTemplate.Clear();
            RuleSelectionState.Clear();
            RuleCompatibilityState.Clear();
            if (!string.IsNullOrEmpty(RulesTemplatePath))
            {
                string relativePath = GetRelativePath(RulesTemplatePath);
                string[] assetGuids = AssetDatabase.FindAssets("t:BaseBonusRule", new[] { relativePath });
                foreach (string guid in assetGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    BaseBonusRule rule = AssetDatabase.LoadAssetAtPath<BaseBonusRule>(assetPath);
                    if (rule != null && GameMode.NumberOfCards >= rule.MinNumberOfCard && !AssetDatabase.IsSubAsset(rule))
                    {
                        BaseBonusRulesTemplate.Add(rule);
                        RuleSelectionState[rule.name] = false;
                        RuleCompatibilityState[rule.name] = true;
                    }
                }
            }
        }

        private string GetRelativePath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
            {
                return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            }
            return absolutePath;
        }

        private void SyncSelectedRules()
        {
            if (GameMode.BonusRules != null)
            {
                foreach (var rule in BaseBonusRulesTemplate)
                {
                    RuleSelectionState[rule.name] = GameMode.BonusRules.Any(r => r.name == rule.name);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            RulesTemplatePath = EditorGUILayout.TextField("Path to Rule Templates", RulesTemplatePath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Rule Templates Folder", RulesTemplatePath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    RulesTemplatePath = selectedPath;
                    LoadRuleTemplates();
                    SyncSelectedRules();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Available Rule Templates", EditorStyles.boldLabel, GUILayout.Width(170));
            if (GUILayout.Button("Select All", GUILayout.Width(80)))
            {
                SelectAllRules();
                TryInitialize();
            }
            if (GUILayout.Button("Deselect All", GUILayout.Width(80)))
            {
                DeselectAllRules();
                TryInitialize();
            }
            EditorGUILayout.Space(10);
            Height = EditorGUILayout.Slider(Height, 50,500 );
            EditorGUILayout.Space(10);
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (BaseBonusRulesTemplate.Count > 0)
            {
                // Sort the rules so that selected ones come on top
                var sortedRules = BaseBonusRulesTemplate.OrderByDescending(rule => RuleSelectionState[rule.name]).ToList();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Height), GUILayout.ExpandHeight(true));
                foreach (var rule in sortedRules)
                {
                    EditorGUILayout.BeginHorizontal();

                    bool isSelected = RuleSelectionState[rule.name];
                    bool newSelectionState = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));

                    if (newSelectionState != isSelected)
                    {
                        RuleSelectionState[rule.name] = newSelectionState;
                        RuleSelectionState[rule.name] = TryInitialize();
                    }

                    if (RuleCompatibilityState[rule.name])
                    {
                        EditorGUILayout.ObjectField(rule, typeof(BaseBonusRule), false);
                    }
                    else
                    {
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(rule, typeof(BaseBonusRule), false);
                        GUI.enabled = true;
                        EditorGUILayout.HelpBox("Incompatible rule for this game mode", MessageType.Warning);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No rule templates found in the selected folder.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            base.OnInspectorGUI();

            if (GUILayout.Button(nameof(TryInitialize)))
            {
                TryInitialize();
            }
        }

        private void SelectAllRules()
        {
            foreach (var rule in BaseBonusRulesTemplate)
            {
                RuleSelectionState[rule.name] = true;
                TryInitialize();
            }
        }

        private void DeselectAllRules()
        {
            foreach (var rule in BaseBonusRulesTemplate)
            {
                RuleSelectionState[rule.name] = false;
                TryInitialize();
            }
        }

        private bool TryInitialize()
        {
            List<BaseBonusRule> selectedBonusRules = new List<BaseBonusRule>();
            foreach (var rule in BaseBonusRulesTemplate)
            {
                if (RuleSelectionState[rule.name])
                {
                    selectedBonusRules.Add(rule);
                }
            }
            return GameMode.TryInitialize(selectedBonusRules);
        }
    }
}
