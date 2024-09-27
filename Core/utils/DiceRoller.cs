using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.utils
{
    public class DiceRoller
    {
        private static Random random = new Random();
        
        public static int rolld6()
        {
            return random.Next(1, 7);
        }


    }
}
