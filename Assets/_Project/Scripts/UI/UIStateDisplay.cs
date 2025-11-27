using TMPro;
using UnityEngine;

namespace Blackjack.Final
{
    public class UIStateDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text stateText;

        private void OnEnable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager_Final.Instance != null)
            {
                GameManager_Final.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(BlackjackState newState)
        {
            if (stateText != null)
            {
                stateText.text = $"State: {newState}";
            }
        }
    }
}
