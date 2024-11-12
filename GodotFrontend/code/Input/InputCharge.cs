using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputManager;
using GodotFrontend.code.Input;


public class InputCharge : BaseMove, ISubInputManager
{
    public InputCharge(BattlefieldCursorPosDel battlefieldCursorPosDel) : base(battlefieldCursorPosDel) { }
    new public void CustomProcess(double delta)
    {
        if (isDragging && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            Vector3? worldPosition = battlefieldCursorPosDel();

            if (worldPosition.HasValue)
            {
                worldPosition = worldPosition.Value;
                switch (dragMode)
                {
                    case DragMode.move:                       
                        break;
                    case DragMode.left_pivot:
                        pivotUnit(worldPosition.Value, unitDragged, dragMode, pivotAnchorPoint);
                        break;
                    case DragMode.right_pivot:
                        pivotUnit(worldPosition.Value, unitDragged, dragMode, pivotAnchorPoint);
                        break;
                    default:
                        GD.Print("Behavior not defined in dragging input");
                        break;
                }
            }
        }
        else if (isDragging)
        {
            unitDragged.distanceRemaining -= distanceMoved;
            GD.Print(unitDragged.distanceRemaining);
            restartStateVars();

        }
    }

}

