using System.Collections.Generic;
using UnityEngine;

namespace Blackjack.Final
{
    public class CardPool_Final : MonoBehaviour
    {
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private int initialSize = 52;

        private readonly Queue<GameObject> pool = new Queue<GameObject>();

        private void Awake()
        {
            Prewarm();
        }

        private void Prewarm()
        {
            pool.Clear();

            for (int i = 0; i < initialSize; i++)
            {
                GameObject card = Instantiate(cardPrefab, transform);
                card.SetActive(false);
                pool.Enqueue(card);
            }
        }

        public GameObject GetCard(RectTransform parent)
        {
            GameObject card;

            if (pool.Count > 0)
            {
                card = pool.Dequeue();
            }
            else
            {
                // Fallback: grow pool if needed
                card = Instantiate(cardPrefab, transform);
            }

            card.transform.SetParent(parent, false);
            card.SetActive(true);

            // Reset basic transform
            var rt = card.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.identity;
            }

            return card;
        }

        public void ReturnCard(GameObject card)
        {
            if (card == null) return;

            card.SetActive(false);
            card.transform.SetParent(transform, false);
            pool.Enqueue(card);
        }
    }
}
