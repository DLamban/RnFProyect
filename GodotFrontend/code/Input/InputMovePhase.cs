using Core.GameLoop;
using Core.GeometricEngine;
using Core.Units;
using Godot;
using System;
using static GodotFrontend.code.Input.InputFSM;

public partial class InputMovePhase
{
	public enum DragMode
	{
		left_pivot,
		right_pivot,
		move,
		rotate_center
	}
	public InputState _inputState = InputState.Empty;
	private Unidad unitSelected;
	private Unidad lastUnitSelected;

	private bool isDragging;
	private Unidad unitDragged;
	// ORIGIN positions when clicking for dragging
	private Vector2 unitOriginPos;
	private Matrix currentTransformMat;
	private Vector3 dragOrigin;
	private float? offsetDistancePicked;
	private float distanceMoved;
	private float originAngle;
	private Vector2 pivotClickPointRect;
	
	private Vector2 pivotAnchorPoint;
	private DragMode dragMode;
	private Camera3D mainCamera;
	private PhysicsDirectSpaceState3D spaceState;

	private Viewport viewport;
	private Node3D battlefieldTerrain;
	//DEBUG Vars
	// Starting pos for resetting
	private AffineTransformCore transformOriUnit1;
	private AffineTransformCore transformOriUnit2;
	// UI 
	private Label Debug1Label;
	private Label Debug2Label;
	private Label Debug3Label;
	private TextEdit Debug1Text;
	private TextEdit Debug2Text;
	private TextEdit Debug3Text;
	private BattleState battleState { 
		get { return PlayerInfoSingleton.Instance.battleStateManager.currentState;} 
	}


	// Called when the node enters the scene tree for the first time.
	//public override void _Ready()
	//{
	//	//mainCamera = GetViewport().GetCamera3D() as Camera3D;
		
		//// recover UI node and childs with bad patterns because it's just "temporal" and for dev
		//Node debugUI = GetParent().GetNode<Node>("debugUI");
		//VBoxContainer debugVBoxContainer = debugUI.GetNode<VBoxContainer>("CanvasGroup/Panel/VBoxContainer");
		
		///// reusing debug textboxs, maybe should change names
		//Debug1Label = debugVBoxContainer.GetNode<Label>("YworldHBox/Label");
		//Debug1Text = debugVBoxContainer.GetNode<TextEdit>("YworldHBox/YWorld");
		//Debug2Label = debugVBoxContainer.GetNode<Label>("DistanceHBox/Label");
		//Debug2Text = debugVBoxContainer.GetNode<TextEdit>("DistanceHBox/Distance");
		//Debug3Label = debugVBoxContainer.GetNode<Label>("UnitPos/Label");
		//Debug3Text = debugVBoxContainer.GetNode<TextEdit>("UnitPos/UnitPos");

	//}
	public InputMovePhase(Viewport _viewport, PhysicsDirectSpaceState3D _spaceState)
	{
		viewport = _viewport;
		spaceState = _spaceState;
		mainCamera = viewport.GetCamera3D() as Camera3D;
	}
	
	private void restartStateVars()
	{
		offsetDistancePicked = null;
		isDragging = false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void CustomProcess(double delta)
	{
		if (isDragging && Input.IsMouseButtonPressed(MouseButton.Left))
		{
			Vector3? worldPosition = getBattlefieldCursorPos();
			
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
			GD.Print("REleased");
		}

	}
	// We are passing global vars, but is for formatting purposes
	//TODO: add collision in pivotting
	private void pivotUnit(Vector3 worldPos, Unidad unit, DragMode pivotOrientation, Vector2 anchorPoint)
	{
		distanceMoved = 0;
		if (unit.distanceRemaining <= 0) return;
		// remember to copy values and not references
		unit.affTrans.matrixTransform = currentTransformMat.cloneMatrix(); 
		Vector2 clickPosRect = new Vector2(worldPos.X -  anchorPoint.X , worldPos.Y - anchorPoint.Y );
		float angle = unit.affTrans.calculateAngle(pivotClickPointRect.X, pivotClickPointRect.Y, clickPosRect.X, clickPosRect.Y);
		//float angleDegrees = (float)(angle * (360 / Math.Tau));
		float maxAnglePivot = (float)(calcMaxDistancePivot(unit));
		//GD.Print("angle degrees", angleDegrees);
		if (pivotOrientation == DragMode.left_pivot)
		{
			//drawDebugLine(new Vector3(anchorPoint.X,anchorPoint.Y,0.25f), worldPos,Color.Color8(200,0,0));		
			angle = Math.Clamp(angle, -maxAnglePivot, maxAnglePivot * 0.5f);
			
			unit.pivot(angle, false);
		}else if (pivotOrientation == DragMode.right_pivot)
		{

			//drawDebugLine(new Vector3(anchorPoint.X, anchorPoint.Y, 0.25f), worldPos, Color.Color8(0, 200, 0));
			angle = Math.Clamp(angle, maxAnglePivot * -0.5f, maxAnglePivot);
			unit.pivot(angle, true);
		}
		distanceMoved = calculateDistancePivot(unit, angle);
		unit.showDistanceRemaining(distanceMoved);
	}
	// the distance of pivot should be the perimeter of an arc of angle size
	// formula is just dist = r x angle (in radians) because a circle is r x 2pi
	private float calculateDistancePivot(Unidad unit, float angle)
	{

		float distance = 0;
		float radius = unit.coreUnit.sizeEnclosedRectangledm.X;
		distance = radius * angle;
		// backwards cost double movement
		if (angle < 0)
		{
			distance *= 2;	
		}
		return Math.Abs(distance);
	}
	// Inverted calc, so it will be angle = dist/r
	private float calcMaxDistancePivot(Unidad unit)
	{
		return unit.distanceRemaining / unit.coreUnit.sizeEnclosedRectangledm.X;
	}
	private void dragUnit(Vector3 worldPos, Unidad unit)
	{
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
		distanceMoved = (float)(unit.getDistanceFrontLine(worldPos) - offsetDistancePicked);		
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
		// Show in debug ui, BROKEN
		//Debug1Text.Text = (distanceMoved).ToString();
		//Debug2Label.Text = "distanceRemain";
		//Debug2Text.Text = unit.distanceRemaining.ToString();
		//Debug3Label.Text = "distanceRemainglob";
		//Debug3Text.Text = unitDragged.distanceRemaining.ToString();
		unit.showDistanceRemaining(distanceMoved);
	}
	public void onArrowClick(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Node collider)
	{
		if (isDragging == false) { 
			Node arrow = collider.GetParent().GetParent();
			isDragging = true;

			unitDragged = (Unidad)arrow.GetParent().GetParent();

			currentTransformMat = unitDragged.affTrans.copyMatrixTransformValues();
			unitOriginPos = unitDragged.position2D();
			Vector3? worldPosNullable = getBattlefieldCursorPos();

			switch (arrow.Name)
			{
				case "middle_movement_arrow":
					dragMode = DragMode.move;
					break;
				case "left_movement_arrow":
					dragMode = DragMode.left_pivot;
					pivotAnchorPoint = calculatePivotAnchorPoint(unitDragged, true);
					pivotClickPointRect = new Vector2(worldPosNullable.Value.X - pivotAnchorPoint.X, worldPosNullable.Value.Y - pivotAnchorPoint.Y);
					break;
				case "right_movement_arrow":
					dragMode = DragMode.right_pivot;
					pivotAnchorPoint = calculatePivotAnchorPoint(unitDragged, false);
					pivotClickPointRect = new Vector2(worldPosNullable.Value.X - pivotAnchorPoint.X, worldPosNullable.Value.Y - pivotAnchorPoint.Y);
					break;
				case "rotate_arrow":
					dragMode = DragMode.rotate_center;
					break;
				default:
					GD.Print("SOMETHING WENT WRONG IN ARROW SELECTION");
					break;
			}
		}
	}
	private Vector3? getBattlefieldCursorPos()
	{
		Vector3 from = mainCamera.ProjectRayOrigin(viewport.GetMousePosition());
		Vector3 to = from + mainCamera.ProjectRayNormal(viewport.GetMousePosition()) * 1000;
		// Raycast
		PhysicsRayQueryParameters3D paramsRaycast = new PhysicsRayQueryParameters3D();
		paramsRaycast.From = from;
		paramsRaycast.To = to;
		paramsRaycast.CollideWithAreas = true;
		paramsRaycast.CollisionMask = 2;
		var result = spaceState.IntersectRay(paramsRaycast);
		if (result.Count > 0)
		{
			return (Vector3)result["position"];
		}
		else
		{
			return null;
		}

	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="unit"></param>
	/// <param name="pivotAnchorPoint">where is the point to rotate so we get the rect from there to the click pos</param>
	/// <returns>angle in radians</returns>
	private float calculateAngle(Unidad unit, Vector2 pivotAnchorPoint, Vector3 clickWorldPos)
	{
		
		// we need the vector that form anchor point and clickpos

		Vector2 rectClick = new Vector2(pivotAnchorPoint.X- clickWorldPos.X, pivotAnchorPoint.Y- clickWorldPos.Y);
			
		float angle = unit.affTrans.calculateAngleToTransform(rectClick.X, rectClick.Y);

		return angle;
	
		
	}
	private float? calculateInitialAngle(Unidad unit, Vector2 pivotAnchorPoint)
	{
		Vector3? clickPos = getBattlefieldCursorPos();

		if (clickPos.HasValue)
		{
			return calculateAngle(unit,pivotAnchorPoint,clickPos.Value);
		}
		return null;
	}
	private Vector2 calculatePivotAnchorPoint(Unidad unit, bool isLeft)
	{
		//Vector2 pivotAnchorPoint = new Vector2();
		float offsetTroop = (float)unit.coreUnit.Troop.Size.Width / (2 * 100);
		float offsetDistanceY = offsetTroop;
		float offsetDistanceX = 0;
		if (isLeft) // we take right top corner point as anchor
		{
			offsetDistanceX = (float)unit.coreUnit.sizeEnclosedRectangle.Width / 100 - offsetTroop;  
		}
		else // we take left top corner point as anchor
		{
			offsetDistanceX = -offsetTroop;
		}
		System.Numerics.Vector2 pivotAnchorPoint = unit.affTrans.localToGlobalTransforms(offsetDistanceX,offsetDistanceY);
		return new Vector2(pivotAnchorPoint.X,pivotAnchorPoint.Y);
	}
	// DEPRECATED: to remove
	private void _on_battlefield_click_event(Node mainCamera, InputEvent @event, Vector3 position, Vector3 normal, long shape_idx)
	{

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
	private void _on_reset_positions_pressed()
	{
		// Replace with function body.
		GD.Print("resetting bullshito");
	}

}
