using OcentraAI.LLMGames.GameModes.Rules;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [CustomEditor(typeof(GameMode), true)]
    public class GameModeEditor : OdinEditor
    {
        [OdinSerialize] public GameMode GameMode { get; set; }
        [OdinSerialize] public Vector2 ScrollPosition { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            GameMode = (GameMode)target;
            LoadRuleTemplates();
            SyncSelectedRules();
            ApplySorting();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        private void LoadRuleTemplates(bool loadFresh = false)
        {
            if (loadFresh)
            {
                GameMode.BaseBonusRulesTemplate = new Dictionary<BaseBonusRule, CustomRuleState>();
            }

            Dictionary<BaseBonusRule, CustomRuleState> loadedTemplates =
                new Dictionary<BaseBonusRule, CustomRuleState>();
            if (!string.IsNullOrEmpty(GameMode.RulesTemplatePath))
            {
                string[] assetGuids = AssetDatabase.FindAssets("t:BaseBonusRule", new[] {GameMode.RulesTemplatePath});
                foreach (string guid in assetGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    BaseBonusRule rule = AssetDatabase.LoadAssetAtPath<BaseBonusRule>(assetPath);
                    if (rule != null && GameMode.NumberOfCards >= rule.MinNumberOfCard &&
                        !AssetDatabase.IsSubAsset(rule))
                    {
                        if (GameMode.BaseBonusRulesTemplate != null &&
                            GameMode.BaseBonusRulesTemplate.TryGetValue(rule,
                                out CustomRuleState existingCustomRuleState))
                        {
                            loadedTemplates[rule] = existingCustomRuleState;
                        }
                        else
                        {
                            loadedTemplates[rule] = new CustomRuleState(rule);
                        }
                    }
                }
            }

            GameMode.BaseBonusRulesTemplate = loadedTemplates;
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
                foreach (KeyValuePair<BaseBonusRule, CustomRuleState> ruleStatePair in GameMode.BaseBonusRulesTemplate)
                {
                    BaseBonusRule rule = ruleStatePair.Key;
                    CustomRuleState customRuleState = ruleStatePair.Value;
                    customRuleState.IsSelected = GameMode.BonusRules.Any(r => r.name == rule.name);

                    foreach (BaseBonusRule baseBonusRule in GameMode.BonusRules)
                    {
                        if (rule.RuleName == baseBonusRule.RuleName)
                        {
                            baseBonusRule.UpdateRule(rule.BonusValue, rule.Priority);
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            GameMode.Foldout = EditorGUILayout.Foldout(GameMode.Foldout, "Additional Settings");
            if (GameMode.Foldout)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.BeginHorizontal();
                GameMode.RulesTemplatePath =
                    EditorGUILayout.TextField("Path to Rule Templates", GameMode.RulesTemplatePath);
                if (GUILayout.Button("Browse", GUILayout.Width(70)))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Rule Templates Folder",
                        GameMode.RulesTemplatePath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        GameMode.RulesTemplatePath = GetRelativePath(selectedPath);
                        LoadRuleTemplates();
                        SyncSelectedRules();
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Available Rule Templates", EditorStyles.boldLabel, GUILayout.Width(170));

                if (GUILayout.Button("Refresh Rules", GUILayout.Width(100)))
                {
                    LoadRuleTemplates(true);
                    SyncSelectedRules();
                    ApplySorting();
                }

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
                GameMode.Height = EditorGUILayout.Slider(GameMode.Height, 50, 500);
                EditorGUILayout.Space(10);
                EditorGUILayout.EndHorizontal();

                DrawSortingToolbar();

                if (GameMode.BaseBonusRulesTemplate.Count > 0)
                {
                    ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(GameMode.Height),
                        GUILayout.ExpandHeight(true));

                    foreach (KeyValuePair<BaseBonusRule, CustomRuleState> ruleStatePair in GameMode
                                 .BaseBonusRulesTemplate)
                    {
                        BaseBonusRule rule = ruleStatePair.Key;
                        CustomRuleState customRuleState = ruleStatePair.Value;

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        bool isSelected = customRuleState.IsSelected;
                        bool newSelectionState = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));

                        if (newSelectionState != isSelected)
                        {
                            customRuleState.IsSelected = newSelectionState;
                            TryInitialize();
                        }

                        EditorGUILayout.ObjectField(rule, typeof(BaseBonusRule), false, GUILayout.Width(300));
                        EditorGUILayout.LabelField(nameof(customRuleState.Priority), GUILayout.Width(50));
                        customRuleState.Priority = EditorGUILayout.Slider(customRuleState.Priority, 0, 100);
                        EditorGUILayout.LabelField(nameof(customRuleState.BonusValue), GUILayout.Width(75));
                        customRuleState.BonusValue = EditorGUILayout.Slider(customRuleState.BonusValue, 0, 200);

                        EditorGUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            rule.UpdateRule((int)customRuleState.BonusValue, (int)customRuleState.Priority);
                            SyncSelectedRules();
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.HelpBox("No rule templates found in the selected folder.", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
            }

            base.OnInspectorGUI();

            if (GUILayout.Button(nameof(TryInitialize)))
            {
                TryInitialize();
            }
        }

        private void DrawSortingToolbar()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (DrawSortButton("Sort by Name", SortingCriteria.NameAscending, SortingCriteria.NameDescending))
            {
                ToggleSortingOrder(SortingCriteria.NameAscending, SortingCriteria.NameDescending);
                ApplySorting();
            }

            if (DrawSortButton("Sort by Priority", SortingCriteria.PriorityAscending,
                    SortingCriteria.PriorityDescending))
            {
                ToggleSortingOrder(SortingCriteria.PriorityAscending, SortingCriteria.PriorityDescending);
                ApplySorting();
            }

            if (DrawSortButton("Sort by Bonus Value", SortingCriteria.BonusValueAscending,
                    SortingCriteria.BonusValueDescending))
            {
                ToggleSortingOrder(SortingCriteria.BonusValueAscending, SortingCriteria.BonusValueDescending);
                ApplySorting();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        private bool DrawSortButton(string label, SortingCriteria ascendingCriteria, SortingCriteria descendingCriteria)
        {
            GUIStyle style = GetCustomButtonStyle(ascendingCriteria, descendingCriteria);
            return GUILayout.Button(label, style);
        }

        private void ToggleSortingOrder(SortingCriteria ascendingCriteria, SortingCriteria descendingCriteria)
        {
            if (GameMode.CurrentSortingCriteria == ascendingCriteria)
            {
                GameMode.CurrentSortingCriteria = descendingCriteria;
            }
            else if (GameMode.CurrentSortingCriteria == descendingCriteria)
            {
                GameMode.CurrentSortingCriteria = ascendingCriteria;
            }
            else
            {
                GameMode.CurrentSortingCriteria = ascendingCriteria;
            }
        }

        private void ApplySorting()
        {
            switch (GameMode.CurrentSortingCriteria)
            {
                case SortingCriteria.NameAscending:
                    GameMode.BaseBonusRulesTemplate = GameMode.BaseBonusRulesTemplate
                        .OrderBy(pair => pair.Key.name)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                    break;
                case SortingCriteria.NameDescending:
                    GameMode.BaseBonusRulesTemplate = GameMode.BaseBonusRulesTemplate
                        .OrderByDescending(pair => pair.Key.name)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                    break;
                case SortingCriteria.PriorityAscending:
                    GameMode.BaseBonusRulesTemplate = GameMode.BaseBonusRulesTemplate
                        .OrderBy(pair => pair.Value.Priority)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                    break;
                case SortingCriteria.PriorityDescending:
                    GameMode.BaseBonusRulesTemplate = GameMode.BaseBonusRulesTemplate
                        .OrderByDescending(pair => pair.Value.Priority)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                    break;
                case SortingCriteria.BonusValueAscending:
                    GameMode.BaseBonusRulesTemplate = GameMode.BaseBonusRulesTemplate
                        .OrderBy(pair => pair.Value.BonusValue)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                    break;
                case SortingCriteria.BonusValueDescending:
                    GameMode.BaseBonusRulesTemplate = GameMode.BaseBonusRulesTemplate
                        .OrderByDescending(pair => pair.Value.BonusValue)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                    break;
            }
        }

        private void SelectAllRules()
        {
            foreach (KeyValuePair<BaseBonusRule, CustomRuleState> ruleStatePair in GameMode.BaseBonusRulesTemplate)
            {
                ruleStatePair.Value.IsSelected = true;
            }

            TryInitialize();
        }

        private void DeselectAllRules()
        {
            foreach (KeyValuePair<BaseBonusRule, CustomRuleState> ruleStatePair in GameMode.BaseBonusRulesTemplate)
            {
                ruleStatePair.Value.IsSelected = false;
            }

            TryInitialize();
        }

        private bool TryInitialize()
        {
            return GameMode.TryInitialize();
        }

        private GUIStyle GetCustomButtonStyle(SortingCriteria ascendingCriteria, SortingCriteria descendingCriteria)
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton)
            {
                normal = {background = MakeTex(2, 2, Color.clear), textColor = Color.white},
                hover = {background = MakeTex(2, 2, Color.gray), textColor = Color.white}
            };

            if (GameMode.CurrentSortingCriteria == ascendingCriteria ||
                GameMode.CurrentSortingCriteria == descendingCriteria)
            {
                style.normal.textColor = Color.green;
            }
            else
            {
                style.normal.textColor = Color.white;
            }

            return style;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}