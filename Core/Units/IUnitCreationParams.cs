using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Core.Networking;

namespace Core.Units
{
    public interface IUnitCreationParams
    {
        UnitEnum UnitTypeEnum { get; }
        List<CharacterEnum> Characters { get; }
        int WidthRank { get; }
        int UnitCount { get; }
        Guid UnitGuid { get; }
    }
    public interface IUnitCreateAndSpawnParams : IUnitCreationParams
    {
        Vector2 posVec { get; }
        Vector2 DirectorVec { get; }

    }
}
