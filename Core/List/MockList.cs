using Core.DB.Data;
using Core.GameLoop;
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
       
        // Start positions Orcs
        Vector2 startPosRatis = new Vector2(-1f, 0);
        Vector2 startPosRatis2 = new Vector2(2.75f, -0.6f);
        Vector2 startPosAbomination = new Vector2(5, 0);
        Vector2 startPosGoblins = new Vector2(6f, 0.14f);
        Vector2 startBoarRiders = new Vector2(-2.35f, -0.94f);
        Vector2 startHeavyOrcs = new Vector2(1.05f, -1.14f);
        // Start positions Dwarfs
        Vector2 startPosDwarfos = new Vector2(0.5f,0.65f);
        Vector2 startPosSlayers = new Vector2(2.8f, 0.65f);
        Vector2 startPosCannon = new Vector2(-.5f, 1.65f);
        Vector2 startPosGyros = new Vector2(4.5f, 1.35f);
        Vector2 startPosKing = new Vector2(5.8f, 0.65f);
        Vector2 startPosElders = new Vector2(5.8f, 1.25f);
        Vector2 startPosCrossbow = new Vector2(-1.8f, 1.25f);
        public List<UnitSpawnDTO> playerunitsParamsToCreateandSpawn;
        

        public MockList(int numberList)
        {
       
            switch (numberList)
            {
                case 1:
                    MockList1();
                    break;
                case 2:
                    MockListNetcode();
                    break;
                default:
                    MockList1();
                    break;
            }
        }

        private Vector2 rotationToDirectorVec(float rotationDeg)
        {
            float radians = rotationDeg * (float)(Math.PI / 180);
            return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }
        private void MockListNetcode()
        {
            if (PlayerInfoNetcode.Instance.playerSpot == PlayerSpotEnum.PLAYER1)
            {
                playerunitsParamsToCreateandSpawn = new List<UnitSpawnDTO>()
                {
                    new UnitSpawnDTO()
                    {
                        UnitTypeEnum = UnitEnum.Goblins,
                        Characters = new List<CharacterEnum>() { CharacterEnum.Goblin_Wizard },
                        WidthRank = 5,
                        UnitCount = 17,
                        posVec = startPosGoblins,
                        DirectorVec = rotationToDirectorVec(15),
                        UnitGuid = Guid.NewGuid() // No olvides el GUID si la interfaz lo pide
                    },
                    new UnitSpawnDTO()
                    {
                        UnitTypeEnum = UnitEnum.Orcs,
                        Characters = new List<CharacterEnum>(),
                        WidthRank = 5,
                        UnitCount = 20,
                        posVec = startPosRatis2,
                        DirectorVec = rotationToDirectorVec(20),
                        UnitGuid = Guid.NewGuid()
                    },
                    new UnitSpawnDTO()
                    {
                        UnitTypeEnum = UnitEnum.Boar_Riders,
                        Characters = new List<CharacterEnum>(),
                        WidthRank = 5,
                        UnitCount = 10,
                        posVec = startBoarRiders,
                        DirectorVec = rotationToDirectorVec(-20),
                        UnitGuid = Guid.NewGuid()
                    },
                    new UnitSpawnDTO()
                    {
                        UnitTypeEnum = UnitEnum.Heavy_Orcs,
                        Characters = new List<CharacterEnum>() { CharacterEnum.Warlord_Black_Orc },
                        WidthRank = 5,
                        UnitCount = 18,
                        posVec = startHeavyOrcs,
                        DirectorVec = rotationToDirectorVec(20),
                        UnitGuid = Guid.NewGuid()
                    }

                };
                foreach (var unitParam in playerunitsParamsToCreateandSpawn)
                {
                    BaseUnit unit = UnitsServerManager.CreateNewUnit(unitParam);
                    UnitsServerManager.addPlayerUnit(unit);
                }
            } else if (PlayerInfoNetcode.Instance.playerSpot == PlayerSpotEnum.PLAYER2)
            {
                //// PLAYER2
                playerunitsParamsToCreateandSpawn = new List<UnitSpawnDTO>
{
                    new UnitSpawnDTO { 
                        UnitTypeEnum = UnitEnum.Dwarf_Warriors, Characters = new List<CharacterEnum>(), WidthRank = 5, UnitCount = 13, posVec = startPosDwarfos, DirectorVec = rotationToDirectorVec(160), UnitGuid = Guid.NewGuid() },
                    new UnitSpawnDTO { 
                        UnitTypeEnum = UnitEnum.Crossbowmen, Characters = new List<CharacterEnum>(), WidthRank = 5, UnitCount = 11, posVec = startPosCrossbow, DirectorVec = rotationToDirectorVec(175), UnitGuid = Guid.NewGuid() },
                    new UnitSpawnDTO { 
                        UnitTypeEnum = UnitEnum.Cannon, Characters = new List<CharacterEnum>(), WidthRank = 1, UnitCount = 1, posVec = startPosCannon, DirectorVec = rotationToDirectorVec(190), UnitGuid = Guid.NewGuid() },
                    new UnitSpawnDTO { 
                        UnitTypeEnum = UnitEnum.Slayers, Characters = new List<CharacterEnum> { CharacterEnum.King_Dwarf }, WidthRank = 5, UnitCount = 9, posVec = startPosSlayers, DirectorVec = rotationToDirectorVec(200), UnitGuid = Guid.NewGuid() },
                    new UnitSpawnDTO { 
                        UnitTypeEnum = UnitEnum.Gyrocopter, Characters = new List<CharacterEnum>(), WidthRank = 1, UnitCount = 1, posVec = startPosGyros, DirectorVec = rotationToDirectorVec(190), UnitGuid = Guid.NewGuid() },
                    new UnitSpawnDTO { 
                        UnitTypeEnum = UnitEnum.Character_Unit, 
                        Characters = new List<CharacterEnum> { CharacterEnum.King_Dwarf_on_shield }, WidthRank = 0, UnitCount = 0, posVec = startPosKing, DirectorVec = rotationToDirectorVec(200), UnitGuid = Guid.NewGuid() },
                    new UnitSpawnDTO { 
                        UnitTypeEnum = UnitEnum.Elder_Dwarfs, 
                        Characters = new List<CharacterEnum> { CharacterEnum.King_Dwarf }, WidthRank = 5, UnitCount = 12, posVec = startPosElders, DirectorVec = rotationToDirectorVec(170), UnitGuid = Guid.NewGuid() }
                };
                foreach (var unitParam in playerunitsParamsToCreateandSpawn)
                {
                    BaseUnit unit = UnitsServerManager.CreateNewUnit(unitParam);
                    UnitsServerManager.addPlayerUnit(unit);
                }
            }

        }
        private void MockList1()
        {

            
            IUnitCreateAndSpawnParams goblinsParam = new UnitSpawnDTO()
            {
                UnitTypeEnum = UnitEnum.Goblins,
                Characters = new List<CharacterEnum>() { CharacterEnum.Goblin_Wizard },
                WidthRank = 5,
                UnitCount = 17,
                posVec = startPosGoblins,
                DirectorVec = rotationToDirectorVec(15),
            };
            BaseUnit goblins = UnitsServerManager.CreateNewUnit(goblinsParam);
            UnitsServerManager.addPlayerUnit(goblins);


            BaseUnit orcs = UnitsServerManager.CreateNewUnit(UnitEnum.Orcs, new List<CharacterEnum>(), 5, 20, startPosRatis2, 20);
            UnitsServerManager.addPlayerUnit(orcs);



            BaseUnit boarRiders = UnitsServerManager.CreateNewUnit(UnitEnum.Boar_Riders, new List<CharacterEnum>(), 5, 10, startBoarRiders, -20);
            UnitsServerManager.addPlayerUnit(boarRiders);

            //BaseUnit dragon = unitManagerCore.CreateNewUnit("Wyvern", new List<CharacterEnum>(), 1, 1, startPosAbomination, 15);
            //unitManagerCore.addPlayerUnit(dragon);



            BaseUnit HeavyOrcs = UnitsServerManager.CreateNewUnit(UnitEnum.Heavy_Orcs, new List<CharacterEnum>() { CharacterEnum.Warlord_Black_Orc}, 5, 18, startHeavyOrcs, 20);
            UnitsServerManager.addPlayerUnit(HeavyOrcs);



            //// PLAYER2
            ///DWARSSSSSSSSSSS
            BaseUnit dwarfos = UnitsServerManager.CreateNewUnit(UnitEnum.Dwarf_Warriors, new List<CharacterEnum>(), 5, 13, startPosDwarfos, 160);
            UnitsServerManager.addEnemyUnit(dwarfos);
            BaseUnit crossbow = UnitsServerManager.CreateNewUnit(UnitEnum.Crossbowmen, new List<CharacterEnum>(), 5, 11, startPosCrossbow, 175);
            UnitsServerManager.addEnemyUnit(crossbow);

            BaseUnit cannon = UnitsServerManager.CreateNewUnit(UnitEnum.Cannon, new List<CharacterEnum>(), 1, 1, startPosCannon, 190);
            UnitsServerManager.addEnemyUnit(cannon);

            BaseUnit slayers = UnitsServerManager.CreateNewUnit(UnitEnum.Slayers, new List<CharacterEnum>() { CharacterEnum.King_Dwarf }, 5, 9, startPosSlayers, 200);
            UnitsServerManager.addEnemyUnit(slayers);


            BaseUnit gyros = UnitsServerManager.CreateNewUnit(UnitEnum.Gyrocopter, new List<CharacterEnum>(), 1, 1, startPosGyros, 190);
            UnitsServerManager.addEnemyUnit(gyros);





            BaseUnit kingDwarfOnShield = UnitsServerManager.CreateNewUnit(UnitEnum.Character_Unit, 
                new List<CharacterEnum>() { CharacterEnum.King_Dwarf_on_shield }, 
                0, 0, startPosKing, 200);


            UnitsServerManager.addEnemyUnit(kingDwarfOnShield);
            BaseUnit elders = UnitsServerManager.CreateNewUnit(UnitEnum.Elder_Dwarfs, new List<CharacterEnum>() { CharacterEnum.King_Dwarf}, 5, 12, startPosElders, 170);
            UnitsServerManager.addEnemyUnit(elders);
        }
    }
}
