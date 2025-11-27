using Blackjack.Baseline;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blackjack.Final
{
    public enum BlackjackState
    {
        Betting,
        Dealing,
        PlayerTurn,
        DealerTurn,
        RoundEnd
    }

    public class GameManager_Final : MonoBehaviour
    {
        public static GameManager_Final Instance { get; private set; }

        [Header("Card Factory & Parents")]
        [SerializeField] private CardViewFactory_Final cardFactory;
        [SerializeField] private RectTransform playerCardsParent;
        [SerializeField] private RectTransform dealerCardsParent;


        [Header("Buttons (for interactable control)")]
        [SerializeField] private Button dealButton;
        [SerializeField] private Button hitButton;
        [SerializeField] private Button standButton;
        [SerializeField] private Button nextRoundButton;

        [Header("Game Settings")]
        [SerializeField] private int startingBalance = 100;
        [SerializeField] private int fixedBetAmount = 10;
        [SerializeField] private int reshuffleThreshold = 15;

        // events
        public event Action<BlackjackState> OnStateChanged;
        public event Action<int> OnBalanceChanged;
        public event Action<int> OnPlayerValueChanged;
        public event Action<int?> OnDealerValueChanged;
        public event Action<string> OnResultChanged;

        
        private BlackjackState _currentState;

        private List<CardData> deck = new List<CardData>();
        private List<CardData> playerHand = new List<CardData>();
        private List<CardData> dealerHand = new List<CardData>();

        private List<GameObject> playerCardViews = new List<GameObject>();
        private List<GameObject> dealerCardViews = new List<GameObject>();

        private int currentBalance;
        private bool roundActive;
        private bool playerTurnActive;
        private bool dealerHiddenCardFaceDown;

        private CardView_Baseline dealerHiddenCardView;
        private int dealerHiddenCardIndex = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            currentBalance = startingBalance;
            InitDeck();
            ShuffleDeck();

            ClearTableVisuals();

            SetState(BlackjackState.Betting);
            RaiseBalanceChanged();
            RaisePlayerValueChanged(nullValue: true);
            RaiseDealerValueChanged(null);

            if (OnResultChanged != null) OnResultChanged(string.Empty);

            // states
            if (dealButton != null) dealButton.interactable = true;
            if (hitButton != null) hitButton.interactable = false;
            if (standButton != null) standButton.interactable = false;
            if (nextRoundButton != null) nextRoundButton.interactable = false;
        }


        private void SetState(BlackjackState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void RaiseBalanceChanged()
        {
            OnBalanceChanged?.Invoke(currentBalance);
        }

        private void RaisePlayerValueChanged(bool nullValue)
        {
            if (nullValue)
            {
                OnPlayerValueChanged?.Invoke(0);
            }
            else
            {
                int total = CalculateHandTotal(playerHand);
                OnPlayerValueChanged?.Invoke(total);
            }
        }

        private void RaiseDealerValueChanged(int? value)
        {
            OnDealerValueChanged?.Invoke(value);
        }

        private void RaiseResult(string message)
        {
            OnResultChanged?.Invoke(message);
        }


        private void InitDeck()
        {
            deck.Clear();
            for (int suit = 0; suit < 4; suit++)
            {
                for (int rank = 2; rank <= 14; rank++)
                {
                    deck.Add(new CardData(rank, suit));
                }
            }
        }

        private void ShuffleDeck()
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int swapIndex = UnityEngine.Random.Range(i, deck.Count);
                CardData temp = deck[i];
                deck[i] = deck[swapIndex];
                deck[swapIndex] = temp;
            }
        }

        private CardData DrawCard()
        {
            if (deck.Count == 0)
            {
                InitDeck();
                ShuffleDeck();
            }

            CardData top = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            return top;
        }

        private int CalculateHandTotal(List<CardData> hand)
        {
            int total = 0;
            int aceCount = 0;

            foreach (var card in hand)
            {
                int value;
                if (card.rank >= 2 && card.rank <= 10)
                {
                    value = card.rank;
                }
                else if (card.rank >= 11 && card.rank <= 13)
                {
                    value = 10;
                }
                else
                {
                    value = 11;
                    aceCount++;
                }

                total += value;
            }

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }



        public void TryStartDealRound()
        {
            if (_currentState != BlackjackState.Betting) return;
            if (roundActive) return;

            if (currentBalance < fixedBetAmount)
            {
                RaiseResult("Not enough balance.");
                return;
            }

            if (deck.Count < reshuffleThreshold)
            {
                InitDeck();
                ShuffleDeck();
            }

            roundActive = true;
            playerTurnActive = false;
            dealerHiddenCardFaceDown = true;
            RaiseResult(string.Empty);

            currentBalance -= fixedBetAmount;
            RaiseBalanceChanged();

            ClearTableVisuals();

            SetState(BlackjackState.Dealing);

            SetButtonsInteractable(deal: false, hit: false, stand: false, nextRound: false);

            DealCardToPlayer(false);
            DealCardToDealer(false);
            DealCardToPlayer(false);
            DealCardToDealer(true);

            playerTurnActive = true;
            SetState(BlackjackState.PlayerTurn);
            SetButtonsInteractable(deal: false, hit: true, stand: true, nextRound: false);

            UpdateHandValues(revealDealer: false);

            int playerTotal = CalculateHandTotal(playerHand);
            if (playerTotal == 21)
            {
                StartDealerTurn();
            }
        }

        public void PlayerHit()
        {
            if (!roundActive || !playerTurnActive || _currentState != BlackjackState.PlayerTurn) return;

            DealCardToPlayer(false);
            UpdateHandValues(revealDealer: false);

            int playerTotal = CalculateHandTotal(playerHand);
            if (playerTotal > 21)
            {
                playerTurnActive = false;
                SetButtonsInteractable(deal: false, hit: false, stand: false, nextRound: true);

                RevealDealerHand();
                UpdateHandValues(revealDealer: true);

                RaiseResult("Player Bust! Dealer wins.");
                EndRound(playerWon: false, push: false);
            }
        }

        public void PlayerStand()
        {
            if (!roundActive || !playerTurnActive || _currentState != BlackjackState.PlayerTurn) return;

            playerTurnActive = false;
            SetButtonsInteractable(deal: false, hit: false, stand: false, nextRound: false);

            StartDealerTurn();
        }

        public void NextRound()
        {
            if (_currentState != BlackjackState.RoundEnd) return;

            ClearTableVisuals();
            RaiseResult(string.Empty);

            SetState(BlackjackState.Betting);
            SetButtonsInteractable(deal: true, hit: false, stand: false, nextRound: false);

            RaisePlayerValueChanged(nullValue: true);
            RaiseDealerValueChanged(null);
        }



        private void StartDealerTurn()
        {
            SetState(BlackjackState.DealerTurn);
            RevealDealerHand();
            UpdateHandValues(revealDealer: true);

            while (CalculateHandTotal(dealerHand) < 17)
            {
                DealCardToDealer(false);
                UpdateHandValues(revealDealer: true);
            }

            int playerTotal = CalculateHandTotal(playerHand);
            int dealerTotal = CalculateHandTotal(dealerHand);

            if (dealerTotal > 21)
            {
                RaiseResult("Dealer Busts! Player wins.");
                EndRound(playerWon: true, push: false);
            }
            else
            {
                if (playerTotal > dealerTotal)
                {
                    RaiseResult("Player wins.");
                    EndRound(playerWon: true, push: false);
                }
                else if (playerTotal < dealerTotal)
                {
                    RaiseResult("Dealer wins.");
                    EndRound(playerWon: false, push: false);
                }
                else
                {
                    RaiseResult("Push.");
                    EndRound(playerWon: false, push: true);
                }
            }
        }

        private void EndRound(bool playerWon, bool push)
        {
            roundActive = false;
            SetState(BlackjackState.RoundEnd);

            if (push)
            {
                currentBalance += fixedBetAmount;
            }
            else if (playerWon)
            {
                currentBalance += fixedBetAmount * 2;
            }

            RaiseBalanceChanged();
            UpdateHandValues(revealDealer: true);

            SetButtonsInteractable(deal: false, hit: false, stand: false, nextRound: true);
        }

        private void UpdateHandValues(bool revealDealer)
        {
            RaisePlayerValueChanged(nullValue: false);

            if (revealDealer)
            {
                int dealerTotal = CalculateHandTotal(dealerHand);
                RaiseDealerValueChanged(dealerTotal);
            }
            else
            {
                RaiseDealerValueChanged(null);
            }
        }

        private void SetButtonsInteractable(bool deal, bool hit, bool stand, bool nextRound)
        {
            if (dealButton != null) dealButton.interactable = deal;
            if (hitButton != null) hitButton.interactable = hit;
            if (standButton != null) standButton.interactable = stand;
            if (nextRoundButton != null) nextRoundButton.interactable = nextRound;
        }

        private void DealCardToPlayer(bool faceDown)
        {
            CardData card = DrawCard();
            playerHand.Add(card);

            if (cardFactory != null && playerCardsParent != null)
            {
                GameObject cardObj = cardFactory.CreateCard(card, playerCardsParent, faceDown);
                playerCardViews.Add(cardObj);
                ArrangeHandVisuals(playerCardViews, playerCardsParent, isDealer: false);
            }
        }


        private void DealCardToDealer(bool faceDown)
        {
            CardData card = DrawCard();
            dealerHand.Add(card);

            if (cardFactory != null && dealerCardsParent != null)
            {
                GameObject cardObj = cardFactory.CreateCard(card, dealerCardsParent, faceDown);
                dealerCardViews.Add(cardObj);
                ArrangeHandVisuals(dealerCardViews, dealerCardsParent, isDealer: true);

                if (faceDown)
                {
                    dealerHiddenCardView = cardObj.GetComponent<CardView_Baseline>();
                    dealerHiddenCardIndex = dealerHand.Count - 1;
                    dealerHiddenCardFaceDown = true;
                }
            }
        }


        private void RevealDealerHand()
        {
            if (dealerHiddenCardFaceDown && dealerHiddenCardView != null && dealerHiddenCardIndex >= 0 && dealerHiddenCardIndex < dealerHand.Count)
            {
                var hiddenCard = dealerHand[dealerHiddenCardIndex];
                dealerHiddenCardView.SetCard(hiddenCard, false);
            }

            dealerHiddenCardFaceDown = false;
        }

        private void ClearTableVisuals()
        {
            // Return player cards to pool
            foreach (var cardObj in playerCardViews)
            {
                if (cardObj != null)
                {
                    if (cardFactory != null)
                        cardFactory.RecycleCard(cardObj);
                    else
                        Destroy(cardObj); // backup
                }
            }
            playerCardViews.Clear();

            // Return dealer cards to pool
            foreach (var cardObj in dealerCardViews)
            {
                if (cardObj != null)
                {
                    if (cardFactory != null)
                        cardFactory.RecycleCard(cardObj);
                    else
                        Destroy(cardObj); // backup
                }
            }
            dealerCardViews.Clear();

            playerHand.Clear();
            dealerHand.Clear();
            dealerHiddenCardView = null;
            dealerHiddenCardIndex = -1;
            dealerHiddenCardFaceDown = false;
        }


        private void ArrangeHandVisuals(List<GameObject> cardViews, RectTransform parent, bool isDealer)
        {
            int count = cardViews.Count;
            if (count == 0) return;

            float spacing = 40f;
            float startX = -spacing * (count - 1) / 2f;

            for (int i = 0; i < count; i++)
            {
                var cardObj = cardViews[i];
                var rt = cardObj.GetComponent<RectTransform>();
                if (rt == null) continue;

                float x = startX + i * spacing;
                rt.anchoredPosition = new Vector2(x, 0f);

                float centerIndex = (count - 1) / 2f;
                float angle = (i - centerIndex) * 3f;
                if (isDealer) angle = -angle;

                rt.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

    }
}
