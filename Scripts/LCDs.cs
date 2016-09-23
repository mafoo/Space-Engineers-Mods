using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{
    public class LCDs : MyGridProgram
    {
        #region  
        List<IMySolarPanel> solars = new List<IMySolarPanel>();
        IMyTextPanel s;
        float minPower = -1;
        float peakPower = 0;
        float totalPower = 0;
        int cycles = 0;
        int lastCount = 0;

        public Program()
        {
            Echo("Starting Up ...");
            var values = new Dictionary<string, string>();
            string[] raw;
            raw = Storage.Split('|');
            int size = raw.Length;
            if (size > 1)
            {
                for (int i = 0; i < size; i++)
                {
                    string[] entry;
                    entry = raw[i].Split(':');
                    values.Add(entry[0], entry[1]);
                }
            }
            Echo("Loading Data ...");
            if (values.ContainsKey("peakPower"))
                peakPower = float.Parse(values["peakPower"]);
            Echo("Loaded peakPower: " + peakPower);
            if (values.ContainsKey("minPower"))
                minPower = float.Parse(values["minPower"]);
            Echo("Loaded minPower: " + peakPower);
            if (values.ContainsKey("totalPower"))
                totalPower = float.Parse(values["totalPower"]);
            Echo("Loaded totalPower: " + totalPower);
            if (values.ContainsKey("cycles"))
                cycles = int.Parse(values["cycles"]);
            Echo("Loaded cycles: " + cycles);
            if (values.ContainsKey("lastCount"))
                cycles = int.Parse(values["lastCount"]);
            Echo("Loaded lastCount: " + lastCount);
        }

        void Save()
        {
            Storage = string.Join("|",
                "peakPower:" + peakPower.ToString(),
                "minPower:" + minPower.ToString(),
                "totalPower:" + totalPower.ToString(),
                "cycles:" + cycles.ToString(),
                "lastCount:" + lastCount.ToString()
                );
        }

        void Main(string argument)
        {
            if (s == null)
                s = GridTerminalSystem.GetBlockWithName("Data - Power") as IMyTextPanel;
            if (solars.Count == 0)
                GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(solars);
            Echo("Checking Grids");
            Echo("Found " + solars.Count + " pannels attached");
            /*solars = solars.FindAll(delegate (IMySolarPanel c) 
            { 
                if(c.CubeGrid.ToString() == s.CubeGrid.ToString())
                    return true;
                return false;
            });*/
            int count = solars.Count;
            Echo("Filtered to " + count + " panels");
            if (argument == "reset" || count != lastCount)
            {
                peakPower = 0;
                totalPower = 0;
                minPower = -1;
                cycles = 0;
            }
            lastCount = count;
            float total = 0;
            cycles++;
            Echo("Cycle #" + cycles + " Started");
            int panel_count = 0;
            Echo("Going to check " + count + " panels");
            for (int i = 0; i < count; i++)
            {
                total += solars[i].MaxOutput;
                panel_count++;
            }
            if (peakPower < total)
                peakPower = total;
            if (minPower == -1 || minPower > total)
                minPower = total;
            totalPower += total;
            string f_peakPower = "None";
            if (peakPower > 0)
                f_peakPower = FormatNumber(peakPower * Math.Pow(1000, 2)) + "W";
            string f_minPower = "None";
            if (minPower > 0)
                f_minPower = FormatNumber(minPower * Math.Pow(1000, 2)) + "W";
            string f_averagePower = "None";
            if (totalPower > 0)
                f_averagePower = FormatNumber(totalPower / cycles * Math.Pow(1000, 2)) + "W";
            s.WritePublicText("Peak Power:\n" + f_peakPower + "\n");
            s.WritePublicText("Min Power:\n" + f_minPower + "\n", true);
            s.WritePublicText("Average Power:\n" + f_averagePower + "\n", true);
            s.WritePublicText("For " + panel_count + " Panels" + "\n", true);
            Echo("Peak Power: " + f_peakPower);
            Echo("Min Power: " + f_minPower);
            Echo("Average Power: " + f_averagePower);
            Echo("For " + panel_count + " Panels");
            Echo("Cycle #" + cycles + " Completed");
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

            string res = Math.Round(compressed, 1, MidpointRounding.AwayFromZero).ToString();

            if (ordinal > 0)
                res += " " + ordinals[ordinal];

            return res;
        }
        #endregion
    }
}
