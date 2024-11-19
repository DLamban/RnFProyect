using Core.Rules;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.code.Input
{
    // THIS class represents the inputs made by the enemy player 
    // try to be hotseat and multiplayer compatible, but will need a lot of work
    public class ReactiveInput
    {
        private TaskCompletionSource<bool> chargeResponseFinish;
        private Queue<Charge> chargeResponses;
        private Panel blockGamePanel;
        public ReactiveInput(Panel _blockPanel) {
            blockGamePanel = _blockPanel;
        }
        public async Task<List<Charge>> ResolveCharges(List<Charge> charges)
        {
            blockGameUntilResponseFinish();
            chargeResponseFinish = new TaskCompletionSource<bool>();
            List<Charge> result = new List<Charge>();
            chargeResponses = new Queue<Charge>();
            foreach (var charge in charges)
            {
                chargeResponses.Enqueue(charge);
            }

            while (chargeResponses.Count > 0)
            {
                var responseCharge = await ChargeRes(chargeResponses.Dequeue());
                result.Add(responseCharge);  
            }

            
            await chargeResponseFinish.Task;
            unblockGame();
            return result;
        }
        private void blockGameUntilResponseFinish()
        {
            blockGamePanel.Visible = true;
        }
        private void unblockGame()
        {
            blockGamePanel.Visible = false;
        }

        private async Task<Charge> ChargeRes(Charge charge)
        {
            // with net code we won't be able to do this parameter modifying
            charge.ChargeResponse = ChargeResponse.HOLD;
            charge.arrow.Visible = false;
            if (chargeResponses.Count == 0) {
                chargeResponseFinish.SetResult(true);
            }
            await Task.Delay(1000);
            return charge;

        }
    }
}
