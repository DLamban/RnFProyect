using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Core.GameLoop;
using Core.List;
using Core.Rules;
using Core.Units;
using Core.GeometricEngine;

public partial class UnitRenderCreator : Node
{
	InputManager inputManager;
	public UICanvas UICanvas { get; set; }
    /// TEST/DEBUG VARS
	public Dictionary<Guid, Unidad> units = new Dictionary<Guid, Unidad>();
    Action loadedList;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		inputManager = GetParent<InputManager>();
	//	CreateAllUnits();
		CanvasLayer canvasLayer = GetNode<CanvasLayer>("HUD") as CanvasLayer;
        UICanvas = canvasLayer as UICanvas;
		BindSignalsHUD(canvasLayer);
        // We will load the list of units from the server
        loadedList += CreateAllUnits;
        UnitsClientManager.Instance.unitMovedNet = unitNetworkMoved;
        UnitsClientManager.Instance.setLoadedListEvent(loadedList);
		
		// We need to wait for the ready function, annoying
        if (HotSeatManager.Instance.isHotseat) HotSeatManager.Instance.populateBoard();
    }
	public void unitNetworkMoved(Guid guid)
	{
		findUnit(guid).updateTransformToRender(true);
    }
    // DEPRECATED, maybe can use later with other event pattern
    private void BindSignalsHUD(CanvasLayer unitHUD)
	{
		Panel charPanelPlayer = unitHUD.GetNode<Panel>("CanvasGroup/AnchorProvider/CharacteristicsPanelPlayer") as Panel;
		Panel charPanelEnemy = unitHUD.GetNode<Panel>("CanvasGroup/AnchorProvider/CharacteristicsPanelEnemy") as Panel;
	}
	/// Populate with the units, TODO: load from lists and stop the hardcoding
	private void CreateAllUnits(){
		foreach(BaseUnit unit in UnitsClientManager.Instance.unitsPlayer.Values)
		{
			var tempunit = createUnitToRender(unit);
            units.Add(unit.Guid, tempunit);
        }
        foreach (BaseUnit unit in UnitsClientManager.Instance.unitsEnemy.Values)
        {
            var tempunit = createUnitToRender(unit);
            units.Add(unit.Guid, tempunit);
        }

    }
	private void deselectAllUnits()
	{
	//	throw new NotImplementedException();
    }
	public Unidad findUnit(Guid guid)
	{
		return units[guid];
    }
	/// <summary>
	/// With this we create the unit, we build it in the core and representation in godot
	/// </summary>
	private Unidad createUnitToRender(BaseUnit unit)
	{
        // load troop asset 
        PackedScene unitAsset = GD.Load<PackedScene>("res://units/"+unit.Troop.AssetFile);

		Unidad unitToSpawn  = (Unidad)unitAsset.Instantiate();
		// Subcribe to the unit selection action so we can deselect the other units
		unitToSpawn.unitSelection += deselectAllUnits;;
		
		unitToSpawn.initGodotUnit(unit, inputManager);
		//unitToSpawn.bindInputs(inputManager);
		//unitToSpawn.moveUnit(new Vector2((float)unit.Transform.offsetX, (float)unit.Transform.offsetY));
		
		CallDeferred("add_child", unitToSpawn);

		unitToSpawn.updateTransformToRender(true);
        //unitToSpawn.Position = new Vector3(1, 0, 1);
        return unitToSpawn;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}


