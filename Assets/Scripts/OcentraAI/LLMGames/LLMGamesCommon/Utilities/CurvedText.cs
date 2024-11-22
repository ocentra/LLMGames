using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.Utilities
{
    public class CurvedText : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] private  float charSpacingMultiplier = 1f;

        [SerializeField] private  float curveRadius = 75f;

        [SerializeField] [Range(1, 20)] private  int maxCharacterLength = 10;

        [SerializeField] [TextArea] private string playerName = "player";

        public TextMeshPro PlayerNameText;

        [SerializeField] private  float yOffset = -75f;


        private void Awake()
        {
            Init();
        }

        private void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            if (PlayerNameText == null)
            {
                PlayerNameText = GetComponent<TextMeshPro>();
            }

            if (PlayerNameText != null)
            {
                SetPlayerName(playerName); // Ensure name is shortened if needed
            }
        }

        public void SetPlayerName(string player)
        {
            // Shorten the player name if it exceeds the max length
            if (player.Length > maxCharacterLength)
            {
                player = player.Substring(0, maxCharacterLength) + "..";
            }

            playerName = player;

            if (PlayerNameText != null)
            {
                PlayerNameText.text = playerName;
            }

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ApplyCurvedTextWithDelay());
            }
        }

        private IEnumerator ApplyCurvedTextWithDelay()
        {
            // Wait for the end of the frame to ensure the text mesh is updated
            yield return new WaitForEndOfFrame();

            ApplyCurvedText();
        }

        [Button("Apply Curved Text")]
        private void ApplyCurvedText()
        {
            if (PlayerNameText == null)
            {
                PlayerNameText = GetComponent<TextMeshPro>();
            }

            if (PlayerNameText == null)
            {
                Debug.LogError("No TextMeshProUGUI component found.");
                return;
            }

            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            PlayerNameText.text = playerName;

            PlayerNameText.ForceMeshUpdate();
            TMP_TextInfo textInfo = PlayerNameText.textInfo;

            if (textInfo == null || textInfo.characterCount == 0)
            {
                return;
            }

            if (textInfo.meshInfo == null || textInfo.meshInfo.Length == 0)
            {
                return;
            }

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                {
                    continue;
                }

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                if (textInfo.meshInfo[materialIndex].vertices == null ||
                    textInfo.meshInfo[materialIndex].vertices.Length < vertexIndex + 4)
                {
                    continue;
                }

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 charMidBaseline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
                float charPos = (float)i / (textInfo.characterCount - 1);

                float circumference = 2 * Mathf.PI * curveRadius;
                float baseCharWidth = circumference / textInfo.characterCount;
                float charAngleStep = Mathf.Clamp(baseCharWidth * charSpacingMultiplier / circumference * 360f, -180f,
                    180f);

                float angle = Mathf.Clamp((charPos - 0.5f) * textInfo.characterCount * charAngleStep, -180f, 180f);

                Vector3 curvedPosition = new Vector3(
                    Mathf.Sin(Mathf.Deg2Rad * angle) * curveRadius,
                    (Mathf.Cos(Mathf.Deg2Rad * angle) * curveRadius) + yOffset,
                    0
                );

                Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Clamp(-angle, -180f, 180f));

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] =
                        (rotation * (vertices[vertexIndex + j] - charMidBaseline)) + curvedPosition;
                }
            }

            PlayerNameText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}