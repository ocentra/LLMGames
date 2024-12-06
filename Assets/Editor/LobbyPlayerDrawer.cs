using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace OcentraAI.LLMGames.Drawers
{
   
    public class LobbyPlayerDrawer : OdinValueDrawer<Player>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Player player = this.ValueEntry.SmartValue;

            if (player == null)
            {
                SirenixEditorGUI.WarningMessageBox("Player is null");
                return;
            }

            SirenixEditorGUI.BeginBox("Player Details");

            SirenixEditorGUI.Title("Player Information", "", TextAlignment.Center, true);
            GUILayout.Label($"ID: {player.Id ?? "Not Set"}");
            GUILayout.Label($"Profile: {player.Profile?.ToString() ?? "No Profile"}");
            GUILayout.Label($"Connection Info: {player.ConnectionInfo ?? "No Connection Info"}");
            GUILayout.Label($"Allocation ID: {player.AllocationId ?? "No Allocation ID"}");
            GUILayout.Label($"Joined: {(player.Joined != default ? player.Joined.ToString("g") : "Not Set")}");
            GUILayout.Label($"Last Updated: {(player.LastUpdated != default ? player.LastUpdated.ToString("g") : "Not Set")}");

            if (player.Data != null && player.Data.Count > 0)
            {
                SirenixEditorGUI.Title("Player Data", "", TextAlignment.Center, false);
                foreach (KeyValuePair<string, PlayerDataObject> entry in player.Data)
                {
                    GUILayout.Label($"{entry.Key}: {entry.Value?.Value ?? "No Data"}");
                }
            }
            else
            {
                SirenixEditorGUI.WarningMessageBox("No custom data available for this player.");
            }

            SirenixEditorGUI.EndBox();
        }
    }
}