using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.Scores;
using OcentraAI.LLMGames.ThreeCardBrag.UI;
using System;
using System.Threading.Tasks;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{

    public class InitializeUIPlayers : EventArgs
    {
        public TaskCompletionSource<bool> CompletionSource { get; }
        public GameManager GameManager { get; }
        public InitializeUIPlayers(TaskCompletionSource<bool> completionSource, GameManager gameManager)
        {
            CompletionSource = completionSource;
            GameManager = gameManager;
        }
    }

    public class UpdateFloorCard : EventArgs
    {
        public Card Card { get; }
        public UpdateFloorCard()
        {

        }

        public UpdateFloorCard(Card card)
        {
            Card = card;
        }
    }


    public class UpdateFloorCardList : EventArgs
    {
        public Card Card { get; }


        public UpdateFloorCardList(Card card)
        {
            Card = card;
        }
    }

    public class NewGameEventArgs : EventArgs
    {
        public int InitialCoins { get; }

        public NewGameEventArgs(int initialCoins)
        {
            InitialCoins = initialCoins;
        }
    }



    public class PlayerActionEvent : EventArgs
    {
        public PlayerAction Action { get; }
        public Type CurrentPlayerType { get; }
        public PlayerActionEvent(Type currentPlayerType, PlayerAction action)
        {
            CurrentPlayerType = currentPlayerType;
            Action = action;
        }
    }

    public class PlayerActionPickAndSwap : EventArgs
    {
        public  Card PickCard { get; }
        public Card SwapCard { get; }
        public Type CurrentPlayerType { get; }
        public PlayerActionPickAndSwap(Type currentPlayerType, Card pickCard, Card swapCard)
        {
            CurrentPlayerType = currentPlayerType;
            PickCard = pickCard;
            SwapCard = swapCard;
        }
    }

    public class PlayerActionRaiseBet : EventArgs
    {
        public string Amount;
        public Type CurrentPlayerType { get; }
        public PlayerActionRaiseBet(Type currentPlayerType, string amount)
        {
            CurrentPlayerType = currentPlayerType;
            Amount = amount;
        }
    }

    public class UpdateGameState : EventArgs
    {
        public bool IsNewRound { get; } = false;
        public GameManager GameManager { get; }
        public UpdateGameState(GameManager gameManager, bool isNewRound =false)
        {
            GameManager = gameManager;
        }


    }




    public class PlayerStartCountDown : EventArgs
    {
        public TaskCompletionSource<bool> ActionCompletionSource { get; }
        public TaskCompletionSource<bool> TimerCompletionSource { get; }
        public float Duration { get; }
        public Player CurrentPlayer { get; }
        public PlayerStartCountDown(Player currentPlayer, TaskCompletionSource<bool> actionCompletionSource, TaskCompletionSource<bool> timerCompletionSource, float duration)
        {
            CurrentPlayer = currentPlayer;
            ActionCompletionSource = actionCompletionSource;
            TimerCompletionSource = timerCompletionSource;
            Duration = duration;
        }
    }

    public class TimerStopEventArgs : EventArgs
    {
        public Player CurrentPlayer { get; }
        public bool IsActionTaken { get; }

        public TimerStopEventArgs(Player currentPlayer, bool isActionTaken)
        {
            CurrentPlayer = currentPlayer;
            IsActionTaken = isActionTaken;
        }
    }



    public class UIMessage : EventArgs
    {
        public string Message { get; }
        public float Delay { get; }

        public UIMessage(string message, float delay)
        {
            Message = message;
            Delay = delay;
        }
    }

    public class ActionCompletedEventArgs : EventArgs
    {
        public bool IsTimer { get; }

        public ActionCompletedEventArgs(bool isTimer)
        {
            IsTimer = isTimer;
        }
    }

    public class TimerCompletedEventArgs : EventArgs
    {
        public TimerCompletedEventArgs()
        {
        }
    }

    public class StopTurnCountdown : EventArgs
    {
        public StopTurnCountdown()
        {
        }
        
    }

    public class SetFloorCard : EventArgs
    {
        public bool SetNull { get; } = false;
        public SetFloorCard(bool setNull = false)
        {
            SetNull = setNull;
        }
    }

    public class AddToFloorCardList : EventArgs
    {
        public bool SetNull { get; } = false;
        public Card Card { get; }
        public AddToFloorCardList(Card card, bool setNull = false)
        {
            Card = card;
            SetNull = setNull;
        }
    }

    public class OfferContinuation : EventArgs
    {
        public float Delay { get; }

        public OfferContinuation(float delay)
        {
            Delay = delay;
        }
    }

    public class ContinueGame : EventArgs
    {
        public bool ShouldContinue { get; }
        public ContinueGame(bool continueGame)
        {
            ShouldContinue = continueGame;
        }
    }

    public class OfferNewGame : EventArgs
    {
        public float Delay { get; }

        public OfferNewGame(float delay)
        {
            Delay = delay;
        }
    }

    public class PurchaseCoins : EventArgs
    {
        public int Amount { get; }

        public PurchaseCoins(int amount)
        {
            Amount = amount;
        }
    }

    public class StartNewGame : EventArgs
    {

        public StartNewGame()
        {

        }
    }

    public class UpdateRoundDisplay : EventArgs
    {
        public ScoreKeeper ScoreKeeper { get; }
        public UpdateRoundDisplay(ScoreKeeper scoreKeeper)
        {
            ScoreKeeper = scoreKeeper;
        }
    }

    public class UpdatePlayerHandDisplay : EventArgs
    {
        public Player Player { get; }

        public UpdatePlayerHandDisplay(Player player)
        {
            Player = player;
        }
    }
}