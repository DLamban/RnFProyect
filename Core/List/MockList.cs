using Core.DB.Data;
using Core.Units;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.List
{
    public class MockList
    {
        public UnitsServerManager unitManagerCore { get; set; }
        // Start positions Orcs
        Vector2 startPosRatis = new Vector2(-1f, 0);
        Vector2 startPosRatis2 = new Vector2(2.75f, -0.6f);
        Vector2 startPosAbomination = new Vector2(5, 0);
        Vector2 startPosGoblins = new Vector2(6f, 0.14f);
        Vector2 startBoarRiders = new Vector2(-.5f, -0.94f);
        Vector2 startHeavyOrcs = new Vector2(1.05f, -1.14f);
        // Start positions Dwarfs
        Vector2 startPosDwarfos = new Vector2(0.5f, 1.15f);
        Vector2 startPosSlayers = new Vector2(2.8f, 0.65f);
        Vector2 startPosCannon = new Vector2(2.8f, 1.65f);
        Vector2 startPosGyros = new Vector2(4.5f, 1.35f);
        Vector2 startPosKing = new Vector2(5.8f, 0.65f);
        Vector2 startPosElders = new Vector2(5.8f, 1.25f);
        Vector2 startPosCrossbow = new Vector2(-1.8f, 1.25f);

        public MockList(int numberList)
        {
            unitManagerCore = new UnitsServerManager();
            switch (numberList)
            {
                case 1:
                    MockList1();
                    break;
                default:
                    MockList1();
                    break;
            }
        }
        private void MockList1()
        {

            //EXAMPLE
            //BaseUnit baseunit = new BaseUnit(unitType.Name, widthRank, Formation_type.CLOSE_ORDER, new List<string> { "Reglaespecial1", "Reglaespecial2" }, troops);

            // ORCSSSSSSSSS
            //CharacterEnum goblinWizard = CodexAll.Instance.getCharCodex("Goblin Wizard");
            //List<CharacterEnum> listChar = new List<CharacterEnum>();
            //listChar.Add(goblinWizard);


            //BaseUnit goblins = unitManagerCore.CreateNewUnit("Goblins",new List<CharacterEnum>(), 5, 17, startPosGoblins, 15);
            //unitManagerCore.addPlayerUnit(goblins);
            //BaseUnit orcs = unitManagerCore.CreateNewUnit("Orcs", new List<CharacterEnum>(), 5, 20, startPosRatis2, 20);
            //unitManagerCore.addPlayerUnit(orcs);

            

            //BaseUnit boarRiders = unitManagerCore.CreateNewUnit("Boar Riders", new List<CharacterEnum>(), 5, 10, startBoarRiders, -20);
            //unitManagerCore.addPlayerUnit(boarRiders);
            //BaseUnit dragon = unitManagerCore.CreateNewUnit("Wyvern", new List<CharacterEnum>(), 1, 1, startPosAbomination, 15);
            //unitManagerCore.addPlayerUnit(dragon);

            //CharacterEnum WArlordBalck = CodexAll.Instance.getCharCodex("Warlord Black Orc");
            //List<CharacterEnum> listChar2 = new List<CharacterEnum>();
            //listChar2.Add(WArlordBalck);
            
            BaseUnit HeavyOrcs = unitManagerCore.CreateNewUnit(UnitEnum.Heavy_Orcs, new List<CharacterEnum>() { CharacterEnum.Warlord_Black_Orc}, 5, 18, startHeavyOrcs, 20);
            unitManagerCore.addPlayerUnit(HeavyOrcs);
            //// PLAYER2
            ///DWARSSSSSSSSSSS
            //BaseUnit dwarfos = unitManagerCore.CreateNewUnit("Dwarf Warriors", new List<CharacterEnum>(), 5, 13, startPosDwarfos, 130);
            //unitManagerCore.addEnemyUnit(dwarfos);
            
            //BaseUnit crossbow = unitManagerCore.CreateNewUnit("Crossbowmen", new List<CharacterEnum>(), 5, 11, startPosCrossbow, 175);
            //unitManagerCore.addEnemyUnit(crossbow);

            //BaseUnit slayers = unitManagerCore.CreateNewUnit("Slayers", new List<CharacterEnum>(), 5, 7, startPosSlayers, 200);
            //unitManagerCore.addEnemyUnit(slayers);
            //BaseUnit cannon = unitManagerCore.CreateNewUnit("Cannon", new List<CharacterEnum>(), 1, 1, startPosCannon, 190);
            //unitManagerCore.addEnemyUnit(cannon);

            //BaseUnit gyros = unitManagerCore.CreateNewUnit("Gyrocopters", new List<CharacterEnum>(), 1, 1, startPosGyros, 190);
            //unitManagerCore.addEnemyUnit(gyros);
  
            
   
            
            
            BaseUnit kingDwarfOnShield = unitManagerCore.CreateNewUnit(UnitEnum.Character_Unit, 
                new List<CharacterEnum>() { CharacterEnum.King_Dwarf_on_shield }, 
                0, 0, startPosKing, 200);


            unitManagerCore.addEnemyUnit(kingDwarfOnShield);
            BaseUnit elders = unitManagerCore.CreateNewUnit(UnitEnum.Elder_Dwarfs, new List<CharacterEnum>(), 5, 12, startPosElders, 170);
            unitManagerCore.addEnemyUnit(elders);
        }
    }
}
