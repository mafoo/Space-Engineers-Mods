#if DEBUG
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

namespace SpaceEngineers
{
    public sealed class HangerControls : MyGridProgram
    {
#endif
        List<IMyLightingBlock> lightsLeft = new List<IMyLightingBlock>();
        List<IMyLightingBlock> lightsRight = new List<IMyLightingBlock>();
        IMyPistonBase piston1;
        IMyPistonBase piston2;
        IMyTimerBlock timer;

        public Program()
        {
            Echo("initializing...");
            for (int i = 1; i <= 7; i++)
            {
                lightsLeft.Add(GridTerminalSystem.GetBlockWithName("Hanger Door - A" + i) as IMyLightingBlock);
                lightsRight.Add(GridTerminalSystem.GetBlockWithName("Hanger Door - B" + i) as IMyLightingBlock);
            }
            piston1 = GridTerminalSystem.GetBlockWithName("Hanger Door Piston 1") as IMyPistonBase;
            piston2 = GridTerminalSystem.GetBlockWithName("Hanger Door Piston 2") as IMyPistonBase;
            timer = GridTerminalSystem.GetBlockWithName("Timer Hanger Door Lights Off") as IMyTimerBlock;
            Echo("Init complete");
        }

        public void Main(string argument)
        {
            try
            {
                argument = argument.ToLower();
                //Echo("Called with '" + argument + "'");
                if (argument == "open")
                {
                    if (piston2.CurrentPosition == 0)
                    {
                        Echo("Closing Hanger Door");
                        piston1.Velocity = -0.5f;
                        piston2.Velocity = 0.5f;
                        timer.StartCountdown();
                        for (int i = 0; i < 7; i++)
                        {
                            lightsLeft[i].BlinkOffset = i * (1 / 7);
                            lightsLeft[i].Enabled = true;
                            lightsRight[i].BlinkOffset = i * (1 / 7);
                            lightsRight[i].Enabled = true;
                        }
                    }
                    else
                    {
                        Echo("Door already open");
                    }
                }
                if (argument == "close")
                {
                    if (piston2.CurrentPosition == 10)
                    {
                        Echo("Opening Hanger Door");
                        piston1.Velocity = 0.5f;
                        piston2.Velocity = -0.5f;
                        timer.StartCountdown();
                        for (int i = 0; i < 7; i++)
                        {
                            lightsLeft[i].BlinkOffset = (7 - i) * (1 / 7);
                            lightsLeft[i].Enabled = true;
                            lightsRight[i].BlinkOffset = (7 - i) * (1 / 7);
                            lightsRight[i].Enabled = true;
                        }
                    }
                    else
                    {
                        Echo("Door already closed");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Echo("I Crashed:\n");
                Echo(ex.Message + "\n");
                Echo(ex.StackTrace + "\n");
            }
        }
#if DEBUG
    }
}
#endif
