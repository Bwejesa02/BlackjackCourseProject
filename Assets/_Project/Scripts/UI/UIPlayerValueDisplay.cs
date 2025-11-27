using TMPro;
using UnityEngine;

namespace Blackjack.Final
{
    public class UIPlayerValueDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerValueText;

        private void OnEnable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnPlayerValueChanged += HandlePlayerValueChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnPlayerValueChanged -= HandlePlayerValueChanged;
            }
        }

        private void HandlePlayerValueChanged(int value)
        {
            if (playerValueText != null)
            {
                
                playerValueText.text = $"Value: {value}";
            }
        }
    }
}
