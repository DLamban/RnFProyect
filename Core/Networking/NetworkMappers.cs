using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Aranfee;
using Core.Units;

namespace Core.Networking
{
    static class NetworkMappers
    {
        public static UnitDefinition MapUnitToProtodefinition(IUnitCreateAndSpawnParams spawnParams)
        {
            UnitDefinition unitDef = new UnitDefinition()
            {
                UnitTypeEnum = (int)spawnParams.UnitTypeEnum,
                WidthRank = spawnParams.WidthRank,
                UnitCount = spawnParams.UnitCount,
                UnitGuid = spawnParams.UnitGuid.ToString(),

                PosVec = new Vector2Proto { X = spawnParams.posVec.X, Y = spawnParams.posVec.Y },
                DirectorVec = new Vector2Proto { X = spawnParams.DirectorVec.X, Y = spawnParams.DirectorVec.Y }
            };

            // Llenamos la lista de personajes
            unitDef.Characters.AddRange(spawnParams.Characters.Select(c => (int)c));

            return unitDef;
        }
        public static List<UnitSpawnDTO> MapProtodefinitionToUnitList(UnitSpawnList unitSpawnList)
        {
            var dtoList = new List<UnitSpawnDTO>();

            foreach (var proto in unitSpawnList.Units)
            {
                var dto = new UnitSpawnDTO()
                {
                    UnitTypeEnum = (UnitEnum)proto.UnitTypeEnum,
                    Characters = proto.Characters.Select(c => (CharacterEnum)c).ToList(),
                    WidthRank = proto.WidthRank,
                    UnitCount = proto.UnitCount,
                    UnitGuid = Guid.Parse(proto.UnitGuid),
                    posVec = new Vector2(proto.PosVec.X, proto.PosVec.Y),
                    DirectorVec = new Vector2(proto.DirectorVec.X, proto.DirectorVec.Y)
                };

                dtoList.Add(dto);
            }

            return dtoList;
        }
        public static void HandleUnitSpawn(UnitDefinition unitDef)
        {            
            var guid = Guid.Parse(unitDef.UnitGuid);
            var pos = new System.Numerics.Vector2(unitDef.PosVec.X, unitDef.PosVec.Y);
            var dir = new System.Numerics.Vector2(unitDef.DirectorVec.X, unitDef.DirectorVec.Y);                        
        }
    }
}
