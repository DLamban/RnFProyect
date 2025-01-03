using Core.Magic;
using Godot;
using GodotFrontend.code.Input;
using System;

public partial class SpellCard : Panel
{
	public Spell spell;
	public EventHandler OnCastingSpell;
	public void SetSpell(Spell _spell)
	{
		spell = _spell;
		// TITLE
		Label title = GetNode<Label>("CenterContainer/MarginContainer/VBoxContainer/MarginContainer/Title");
		title.Text = spell.Name;
		// IMAGE
		var image = GetNode<TextureRect>("CenterContainer/MarginContainer/VBoxContainer/Panel/PanelContainer/TextureRect");
		image.Texture = (Texture2D)GD.Load<Texture>("res://assets/UI/Spells/" + spell.Image);

		//Description
		Label description = GetNode<Label>("CenterContainer/MarginContainer/VBoxContainer/Description");
		description.Text = spell.Description;

		//Difficulty
		Label difficulty = GetNode<Label>("CenterContainer/MarginContainer/VBoxContainer/Difficulty");
		difficulty.Text = "Difficulty: " + spell.Difficulty +"+";

		//Range
		Label range = GetNode<Label>("CenterContainer/MarginContainer/VBoxContainer/Range");
		if (spell.Range == 0)
			range.Text = "Range: Self";
		else{
			range.Text = "Range: " + spell.Range + "''";
		}
		// Type
		Label type = GetNode<Label>("CenterContainer/MarginContainer/VBoxContainer/Type");
		type.Text = "Type: " + spell.Type;				
	}
	public override void _Ready()
	{
		Button castButton = GetNode<Button>("CenterContainer/MarginContainer/VBoxContainer/MarginContainer2/CastBtn");
		castButton.Pressed += ()=> {
			CastSpell();
		};
	}
	private void CastSpell()
	{		
		// getting this node the dirty way, it happens few times, so it's oki
		InputManager inputManager = GetTree().CurrentScene.GetNode<Node3D>("Battlefield") as InputManager;
		inputManager.SpellSelection(spell.Target, spell);
		OnCastingSpell.Invoke(this,null);
	}
	

}
