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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
public partial class UnitGodot : Node3D
{
	private class ColumnAdvance
	{
		public int column;
        public int positions;
    }
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
	public Vector2 offsetTroop;
	const float arrowHeight = 0.25f;
	Label3D responseLabel;
	Godot.RandomNumberGenerator randomNumberGenerator = new Godot.RandomNumberGenerator();
	// EVENTS
	public event Action unitSelection;
	
	// EVENTS TO VINCULATE WITH CORE UNIT
	
	// Define the signal
	public delegate void UnitClickEventHandler(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, UnitGodot unitSelected);
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
	MatrixAffine originalMatAffineTrans;
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
		distanceRemaining = coreUnit.temporalCombatVars.distanceRemaining;
		Size troopSize = coreUnit.Troop.Size;
		center = new Vector2((float)coreUnit.sizeEnclosedRectangle.Width / 2, (float)coreUnit.sizeEnclosedRectangle.Height / 2);
		offsetTroop = new Vector2(((float)troopSize.Width / 100), ((float)troopSize.Height / 100));
		
		center.X = (center.X / 100);
		center.Y = -((center.Y / 100));
		Node3D gizmo = GetChild<Node3D>(0);
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
		distanceRemaining = coreUnit.temporalCombatVars.distanceRemaining;
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
		UnitMovementManager.ApplyAffineTransformation( this);
		
		// TODO: if we're playing as hotseat, clientNetworkController is null, and sholud avoid this line....
		// a bit ugly, but performant
		//if (!HotSeatManager.Instance.isHotseat && !netReceived) PlayerInfoSingleton.Instance.clientNetworkController.updateUnitTransform(affTrans, coreUnit.Guid);
	}

	private void enableInput()
	{
		//selectMenu.layer.Visible = true;
		inputButtonsNode.Visible = true;
		inputButtonsNode.SetProcessInput(true);
		selectionFx.Visible = true;
	}
	private void disableInput()
	{
		//selectMenu.layer.Visible = false;
		inputButtonsNode.Visible = false;
		inputButtonsNode.SetProcessInput(false);
		selectionFx.Visible = false;
	}
	
	private void OnTroopsKilled(BaseUnit unit, int deaths)
	{
		killTroops(unit, deaths);
	}
	// we are accumulating code that may be encapsulated
	async public void killTroops(BaseUnit unit, int deaths)
	{

		//List<Node3D> troopstodie = troopNodes.GetRange(0,deaths);
		var baseTroopsTodieIndexes = unit.Troops
			.Select((troop, index) => new { troop, index })
			.Where((tuple) => tuple.troop.Wounds == 0)
			.Select((tuple) => tuple.index)
            .ToList();
        List<Node3D> troopstodie = new List<Node3D>();
		foreach (var index in baseTroopsTodieIndexes)
		{
            troopstodie.Add(troopNodes[index]);
        }
        foreach (Node3D node in troopstodie)
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
        //reorder the troopnodes array after removing elements. don't know why?!
        troopNodes = troopNodes.OrderByDescending(troop => MathF.Round(troop.Position.Y, 4)).ThenBy(troop => MathF.Round(troop.Position.X, 4)).ToList();

        ReformAfterCombat(baseTroopsTodieIndexes);
	}
	private Dictionary<(int,int), int> getXYorderedTroops()
	{
		Dictionary<(int, int), int> xyOrderedTroops = new Dictionary<(int, int), int>();
        foreach (var troop in troopNodes)
		{
            int realpositioninformationy = -1 * (int)Math.Floor(troop.Position.Y / offsetTroop.Y);
            int realpositioninformationx = (int)Math.Floor(troop.Position.X / offsetTroop.X);
			xyOrderedTroops.Add((realpositioninformationy, realpositioninformationx), troopNodes.IndexOf(troop));
        }
        return xyOrderedTroops;
    }
    /// <summary>
    /// Reform Now needs to take care of Front characters, i have been delaying this for too long
    /// </summary>
    /// <param name="deaths"></param>
    async private void ReformAfterCombat(List<int> baseTroopsTodieIndexes)
    {
        if (baseTroopsTodieIndexes.Count == 0) return;
        // Columns to advance, the index of column, and the positions to advance
        List<ColumnAdvance> columnsAdvance = new List<ColumnAdvance>();


        int deathtroops = baseTroopsTodieIndexes.Count;
        TaskCompletionSource<bool> tweenY = new TaskCompletionSource<bool>();
        coreUnit.DeleteDeathUnits();
        // check columns to move
        Dictionary<int, int> columnsToAdvance = new Dictionary<int, int>();
        for (int i = 0; i < coreUnit.TroopsWidth; i++)
        {
            columnsToAdvance.Add(i, 0);
        }
        foreach (int index in baseTroopsTodieIndexes)
        {
            columnsToAdvance[index % coreUnit.TroopsWidth] += 1;
        }
        Tween tween = CreateTween();
        int startColumnDeath = baseTroopsTodieIndexes[0];
        Dictionary<(int, int), float> ymovedebug = new();
        foreach (var kvp in columnsToAdvance.Where(entry => entry.Value > 0))
        {
            for (int i = 0; i < troopNodes.Count; i++)
            {
                int tempIndex = i + deathtroops;
                if ((i + deathtroops + startColumnDeath) / coreUnit.TroopsWidth == 0)
                {
                    // do nothing, first rank
                    //tempIndex = i + baseTroopsTodieIndexes.Count;// Offset by deaths
                }
                else if ((tempIndex % coreUnit.TroopsWidth) == kvp.Key
                    && i >= startColumnDeath)// ignore troops before => i > columnsAdvance[0].column
                {

                    Node3D node = troopNodes[i];
					int realpositioninformationy =  -1* (int)Math.Floor(node.Position.Y / offsetTroop.Y);
					int realpositioninformationx = (int)Math.Floor(node.Position.X / offsetTroop.X);
					if (realpositioninformationx == 1)
					{
						var test = 0;
					}
                    // MOVE TO FRONT
                    float Ymove = (offsetTroop.Y) * kvp.Value;
                    Vector3 targetPos = node.Position + new Vector3(0, Ymove, 0);
                    ymovedebug.Add((realpositioninformationx, realpositioninformationy), targetPos.Y);

                    tween.SetParallel();
                    tween.TweenProperty(node, "position", targetPos, 1.0f).SetTrans(Tween.TransitionType.Quint);
                    // need the delay because we are running in paralell
                    //tween.TweenCallback(Callable.From(() => tweenY.TrySetResult(true))).SetDelay(1.0f);

                }
            }
        }
        // check coherency  

        if (coreUnit.Troops.Count <= coreUnit.TroopsWidth)
        {
            // reform backline to front
            // write to output

            Console.WriteLine("less troop than width");

        }
        await tween.ToSignal(tween, "finished");

        // Reform backline
        // we need to check how many ranks we still have
        // and check the last rank troops and penultimate rank troops
        // because we can have a last rank that is behind the character that didn't advance
        // also, the check must be with positional info, maybe could be done with more matrix calculations, but it's confusing
        // and also, i think is more robust for more strange ocurrences in the future
        int backlineTroops = coreUnit.Troops.Count % coreUnit.TroopsWidth;

        int desiredRanksCount = coreUnit.Troops.Count / coreUnit.TroopsWidth;

        desiredRanksCount = backlineTroops != 0 ? desiredRanksCount + 1 : desiredRanksCount;// add partial rank count

        if (desiredRanksCount == 1) return; //happy code return

        // We had to reorganize the list because the troops got disordered
        troopNodes = troopNodes.OrderByDescending(troop => MathF.Round(troop.Position.Y, 4)).ThenBy(troop => MathF.Round(troop.Position.X, 4)).ToList();

        List<IGrouping<float, Node3D>> ranksGroupsByPosition = troopNodes.GroupBy(troop => (float)Math.Round(troop.Position.Y, 2)).ToList(); // FP precision error
        int actualRanksCount = ranksGroupsByPosition.Count();
        // check if penultimate rank is filled
        bool penultimateRankfilled = ranksGroupsByPosition[actualRanksCount - 2].Count() == coreUnit.TroopsWidth;

        // penultimate rank not filled means that we have the last ranks fragmented



        if (penultimateRankfilled)// OLD CODE that works for common ocurrences
        {
            int startBackline = coreUnit.Troops.Count - backlineTroops;
            Tween tweenx = CreateTween();
            for (int i = startBackline; i < troopNodes.Count; i++)
            {
                Node3D node = troopNodes[i];
                float Xobj = 0;
                Xobj = (i % coreUnit.TroopsWidth) * offsetTroop.X;
                Xobj = Xobj + ((float)coreUnit.Troop.Size.Width / 100) / 2;
                Vector3 targetPos = new Vector3(Xobj, node.Position.Y, node.Position.Z);
                tweenx.SetParallel();
                tweenx.TweenProperty(node, "position", targetPos, .6f).SetTrans(Tween.TransitionType.Quint);
            }
        }
        else
        {
            //float backRankPos = troopNodes.Min(troop => troop.Position.Y);// Y is negative
            int offsetForPenultimateLine = ranksGroupsByPosition[actualRanksCount - 1].Count();
            // ofset the penultimate line to the right and advance the loose troops of the last rank
            // first the horizontal move

            Tween tweenx = CreateTween();
            List<Node3D> penultimateRank = ranksGroupsByPosition[actualRanksCount - 2].ToList();
            for (int i = 0; i < penultimateRank.Count(); i++)
            {
                Node3D node = penultimateRank[i];
                float Xobj = 0;
                Xobj = (i + offsetForPenultimateLine) * offsetTroop.X;
                Xobj = Xobj + ((float)coreUnit.Troop.Size.Width / 100) / 2;
                Vector3 targetPos = new Vector3(Xobj, node.Position.Y, node.Position.Z);
                tweenx.SetParallel();
                tweenx.TweenProperty(node, "position", targetPos, .6f).SetTrans(Tween.TransitionType.Quint);
            }

            // now the last advance of the last rank
            List<Node3D> lastRank = ranksGroupsByPosition[actualRanksCount - 1].ToList();
            foreach (Node3D node in lastRank)
            {
                float yObj = node.Position.Y + offsetTroop.Y;
                Vector3 targetPos = new Vector3(node.Position.X, yObj, node.Position.Z);
                tweenx.SetParallel();
                tweenx.TweenProperty(node, "position", targetPos, .6f).SetTrans(Tween.TransitionType.Quint);
            }

        }
        //reorder the troopnodes array
        troopNodes = troopNodes.OrderByDescending(troop => MathF.Round(troop.Position.Y, 4)).ThenBy(troop => MathF.Round(troop.Position.X, 4)).ToList();
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
		_unitSelect = (Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, UnitGodot unitSelect) => {            
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				Vector2I mousePos = (Vector2I)mouseEvent.Position;
				//selectMenu.positionUnderMouse(mousePos.X, mousePos.Y);
				unitSelection?.Invoke();
				inputManager.clickUnit(this);
			}
		};
		area.InputEvent += (camera, @event, position, normal, shapeIdx) => _unitSelect(camera, @event, position, normal, shapeIdx, this);
		
		area.MouseEntered += () => { 
			inputManager.HoveringUnit(this);
        };

        area.Position = new Vector3(center.X, center.Y, 0.2f);
		this.AddChild(area);
	}
	
	private void createUIElements(InputManager inputManager)
	{
		createDragArrows(inputManager);
		createDistBillboard();
		//createChargingLayer();
		createChargingResponseBillboard();
		createFX();
		//createSelectMenu(inputManager);
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
	public async Task charge()
	{
		unitState = UnitState.chargeDiceRolling;			
		int result = await diceThrower.ThrowDicesCharge();
		float resultToDm = (result * 2.54f) / 10;
		unitState = UnitState.charging;
        coreUnit.temporalCombatVars.isCharging = true;
        foreach (HitCollider hitCollider in this.coreUnit.checkCharge())
		{
			if (hitCollider.distance <= distanceRemaining + resultToDm)
			{
				
				if (hitCollider.HitObject.unit != null && hitCollider.HitObject.owner == CollidedObject.Owner.enemy)
				{
                    CombatSide combatSide = checkSideCharge(hitCollider);
                    ChargeMovement(hitCollider, combatSide);

                    calcChargeResult(hitCollider);
					
					unitState = UnitState.charging;
					// CHECK IF CHARGED UNIT FLEES
					coreUnit.temporalCombatVars.isInCombatRange = true;
					coreUnit.temporalCombatVars.inCombatUnits.Add(hitCollider.HitObject.unit);
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
			// BREAK FOREACH FOR NOW
			break;
		}	
		return;
    }
	// Watch out for charge reactions
	private void calcChargeResult(HitCollider hitCollider)
	{
		coreUnit.temporalCombatVars.isCharging = true;
		hitCollider.HitObject.unit.temporalCombatVars.isCharged = true;
	}
    // the hitted unit mark the side of the charge
	// complicated code...
    private CombatSide checkSideCharge(HitCollider hitCollider)
	{
		float maxArea = 0;
        CombatSide side = CombatSide.FRONT;

        foreach (KeyValuePair<ArcSeparatorName, ArcSeparatorStruct> item in hitCollider.HitObject.unit.ArcSeparators)
        {
            //check what lines intersect with the unit
            var cross = GeometryUtils.checkSemisegmentUnitCross(item.Value.origin, item.Value.dir, coreUnit.unitBordersWorld);
			if (cross)
			{
                (float, bool) isLeftBigger = GeometryUtils.isLeftSideBiggerWithArea(item.Value.origin, item.Value.dir, coreUnit.unitBordersWorld);
                if (isLeftBigger.Item1 > maxArea)
				{
					maxArea = isLeftBigger.Item1;
					switch (item.Key)
					{
						case ArcSeparatorName.point00:
                            if (isLeftBigger.Item2) side = CombatSide.FRONT;
                            else side = CombatSide.LEFTFLANK;
							break;
						case ArcSeparatorName.point01:
                            if (isLeftBigger.Item2) side = CombatSide.RIGHTFLANK;
                            else side = CombatSide.FRONT;
							break;
						case ArcSeparatorName.point11:
                            if (isLeftBigger.Item2) side = CombatSide.REAR;
                            else side = CombatSide.RIGHTFLANK;
                            break;
                        case ArcSeparatorName.point10:
                            if (isLeftBigger.Item2) side = CombatSide.REAR;
                            else side = CombatSide.LEFTFLANK;
                            break;
                    }
                }
            }			
        }
		return side;        
    }
	private async void ChargeMovement(HitCollider hitCollider, CombatSide combatSide)
	{
		moveForward(hitCollider.distance,false);
		Vector2 vectorForward = new Vector2(affTrans.ForwardVec.X, affTrans.ForwardVec.Y);
		float deltaAngleDegrees = (float)CloseTheDoor(hitCollider.HitObject.unit, combatSide);
		
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
	/// TODO: check collisions on closing the door, ye, annoying
	/// </summary>
	/// <param name="collidedUnit">the collided unit</param>
	/// <returns>Degrees needed for the rotation</returns>
	private double CloseTheDoor(BaseUnit collidedUnit, CombatSide combatSide)
	{

		float angleenemy = collidedUnit.Transform.currentAngleDegrees;

        switch (combatSide)
		{
            case CombatSide.FRONT:
				angleenemy += 180;
                break;
            case CombatSide.LEFTFLANK:
                angleenemy -= 90;
				break;
            case CombatSide.RIGHTFLANK:
                angleenemy += 90;
                break;
            case CombatSide.REAR:
				break;
            
                
        }
		float angleEnemy = angleenemy % 360;
		

		float currentAngle = (float)affTrans.currentAngleDegrees;

		currentAngle = currentAngle >= 0 ? currentAngle : 360 + currentAngle;
		float angle = angleEnemy - currentAngle;
		if (angle > 180)
		{
			angle = angle - 360;
		}else if (angle<-180) {
            angle = 360 + angle;
        }
        return angle;

	}
	private void createChargingResponseBillboard()
	{
		responseLabel = GD.Load<PackedScene>("res://units/label_response.tscn").Instantiate() as Label3D;
		AddChild(responseLabel);
		responseLabel.Position = new Vector3(center.X, center.Y, 0.1f);		
		responseLabel.Visible = false;
		
	}
	public void showChargingResponseBillboard(ChargeResponse chargeResponse)
	{
		responseLabel.Visible = true;		
		responseLabel.Text = chargeResponse.ToString();

	}
	public void hideChargingResponseBillboard()
	{
		responseLabel.Visible = false;
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
		

		//Node3D original = GetChild<Node3D>(0);
		//original.Position = original.Position + new Vector3(offsetTroop.X/2,-offsetTroop.Y/2,0);
	//	original.Position = new Vector3(0, 0, width);
		AnimationPlayer animationPlayer;
		for (int i = 0; i < troopCount; i++)
		{
			var troop = this.coreUnit.Troops[i];
			PackedScene troopAsset = GD.Load<PackedScene>("res://units/troops/"+coreUnit.Race +"/"+ troop.AssetFile);
			Node3D troopWithBaseNode = (Node3D)troopAsset.Instantiate();			
			
			troopNodes.Add(troopWithBaseNode);
			AddChild(troopWithBaseNode);
			// position in the unit
			int x = i % width;
			int y = i / width;
			troopWithBaseNode.Position = troopWithBaseNode.Position + new Vector3(x*offsetTroop.X, y*-offsetTroop.Y, 0f) + new Vector3(offsetTroop.X / 2, -offsetTroop.Y / 2, 0);
			AddVariationToTroop(troopWithBaseNode);


			try
			{
				animationPlayer = troopWithBaseNode.GetChild(1).GetNode<AnimationPlayer>("AnimationPlayer");
			}catch(Exception e) {
				animationPlayer = null;
			}
			if (animationPlayer != null)
			{
				addModifierSkeleton3D(troopWithBaseNode);
				AnimateTroop(animationPlayer);
			}
		}
	}
	#region ANIMATION
	async Task MoveBacklineToFront(int firstRankDeaths)
	{ 
	
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
