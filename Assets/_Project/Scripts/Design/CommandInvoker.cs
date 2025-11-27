using UnityEngine;

namespace Blackjack.Final
{
    public class CommandInvoker : MonoBehaviour
    {
        private ICommand dealCommand;
        private ICommand hitCommand;
        private ICommand standCommand;
        private ICommand nextRoundCommand;

        private void Start()
        {
            var gm = GameManager_Final.Instance;
            dealCommand = new DealCommand(gm);
            hitCommand = new HitCommand(gm);
            standCommand = new StandCommand(gm);
            nextRoundCommand = new NextRoundCommand(gm);
        }

        public void OnDealPressed()
        {
            dealCommand.Execute();
        }

        public void OnHitPressed()
        {
            hitCommand.Execute();
        }

        public void OnStandPressed()
        {
            standCommand.Execute();
        }

        public void OnNextRoundPressed()
        {
            nextRoundCommand.Execute();
        }
    }
}
