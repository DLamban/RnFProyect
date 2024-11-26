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
        // EVENTS
        public event Action<bool> OnChargeSelectedToExecute;
        public  InputResolveCharge()
        {

        }
        public void setChargesToResolve(List<Charge> charges)
        {
            chargesToResolve = charges;
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
        public void executeCharge()
        {
            OnChargeSelectedToExecute?.Invoke(false);
            chargeSelected.arrow.Visible = false;
            chargeSelected.chargingUnit.charge();
            chargeSelected.chargedUnit.hideChargingResponseBillboard();

        }
        new public void CustomProcess(double delta)
        {          
        }
    }
}
