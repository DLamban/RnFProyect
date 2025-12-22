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
using GodotFrontend.code.Input;
using Core.DB.Models;

public partial class UnitRenderCreator : Node
{
	InputManager inputManager;
	public UICanvas UICanvas { get; set; }
	/// TEST/DEBUG VARS
	public Dictionary<Guid, UnitGodot> units = new Dictionary<Guid, UnitGodot>();
	Action loadedList;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		inputManager = GetParent<InputManager>();
	//	CreateAllUnits();
		CanvasLayer canvasLayer = GetNode<CanvasLayer>("HUD") as CanvasLayer;
		UICanvas = canvasLayer as UICanvas;
		BindSignalsHUD(canvasLayer);
		
		
		// We need to wait for the ready function, annoying
		if (HotSeatManager.Instance.isHotseat)
		{
			HotSeatManager.Instance.populateBoard();
		}
		else
		{
			CreateAllUnits();
		}
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
	private void CreateAllUnits(){
		foreach(BaseUnit unit in UnitsClientManager.Instance.unitsPlayer.Values)
		{
			UnitGodot tempunit = createUnitToRender(unit);
			units.Add(unit.Guid, tempunit);
		}
		foreach (BaseUnit unit in UnitsClientManager.Instance.unitsEnemy.Values)
		{
			UnitGodot tempunit = createUnitToRender(unit);
			units.Add(unit.Guid, tempunit);
		}

	}
	private void deselectAllUnits()
	{
	//	throw new NotImplementedException();
	}
	public UnitGodot findUnit(Guid guid)
	{
		return units[guid];
	}
	/// <summary>
	/// With this we create the unit, we build it in the core and representation in godot
	/// </summary>
	private UnitGodot createUnitToRender(BaseUnit unit)
	{
		// load troop asset 
		PackedScene unitAsset = GD.Load<PackedScene>("res://units/troops/base_unit.tscn");

		UnitGodot unitToSpawn  = (UnitGodot)unitAsset.Instantiate();

		// Subcribe to the unit selection action so we can deselect the other units
		unitToSpawn.unitSelection += deselectAllUnits;;
		
		unitToSpawn.initGodotUnit(unit, inputManager);
		CallDeferred("add_child", unitToSpawn);

		unitToSpawn.updateTransformToRender(true);

		return unitToSpawn;

	}
	public void disableMoveInputAllTroops()
	{
		foreach (KeyValuePair<Guid, UnitGodot> unitTuple in units)
		{
			unitTuple.Value.inputEnabled = false;
		}

	}
	public void disableOutOfCombatTroops()
	{
		foreach (KeyValuePair<Guid, UnitGodot> unitTuple in units)
		{
			if (!unitTuple.Value.coreUnit.temporalCombatVars.isInCombatRange && UnitsClientManager.Instance.isPlayerUnit(unitTuple.Key))
			{
				grayscaleUnit(unitTuple.Value);
			}
		}
	}
	//gray out the units that can't shoot
	public void disableNoShootingTroops()	
	{
		foreach(KeyValuePair<Guid, UnitGodot> unitTuple in units)
		{
			if (!unitTuple.Value.coreUnit.canShoot && UnitsClientManager.Instance.isPlayerUnit(unitTuple.Key))
			{
			grayscaleUnit(unitTuple.Value);
			}
		}
	}
	public void enableTroops()
	{
		foreach (KeyValuePair<Guid, UnitGodot> unitTuple in units)
		{
			if (UnitsClientManager.Instance.isPlayerUnit(unitTuple.Key))
			{
				removeGrayscale(unitTuple.Value);
			}
		}
	}
	private void grayscaleUnit(UnitGodot unit)
	{
		ShaderMaterial grayscaleShader = new ShaderMaterial();

		grayscaleShader.Shader = GD.Load<Shader>("res://shaders/grayscale.gdshader");
		grayscaleMesh(unit, grayscaleShader);
	}
	private void removeGrayscale(Node3D node) 
	{

		foreach (Node child in node.GetChildren())
		{
			if (child is Node3D childNode3D)
			{
				removeGrayscale(childNode3D);
			}
		}
		// Luego aplicarlo al nodo actual si es un MeshInstance3D
		if (node is MeshInstance3D mesh)
		{

			for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
			{
				if (mesh.GetSurfaceOverrideMaterial(i) != null)
				{
					mesh.SetSurfaceOverrideMaterial(i, null);
				}
			}
		}
	}
	private void grayscaleMesh(Node3D node, ShaderMaterial shaderMaterial)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is Node3D childNode3D)
			{
				grayscaleMesh(childNode3D, shaderMaterial);
			}
		}
		// Luego aplicarlo al nodo actual si es un MeshInstance3D
		if (node is MeshInstance3D mesh)
		{
			// very sloppy
			for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
			{
				if (mesh.GetActiveMaterial(i) != null)
				{
					Material copy = mesh.GetActiveMaterial(i).Duplicate() as Material;
					copy.NextPass = shaderMaterial;
					mesh.SetSurfaceOverrideMaterial(i, copy);
				}
			}
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
