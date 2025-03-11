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
        private ChargeResponseUI chargeResponseUI;
        public ReactiveInput(Panel _blockPanel) {
            chargeResponseUI = _blockPanel as ChargeResponseUI;

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
            chargeResponseUI.Visible = true;
        }
        private void unblockGame()
        {
            chargeResponseUI.Visible = false;
        }

        private async Task<Charge> ChargeRes(Charge charge)
        {
            charge.chargedUnit.coreUnit.temporalCombatVars.chargeResponse = ChargeResponse.HOLD;                        
            await chargeResponseUI.ShowToast(charge.chargedUnit.coreUnit.temporalCombatVars.chargeResponse.ToString() + "!");
            charge.chargedUnit.showChargingResponseBillboard(charge.chargedUnit.coreUnit.temporalCombatVars.chargeResponse);
            //charge.arrow.Visible = false;
            if (chargeResponses.Count == 0) {
                chargeResponseFinish.SetResult(true);
            }
            await Task.Delay(1000);
            return charge;

        }
    }
}
