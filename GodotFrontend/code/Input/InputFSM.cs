﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.code.Input
{
    static public class InputFSM
    {
        public enum InputState
        {
            Empty,
            Movement,
            Magic,
            Charging,
            CastingSpell
        }
        public static InputState currentState { get; set; } = InputState.Empty;
        public static void changeState(InputState inputState)
        {
            currentState = inputState;
        }
        
    }
}