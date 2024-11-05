using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Core.GeometricEngine;
using Core.Rules;
using Core.Units;
using System.Diagnostics;
using GodotFrontend.units;
using GodotFrontend.UIcode;
using Core.Networking;
using Core.GameLoop;
using GodotFrontend.units.Animation;
using GodotFrontend.code.Input;
public partial class Unidad : Node3D
{
	// primitive state manager to handle the cases of UI
	private enum UnitState
	{
		idle,
		chargeDiceRolling,
		charging,
		fighting,
		moving,
		shooting
	}
	private UnitState _unitState;
	private UnitState unitState {
		get { return _unitState; }
		set {
			// when change state, hide all the ui bell and whistles
			disableInput();
			_unitState=value;
		}
	}
	
	public AffineTransformCore affTrans { 
		get => coreUnit.Transform; 
	}
	private Node3D inputButtonsNode = new Node3D();    
	public bool inputEnabled {
		set{
			if (value) enableInput();
			else disableInput();
		}
		 
	}
	private SelectMenu selectMenu;
	private DiceThrower diceThrower;
	//FX
	public Node3D selectionFx;
	public Node3D magicSelectionFX;
	public BaseUnit coreUnit;
	public Vector2 center;
	public List<Node3D> troopNodes = new List<Node3D>();
	private Sprite3D distBillboard;
	Vector2 offsetTroop;
	const float arrowHeight = 0.25f;	
	
	Godot.RandomNumberGenerator randomNumberGenerator = new Godot.RandomNumberGenerator();
	// EVENTS
	public event Action unitSelection;
	// EVENTS TO VINCULATE WITH CORE UNIT
	
	// Define the signal
	public delegate void UnitClickEventHandler(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Unidad unitSelected);
	public UnitClickEventHandler _unitSelect;    
	public delegate void ArrowClickedEventHandler(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Node arrow);
	// Movement vars
	Stack<AffineTransformCore> transformsStack = new Stack<AffineTransformCore>();
	public float distanceMoved { get; set; }
	public float distanceRemaining { get; set; }
	// render vars
	private float speed = 0.5f;
	// ****************************************************************************************************************//
	// ROTATIN TWEEN, MAYBE should be a custom class
	private bool isRotating = false;
	private TaskCompletionSource<bool> rotationTween;
	private double rotTimePassed;
	private double rotTime;
	private Vector2 rotPivotPoint;
	private float originalAngleDeg;
	private float targetAngleDeg;
	Matrix originalMatAffineTrans;
	Vector2 rotPivot;
	

	public override void _Ready()
	{
		inputEnabled = false;// disabled as default
		this.AddChild(inputButtonsNode);
		diceThrower = DiceThrower.Instance;
		this.coreUnit.vinculateDiceThrower(diceThrower.diceThrowerTaskDel);
	}
	public void initGodotUnit(BaseUnit coreUnit, InputManager inputManager)
	{
		this.coreUnit = coreUnit;

		this.Name = "UNIT:" + coreUnit.Guid;
		distanceRemaining = coreUnit.distanceRemaining;
		Size troopSize = coreUnit.Troop.Size;
		center = new Vector2((float)coreUnit.sizeEnclosedRectangle.Width / 2, (float)coreUnit.sizeEnclosedRectangle.Height / 2);
		offsetTroop = new Vector2(((float)troopSize.Width / 100), ((float)troopSize.Height / 100));

		center.X = (center.X / 100);
		center.Y = -((center.Y / 100));
		Node3D gizmo = GetChild<Node3D>(1);
		gizmo.Position = gizmo.Position + new Vector3(center.X, center.Y, 0);
		//affTrans = coreUnit.Transform;
		createUIElements(inputManager);
		createUnitTroopsBase(coreUnit.TroopsWidth, coreUnit.UnitCount);
		createColliderForInput(inputManager);
		coreUnit.OnDeathTroops += OnTroopsKilled;
	}
	// set variables to fresh
	public void restartUnit()
	{
		distanceRemaining = coreUnit.distanceRemaining;
		affTrans.resetTransform();
		updateTransformToRender();
	}
	public override void _Process(double delta)
	{
		if (isRotating)
		{ 
			RotateCustomTween(delta);
		}
	}
	/// <summary>
	/// Update the graphic transform so we can saw the actual changes in the affine matrix
	/// </summary>
	/// <param name="instantiation">When we instantiate, we shouldn't send the position or we will have ciclic calls, and an infinite loop</param>
	public void updateTransformToRender(bool netReceived = false)
	{
		UnitMovementManager.ApplyAffineTransformation(affTrans, this);
		// TODO: if we're playing as hotseat, clientNetworkController is null, and sholud avoid this line....
		// a bit ugly, but performant
		if (!HotSeatManager.Instance.isHotseat && !netReceived) PlayerInfoSingleton.Instance.clientNetworkController.updateUnitTransform(affTrans, coreUnit.Guid);
	}

	private void enableInput()
	{
		selectMenu.layer.Visible = true;
		inputButtonsNode.Visible = true;
		inputButtonsNode.SetProcessInput(true);
		selectionFx.Visible = true;
	}
	private void disableInput()
	{
		selectMenu.layer.Visible = false;
		inputButtonsNode.Visible = false;
		inputButtonsNode.SetProcessInput(false);
		selectionFx.Visible = false;
	}
	
	private void OnTroopsKilled(int deaths)
	{
		killTroops(deaths);
	}
	// we are accumulating code that may be encapsulated
	async public void killTroops(int deaths)
	{
		List<Node3D> troopstodie = troopNodes.GetRange(0,deaths);
		foreach(Node3D node in troopstodie)
		{
			AnimationPlayer animationPlayer = node.GetChild(1).GetNode<AnimationPlayer>("AnimationPlayer");

			// adding some randomness to make it more appeal
			animationPlayer.SpeedScale = randomNumberGenerator.RandfRange(0.75f, 1.25f);
			animationPlayer.Play("Death");

		}
		// Wait for 3 secs
		await Task.Delay(3000);
		foreach (var item in troopstodie)
		{
			item.QueueFree();
			troopNodes.Remove(item);
		}
		ReformAfterCombat(deaths);
	}
	
	private void createColliderForInput(InputManager inputManager)
	{
		Area3D area = new Area3D();
		area.SetProcessInput(true);
		CollisionShape3D collisionShape3D = new CollisionShape3D();
		BoxShape3D boxShape3D = new BoxShape3D();
		boxShape3D.Size = new Vector3(coreUnit.sizeEnclosedRectangledm.X, coreUnit.sizeEnclosedRectangledm.Y, 0.5f);
		// connect user input
		collisionShape3D.Shape = boxShape3D;
		area.AddChild(collisionShape3D);
		area.InputRayPickable = true;
		_unitSelect = (Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Unidad unitSelect) => {            
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				Vector2I mousePos = (Vector2I)mouseEvent.Position;
				selectMenu.positionUnderMouse(mousePos.X, mousePos.Y);
				unitSelection?.Invoke();
				inputManager.selectUnit(this);
			}
		};
		area.InputEvent += (camera, @event, position, normal, shapeIdx) => _unitSelect(camera, @event, position, normal, shapeIdx, this);
		area.Position = new Vector3(center.X, center.Y, 0.2f);
		this.AddChild(area);
	}
	
	private void createUIElements(InputManager inputManager)
	{
		createDragArrows(inputManager);
		createDistBillboard();
		createChargingLayer();
		createFX();
		createSelectMenu(inputManager);
		showDistanceRemaining(0);
	}
	#region FXEFFECTS
	private void createFX()
	{
		createMagicSelectionFX();
		createMoveSelectionFX();
	}
	private void createMagicSelectionFX()
	{
		PackedScene selectionFxAsset = GD.Load<PackedScene>("res://units/vfx/magic_selection_fx.tscn");
		magicSelectionFX = (Node3D)selectionFxAsset.Instantiate();
		magicSelectionFX.Position = new Vector3(center.X, center.Y, 0.24f);
		float xscale = this.coreUnit.sizeEnclosedRectangledm.X;
		float yscale = this.coreUnit.sizeEnclosedRectangledm.Y;
		magicSelectionFX.Scale = new Vector3(xscale, 1.0f, yscale);
		this.AddChild(magicSelectionFX);
		magicSelectionFX.Visible = false;
	}
	private void createMoveSelectionFX()
	{		
		PackedScene selectionFxAsset = GD.Load<PackedScene>("res://units/vfx/selection_fx.tscn");
		selectionFx = (Node3D)selectionFxAsset.Instantiate();
		selectionFx.Position= new Vector3(center.X, center.Y, 0.24f);
		float xscale = this.coreUnit.sizeEnclosedRectangledm.X;
		float yscale = this.coreUnit.sizeEnclosedRectangledm.Y;
		selectionFx.Scale= new Vector3( xscale, 1.0f,yscale);
		this.AddChild(selectionFx);
		selectionFx.Visible = false;
	}
	#endregion
	private void createSelectMenu(InputManager inputManager)
	{
		selectMenu = new SelectMenu(this, inputManager);
		//inputButtonsNode.AddChild(selectMenu.layer);
	}
	#region CHARGE_REGION
	public async void charge()
	{
		
		unitState = UnitState.chargeDiceRolling;			
		int result = await diceThrower.ThrowDicesCharge();
		float resultToDm = (result * 2.54f) / 10;
		unitState = UnitState.charging;
		foreach (HitCollider hitCollider in this.coreUnit.checkCharge())
		{
			if (hitCollider.distance <= distanceRemaining + resultToDm)
			{
				
				if (hitCollider.HitObject.unit != null && hitCollider.HitObject.owner == CollidedObject.Owner.enemy)
				{

					ChargeMovement(hitCollider);
					unitState = UnitState.charging;

				}
				else
				{
					throw new NotImplementedException();
				}
			}
			else// FAILED CHARGE
			{
				FailedChargeMovement(resultToDm);
				unitState = UnitState.idle;
			}
			// BREAK FOREACH FFOR NOW
			break;
		}	
		
	}
	private async void ChargeMovement(HitCollider hitCollider)
	{
		moveForward(hitCollider.distance,false);
		Vector2 vectorForward = new Vector2(affTrans.ForwardVec.X, affTrans.ForwardVec.Y);
		float deltaAngleDegrees = (float)CloseTheDoor(hitCollider.HitObject.unit);
		
		float time = (float)(hitCollider.distance / speed);
		
		Tween tween = CreateTween();
		float tweenduration = time;

		Vector3 targetPos = this.Position + new Vector3((float)(vectorForward.X * hitCollider.distance), (float)(vectorForward.Y * hitCollider.distance), 0);
		TaskCompletionSource<bool> moveFinish = new TaskCompletionSource<bool>();
		tween.TweenProperty(this, "position", targetPos, tweenduration).SetTrans(Tween.TransitionType.Quint);

		tween.TweenCallback(Callable.From(() => moveFinish.SetResult(true)));
		await moveFinish.Task;
		// rotate to face the enemy
		
		System.Numerics.Vector2 vec = this.affTrans.GlobalToLocalTransforms(hitCollider.hitpoint.X, hitCollider.hitpoint.Y);
		await startRotationCustomTween(deltaAngleDegrees, 1, new Vector2(vec.X,vec.Y));		
	}
	private void FailedChargeMovement(float distance)
	{
		float time = (float)(distance / (speed * 2));

		Tween tween = CreateTween();
		float tweenduration = time;
		Vector2 vector = new Vector2(affTrans.ForwardVec.X, affTrans.ForwardVec.Y);
		Vector3 targetPos = this.Position + new Vector3((float)(vector.X * distance), (float)(vector.Y * distance), 0);
		tween.TweenProperty(this, "position", targetPos, tweenduration).SetTrans(Tween.TransitionType.Quint);

	}
	/// <summary>
	/// rotate unit to face enemy, only in the transform, not visible so we can tween for better visuals
	/// </summary>
	/// <param name="collidedUnit">the collided unit</param>
	/// <returns>Degrees needed for the rotation</returns>
	private double CloseTheDoor(BaseUnit collidedUnit)
	{
		float angleEnemy = collidedUnit.Transform.currentAngleDegrees + 180 % 360;
		float currentAngle = (float)affTrans.currentAngleDegrees;
		currentAngle = currentAngle >= 0 ? currentAngle : 360 + currentAngle;
		float angle = angleEnemy - currentAngle;
		if (angle > 180)
		{
			angle = angle - 360;
		}
		return angle;

	}
	private void createChargingLayer()
	{
		ChargingLayer chargingLayer = new ChargingLayer(this);
		inputButtonsNode.AddChild(chargingLayer.sprite);

	}
	#endregion
	// Godot tween is not enough, we need our custom tween
	private async Task startRotationCustomTween(float deltaAngleDegrees, double time, Vector2 rotPivotPoint)
	{
		rotationTween = new TaskCompletionSource<bool>();
		rotTimePassed = 0;
		rotTime = time;
		//originalAngleDeg = affTrans.currentAngleDegrees;
		targetAngleDeg = deltaAngleDegrees;
		isRotating = true;
		originalMatAffineTrans = this.affTrans.copyMatrixTransformValues();
		rotPivot = rotPivotPoint;
		await rotationTween.Task;
	}
	private void RotateCustomTween(double delta)
	{
		rotTimePassed += delta;

		float t = (float)(rotTimePassed / rotTime);
		float interpolatedAngle = 0;

		// interpolate cuadratic t
		bool cuadraticInter = true;
		if (cuadraticInter)
		{ // we can remover the original angle and use the delta, so we interpolate from zero to the delta
			//interpolatedAngle = (1-t) * (1 - t) * originalAngleDeg + 2 * (1 - t) * t * targetAngleDeg + t * t * targetAngleDeg;
			interpolatedAngle = 2 * (1 - t) * t * targetAngleDeg + t * t * targetAngleDeg;
		}
		AffineTransformCore interpolatedRot = new AffineTransformCore();
		interpolatedRot.matrixTransform = originalMatAffineTrans;
		interpolatedRot.rotate(interpolatedAngle, rotPivot.X,rotPivot.Y);        
		if (rotTimePassed >= rotTime)
		{
			isRotating = false;
			rotationTween.SetResult(true);
		}
		else
		{
			affTrans.matrixTransform = interpolatedRot.matrixTransform;
			updateTransformToRender();
		}		
	}

	private void createDistBillboard()
	{
		PackedScene distBillboardAsset = GD.Load<PackedScene>("res://assets/UI/dist_billboard.tscn");
		distBillboard = (Sprite3D)distBillboardAsset.Instantiate();
		distBillboard.NoDepthTest = true;
		inputButtonsNode.AddChild(distBillboard);
		distBillboard.Position = new Vector3(center.X, center.Y, 0.45f);
	}

	public void showDistanceRemaining(float distMovedTemp)
	{
		float distRemainingTemp = distanceRemaining - distMovedTemp;
		distBillboard.GetNode<Label>("SubViewport/CanvasLayer/distance").Text = distRemainingTemp.ToString("F2");
	}   
	public Vector2 position2D()
	{
		return new Vector2((float)affTrans.offsetX, (float)affTrans.offsetY);
	}
	private void createDragArrows(InputManager inputManager)
	{

		PackedScene unitAssetArrow = GD.Load<PackedScene>("res://units/arrow.tscn");
		PackedScene unitAssetArrowTwist = GD.Load<PackedScene>("res://units/arrowtwisted.tscn");
		// ****************************************************************************************************************
		// MIDDLE ARROW
		// ****************************************************************************************************************
		Node3D middlearrowToSpawn = (Node3D)unitAssetArrow.Instantiate();

		middlearrowToSpawn.Name = "middle_movement_arrow";
		middlearrowToSpawn.Position = new Vector3(center.X, 0, arrowHeight);
		// we want the middle arrow to be diferent
		BaseMaterial3D arrowMat = middlearrowToSpawn.GetChildOrNull<MeshInstance3D>(0).GetSurfaceOverrideMaterial(0) as BaseMaterial3D;
		BaseMaterial3D greenArrowMat = (BaseMaterial3D)arrowMat.Duplicate();
		greenArrowMat.AlbedoColor = new Godot.Color(0, 1, 0, 0.6f);
		middlearrowToSpawn.GetChildOrNull<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, greenArrowMat);
		addColliderToArrow(middlearrowToSpawn, inputManager);
		inputButtonsNode.AddChild(middlearrowToSpawn);

		// ****************************************************************************************************************
		// LEFT ARROW
		// ****************************************************************************************************************


		Node3D leftarrowToSpawn = (Node3D)unitAssetArrowTwist.Instantiate();
		leftarrowToSpawn.Name = "left_movement_arrow";
		leftarrowToSpawn.Position = new Vector3(0, 0, arrowHeight);
		addColliderToArrow(leftarrowToSpawn, inputManager);

		inputButtonsNode.AddChild(leftarrowToSpawn);
		
		// ****************************************************************************************************************
		// RIGHT ARROW
		// ****************************************************************************************************************
		float rightalign = (float)coreUnit.sizeEnclosedRectangle.Width / 100;
		Node3D rightarrowToSpawn = (Node3D)unitAssetArrowTwist.Instantiate();
		rightarrowToSpawn.Name = "right_movement_arrow";
		rightarrowToSpawn.Position = new Vector3(rightalign, 0, arrowHeight);
		// do a mirror flip
		Vector3 scale = rightarrowToSpawn.Scale;
		scale.X = -scale.Y;
		rightarrowToSpawn.Scale = scale;

		addColliderToArrow (rightarrowToSpawn, inputManager);
		inputButtonsNode.AddChild(rightarrowToSpawn);
		

	}
	private void addColliderToArrow(Node3D arrow, InputManager inputManager)
	{
		CollisionObject3D collider = arrow.GetNode<CollisionObject3D>("Cube/Area3D");
		ArrowClickedEventHandler arrowClickedEventHandler;

		arrowClickedEventHandler = (Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Node collider) => {
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				inputManager.onArrowClick(camera,@event,position,normal,shapeIdx, collider);
			}
		};
		collider.InputEvent += (camera, @event, position, normal, shapeIdx) => arrowClickedEventHandler(camera, @event, position, normal, shapeIdx, collider);
		//		collider.Connect("input_event", new Callable(inputManager, nameof(inputManager._on_arrow_click_event)));
	}
	public void moveForward(double distance, bool updateRender=true)
	{
		Vector2 vector = new Vector2(affTrans.ForwardVec.X, affTrans.ForwardVec.Y);
		affTrans.offsetX += vector.X * distance;
		affTrans.offsetY += vector.Y * distance;
		if (updateRender) updateTransformToRender();
	}
	public void moveUnit(Vector2 destination)
	{
		affTrans.offsetX = destination.X;
		affTrans.offsetY = destination.Y;
		updateTransformToRender();
	}
	public float getDistanceFrontLine(Vector3 worldPos) { 
		return affTrans.distanceToFrontLine(worldPos.X,worldPos.Y);
	}
	/// <summary>
	/// Important, turn means a rotation of the unit, by the center of it
	/// </summary>
	/// <param name="degrees"></param>
	public void turn(double degrees)
	{
		rotateByCenter(degrees);
	}
	private void rotateByCenter(double degrees, bool updateRender=true)
	{
		// Center is the center of the enclosing rectangle
		affTrans.rotate(degrees, center.X, center.Y);
		if (updateRender)updateTransformToRender();
	}
	public void pivot(double radians, bool anchorPointLeft) {
		float degrees = (float)(radians * (360 / Math.Tau));
		if (anchorPointLeft)
		{
			affTrans.rotate(degrees, 0,0);
		}else
		{
			affTrans.rotate(degrees, (float)coreUnit.sizeEnclosedRectangledm.X,0);
		}
		updateTransformToRender(); 
	}

	private void createUnitTroopsBase(int width, int troopCount)
	{        		
		Node3D original = GetChild<Node3D>(0);
		original.Position = original.Position + new Vector3(offsetTroop.X/2,-offsetTroop.Y/2,0);
	//	original.Position = new Vector3(0, 0, width);
		AnimationPlayer animationPlayer;
		for (int i = 0; i < troopCount; i++)
		{
			Node3D clone = (Node3D)original.Duplicate();
			
			troopNodes.Add(clone);
			AddChild(clone);
			// position in the unit
			int x = i % width;
			int y = i / width;
			clone.Position = clone.Position + new Vector3(x*offsetTroop.X, y*-offsetTroop.Y, 0f);
			AddVariationToTroop(clone);


			try
			{
				animationPlayer = clone.GetChild(1).GetNode<AnimationPlayer>("AnimationPlayer");
			}catch(Exception e) {
				animationPlayer = null;
			}
			if (animationPlayer != null)
			{
				addModifierSkeleton3D(clone);
				AnimateTroop(animationPlayer);
			}
		}
		original.QueueFree();
	}
	#region ANIMATION
	async private void ReformAfterCombat(int deaths)
	{

		TaskCompletionSource<bool> tweenY = new TaskCompletionSource<bool>();
		coreUnit.Troops.RemoveRange(0, deaths);
		int partialRankDeaths = deaths % coreUnit.TroopsWidth;
		int rankDeaths = deaths / coreUnit.TroopsWidth;
		GD.Print("partialRankdetaghs", partialRankDeaths, "rankdeathjs ", rankDeaths);

		// move to the front and reform backline

		int lastFullRank = coreUnit.Troops.Count / coreUnit.TroopsWidth;
		bool partialBackline = coreUnit.Troops.Count % coreUnit.TroopsWidth != 0;

		GD.Print("partialbackline", partialBackline);

		Tween tween = CreateTween();
		float tweenduration = 1.0f;


		for (int i = 0; i < troopNodes.Count; i++)
		{
			Node3D node = troopNodes[i];
			int realpositioninformation = (int)Math.Floor(node.Position.Y / offsetTroop.Y) * coreUnit.TroopsWidth * -1 + (int)Math.Floor(node.Position.X / offsetTroop.X);
			//GD.Print("realpos", realpositioninformation);
			// MOVE TO FRONT
			int movetoFrontPositions = 0;
			if (realpositioninformation % coreUnit.TroopsWidth < partialRankDeaths)
			{
				movetoFrontPositions = rankDeaths + 1;
			}
			else
			{
				movetoFrontPositions = rankDeaths;
			}
			float Ymove = (offsetTroop.Y) * movetoFrontPositions;
			Vector3 targetPos = node.Position + new Vector3(0, Ymove, 0);
			tween.SetParallel();
			tween.TweenProperty(node, "position", targetPos, tweenduration).SetTrans(Tween.TransitionType.Quint);
			// need the delay because we are running in paralell
			tween.TweenCallback(Callable.From(() => tweenY.TrySetResult(true))).SetDelay(tweenduration);

		}
		// Await first tween
		await tweenY.Task;
		Tween tweenx = CreateTween();
		// We had to reorganize the list because the troops got disordered
		troopNodes = troopNodes.OrderByDescending(troop => troop.Position.Y).ThenBy(troop => troop.Position.X).ToList();
		for (int i = 0; i < troopNodes.Count; i++)
		{
			Node3D node = troopNodes[i];
			float Xobj = 0;
			Xobj = (i % coreUnit.TroopsWidth) * offsetTroop.X;
			Xobj = Xobj + ((float)coreUnit.Troop.Size.Width/100) / 2;
			Vector3 targetPos = new Vector3(Xobj, node.Position.Y, node.Position.Z);
			tweenx.SetParallel();
			tweenx.TweenProperty(node, "position", targetPos, .6f).SetTrans(Tween.TransitionType.Quint);
		}

	}
	// Rotate, scale and translate every troop to break pattens
	private void AddVariationToTroop(Node3D clone)
	{
		(clone.GetChild(1) as Node3D).RotateY(randomNumberGenerator.RandfRange(-Mathf.Tau / 60, Mathf.Tau/60));
		
	}
	private void addModifierSkeleton3D(Node3D clone)
	{
		Skeleton3D skeleton = clone.GetChild(1).GetNodeOrNull<Skeleton3D>("CharacterArmature/Skeleton3D");
		if (skeleton == null) return;
		skeleton.AddChild(new InfantrySkelModifier());

	}
	/// <summary>
	/// DEPRECATED??
	/// </summary>
	/// <param name="clone"></param>
	private void ModifySkeleton(Node3D clone)
	{
		Skeleton3D skeleton = clone.GetChild(1).GetNodeOrNull<Skeleton3D>("CharacterArmature/Skeleton3D");
  
		if (skeleton == null) return;
		int upperArmRindex = skeleton.FindBone("UpperArm.R");
		
		var t  = skeleton.GetBoneGlobalPose(upperArmRindex);
		float randomAngle = randomNumberGenerator.RandfRange(0, Mathf.Tau / 40);
		t = t.Rotated(new Vector3(1.0f, 0.0f, 0.0f), randomAngle);
		randomAngle = randomNumberGenerator.RandfRange(0, Mathf.Tau / 40);

		t = t.Rotated(new Vector3(0.0f, 1.0f, 0.0f), randomAngle);
		randomAngle = randomNumberGenerator.RandfRange(0, Mathf.Tau / 40);
		
		t = t.Rotated(new Vector3(0.0f, 0.0f, 1.0f), randomAngle);
		skeleton.SetBoneGlobalPose(upperArmRindex, t);
	}
	private void AnimateTroop(AnimationPlayer animationPlayer)
	{
		if (animationPlayer.GetAnimationList().Contains("Idle"))
		{
			// Set idle animation        
			animationPlayer.GetAnimation("Idle").LoopMode = Animation.LoopModeEnum.Pingpong;
			animationPlayer.Play("Idle");
			// adding some randomness to make it more appeal
			animationPlayer.SpeedScale = randomNumberGenerator.RandfRange(0.75f, 1.25f);
			animationPlayer.Seek(randomNumberGenerator.RandfRange(0, animationPlayer.GetAnimation("Idle").Length));
		}
	}
	#endregion
}
