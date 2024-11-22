
#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.EditorWindow
{
    public class ConnectionStatusWindow : UnityEditor.EditorWindow
    {
        private static ConnectionStatusWindow window;
        public static ConnectionStatusWindow Window
        {
            get
            {
                if (window == null)
                {
                    window = GetWindow<ConnectionStatusWindow>("Connection Status");
                }
                return window;
            }
        }
        private string StatusMessage { get; set; } = "Connecting...";
        private string Role { get; set; } = "N/A";
        private int CurrentPlayers { get; set; } = 0;
        private int MaxPlayers { get; set; } = 0;
        private bool IsReady { get; set; } = false;

        //public static void ShowWindow(string message, string initialRole, int currentPlayers, int maxPlayers)
        //{
        //    Window.StatusMessage = message;
        //    Window.Role = initialRole;
        //    Window.MaxPlayers = maxPlayers;
        //    Window.CurrentPlayers = currentPlayers;
        //    Window.IsReady = false;
        //    Window.Show();
        //}

        //public void UpdateStatus(string message, int currentPlayers, bool isReady)
        //{

        //    CurrentPlayers = currentPlayers;
        //    IsReady = isReady;
        //    StatusMessage += $" {Environment.NewLine}{message} Players: {CurrentPlayers}/{MaxPlayers}  IsReady {IsReady}";
        //    Repaint();
        //}


        //public void ClearAndClose()
        //{

        //    CurrentPlayers = 0;
        //    IsReady = false;
        //    StatusMessage = "Connecting...";
        //    Close();
        //    Repaint();
            
        //}


        private void OnGUI()
        {
            GUILayout.Label($"Role: {Role}", EditorStyles.boldLabel);
            GUILayout.Label($"Players: {CurrentPlayers}/{MaxPlayers}", EditorStyles.label);
            GUILayout.Label($"Status: {StatusMessage}", EditorStyles.wordWrappedLabel);

            if (IsReady)
            {
                GUILayout.Label("All players are connected!", EditorStyles.boldLabel);
            }
        }
    }
}

#endif