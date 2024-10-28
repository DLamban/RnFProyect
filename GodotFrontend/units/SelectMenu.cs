using Godot;
using GodotFrontend.code.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.units
{
    internal class SelectMenu
    {
        public CanvasLayer layer;
        private GridContainer gridContainer;
        private Button chargeBtn;
        private Button shootBtn;
        private Button moveBtn;
        private InputManager inputManager;
        private Unidad currentunit;
        internal SelectMenu(Unidad unit, InputManager _inputManager) {
            layer = new CanvasLayer();
            unit.AddChild(layer);
            gridContainer = new GridContainer();
            gridContainer.Columns = 3;
            addButtons();
            layer.AddChild(gridContainer);
            inputManager = _inputManager;
            currentunit = unit;
        }
        public void positionUnderMouse(int x, int y) {
            // add a little offset on Y so is below cursor, yes, it will break at bottom
            // and try to center X, all of this is hardcoded because is only for debug and develop 
            gridContainer.Position = new Vector2(x - 82, y+15);
        }
        // add button and vinculate events
        private void addButtons()
        {
            chargeBtn = new Button();
            chargeBtn.Text = "Charge";
            shootBtn = new Button();
            shootBtn.Text = "Shoot";
            moveBtn = new Button();
            moveBtn.Text = "Move";
            chargeBtn.Pressed += selectCharge;
            gridContainer.AddChild(chargeBtn);
            gridContainer.AddChild(shootBtn);
            gridContainer.AddChild(moveBtn);

        }
        private void selectCharge()
        {
            inputManager.inputState = InputFSM.InputState.Charging;
            
            currentunit.charge();
        }
    }
}
