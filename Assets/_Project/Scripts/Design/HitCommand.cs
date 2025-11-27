namespace Blackjack.Final
{
    public class HitCommand : ICommand
    {
        private readonly GameManager_Final _gm;

        public HitCommand(GameManager_Final gm)
        {
            _gm = gm;
        }

        public void Execute()
        {
            _gm.PlayerHit();
        }
    }
}
