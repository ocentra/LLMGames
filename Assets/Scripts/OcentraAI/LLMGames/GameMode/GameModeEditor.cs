using OcentraAI.LLMGames.GameModes.Rules;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [CustomEditor(typeof(GameMode), true)]
    public class GameModeEditor : OdinEditor
    {
        private List<BaseBonusRule> BaseBonusRulesTemplate { get; set; } = new List<BaseBonusRule>();

        private Dictionary<string, bool> ruleSelectionState = new Dictionary<string, bool>();

        private GameMode gameMode;

        [FolderPath(AbsolutePath = true),ReadOnly]
        public string RulesTemplatePath = "Assets/Resources/GameMode";

        private Vector2 scrollPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            gameMode = (GameMode)target;
            LoadRuleTemplates();
            SyncSelectedRules();
        }

        private void LoadRuleTemplates()
        {
            BaseBonusRulesTemplate.Clear();
            ruleSelectionState.Clear();
            if (!string.IsNullOrEmpty(RulesTemplatePath))
            {
                string relativePath = GetRelativePath(RulesTemplatePath);
                string[] assetGuids = AssetDatabase.FindAssets("t:BaseBonusRule", new[] { relativePath });
                foreach (string guid in assetGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    BaseBonusRule rule = AssetDatabase.LoadAssetAtPath<BaseBonusRule>(assetPath);
                    if (rule != null && !AssetDatabase.IsSubAsset(rule))
                    {
                        BaseBonusRulesTemplate.Add(rule);
                        ruleSelectionState[rule.name] = false;

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
            if (gameMode.BonusRules != null)
            {
                foreach (var rule in BaseBonusRulesTemplate)
                {
                    ruleSelectionState[rule.name] = gameMode.BonusRules.Any(r => r.name == rule.name);
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
            EditorGUILayout.LabelField("Available Rule Templates", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (BaseBonusRulesTemplate.Count > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100), GUILayout.ExpandHeight(true));
                foreach (var rule in BaseBonusRulesTemplate)
                {
                    EditorGUILayout.BeginHorizontal();

                    bool isSelected = ruleSelectionState[rule.name];
                    bool newSelectionState = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                    if (newSelectionState != isSelected)
                    {
                        ruleSelectionState[rule.name] = newSelectionState;
                        InitializeGameMode();
                    }

                    EditorGUILayout.ObjectField(rule, typeof(BaseBonusRule), false);

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
        }

        private void InitializeGameMode()
        {
            List<BaseBonusRule> selectedBonusRules = new List<BaseBonusRule>();
            foreach (var rule in BaseBonusRulesTemplate)
            {
                if (ruleSelectionState[rule.name])
                {
                    selectedBonusRules.Add(rule);
                }
            }
            gameMode.Initialize(selectedBonusRules);
        }
    }
}
