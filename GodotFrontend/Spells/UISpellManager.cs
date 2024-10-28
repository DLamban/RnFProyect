using Core.List;
using Godot;
using System;

public partial class UISpellManager : PanelContainer
{

	public override void _Ready()
	{
		Button closeButton = GetNode<Button>("Panel/CloseBtn");
		closeButton.Pressed += ()=> CloseMagicSelector();
		System.Collections.Generic.Dictionary<string, Core.Magic.MagicSchool> magicSchools = JSONLoader.LoadSpellJSON();
		// start with the basic mock magic school		
		HBoxContainer listSpellsUI = GetNode<HBoxContainer>("SpellsCenterContainer/SpellListHBox");
		PackedScene spell_scn = GD.Load<PackedScene>("res://Spells/spell_card.tscn");
		//instantiate
		foreach (var school in magicSchools)
		{
			foreach (var spell in school.Value.Spells)
			{

				SpellCard spell_card = spell_scn.Instantiate() as SpellCard;
				spell_card.SetSpell(spell);
				listSpellsUI.AddChild(spell_card);
				spell_card.OnCastingSpell += CastingSpell;

			}
		}

	}
	private void CastingSpell(object sender, EventArgs e)
	{
		this.Visible = false;
	}
	private void CloseMagicSelector()
	{
		this.Visible = false;
	}


}
