namespace Blackjack.Final
{
    public class NextRoundCommand : ICommand
    {
        private readonly GameManager_Final _gm;

        public NextRoundCommand(GameManager_Final gm)
        {
            _gm = gm;
        }

        public void Execute()
        {
            _gm.NextRound();
        }
    }
}
