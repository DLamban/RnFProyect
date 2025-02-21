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
        Vector2 startPosCannon = new Vector2(1.25f, 1.35f);
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

            

            BaseUnit goblins = unitManagerCore.CreateNewUnit(UnitEnum.Goblins,new List<CharacterEnum>() { CharacterEnum.Goblin_Wizard}, 5, 17, startPosGoblins, 15);
            unitManagerCore.addPlayerUnit(goblins);


            BaseUnit orcs = unitManagerCore.CreateNewUnit(UnitEnum.Orcs, new List<CharacterEnum>(), 5, 20, startPosRatis2, 20);
            unitManagerCore.addPlayerUnit(orcs);



            BaseUnit boarRiders = unitManagerCore.CreateNewUnit(UnitEnum.Boar_Riders, new List<CharacterEnum>(), 5, 10, startBoarRiders, -20);
            unitManagerCore.addPlayerUnit(boarRiders);

            //BaseUnit dragon = unitManagerCore.CreateNewUnit("Wyvern", new List<CharacterEnum>(), 1, 1, startPosAbomination, 15);
            //unitManagerCore.addPlayerUnit(dragon);



            BaseUnit HeavyOrcs = unitManagerCore.CreateNewUnit(UnitEnum.Heavy_Orcs, new List<CharacterEnum>() { CharacterEnum.Warlord_Black_Orc}, 5, 18, startHeavyOrcs, 20);
            unitManagerCore.addPlayerUnit(HeavyOrcs);



            //// PLAYER2
            ///DWARSSSSSSSSSSS
            BaseUnit dwarfos = unitManagerCore.CreateNewUnit(UnitEnum.Dwarf_Warriors, new List<CharacterEnum>(), 5, 13, startPosDwarfos, 130);
            unitManagerCore.addEnemyUnit(dwarfos);

            BaseUnit crossbow = unitManagerCore.CreateNewUnit(UnitEnum.Crossbowmen, new List<CharacterEnum>(), 5, 11, startPosCrossbow, 175);
            unitManagerCore.addEnemyUnit(crossbow);

            BaseUnit cannon = unitManagerCore.CreateNewUnit(UnitEnum.Cannon, new List<CharacterEnum>(), 1, 1, startPosCannon, 190);
            unitManagerCore.addEnemyUnit(cannon);

            BaseUnit slayers = unitManagerCore.CreateNewUnit(UnitEnum.Slayers, new List<CharacterEnum>() { CharacterEnum.King_Dwarf }, 5, 9, startPosSlayers, 200);
            unitManagerCore.addEnemyUnit(slayers);


            BaseUnit gyros = unitManagerCore.CreateNewUnit(UnitEnum.Gyrocopter, new List<CharacterEnum>(), 1, 1, startPosGyros, 190);
            unitManagerCore.addEnemyUnit(gyros);





            BaseUnit kingDwarfOnShield = unitManagerCore.CreateNewUnit(UnitEnum.Character_Unit, 
                new List<CharacterEnum>() { CharacterEnum.King_Dwarf_on_shield }, 
                0, 0, startPosKing, 200);


            unitManagerCore.addEnemyUnit(kingDwarfOnShield);
            BaseUnit elders = unitManagerCore.CreateNewUnit(UnitEnum.Elder_Dwarfs, new List<CharacterEnum>() { CharacterEnum.King_Dwarf}, 5, 12, startPosElders, 170);
            unitManagerCore.addEnemyUnit(elders);
        }
    }
}
