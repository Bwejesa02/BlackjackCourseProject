namespace Blackjack.Final
{
    public class StandCommand : ICommand
    {
        private readonly GameManager_Final _gm;

        public StandCommand(GameManager_Final gm)
        {
            _gm = gm;
        }

        public void Execute()
        {
            _gm.PlayerStand();
        }
    }
}
