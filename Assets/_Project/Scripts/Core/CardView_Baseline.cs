using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blackjack.Baseline
{
    public class CardView_Baseline : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;

        private CardData _data;
        private bool _isFaceDown;

        public void SetCard(CardData data, bool faceDown)
        {
            _data = data;
            _isFaceDown = faceDown;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (labelText == null) return;

            if (_isFaceDown)
            {
                labelText.text = "??";
            }
            else
            {
                // example "10♥"
                labelText.text = _data.ToString();
            }

        }
    }
}
