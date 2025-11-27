Contributers: AJ Bwejesa 100792221

I acknowledge that all assets are self made
Resources used include: 
lectures for reference
lab for reference
stack overflow for reference
unity documentation for reference
ChatGPT was used as a reference for debugging ideation only
Google Images for slide pictures

SCENARIO DESCRIPTION
Purpose: small casino-style Blackjack experience to demonstrate patterns and optimization.
Player’s goal: win chips by beating the dealer in Blackjack.
Mechanics: betting, dealing, Hit/Stand, dealer AI, round loop.
Why it’s good for patterns: clear turn flow, repeatable rounds, lots of card spawning, and constant UI updates that benefit from Observer, State, Command, and Object Pool patterns.

GAME RULES
Player vs Dealer
● Single deck of 52 cards.
● At round start:
● Player places a fixed bet.
● Deal 2 cards to the player face up.
● Deal 2 cards to the dealer, one face up and one face down.
● Player turn:
● Buttons: Hit, Stand.
● If total > 21 - bust - immediate loss.
● After Stand:
● Reveal dealer’s hidden card.
● Dealer hits until total is 17 or more.
● If dealer busts - player wins.
● Else compare totals:
● Higher total ≤ 21 wins.
● Equal totals - push (draw, bet returned).
● Default Ace = 11, but if the hand total > 21 and Aces are present, they are reduced from 11 to 1 until the total is ≤ 21 or no more Aces to downgrade.

STATES
States:

Betting – waiting to start the round.

Dealing – dealing initial cards.

PlayerTurn – player can Hit or Stand.

DealerTurn – dealer reveals and draws according to the rules.

RoundEnd – show result, adjust balance, then move back to Betting.

Transitions:
● Betting - (player clicks Deal) - Dealing
● Dealing - (both have 2 cards) - PlayerTurn
● PlayerTurn - (player busts) - RoundEnd
● PlayerTurn - (player clicks Stand) - DealerTurn
● DealerTurn - (dealer finished drawing) - RoundEnd
● RoundEnd - (player clicks Next Round) - Betting

OBSERVER EVENTS AND UI LISTENERS
Events from GameManager_Final:
● OnStateChanged(BlackjackState newState)
● OnBalanceChanged(int newBalance)
● OnPlayerValueChanged(int value)
● OnDealerValueChanged(int? valueOrNull) // null means still hidden
● OnResultChanged(string message)

UI listeners:
● UIStateDisplay – listens to OnStateChanged and shows the current state.
● UIBalanceDisplay – listens to OnBalanceChanged and shows the chip count.
● UIResultBanner – listens to OnResultChanged and shows win/lose/push messages.
● UIPlayerValueDisplay – listens to OnPlayerValueChanged and shows the player’s hand value.
● UIDealerValueDisplay – listens to OnDealerValueChanged and shows “Value: ?” while hidden and the real value once revealed.

ClassGameManager_Baseline
deck list - stores all cards in the shoe
playerHand list - cards in the player’s hand
dealerHand list - cards in the dealer’s hand
playerCardViews list - all card GameObjects for the player
dealerCardViews list - all card GameObjects for the dealer
currentBalance - player’s chips
roundActive - true while one round is running
OnDealButtonClicked() - deals the first four cards and starts the round
OnHitButtonClicked() - adds a card to the player and checks for bust
OnStandButtonClicked() - plays the dealer’s turn and decides the result
OnNextRoundButtonClicked() - clears the table and lets you deal again
ClearTableVisuals() - loops through all card GameObjects and Destroy()s them
CalculateHandTotal(hand) - adds up card values and handles aces as 1 or 11

This version keeps almost all the logic in one script. it also talks directly to the UI texts and buttons and uses Instantiate and Destroy for every card each round.

ClassGameManager_Final
Instance - singleton reference so other scripts can find the manager
currentState - BlackjackState (Betting, Dealing, PlayerTurn, DealerTurn, RoundEnd)
deck, playerHand, dealerHand - same idea as baseline but controlled through state transitions
playerCardViews, dealerCardViews - tracked so they can be returned to the pool instead of destroyed
OnStateChanged(state) - event fired whenever the round changes phase
OnBalanceChanged(value) - event fired whenever the chip count changes
OnPlayerValueChanged(value) - event fired when the player’s hand changes
OnDealerValueChanged(valueOrNull) - event fired when the dealer’s value is known or still hidden
OnResultChanged(message) - event fired when the result banner should show a message
TryStartDealRound() - checks state and balance, moves to Dealing, and sets up PlayerTurn
PlayerHit() - only runs when state is PlayerTurn, adds a card and checks for bust
PlayerStand() - ends the player turn and starts the dealer turn
NextRound() - only runs when state is RoundEnd, clears the table and goes back to Betting
StartDealerTurn() - handles the full dealer flow and switches to RoundEnd
SetButtonsInteractable(...) - central place to enable or disable Deal, Hit, Stand, and Next Round

The final version uses a state enum instead of loose booleans. it also fires events when things change instead of editing the UI directly. this makes the round flow easier to read and prevents the UI from being hardwired into the game logic.

InterfaceICommand
Execute() - single method to run the action

ClassDealCommand ICommand
gm - reference to GameManager_Final
Execute() - calls gm.TryStartDealRound()

ClassHitCommand ICommand
gm - reference to GameManager_Final
Execute() - calls gm.PlayerHit()

ClassStandCommand ICommand
gm - reference to GameManager_Final
Execute() - calls gm.PlayerStand()

ClassNextRoundCommand ICommand
gm - reference to GameManager_Final
Execute() - calls gm.NextRound()

ClassCommandInvoker
dealCommand - DealCommand for starting a round
hitCommand - HitCommand for taking another card
standCommand - StandCommand for ending the player turn
nextRoundCommand - NextRoundCommand for resetting
Start() - grabs GameManager_Final.Instance and builds each command
OnDealPressed() - runs dealCommand.Execute()
OnHitPressed() - runs hitCommand.Execute()
OnStandPressed() - runs standCommand.Execute()
OnNextRoundPressed() - runs nextRoundCommand.Execute()

In the baseline version the UI buttons called GameManager_Baseline methods directly. in the final version the UI only talks to the invoker, and the invoker runs command objects. this makes the actions reusable and easier to swap or expand later without editing the button hookups.

ClassCardPool_Final
cardPrefab - base card visual to copy
initialSize - how many cards to pre-create
pool queue - inactive card GameObjects waiting to be reused
Awake() - prewarms the pool by instantiating a batch of cards and disabling them
GetCard(parent) - pulls a card from the pool, re-parents it, activates it, and resets its transform
ReturnCard(card) - deactivates the card, re-parents it to the pool root, and queues it

ClassCardViewFactory_Final
cardPool - reference to CardPool_Final
CreateCard(data, parent, faceDown) - asks the pool for a card, sets the parent, and calls SetCard on CardView_Baseline
RecycleCard(card) - returns the card to the pool through CardPool_Final

In the baseline version every card was created with Instantiate and destroyed at the end of the round. in the final version cards are pre-created and reused through the pool. the factory isolates the creation logic so GameManager_Final doesn’t need to know if a card comes from Instantiate or a pool.

ClassUIBalanceDisplay
balanceText - TMP_Text to show the chip count
OnEnable - subscribes to GameManager_Final.OnBalanceChanged
OnDisable - unsubscribes from the event
HandleBalanceChanged(value) - writes “Balance: value” to the text

ClassUIStateDisplay
stateText - TMP_Text to show the current state
OnEnable/OnDisable - subscribes and unsubscribes from OnStateChanged
HandleStateChanged(state) - writes “State: state” to the text

ClassUIResultBanner
resultText - TMP_Text for the win/lose/push message
OnEnable/OnDisable - subscribes and unsubscribes from OnResultChanged
HandleResultChanged(message) - writes the message to the banner

ClassUIPlayerValueDisplay
playerValueText - TMP_Text for the player’s hand value
OnEnable/OnDisable - listens to OnPlayerValueChanged
HandlePlayerValueChanged(value) - writes “Value: value”

ClassUIDealerValueDisplay
dealerValueText - TMP_Text for the dealer’s hand value
OnEnable/OnDisable - listens to OnDealerValueChanged
HandleDealerValueChanged(valueOrNull) - shows “Value: ?” while hidden and the real value when revealed

In the baseline version GameManager_Baseline directly changed the UI texts whenever something happened. in the final version the UI listens to events from GameManager_Final instead. this uses the observer pattern: the game publishes updates like balance, state, and results, and the HUD chooses how to display them.

ArrangeHandVisuals (inside GameManager_Final)
cardViews list - all current card GameObjects for that hand
parent - RectTransform that owns the cards
isDealer - used to flip the fan direction
calculates spacing so cards are spread out
sets anchoredPosition for each card so they don’t overlap
rotates cards slightly based on index to create a fan effect

In the baseline version all cards were spawned at the same position so only one was visible. the final version adds a small layout step that offsets and rotates the cards in each hand. this makes it much clearer how many cards each side has and looks more like an actual table layout.

What I did was take the original single-script blackjack flow and split it into separate responsibilities using multiple design patterns. The state pattern in GameManager_Final keeps the round flow organized into clear phases like Betting, PlayerTurn, DealerTurn, and RoundEnd. this avoids scattered if statements and makes it obvious which actions are allowed at each point.

The command pattern separates button clicks from the game logic. instead of UI calling methods directly, the buttons trigger simple commands through the invoker. this makes it easy to change controls or add features like replays or input remapping without touching GameManager_Final.

The factory and object pool work together to optimize card creation. instead of constantly instantiating and destroying card GameObjects, the game reuses a pre-built pool. this reduces spikes in CPU time and garbage collection, which I confirmed by profiling the baseline scene versus the final scene.

The observer pattern decouples the HUD from the game. GameManager_Final only raises events when something changes, and the UI scripts listen and update their texts. this means the blackjack logic doesn’t depend on the HUD objects and new listeners like sound, particle effects, or logging could be added later by subscribing to the same events.

Overall, the patterns help keep the blackjack project cleaner and easier to extend. the baseline version works but mixes UI, state, and card spawning in one place and creates a lot of garbage by constantly creating cards. the improved version organizes behavior into focused classes, reduces dependencies between systems, and improves performance during repeated rounds.
