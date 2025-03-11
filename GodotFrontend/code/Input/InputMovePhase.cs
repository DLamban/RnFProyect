using Core.GameLoop;
using Core.GeometricEngine;
using Core.Units;
using Godot;
using GodotFrontend.code.Input;
using System;
using static GodotFrontend.code.Input.InputFSM;
using static GodotFrontend.code.Input.InputManager;

public partial class InputMovePhase:BaseMove, ISubInputManager
{
    public InputMovePhase(BattlefieldCursorPosDel battlefieldCursorPosDel):base(battlefieldCursorPosDel)
	{		
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
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
						dragUnit(worldPosition.Value, unitDragged);
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

	private float intentMoveUnit(float distance, UnitGodot unit)
	{
		return distance;

    }
    // We're gonna divide in 2 phases, intent to move and real move so we can check collisions
    private void dragUnit(Vector3 worldPos, UnitGodot unit)
	{
		// We need to move th unit only when posible!!!

		distanceMoved = 0;
		if (unit.distanceRemaining <= 0) return;
		// remember to copy values and not references
		
		AffineTransformCore beginningTransform = new AffineTransformCore();
		beginningTransform.matrixTransform = unit.affTrans.matrixTransform.cloneMatrix();

		unit.affTrans.matrixTransform = currentTransformMat.cloneMatrix();
		//unit.moveUnit(unitOriginPos);
		// we move based in orginal pos, so we can use moveforward function
		if (offsetDistancePicked == null)
		{
			offsetDistancePicked = (float)unit.getDistanceFrontLine(worldPos);
		}
		distanceMoved = intentMoveUnit((float)(unit.getDistanceFrontLine(worldPos) - offsetDistancePicked), unit);
        	
		// Remembar that going backwards is just half of the movement, do it better
		distanceMoved = (float)Math.Clamp(distanceMoved, unit.distanceRemaining * -0.5, unit.distanceRemaining);
		
		unit.moveForward(distanceMoved);
		if (UnitsClientManager.Instance.checkGeneralCollision(unit.coreUnit))
		{
			unit.affTrans.matrixTransform = beginningTransform.matrixTransform.cloneMatrix();
			unit.updateTransformToRender();
		
		};
		if (distanceMoved<0)
		{
			// half the move cost double movement
			distanceMoved *=-4; 
		}
		unit.showDistanceRemaining(distanceMoved);
	}
	private void drawDebugLine(Vector3 origin,Vector3 end, Color color)
	{
		var immediateMesh = new ImmediateMesh();
		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

		immediateMesh.SurfaceAddVertex(origin);
		immediateMesh.SurfaceAddVertex(end);

		immediateMesh.SurfaceEnd();

		var meshInstance = new MeshInstance3D();
		meshInstance.Mesh = immediateMesh;

		var material = new StandardMaterial3D();
		material.AlbedoColor = color;
		meshInstance.MaterialOverride = material;
		//  TEMPORAL ERASED
		//AddChild(meshInstance);
	}

    internal void finishMoveSubphase(UnitRenderCreator unitRenderCreator)
    {
		unitRenderCreator.disableMoveInputAllTroops();
    }
}
