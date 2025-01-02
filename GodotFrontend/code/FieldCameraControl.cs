using Godot;
using System;
using Core;
public partial class FieldCameraControl : Node3D
{
	Vector3 velocity = new Vector3(0, 0, 0);
	Vector3 direction = new Vector3(0, 0, 0);
	float acceleration = 5;
	float deceleration = -1;
	float vel_Max = 4;
	float vel_multiplier = 10;
	float rotation = 0;
	float rotation_speed = 1.0f;	
	ShaderMaterial postProcessShader;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MeshInstance3D postProcessMesh = GetNode<MeshInstance3D>("BattlefieldCamera/posprocessquad");
		postProcessShader = postProcessMesh.Mesh.SurfaceGetMaterial(0) as ShaderMaterial;
		
	}
	private void updateCamera(double delta)
	{		
		
		Vector3 offset = (direction.Normalized() * (float)(acceleration * delta * vel_multiplier)) + (velocity.Normalized() * (float)(deceleration * delta * vel_multiplier));
		if (direction == Vector3.Zero && offset.LengthSquared() > velocity.LengthSquared())
		{
			velocity = Vector3.Zero;
		}
		else
		{
			//clamp to min max speed, we have a classic strafe jump case, don't care
			velocity.X = Mathf.Clamp(velocity.X + offset.X, -vel_multiplier, vel_multiplier);
			velocity.Y = Mathf.Clamp(velocity.Y + offset.Y, -vel_multiplier, vel_multiplier);
			velocity.Z = Mathf.Clamp(velocity.Y + offset.Z, -vel_multiplier, vel_multiplier);
			Translate(direction * (float)(acceleration * delta));
		}


		 
		//postProcessShader.SetShaderParameter("focal_point", new Vector3(GlobalPosition.X,GlobalPosition.Y+01.1f,.00f));
	  
		
		RotateZ((float)(rotation *  rotation_speed * delta));

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)	
	{
		// We get the input as float because we want dat juicy controller input
		float rot_left = 0; float forward = 0; float rot_right = 0;
		float left = 0;     float backward= 0;     float right = 0;

		float zoom_in = 0; 
		float zoom_out = 0;	
		if (Input.IsActionPressed("camera_left"))
		{
			left =  Input.GetActionStrength("camera_left");
		} 
		if (Input.IsActionPressed("camera_right")){
			right = Input.GetActionStrength("camera_right");
		}
		if (Input.IsActionPressed("camera_forward"))
		{
			forward = Input.GetActionStrength("camera_forward");
		}
		if (Input.IsActionPressed("camera_backward"))
		{
			backward = Input.GetActionStrength("camera_backward");
		}
		if (Input.IsActionJustReleased("camera_zoom_in"))
		{
			zoom_in = 1;
		}
		if (Input.IsActionJustReleased("camera_zoom_out"))
		{
			zoom_out = 1;
		}
		// ROTATION
		if (Input.IsActionPressed("camera_rot_left"))
		{
			rot_left = Input.GetActionStrength("camera_rot_left");
		}
		if (Input.IsActionPressed("camera_rot_right"))
		{
			rot_right = Input.GetActionStrength("camera_rot_right");
		}

		direction.X = right - left; 
		direction.Y = forward - backward;
		direction.Z = zoom_out - zoom_in;
		rotation =   rot_left - rot_right; 

		updateCamera(delta);
	}
}
