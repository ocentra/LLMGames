#if UNITY_EDITOR

using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ParrelSync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;

namespace Assets.Plugins.ParrelSync.Editor
{
    [Serializable]
    public class ArgumentData
    {
        public List<string> Keys = new List<string>();
        public List<string> Values = new List<string>();

        public static ArgumentData FromDictionary(Dictionary<string, string> dictionary)
        {
            ArgumentData data = new ArgumentData();
            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                data.Keys.Add(kvp.Key);
                data.Values.Add(kvp.Value);
            }
            return data;
        }

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int i = 0; i < Keys.Count; i++)
            {
                dictionary[Keys[i]] = Values[i];
            }
            return dictionary;
        }
    }

    public static class ClonesManagerExtensions
    {
        private static readonly object fileLock = new object();

        private static async UniTask<Dictionary<string, string>> LoadArgumentsAsync(string filePath)
        {
            lock (fileLock)
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning("Argument file does not exist. Returning an empty dictionary.");
                    return new Dictionary<string, string>();
                }
            }

            try
            {
                string jsonContent;
                lock (fileLock)
                {
                    jsonContent = File.ReadAllText(filePath);
                }
                ArgumentData argumentData = JsonUtility.FromJson<ArgumentData>(jsonContent);
                await UniTask.Yield();
                return argumentData?.ToDictionary() ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load arguments: {ex.Message} {ex.StackTrace}");
                return new Dictionary<string, string>();
            }

           
        }

        private static async UniTask<bool> SaveArgumentsAsync(string filePath, Dictionary<string, string> arguments)
        {
            try
            {
                ArgumentData argumentData = ArgumentData.FromDictionary(arguments);
                string jsonContent = JsonUtility.ToJson(argumentData, prettyPrint: true);

                lock (fileLock)
                {
                    using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        writer.Write(jsonContent);
                    }
                }

                AssetDatabase.Refresh();
                await UniTask.Yield();
                return true;
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"IO error while setting argument: {ioEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error while setting argument: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }

        public static async UniTask<bool> TrySetLobbyDataAsync(string filePath, Lobby lobbyData)
        {
            try
            {
                string lobbyJson = JsonConvert.SerializeObject(lobbyData, Formatting.Indented);

                Dictionary<string, string> arguments = await LoadArgumentsAsync(filePath);

                // Remove the old lobby data entry if it exists
                lock (fileLock)
                {
                    if (arguments.ContainsKey(nameof(Lobby)))
                    {
                        arguments.Remove(nameof(Lobby));
                    }

                    arguments[nameof(Lobby)] = lobbyJson;
                }

                bool saveSuccess = await SaveArgumentsAsync(filePath, arguments);

                if (saveSuccess)
                {
                    (bool success, Lobby lobbyData) result = await TryGetLobbyDataAsync(filePath);
                    if (result.success)
                    {
                        if (AreLobbiesEqual(lobbyData, result.lobbyData))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to sync lobby data: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }


        public static bool AreLobbiesEqual(Lobby lobby1, Lobby lobby2)
        {
            if (lobby1 == null || lobby2 == null)
                return false;

            // Compare basic properties
            if (lobby1.Id != lobby2.Id ||
                lobby1.Name != lobby2.Name ||
                lobby1.HostId != lobby2.HostId ||
                lobby1.MaxPlayers != lobby2.MaxPlayers)
            {
                return false;
            }

            // Compare custom data if necessary
            if (lobby1.Data != null && lobby2.Data != null)
            {
                if (lobby1.Data.Count != lobby2.Data.Count) return false;

                foreach (string key in lobby1.Data.Keys)
                {
                    if (!lobby2.Data.ContainsKey(key) || lobby1.Data[key].Value != lobby2.Data[key].Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public static async UniTask<(bool success, Lobby lobbyData)> TryGetLobbyDataAsync(string filePath, int maxRetries = 600, int initialDelaySeconds = 1, int maxDelaySeconds = 600)
        {
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    Dictionary<string, string> arguments = await LoadArgumentsAsync(filePath);

                    if (arguments.TryGetValue(nameof(Lobby), out string lobbyJson) && !string.IsNullOrEmpty(lobbyJson))
                    {
                        Lobby lobbyData = JsonConvert.DeserializeObject<Lobby>(lobbyJson);
                        return (true, lobbyData);
                    }

                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.Log($"Attempt {attempt}/{maxRetries} failed. Retrying in {initialDelaySeconds} seconds...");
                        await UniTask.Delay(TimeSpan.FromSeconds(initialDelaySeconds));
                        initialDelaySeconds = Math.Min(initialDelaySeconds, maxDelaySeconds);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load lobby data on attempt {attempt + 1}: {ex.Message} {ex.StackTrace}");

                    attempt++;
                    if (attempt < maxRetries)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(initialDelaySeconds));
                        initialDelaySeconds = Math.Min(initialDelaySeconds, maxDelaySeconds);
                    }
                    else
                    {
                        return (false, null);
                    }
                }
            }

            return (false, null);
        }

        public static async UniTask<bool> ClearArgumentFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    ArgumentData emptyData = new ArgumentData();
                    string jsonContent = JsonUtility.ToJson(emptyData, prettyPrint: true);
                    lock (fileLock)
                    {
                        File.WriteAllText(filePath, jsonContent);
                    }
                    AssetDatabase.Refresh();
                    await UniTask.Yield();
                    return true;
                }
                else
                {
                    Debug.LogWarning($"File not found: {filePath}. No action taken.");
                    return false;
                }
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"IO error while clearing the argument file: {ioEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error while clearing the argument file: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }

        public static async UniTask<bool> ClearLobbyDataFromFile(string filePath)
        {
            if (!ClonesManager.IsClone())
            {
                try
                {
                    Dictionary<string, string> arguments = await LoadArgumentsAsync(filePath);

                    lock (fileLock)
                    {
                        if (arguments.ContainsKey(nameof(Lobby)))
                        {
                            arguments.Remove(nameof(Lobby));
                        }
                    }

                    return await SaveArgumentsAsync(filePath, arguments);
                }
                catch (IOException ioEx)
                {
                    Debug.LogError($"IO error while removing the key: {ioEx.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected error while removing the key: {ex.Message} {ex.StackTrace}");
                    return false;
                }
            }

            return false;
        }
    }
}
#endif
