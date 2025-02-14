using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DB.Data
{
    internal class DBSingleton
    {        
        private static GameDbContext _instance;
        public static GameDbContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameDbContext();
                }
                return _instance;
            }
        }
    }
}
