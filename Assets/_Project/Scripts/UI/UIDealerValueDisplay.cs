using TMPro;
using UnityEngine;

namespace Blackjack.Final
{
    public class UIDealerValueDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text dealerValueText;

        private void OnEnable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnDealerValueChanged += HandleDealerValueChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnDealerValueChanged -= HandleDealerValueChanged;
            }
        }

        private void HandleDealerValueChanged(int? value)
        {
            if (dealerValueText == null) return;

            if (value.HasValue)
            {
                dealerValueText.text = $"Value: {value.Value}";
            }
            else
            {
                dealerValueText.text = "Value: ?";
            }
        }
    }
}
