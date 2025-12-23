using Core.DB.Data;
using Core.DB.Models;
using Core.GeometricEngine;
using Core.List;
using Core.Networking;
using Core.Rules;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Units
{
    public class MinimumUnitTransferInfo
    {
        public string Name { get; set; }
        public UnitEnum UnitEnum { get; set; }
        public int WidthRank { get; set; }
        public List<CharacterEnum> characters { get; set; }
        public Guid guid { get; set; }
        public int UnitCount { get; set; }        
        public SerializableAffineTrans affTransSer { get; set; }
        public MinimumUnitTransferInfo()
        {

        }
        public MinimumUnitTransferInfo(BaseUnit unit)
        {
            Name = unit.Name;
            List<Character> characters = new List<Character>();
            WidthRank = unit.WidthRank;
            UnitCount = unit.UnitCount;
            guid = unit.UnitGuid;
            affTransSer = new SerializableAffineTrans(unit.Transform);
            
        }
    }

    public static class UnitsServerManager
    {
        public static Dictionary<string, BaseUnit> unitsPlayer { get; set; }
        public static Dictionary<string, BaseUnit> unitsEnemy { get; set; }


        static UnitsServerManager()
        {
            unitsPlayer = new Dictionary<string, BaseUnit>();
            unitsEnemy = new Dictionary<string, BaseUnit>();
        }
        public static void restartLists()
        {
            unitsPlayer = new Dictionary<string, BaseUnit>();
            unitsEnemy = new Dictionary<string, BaseUnit>();
        }
        public static BaseUnit CreateNewUnit(UnitEnum unitEnum, List<CharacterEnum> characters, int widthRank, int unitCount, Vector2 startPos, float rotationDeg)
        {
            Guid guid = Guid.NewGuid();
            IUnitCreateAndSpawnParams unitCreationParams = new UnitSpawnDTO()
            {
                UnitTypeEnum = unitEnum,
                Characters = characters,
                WidthRank = widthRank,
                UnitCount = unitCount,
                UnitGuid = guid,
                posVec = startPos,
                DirectorVec = new Vector2((float)Math.Cos(rotationDeg * (Math.PI / 180.0)), (float)Math.Sin(rotationDeg * (Math.PI / 180.0)))
            };
            return CreateNewUnit(unitCreationParams);
        }
        
        public static BaseUnit CreateNewUnit(IUnitCreateAndSpawnParams unitCreateAndSpawnParams)
        {
            Guid guid = Guid.NewGuid();

            BaseUnit unit = instantiateUnit(unitCreateAndSpawnParams);
            

            float rotationDeg = (float)(Math.Atan2(unitCreateAndSpawnParams.DirectorVec.Y, unitCreateAndSpawnParams.DirectorVec.X) * (180.0 / Math.PI));

            unit.Transform.offsetX = unitCreateAndSpawnParams.posVec.X;
            unit.Transform.offsetY = unitCreateAndSpawnParams.posVec.Y;
            unit.Transform.rotate(rotationDeg, unit.sizeEnclosedRectangledm.X / 2, -unit.sizeEnclosedRectangledm.Y / 2);            
            return unit;
        }
        private static BaseUnit instantiateUnit(IUnitCreationParams unitCreationParams)
        {

            // MIgrating to db, test first
            
            var unitDetail = DBSingleton.GetUnit(unitCreationParams.UnitTypeEnum);
            BaseUnit baseUnit;
            if (unitCreationParams.UnitCount != 0)
            {
                List<BaseTroop> troops = new List<BaseTroop>();
                for (int i = 0; i < unitCreationParams.UnitCount; i++)
                {
                    BaseTroop baseTroop = new BaseTroop(unitDetail.TroopProfiles.FirstOrDefault(t => t.IsMainProfile != 0));

                    troops.Add(baseTroop);
                }
                baseUnit = new BaseUnit(unitDetail.Race.Code, unitDetail.Name, unitDetail.TroopProfiles.FirstOrDefault(t => t.IsMainProfile != 0).TroopType.Code,
                    unitCreationParams.WidthRank, Formation_type.CLOSE_ORDER, new List<string> { "Reglaespecial1", "Reglaespecial2" }, troops);
                foreach (CharacterEnum character in unitCreationParams.Characters)
                {
                    DB.Models.Character character1 = DBSingleton.GetCharacter(character);
                    Character cahracterfromdb = new Character(character1);
                    baseUnit.AddCharacter(cahracterfromdb);
                }
            }
            else// it's single char unit
            {
                DB.Models.Character characterDb = DBSingleton.GetCharacter(unitCreationParams.Characters[0]);
                Character cahracterfromdb = new Character(characterDb);
                    
                baseUnit = new BaseUnit(characterDb.Race.Code, characterDb.Name, cahracterfromdb);
            }


               
            baseUnit.UnitGuid = unitCreationParams.UnitGuid;
            return baseUnit;
            
           
            
        }
        
        public static void addPlayerUnit(BaseUnit unit)
        {   
            unitsPlayer[unit.UnitGuid.ToString()] = unit;
        }

        public static void addEnemyUnit(BaseUnit unit)
        {            
            unitsEnemy[unit.UnitGuid.ToString()] = unit;
        }
        public static List<BaseUnit> getUnitsPlayer()
        {
            List<BaseUnit> units = new List<BaseUnit>();
            foreach (KeyValuePair<string, BaseUnit> unit in unitsPlayer)
            {
                units.Add(unit.Value);
            }
            return units;
        }

        public static List<BaseUnit> getUnitsEnemy()
        {
            List<BaseUnit> units = new List<BaseUnit>();
            foreach (KeyValuePair<string, BaseUnit> unit in unitsEnemy)
            {
                units.Add(unit.Value);
            }
            return units;
        }
        // call player1 to player
        public static List<BaseUnit> getUnitsPlayer1()
        {
            return getUnitsPlayer();
        }
        // call player2 to enemy
        public static List<BaseUnit> getUnitsPlayer2()
        {
            return getUnitsEnemy();
        }        
    }
}
