using OcentraAI.LLMGames.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class PlayerItemUI : MonoBehaviour
    {
       
        [Required][SerializeField] private TextMeshProUGUI playerName;
        [Required][SerializeField] private TextMeshProUGUI userOwner;
        [Required][SerializeField] public Player Player { get; private set; }

        void OnValidate()
        {
            Init();
        }
        void Awake()
        {
            Init();
        }
        public void Init()
        {
            transform.FindChildWithComponent(ref playerName, nameof(playerName));
            transform.FindChildWithComponent(ref userOwner, nameof(userOwner));
        }
        public void InitLobby(Player player, string role)
        {
            Player = player;

            PlayerDataObject playerDataObject = player.Data["Name"];
            if (playerDataObject != null)
            {
                string playerNameText = playerDataObject.Value;
           
                if (playerName != null)
                {
                    playerName.text = playerNameText;
                }
            }

            if (userOwner != null)
            {
                userOwner.text = role;
            }


        }
    }
}