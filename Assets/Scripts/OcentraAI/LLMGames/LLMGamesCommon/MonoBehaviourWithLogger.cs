using Sirenix.OdinInspector;
using UnityEngine;

namespace OcentraAI.LLMGames.Commons
{
    public class MonoBehaviourWithLogger : MonoBehaviour
    {
        [Header("File Logging Settings")]
        [ShowInInspector] public bool ToEditor { get; set; } = true;
        [ShowInInspector] public bool ToFile { get; set; }



    }
}