using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blackjack.Baseline
{
    public class GameManager_Baseline : MonoBehaviour
    {
        [Header("Card Prefab & Parents")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform playerCardsParent;
        [SerializeField] private Transform dealerCardsParent;

        [Header("UI Text")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text playerValueText;
        [SerializeField] private TMP_Text dealerNameText;
        [SerializeField] private TMP_Text dealerValueText;

        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text betText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text resultBannerText;

        [Header("Buttons")]
        [SerializeField] private Button dealButton;
        [SerializeField] private Button hitButton;
        [SerializeField] private Button standButton;
        [SerializeField] private Button nextRoundButton;

        [Header("Game Settings")]
        [SerializeField] private int startingBalance = 100;
        [SerializeField] private int fixedBetAmount = 10;
        [SerializeField] private int reshuffleThreshold = 15;

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
        private int dealerHiddenCardIndex = -1; // index in dealerHand

        private void Start()
        {
            currentBalance = startingBalance;
            InitDeck();
            ShuffleDeck();

            if (playerNameText != null) playerNameText.text = "Player";
            if (dealerNameText != null) dealerNameText.text = "Dealer";

            UpdateBalanceUI();
            UpdateBetUI();
            SetStateText("Betting");

            ClearTableVisuals();

            // Initial button state
            if (dealButton != null) dealButton.interactable = true;
            if (hitButton != null) hitButton.interactable = false;
            if (standButton != null) standButton.interactable = false;
            if (nextRoundButton != null) nextRoundButton.interactable = false;

            if (resultBannerText != null) resultBannerText.text = "";
        }

        #region Deck & Dealing

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
            // Fisher-Yates shuffle
            for (int i = 0; i < deck.Count; i++)
            {
                int swapIndex = Random.Range(i, deck.Count);
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

        #endregion

        #region UI Helpers

        private void UpdateBalanceUI()
        {
            if (balanceText != null)
            {
                balanceText.text = $"Balance: {currentBalance}";
            }
        }

        private void UpdateBetUI()
        {
            if (betText != null)
            {
                betText.text = $"Bet: {fixedBetAmount}";
            }
        }

        private void SetStateText(string state)
        {
            if (stateText != null)
            {
                stateText.text = $"State: {state}";
            }
        }

        private void UpdateHandValues(bool revealDealer)
        {
            int playerTotal = CalculateHandTotal(playerHand);
            if (playerValueText != null)
            {
                playerValueText.text = $"Value: {playerTotal}";
            }

            if (dealerValueText != null)
            {
                if (revealDealer)
                {
                    int dealerTotal = CalculateHandTotal(dealerHand);
                    dealerValueText.text = $"Value: {dealerTotal}";
                }
                else
                {
                    dealerValueText.text = "Value: ?";
                }
            }
        }

        private void ClearTableVisuals()
        {
            foreach (var cardObj in playerCardViews)
            {
                if (cardObj != null) Destroy(cardObj);
            }
            playerCardViews.Clear();

            foreach (var cardObj in dealerCardViews)
            {
                if (cardObj != null) Destroy(cardObj);
            }
            dealerCardViews.Clear();

            playerHand.Clear();
            dealerHand.Clear();
            dealerHiddenCardView = null;
            dealerHiddenCardIndex = -1;

            if (playerValueText != null) playerValueText.text = "Value: --";
            if (dealerValueText != null) dealerValueText.text = "Value: --";
        }

        private void ShowResult(string message)
        {
            if (resultBannerText != null)
            {
                resultBannerText.text = message;
            }
        }

        #endregion

        #region Hand Calculation

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
                    value = 10; // J, Q, K
                }
                else
                {
                    value = 11; // Ace as 11 initially
                    aceCount++;
                }

                total += value;
            }

            // If over 21, turn some Aces into 1
            while (total > 21 && aceCount > 0)
            {
                total -= 10; // 11 -> 1
                aceCount--;
            }

            return total;
        }

        #endregion

        #region Round Flow

        public void OnDealButtonClicked()
        {
            if (roundActive) return;
            if (currentBalance < fixedBetAmount)
            {
                ShowResult("Not enough balance to bet.");
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
            ShowResult("");

            currentBalance -= fixedBetAmount;
            UpdateBalanceUI();

            ClearTableVisuals();

            SetStateText("Dealing");
            if (dealButton != null) dealButton.interactable = false;
            if (hitButton != null) hitButton.interactable = false;
            if (standButton != null) standButton.interactable = false;
            if (nextRoundButton != null) nextRoundButton.interactable = false;

            // Deal initial cards: Player, Dealer, Player, Dealer
            DealCardToPlayer(false);
            DealCardToDealer(false); // face up
            DealCardToPlayer(false);
            DealCardToDealer(true);  // face down

            playerTurnActive = true;
            SetStateText("Player Turn");

            if (hitButton != null) hitButton.interactable = true;
            if (standButton != null) standButton.interactable = true;

            UpdateHandValues(revealDealer: false);

            // Auto-check for immediate blackjack (optional)
            int playerTotal = CalculateHandTotal(playerHand);
            if (playerTotal == 21)
            {
                // Simple version: treat as normal 21, just go to dealer turn
                StartDealerTurn();
            }
        }

        private void DealCardToPlayer(bool faceDown)
        {
            CardData card = DrawCard();
            playerHand.Add(card);

            if (cardPrefab != null && playerCardsParent != null)
            {
                GameObject cardObj = Instantiate(cardPrefab, playerCardsParent);
                var view = cardObj.GetComponent<CardView_Baseline>();
                if (view != null)
                {
                    view.SetCard(card, faceDown);
                }
                playerCardViews.Add(cardObj);
            }
        }

        private void DealCardToDealer(bool faceDown)
        {
            CardData card = DrawCard();
            dealerHand.Add(card);

            if (cardPrefab != null && dealerCardsParent != null)
            {
                GameObject cardObj = Instantiate(cardPrefab, dealerCardsParent);
                var view = cardObj.GetComponent<CardView_Baseline>();
                if (view != null)
                {
                    view.SetCard(card, faceDown);
                }
                dealerCardViews.Add(cardObj);

                if (faceDown)
                {
                    dealerHiddenCardView = view;
                    dealerHiddenCardIndex = dealerHand.Count - 1;
                    dealerHiddenCardFaceDown = true;
                }
            }
        }

        public void OnHitButtonClicked()
        {
            if (!roundActive || !playerTurnActive) return;

            DealCardToPlayer(false);
            UpdateHandValues(revealDealer: false);

            int playerTotal = CalculateHandTotal(playerHand);
            if (playerTotal > 21)
            {
                // Player busts
                playerTurnActive = false;
                if (hitButton != null) hitButton.interactable = false;
                if (standButton != null) standButton.interactable = false;

                RevealDealerHand();
                int dealerTotal = CalculateHandTotal(dealerHand);
                UpdateHandValues(revealDealer: true);

                ShowResult("Player Bust! Dealer wins.");
                EndRound(playerWon: false, push: false);
            }
        }

        public void OnStandButtonClicked()
        {
            if (!roundActive || !playerTurnActive) return;

            playerTurnActive = false;
            if (hitButton != null) hitButton.interactable = false;
            if (standButton != null) standButton.interactable = false;

            StartDealerTurn();
        }

        private void StartDealerTurn()
        {
            SetStateText("Dealer Turn");
            RevealDealerHand();
            UpdateHandValues(revealDealer: true);

            // Dealer draws until 17 or more
            while (CalculateHandTotal(dealerHand) < 17)
            {
                DealCardToDealer(false);
                UpdateHandValues(revealDealer: true);
            }

            int playerTotal = CalculateHandTotal(playerHand);
            int dealerTotalFinal = CalculateHandTotal(dealerHand);

            if (dealerTotalFinal > 21)
            {
                ShowResult("Dealer Busts! Player wins.");
                EndRound(playerWon: true, push: false);
            }
            else
            {
                if (playerTotal > dealerTotalFinal)
                {
                    ShowResult("Player wins.");
                    EndRound(playerWon: true, push: false);
                }
                else if (playerTotal < dealerTotalFinal)
                {
                    ShowResult("Dealer wins.");
                    EndRound(playerWon: false, push: false);
                }
                else
                {
                    ShowResult("Push.");
                    EndRound(playerWon: false, push: true);
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

        private void EndRound(bool playerWon, bool push)
        {
            roundActive = false;
            SetStateText("Round End");

            if (push)
            {
                // Return bet
                currentBalance += fixedBetAmount;
            }
            else if (playerWon)
            {
                // Winnings: get bet back + same amount as profit
                currentBalance += fixedBetAmount * 2;
            }

            UpdateBalanceUI();
            UpdateHandValues(revealDealer: true);

            if (nextRoundButton != null) nextRoundButton.interactable = true;
            if (dealButton != null) dealButton.interactable = false;
            if (hitButton != null) hitButton.interactable = false;
            if (standButton != null) standButton.interactable = false;
        }

        public void OnNextRoundButtonClicked()
        {
            ClearTableVisuals();
            if (resultBannerText != null) resultBannerText.text = "";

            SetStateText("Betting");
            if (dealButton != null) dealButton.interactable = true;
            if (hitButton != null) hitButton.interactable = false;
            if (standButton != null) standButton.interactable = false;
            if (nextRoundButton != null) nextRoundButton.interactable = false;
        }

        #endregion
    }
}
