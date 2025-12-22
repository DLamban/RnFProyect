using Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.GameLoop
{
    public class PlayerInfoSingleton
    {
        private static readonly PlayerInfo instance = new PlayerInfo();
        public static ClientNetworkController clientNetworkController { get; set; } = new ClientNetworkController();
        public static PlayerInfo Instance
        {
            get
            {
                return HotSeatManager.Instance.getCurrentPlayer();
            }
        }
    }
}
