using System;
using System.Collections.Generic;
using System.Linq;
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

                // Mapeo a los sub-mensajes de Proto
                PosVec = new Vector2Proto { X = spawnParams.posVec.X, Y = spawnParams.posVec.Y },
                DirectorVec = new Vector2Proto { X = spawnParams.DirectorVec.X, Y = spawnParams.DirectorVec.Y }
            };

            // Llenamos la lista de personajes
            unitDef.Characters.AddRange(spawnParams.Characters.Select(c => (int)c));

            return unitDef;
        }
        public static void HandleUnitSpawn(UnitDefinition unitDef)
        {
            // Reconstruimos los tipos de .NET/Godot desde el Proto
            var guid = Guid.Parse(unitDef.UnitGuid);
            var pos = new System.Numerics.Vector2(unitDef.PosVec.X, unitDef.PosVec.Y);
            var dir = new System.Numerics.Vector2(unitDef.DirectorVec.X, unitDef.DirectorVec.Y);

            // Aquí podrías crear un objeto anónimo o un DTO que implemente IUnitCreateAndSpawnParams
            // Y finalmente llamar a tu función estrella:
            // BaseUnit unit = instantiateUnit(reconstructedParams);
        }
    }
}
