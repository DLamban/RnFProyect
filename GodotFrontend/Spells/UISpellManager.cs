using Core.List;
using Core.Magic;
using Godot;
using System;
using System.Collections.Generic;

public partial class UISpellManager : PanelContainer
{

	SpellManager spellManager { get; set; }
	List<SpellCard> spellCards = new List<SpellCard>();
	public override void _Ready()
	{
		Button closeButton = GetNode<Button>("Panel/CloseBtn");
		closeButton.Pressed += ()=> CloseMagicSelector();
		// start with the basic mock magic school		
		HBoxContainer listSpellsUI = GetNode<HBoxContainer>("SpellsCenterContainer/SpellListHBox");
		PackedScene spell_scn = GD.Load<PackedScene>("res://Spells/spell_card.tscn");
        SpellManager spellManager = SpellManager.Instance;
		//instantiate
		spellManager.OnSpellUsed += spellUsed;
        foreach (Spell spell in spellManager.getSpellsByWizardLevelAndSchool(3, spellManager.magicSchools["Battle Magic"]))
			{
				SpellCard spell_card = spell_scn.Instantiate() as SpellCard;
				spellCards.Add(spell_card);
				spell_card.SetSpell(spell);
				listSpellsUI.AddChild(spell_card);
				spell_card.OnCastingSpell += CastingSpell;
			}

	}
	private void spellUsed(Spell spell)
	{
		foreach (var spellCard in spellCards) {
			if (spellCard.spell == spell) { 
				spellCard.Visible = false;
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
