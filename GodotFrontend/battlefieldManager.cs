using Core.GameLoop;
using Godot;
using GodotFrontend.code.UIOverlay;
using System;

public partial class battlefieldManager : Node3D
{
	private Node3D gimballCamera;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gimballCamera = GetNode<Node3D>("Battlefield/Environment/gimball");
		if (PlayerInfoSingletonHotSeat.Instance.playerSpot == PlayerSpotEnum.PLAYER1)
		{
			// do nothing, camera as default
		}
		else if (PlayerInfoSingletonHotSeat.Instance.playerSpot == PlayerSpotEnum.PLAYER2)
		{
			// change camera position to reflect battlefield
			gimballCamera.Position = new Vector3(3, 4.5f, 4);
			gimballCamera.RotateZ(Mathf.Pi);// 180 degrees
			
		}
		BattlefieldOverlay.Instance.vinculateBattlefield(this);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
