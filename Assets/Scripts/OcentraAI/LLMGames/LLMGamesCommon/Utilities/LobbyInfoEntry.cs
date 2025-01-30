using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LobbyInfoEntry: ILobbyInfoEntry
{
    [HorizontalGroup("Row", Width = 150), HideLabel,ReadOnly]
    public string Key;

    [HorizontalGroup("Row", Width = 150), HideLabel, HideIf(nameof(IsBoolean)), ReadOnly]
    public string Value;

    [HorizontalGroup("Row", Width = 50), HideLabel]
    public string Icon;

    [HorizontalGroup("Row", Width = 50), ShowIf(nameof(IsBoolean)), ReadOnly]
    public bool State;

    [HideInInspector]
    public bool IsBoolean;


    public LobbyInfoEntry(string key, string icon, bool state = false, string value = "")
    {
        Key = key;
        Value = value;
        Icon = icon;
        State = state;
        IsBoolean = string.IsNullOrEmpty(value);
    }

    public (string key, string value, string icon) GetKeyValueTuple()
    {
        string formattedValue;

        if (IsBoolean)
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(State ? Color.green : Color.red);
            string stateSymbol = State ? "✔" : "❌";
            formattedValue = $"<color=#{colorHex}>{stateSymbol}</color>";
        }
        else
        {
            formattedValue = Value;
        }

        return (Key, formattedValue, Icon);
    }


}