using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace OcentraAI.LLMGames.Networking.Manager
{
    public class MultiplayManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField ipAddressInputField;
        [SerializeField] private TMP_InputField portInputField;


        public void JoinToServer()
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // Set connection data for the client based on user input
            transport.SetConnectionData(ipAddressInputField.text, ushort.Parse(portInputField.text));

            // Start the client to connect to the server
            NetworkManager.Singleton.StartClient();
        }
    }
}