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
            // NOT IMPLEMENTED
            List<Character> characters = new List<Character>();
            WidthRank = unit.TroopsWidth;
            UnitCount = unit.UnitCount;
            guid = unit.Guid;
            affTransSer = new SerializableAffineTrans(unit.Transform);
            
        }
    }

    public class UnitsServerManager
    {
        public Dictionary<string, BaseUnit> unitsPlayer { get; set; }
        public Dictionary<string, BaseUnit> unitsEnemy { get; set; }


        public UnitsServerManager()
        {
            unitsPlayer = new Dictionary<string, BaseUnit>();
            unitsEnemy = new Dictionary<string, BaseUnit>();
        }
        public List<BaseUnit> createUnits(List<MinimumUnitTransferInfo> minimumUnitTransfers)
        {
            List<BaseUnit> units = new List<BaseUnit>();    
            foreach(MinimumUnitTransferInfo minimumUnitTransferInfo in minimumUnitTransfers)
            {
                units.Add(CreateUnit(minimumUnitTransferInfo));
            }
            return units;
        }
        public BaseUnit CreateUnit(MinimumUnitTransferInfo minimumUnitTransferInfo)
        {
            return CreateNetworkUnit(minimumUnitTransferInfo.UnitEnum,minimumUnitTransferInfo.characters,  minimumUnitTransferInfo.WidthRank, minimumUnitTransferInfo.UnitCount,  minimumUnitTransferInfo.guid, minimumUnitTransferInfo.affTransSer);
        }        
        public BaseUnit CreateNetworkUnit(UnitEnum unitEnum, List<CharacterEnum> characters, int widthRank, int unitCount, Guid guid, SerializableAffineTrans serializableAffineTrans)
        {            
            BaseUnit unit = instantiateUnit(unitEnum, characters,widthRank, unitCount, guid);
            unit.Transform = new AffineTransformCore(serializableAffineTrans);
            return unit;
        }
        public BaseUnit CreateNewUnit(UnitEnum unitEnum, List<CharacterEnum> characters,int widthRank, int unitCount, Vector2 startPos, float rotationDeg)
        {
            Guid guid = Guid.NewGuid();

            BaseUnit unit = instantiateUnit(unitEnum, characters, widthRank,unitCount,guid);
            unit.Transform.offsetX = startPos.X;
            unit.Transform.offsetY = startPos.Y;
            unit.Transform.rotate(rotationDeg, unit.sizeEnclosedRectangledm.X / 2, -unit.sizeEnclosedRectangledm.Y / 2);            
            return unit;
        }
        private BaseUnit instantiateUnit(UnitEnum unitEnum,List<CharacterEnum> characters, int widthRank, int unitCount, Guid guid)
        {

            // MIgrating to db, test first
            
            var unitDetail = DBSingleton.GetUnit(unitEnum);
            BaseUnit baseUnit;
            if (unitCount != 0)
            {
                List<BaseTroop> troops = new List<BaseTroop>();
                for (int i = 0; i < unitCount; i++)
                {
                    BaseTroop baseTroop = new BaseTroop(unitDetail.TroopProfiles.FirstOrDefault(t => t.IsMainProfile != 0));

                    troops.Add(baseTroop);
                }
                baseUnit = new BaseUnit(unitDetail.Race.Code, unitDetail.Name, widthRank, Formation_type.CLOSE_ORDER, new List<string> { "Reglaespecial1", "Reglaespecial2" }, troops);
                foreach (CharacterEnum character in characters)
                {
                    DB.Models.Character character1 = DBSingleton.GetCharacter(character);
                    Character cahracterfromdb = new Character(character1);
                    baseUnit.AddCharacter(cahracterfromdb);
                }
            }
            else// it's single char unit
            {
                DB.Models.Character characterDb = DBSingleton.GetCharacter(characters[0]);
                Character cahracterfromdb = new Character(characterDb);
                    
                baseUnit = new BaseUnit(characterDb.Race.Code, characterDb.Name, cahracterfromdb);
            }


               
            baseUnit.Guid = guid;
            return baseUnit;
            
           
            
        }
        
        public void addPlayerUnit(BaseUnit unit)
        {   
            unitsPlayer[unit.Guid.ToString()] = unit;
        }

        public void addEnemyUnit(BaseUnit unit)
        {
            Guid guid = Guid.NewGuid();            
            unit.Guid = guid;
            unitsEnemy[unit.Guid.ToString()] = unit;
        }
        public List<BaseUnit> getUnitsPlayer()
        {
            List<BaseUnit> units = new List<BaseUnit>();
            foreach (KeyValuePair<string, BaseUnit> unit in unitsPlayer)
            {
                units.Add(unit.Value);
            }
            return units;
        }

        public List<BaseUnit> getUnitsEnemy()
        {
            List<BaseUnit> units = new List<BaseUnit>();
            foreach (KeyValuePair<string, BaseUnit> unit in unitsEnemy)
            {
                units.Add(unit.Value);
            }
            return units;
        }
        public List<MinimumUnitTransferInfo> getPlayerTransUnits()
        {
            List<MinimumUnitTransferInfo> units = new List<MinimumUnitTransferInfo>();
            foreach (KeyValuePair<string, BaseUnit> unit in unitsPlayer)
            {
                MinimumUnitTransferInfo minimumUnitTransferInfo = new MinimumUnitTransferInfo(unit.Value);
                units.Add(minimumUnitTransferInfo);
            }
            return units;
        }
        public List<MinimumUnitTransferInfo> getEnemyTransUnits()
        {
            List<MinimumUnitTransferInfo> units = new List<MinimumUnitTransferInfo>();
            foreach (KeyValuePair<string, BaseUnit> unit in unitsEnemy)
            {
                MinimumUnitTransferInfo minimumUnitTransferInfo = new MinimumUnitTransferInfo(unit.Value);
                units.Add(minimumUnitTransferInfo);
            }
            return units;
        }
        // ALIAS
        // call player1 to player
        public List<BaseUnit> getUnitsPlayer1()
        {
            return getUnitsPlayer();
        }
        // call player2 to enemy
        public List<BaseUnit> getUnitsPlayer2()
        {
            return getUnitsEnemy();
        }
        public List<MinimumUnitTransferInfo> getPlayer1TransUnits()
        {
            return getPlayerTransUnits();
        }
        public List<MinimumUnitTransferInfo> getPlayer2TransUnits()
        {
            return getEnemyTransUnits();
        }
    }
}
