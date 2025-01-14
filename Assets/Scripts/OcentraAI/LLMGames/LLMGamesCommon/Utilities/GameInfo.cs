using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Manager.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.Commons
{
    [CreateAssetMenu(fileName = nameof(GameInfo), menuName = "GameMode/GameInfo")]
    public class GameInfo : SerializedScriptableObject, ISaveScriptable
    {
        [OdinSerialize, InlineEditor] private List<GameInfoPair> infos = new List<GameInfoPair>();

        private GameGenre[] gameGenres;

        private bool isSavePending;

        private Object[] childObjects;

        protected virtual void OnEnable()
        {
            gameGenres = GameGenre.GetAll();
            childObjects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));
            if (childObjects.Length != gameGenres.Length)
            {
                PopulateInfos();
            }

            infos.Clear();
            foreach (Object obj in childObjects)
            {
                if (obj is GameInfoPair pair)
                {
                    pair.OnValueChanged += HandleGameInfoPairValueChanged;
                    pair.name = pair.GameGenre.Name;
                    infos.Add(pair);
                }
            }

        }

        private async void HandleGameInfoPairValueChanged(GameInfoPair obj)
        {
            await EditorSaveManager.RequestSave(obj);
            await EditorSaveManager.RequestSave(this);
            await UniTask.Yield();
        }


        private string GetDefaultGameInfo(GameGenre gameGenre)
        {

            string infotext = "";


            if (gameGenre == GameGenre.CardGames)
            {
                infotext =
                    $"<size=16><color=#FFD700>CardGames</color></size>\n" +
                    $"<size=14><color=#FFFFFF>" +
                    "A collection of exciting card games to play with LLM or your friends in a multiplayer setup. " +
                    "Advance your skills in games like Three Card Brag, Poker, and more. " +
                    "In non-RMG (non-real money games), you can play alone or with AI models and friends, " +
                    "leveraging AI to analyze gameplay and performance after each match. " +
                    "For RMG, AI provides post-game analysis to help you improve. " +
                    "All games are trackable on the Solana Blockchain, with a link to review your matches available after each game." +
                    "</color></size>";
            }
            else if (gameGenre == GameGenre.WordGames)
            {
                infotext =
                    $"<size=16><color=#FFD700>WordGames</color></size>\n" +
                    $"<size=14><color=#FFFFFF>" +
                    "Challenge your vocabulary and wit in a variety of word games designed for solo play or with LLMs and friends. " +
                    "These games help improve your language skills while having fun. " +
                    "Whether you're practicing alone or competing in multiplayer, AI models provide feedback to enhance your abilities. " +
                    "Results and performance analytics are recorded on the Solana Blockchain, ensuring transparency and secure tracking." +
                    "</color></size>";
            }
            else if (gameGenre == GameGenre.MindFul)
            {
                infotext =
                    $"<size=16><color=#FFD700>MindFul & Emotional</color></size>\n" +
                    $"<size=14><color=#FFFFFF>" +
                    "Immerse yourself in games that focus on mindfulness and emotional well-being. " +
                    "Play alone or with LLMs that guide you through exercises to enhance self-awareness, reduce stress, and build emotional resilience. " +
                    "Perfect for relaxation or developing a balanced mindset. " +
                    "Game progress and sessions are securely recorded on the Solana Blockchain for reflection and tracking." +
                    "</color></size>";
            }
            else if (gameGenre == GameGenre.Voice)
            {
                infotext =
                    $"<size=16><color=#FFD700>VoiceControlled</color></size>\n" +
                    $"<size=14><color=#FFFFFF>" +
                    "Engage in innovative voice-controlled games where you interact with LLMs or friends through voice commands. " +
                    "Perfect for hands-free gaming, these experiences challenge your quick thinking and verbal skills. " +
                    "Whether competing or collaborating, AI ensures a seamless and dynamic experience. " +
                    "Results are logged on the Solana Blockchain for review and sharing." +
                    "</color></size>";
            }
            else
            {
                infotext =
                    $"<size=16><color=#FFD700>All Games</color></size>\n" +
                    $"<size=14><color=#FFFFFF>" +
                    "Select this option if you’re exploring the platform without committing to a specific game category. " +
                    "Discover the range of available games, from card and word games to mindfulness exercises and voice-controlled experiences. " +
                    "AI models are ready to assist you in choosing your next adventure. " +
                    "All activities are tracked on the Solana Blockchain for your convenience." +
                    "</color></size>";
            }

            return infotext;

        }

        public bool TryGetValue(GameGenre gameGenre, out Info info)
        {


            info = null;
            foreach (GameInfoPair gameInfoPair in infos)
            {
                if (gameInfoPair.GameGenre == gameGenre)
                {
                    info = gameInfoPair.Info;
                    return true;
                }
            }

            return false;
        }

        [Button]
        private async void PopulateInfos()
        {
#if UNITY_EDITOR
            List<Object> objectsToRemove = new List<Object>();


            if (infos == null)
            {
                infos = new List<GameInfoPair>();
            }

            Dictionary<GameGenre, GameInfoPair> existingPairs = new Dictionary<GameGenre, GameInfoPair>();

            foreach (Object obj in childObjects)
            {
                if (obj is GameInfoPair childPair)
                {
                    if (!gameGenres.Contains(childPair.GameGenre))
                    {
                        objectsToRemove.Add(childPair);
                        break;
                    }
                    if (existingPairs.ContainsKey(childPair.GameGenre) )
                    {
                        objectsToRemove.Add(childPair);
                    }
                    else
                    {
                        existingPairs[childPair.GameGenre] = childPair;

                    }
                }
            }

            foreach (GameGenre gameGenre in gameGenres)
            {
                string defaultGameInfo = GetDefaultGameInfo(gameGenre);

                if (!existingPairs.ContainsKey(gameGenre))
                {
                    Info newInfo = new Info(defaultGameInfo);
                    GameInfoPair newPair = GameInfoPair.Create(this, gameGenre, newInfo);
                    existingPairs[gameGenre] = newPair;
                    await EditorSaveManager.RequestSave(newPair);
                }
                else 
                {
                    if ( (string.IsNullOrEmpty(existingPairs[gameGenre].Info.Value)))
                    {
                        existingPairs[gameGenre].Info.Value = defaultGameInfo;
                        await EditorSaveManager.RequestSave(existingPairs[gameGenre]);
                    }

                }
            }

            foreach (Object obj in objectsToRemove)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(obj);
                DestroyImmediate(obj, true);
            }

            await EditorSaveManager.RequestSave(this);


#endif
        }



        [Button]
        public virtual async void SaveChanges()
        {
            foreach (GameInfoPair gameInfo in infos)
            {
               await EditorSaveManager.RequestSave(gameInfo);
            }
            await EditorSaveManager.RequestSave(this);
        }






    }
}
