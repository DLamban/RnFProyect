using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputFSM;

namespace GodotFrontend.code.Input
{
    public interface ISubInputManager
    {
        public void CustomProcess(double delta);
    }
}
