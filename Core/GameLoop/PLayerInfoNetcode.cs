using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.GameLoop
{
    public class PLayerInfoNetcode
    {
        private static readonly PlayerInfo instance = new PlayerInfo();
        public static PlayerInfo Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
