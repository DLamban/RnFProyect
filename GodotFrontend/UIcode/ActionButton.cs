using Godot;
using System;

public partial class ActionButton : MarginContainer
{
	public event Action OnPressed;
	private Button button;
	public override void _Ready()
	{
		button = GetNode<Button>("ActionButton");
		button.Pressed += Click;
	}
	// just for clarity and refactoring
	private void Click()
	{
		OnPressed?.Invoke();
	}
	public void initBtn(string label)
	{
		button?.SetText(label);
	}
}
