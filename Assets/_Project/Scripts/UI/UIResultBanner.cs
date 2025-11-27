using TMPro;
using UnityEngine;

namespace Blackjack.Final
{
    public class UIResultBanner : MonoBehaviour
    {
        [SerializeField] private TMP_Text resultText;

        private void OnEnable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnResultChanged += HandleResultChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnResultChanged -= HandleResultChanged;
            }
        }

        private void HandleResultChanged(string message)
        {
            if (resultText != null)
            {
                resultText.text = message;
            }
        }
    }
}
