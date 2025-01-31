using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.code.Input
{
    public class InputResolveCharge : ISubInputManager
    {
        private List<Charge> chargesToResolve;
        private Charge chargeSelected;
        private CanvasLayer canvasLayer;
        private Action OnResolvedAllCharges;
        // EVENTS
        public event Action<bool> OnChargeSelectedToExecute;
        
        public  InputResolveCharge()
        {

        }
        public void setChargesToResolve(List<Charge> charges,Action _OnfinishResolvingCharges)
        {
            chargesToResolve = charges;
            OnResolvedAllCharges = _OnfinishResolvingCharges;
        }
        public void selectCharge(UnitGodot unit)
        {
            Charge charge = chargesToResolve.Find(x => x.chargingUnit == unit);
            if (charge != null)
            {
                chargeSelected = charge;
                OnChargeSelectedToExecute?.Invoke(true);
            }
            else
            {                
                chargeSelected = null;
                OnChargeSelectedToExecute?.Invoke(false);
            }
        }
        public async void executeCharge()
        {
            OnChargeSelectedToExecute?.Invoke(false);
            chargeSelected.arrow.Visible = false;
            await chargeSelected.chargingUnit.charge();
            chargeSelected.chargedUnit.hideChargingResponseBillboard();
            chargesToResolve.Remove(chargeSelected);
            if (chargesToResolve.Count == 0)
            {
                chargeSelected = null;
                OnResolvedAllCharges?.Invoke();
            }

        }
        new public void CustomProcess(double delta)
        {          
        }
    }
}
