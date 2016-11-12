using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace Scripts
{
    public class PowerDiagnostics : MyGridProgram
    {
        #region  
        IMyTextPanel s;
        string lcd = "Status - [PowerDiag]";
        bool onlyThisGrid = true;
        VRageMath.Matrix m;

        public Program()
        {
        }

        void Save()
        {
        }

        void Main(string argument)
        {
            double upThrustPower = 0;
            double downThrustPower = 0;
            double leftThrustPower = 0;
            double rightThrustPower = 0;
            double forwardThrustPower = 0;
            double reverseThrustPower = 0;
            double upThrustMaxPower = 0;
            double downThrustMaxPower = 0;
            double leftThrustMaxPower = 0;
            double rightThrustMaxPower = 0;
            double forwardThrustMaxPower = 0;
            double reverseThrustMaxPower = 0;

            Dictionary<string, double> currentPowerTypes = new Dictionary<string, double>();
            Dictionary<string, double> maxPowerTypes = new Dictionary<string, double>();

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            if (s == null)
                s = GridTerminalSystem.GetBlockWithName(lcd) as IMyTextPanel;
            if (s == null)
                throw new System.Exception("Could not find the lcd with the tag " + lcd);
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);


            Echo("Checking Grids");
            int count = blocks.Count;
            Echo("Found " + count + " blocks");
            if (onlyThisGrid)
            {
                blocks = blocks.FindAll(delegate (IMyTerminalBlock c)
                {
                    if (c.CubeGrid.ToString() == s.CubeGrid.ToString())
                        return true;
                    return false;
                });
                count = blocks.Count;
                Echo("Filtered to " + count + " blocks");
            }
            DisplayLine("Results...", true);
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var block = blocks[i];
                    double maxPower = 0;
                    double currentPower = 0;
                    Dictionary<string, string> details = new Dictionary<string, string>();
                    List<ITerminalProperty> properties = new List<ITerminalProperty>();

                    block.GetProperties(properties);
                    if (block.DetailedInfo != "") {
                        string[] attrLines = block.DetailedInfo.Split('\n');
                        try
                        {
                            for (int d = 0; d < attrLines.Length; d++)
                            {
                                try
                                {
                                    string[] values = attrLines[d].Split(':');
                                    if (values.Length == 2)
                                    {
                                        details.Add(values[0], values[1].TrimStart(' '));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Echo("\n\nError in iterating attributes at index '"+d+"' at '" + block.CustomName + "' \n" + ex.Message);
                                }
                            }
                            string tempString;
                            details.TryGetValue("Max Required Input", out tempString);
                            if (tempString != null)
                                maxPower = ParseNumber(tempString);
                            details.TryGetValue("Required Input", out tempString);
                            if (tempString != null)
                                currentPower = ParseNumber(tempString);
                            details.TryGetValue("Current Input", out tempString);
                            if (tempString != null)
                                currentPower = ParseNumber(tempString);
                            if (currentPower > maxPower)
                                maxPower = currentPower;
                        }
                        catch (Exception ex)
                        {
                            Echo("\n\nError in attributes loop for '" + block.CustomName + "' \n" + ex.Message);
                            Echo("found " + attrLines.Length + " items ...\n'" + block.DetailedInfo + "'");
                        }
                    }

                    //DisplayLine(block.CustomName);
                    //DisplayLine("\nDetailed Info:");
                    //DisplayLine(block.DetailedInfo);

                    string[] types = block.GetType().ToString().Split('.');
                    string type = types[types.Length-1].Substring(2);
                    if (type == "Thrust")
                    {
                        currentPower = maxPower * block.GetValueFloat("CurrentThrust");
                        block.Orientation.GetMatrix(out m);
                        if (m.M32 == -1)//---------------------------------------------------------down
                        {
                            downThrustPower += currentPower;
                            downThrustMaxPower += maxPower;
                        }
                        else if (m.M32 == 1)//---------------------------------------------------------up
                        {
                            upThrustPower += currentPower;
                            upThrustMaxPower += maxPower;
                        }
                        else if (m.M31 == 1)//---------------------------------------------------------right
                        {
                            rightThrustPower += currentPower;
                            rightThrustMaxPower += maxPower;
                        }
                        else if (m.M31 == -1)//---------------------------------------------------------left
                        {
                            leftThrustPower += currentPower;
                            leftThrustMaxPower += maxPower;
                        }
                        else if (m.M33 == 1)//---------------------------------------------------------forward
                        {
                            forwardThrustPower += currentPower;
                            forwardThrustMaxPower += maxPower;
                        }
                        else if (m.M33 == -1)//---------------------------------------------------------backward
                        {
                            reverseThrustPower += currentPower;
                            reverseThrustMaxPower += maxPower;
                        }
                    }
                    else if(type == "Reactor" || type == "BatteryBlock")
                    {
                        /* do nothing */
                    }
                    else if(maxPower > 0)
                    {
                        double e_currentPower;
                        currentPowerTypes.TryGetValue(type, out e_currentPower);
                        currentPowerTypes.Remove(type);
                        currentPowerTypes.Add(type, e_currentPower + currentPower);

                        double e_maxPower;
                        maxPowerTypes.TryGetValue(type, out e_maxPower);
                        maxPowerTypes.Remove(type);
                        maxPowerTypes.Add(type, e_maxPower + maxPower);
                    }
                    else
                    {
                        Echo(block.CustomName +" ("+ type+") doesn't have Max Required Input");
                        Echo("Detailed Info:");
                        Echo(block.DetailedInfo);

                        //DisplayLine("Type: " + block.GetType().ToString());

                        //DisplayLine("Properties:");
                        /*Dictionary<string, string> propertyMap = new Dictionary<string, string>();
                        for (int p = 0; p < properties.Count; p++)
                        {
                            if (!propertyMap.ContainsKey(properties[p].Id))
                            {
                                propertyMap.Add(properties[p].Id, properties[p].TypeName);
                                if (properties[p].TypeName == "Boolean")
                                    DisplayLine(properties[p].Id + ": " + block.GetValueBool(properties[p].Id).ToString());
                                else if (properties[p].TypeName == "Single")
                                    DisplayLine(properties[p].Id + ": " + block.GetValueFloat(properties[p].Id).ToString());
                                else if (properties[p].TypeName == "StringBuilder")
                                    DisplayLine(properties[p].Id + ": " + block.GetValue<StringBuilder>(properties[p].Id).ToString());
                                else
                                    DisplayLine(properties[p].Id + ": unknown type:" + properties[p].TypeName);
                            }
                        }
                        */
                    }
                }
                catch (Exception ex)
                {
                    Echo("Error in blocks loop on "+i+"\n" + ex.Message);
                    Echo("Trying to process '" + blocks[i].CustomName+"'");

                }
            }
            double totalCurrentPower = 0;
            double totalMaxPower = 0;

            DisplayLine("Up Thrust: " + FormatNumber(upThrustPower) + "W / " + FormatNumber(upThrustMaxPower) + "W");
            DisplayLine("Down Thrust: " + FormatNumber(downThrustPower) + "W / " + FormatNumber(downThrustMaxPower) + "W");

            DisplayLine("Left Thrust: " + FormatNumber(leftThrustPower) + "W / " + FormatNumber(leftThrustMaxPower) + "W");
            DisplayLine("Right Thrust: " + FormatNumber(rightThrustPower) + "W / " + FormatNumber(rightThrustMaxPower) + "W");

            DisplayLine("Forward Thrust: " + FormatNumber(forwardThrustPower) + "W / " + FormatNumber(forwardThrustMaxPower) + "W");
            DisplayLine("Reverse Thrust: " + FormatNumber(reverseThrustPower) + "W / " + FormatNumber(reverseThrustMaxPower) + "W");

            string[] keys = new string[currentPowerTypes.Keys.Count];
            currentPowerTypes.Keys.CopyTo(keys, 0);

            if (upThrustMaxPower > downThrustMaxPower)
                totalMaxPower += upThrustMaxPower;
            else
                totalMaxPower += downThrustMaxPower;
            if (upThrustPower > downThrustPower)
                totalCurrentPower += upThrustPower;
            else
                totalCurrentPower += downThrustPower;

            if (leftThrustMaxPower > rightThrustMaxPower)
                totalMaxPower += leftThrustMaxPower;
            else
                totalMaxPower += rightThrustMaxPower;
            if (leftThrustPower > rightThrustPower)
                totalCurrentPower += leftThrustPower;
            else
                totalCurrentPower += rightThrustPower;

            if (forwardThrustMaxPower > reverseThrustMaxPower)
                totalMaxPower += forwardThrustMaxPower;
            else
                totalMaxPower += reverseThrustMaxPower;
            if (forwardThrustPower > reverseThrustPower)
                totalCurrentPower += forwardThrustPower;
            else
                totalCurrentPower += reverseThrustPower;

            for (int i = 0; i < keys.Length; i++)
            {
                double currentPower;
                double maxPower;
                currentPowerTypes.TryGetValue(keys[i], out currentPower);
                currentPowerTypes.TryGetValue(keys[i], out maxPower);
                if (maxPower > 0)
                    DisplayLine(keys[i] + ": " + FormatNumber(currentPower) + "W / " + FormatNumber(maxPower) + "W");
                totalCurrentPower += currentPower;
                totalMaxPower += maxPower;
            }

            DisplayLine("\n" + "total: " + FormatNumber(totalCurrentPower) + "W / " + FormatNumber(totalMaxPower) + "W");

            Echo("Done");
        }

        public static string FormatNumber(double number)
        {
            string ordinals = " kMGTPEZY";
            double compressed = number;

            var ordinal = 0;

            while (compressed >= 1000)
            {
                compressed /= 1000;
                ordinal++;
            }

            string res = Math.Round(compressed, 2, MidpointRounding.AwayFromZero).ToString();

            if (ordinal > 0)
                res += " " + ordinals[ordinal];

            return res;
        }

        public static double ParseNumber(string input)
        {
            string ordinals = " kMGTPEZY";
            input = input.TrimEnd('W');
            double result;
            double.TryParse(input.Substring(0,input.Length - 2), out result);

            int ordinal = ordinals.IndexOf(input.Substring(input.Length - 1, 1));
            while (ordinal > 0)
            {
                result *= 1000;
                ordinal--;
            }

            return result;
        }

        public void DisplayLine(string text, bool clearScrean = false)
        {
            clearScrean = !clearScrean;
            s.WritePublicText(text + "\n", clearScrean);
        }
        #endregion
    }
}
