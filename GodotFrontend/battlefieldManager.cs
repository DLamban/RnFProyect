using Core.GameLoop;
using Godot;
using System;

public partial class battlefieldManager : Node3D
{
	private Node3D gimballCamera;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        gimballCamera = GetNode<Node3D>("Battlefield/Environment/gimball");
		if (PlayerInfoSingleton.Instance.playerSpot == PlayerSpotEnum.PLAYER1)
		{
			// do nothing, camera as default
		}
		else if (PlayerInfoSingleton.Instance.playerSpot == PlayerSpotEnum.PLAYER2)
		{
            // change camera position to reflect battlefield
            gimballCamera.Position = new Vector3(5, 6.5f, 5);
            gimballCamera.RotateZ(Mathf.Pi);// 180 degrees
			
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
