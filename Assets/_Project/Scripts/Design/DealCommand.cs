namespace Blackjack.Final
{
    public class DealCommand : ICommand
    {
        private readonly GameManager_Final _gm;

        public DealCommand(GameManager_Final gm)
        {
            _gm = gm;
        }

        public void Execute()
        {
            _gm.TryStartDealRound();
        }
    }
}
