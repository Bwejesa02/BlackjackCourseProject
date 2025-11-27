using Blackjack.Baseline;  // For CardData and CardView_Baseline
using UnityEngine;

namespace Blackjack.Final
{
    public class CardViewFactory_Final : MonoBehaviour
    {
        [SerializeField] private CardPool_Final cardPool;

        public GameObject CreateCard(CardData data, RectTransform parent, bool faceDown)
        {
            var cardObj = cardPool.GetCard(parent);
            var view = cardObj.GetComponent<CardView_Baseline>();
            if (view != null)
            {
                view.SetCard(data, faceDown);
            }
            return cardObj;
        }

        public void RecycleCard(GameObject card)
        {
            cardPool.ReturnCard(card);
        }
    }
}
