using Godot;
using GodotFrontend.code.Input;
using System;

public partial class ConfirmPanel : Control
{
	private InputManager inputManager;
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		inputManager = GetTree().CurrentScene.GetNode<Node3D>("Battlefield") as InputManager;
		await ToSignal(inputManager, SignalName.Ready);

		inputManager.inputMagic.OnOpenConfirmMenu += openPanel;
		Button confirmBtn = GetNode<Button>("Panel/HBoxContainer/Confirm");
        Button cancelBtn = GetNode<Button>("Panel/HBoxContainer/Cancel");
		cancelBtn.Pressed += cancelSpell;
		confirmBtn.Pressed += confirmSpell;
    }
	private void confirmSpell()
	{
		inputManager.inputMagic.executeSpell();
		this.Visible = false;

    }
    private void cancelSpell()
    {
		inputManager.inputMagic.cancelSpell();
        this.Visible = false;
    }
    private void openPanel()
	{
		this.Visible = true;
	}
}
