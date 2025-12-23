using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.Units
{
    public class UnitSpawnDTO : IUnitCreateAndSpawnParams
    {
        public UnitSpawnDTO() { }
        public UnitEnum UnitTypeEnum { get; set; }
        public List<CharacterEnum> Characters { get; set; }
        public int WidthRank { get; set; }
        public int UnitCount { get; set; }
        public Guid UnitGuid { get; set; }
        public Vector2 posVec { get; set; }
        public Vector2 DirectorVec { get; set; }
            
    }
}
