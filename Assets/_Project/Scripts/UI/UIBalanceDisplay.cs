using TMPro;
using UnityEngine;

namespace Blackjack.Final
{
    public class UIBalanceDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text balanceText;

        private void OnEnable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnBalanceChanged += HandleBalanceChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnBalanceChanged -= HandleBalanceChanged;
            }
        }

        private void HandleBalanceChanged(int newBalance)
        {
            if (balanceText != null)
            {
                balanceText.text = $"Balance: {newBalance}";
            }
        }
    }
}
