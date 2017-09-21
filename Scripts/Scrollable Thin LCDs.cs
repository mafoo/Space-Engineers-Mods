using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Scripts
{
    public class ScrollableThinLCDs : MyGridProgram
    {
        #region
        Dictionary<string, LCDDefinition> LCDDefinitions = new Dictionary<string, LCDDefinition>();
        Dictionary<string, List<ScrollablePanel>> panelSets = new Dictionary<string, List<ScrollablePanel>>();

        string groupTag = "[SLCD]";


        public Program()
        {
            Echo("initializing...");
            LCDDefinitions.Add("ThinLCDCurvedInside", new LCDDefinition(1024, 120));
            LCDDefinitions.Add("ThinLCDCurvedOutside", new LCDDefinition(1024, 120));
            LCDDefinitions.Add("ThinLCDCurvedOutsideHanging", new LCDDefinition(1024, 120));
            LCDDefinitions.Add("ThinLCDStraight", new LCDDefinition(672, 120));
            tr.sy();
            Echo("Init complete");
            Main("bootup");
        }

        public void Main(string argument)
        {
            try
            {
                argument = argument.ToLower();
                Echo("Called with '" + argument + "'");
                Echo("Scanning for groups");
                List<IMyBlockGroup> blockGroups = new List<IMyBlockGroup>();
                GridTerminalSystem.GetBlockGroups(blockGroups);
                Echo("Found "+ blockGroups.Count+" groups");
                for (int g = 0; g < blockGroups.Count; g++)
                {
                    if (blockGroups[g].Name.Contains(groupTag))
                    {
                        var blockGroup = blockGroups[g];
                        if (!panelSets.ContainsKey(blockGroup.Name))
                        {
                            var blocks = new List<IMyTextPanel>();
                            blockGroup.GetBlocksOfType(blocks);
                            if (blocks.Count > 0)
                            {
                                bool configured = false;
                                List<ScrollablePanel> panels = new List<ScrollablePanel>();
                                for (int i = 0; i < blocks.Count; i++)
                                {
                                    var block = blocks[i];
                                    var panelData = new ScrollablePanel(blocks[i]);
                                    if (panelData.configured == true)
                                        configured = true;
                                    panels.Add(panelData);
                                }
                                if (configured)
                                {
                                    panels.Sort();
                                    panelSets.Add(blockGroup.Name, panels);
                                }
                                else
                                {
                                    for (int i = 0; i < panels.Count; i++)
                                    {
                                        var panelData = panels[i];
                                        if (panelData.index == 0)
                                        {
                                            panelData.block.WritePublicText(string.Join("", blockGroup.Name, " Auto id#", i + 1));
                                            panelData.index = i + 1;
                                        }
                                        else
                                        {
                                            panelData.block.WritePublicText(string.Join("", blockGroup.Name, " id#", panelData.index));
                                        }
                                        panelData.block.CustomName = panelData.block.GetPublicText();
                                        panelData.block.ShowPublicTextOnScreen();
                                        panelData.block.SetValueFloat("FontSize", 1f);
                                        panelData.write();
                                    }
                                }
                            }
                        }
                    }
                }
                Echo("Going to update " + panelSets.Count + " groups");
                for (int i = 0; i < panelSets.Count; i++)
                {

                }
            }
            catch (System.Exception ex)
            {
                Echo("I Crashed:\n");
                Echo(ex.Message + "\n");
                Echo(ex.StackTrace + "\n");
            }
        }


        private class LCDDefinition
        {

            public ushort xSize;
            public ushort ySize;

            public LCDDefinition(ushort _xSize, ushort _ySize)
            {
                xSize = _xSize;
                ySize = _ySize;
            }

        }

        private class ScrollablePanel : IEquatable<ScrollablePanel>, IComparable<ScrollablePanel>
        {
            public IMyTextPanel block;
            public int index;
            public bool configured;
            public string text;

            public ScrollablePanel(IMyTextPanel _panel)
            {
                block = _panel;
                var lines = block.CustomData.Split(new char[] { '\r', '\n' });
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(':');
                    string name = values[0];
                    string value = values[1];
                    switch (name)
                    {
                        case "index":
                            int.TryParse(value, out index);
                            break;
                        case "configured":
                            bool.TryParse(value, out configured);
                            break;
                        case "text":
                            text = string.Join("\n", lines.Skip<string>(i+1));
                            i = lines.Length;
                            break;
                        default:
                            break;
                    }
                }
            }
            public void write()
            {
                block.CustomData = string.Join("\n",
                    "index:" + index,
                    "configured:" + configured,
                    "text:", text
                );
            }

            public bool Equals(ScrollablePanel other)
            {
                if (other.index == 0) return false;
                return (index.Equals(other.index));
            }

            public int CompareTo(ScrollablePanel other)
            {
                if (index == 0)
                    return 1;
                else
                    return this.index.CompareTo(other.index);
            }
        }

        public static class tr
        {
            static Dictionary<char, float> sr = new Dictionary<char, float>();
            static float ss = 1f;
            static float st = 10f;
            public static float su = -1f;
            static void AddCharsSize(string jb,float jc)
            {
                jc += 1;
                for (int jd = 0; jd < jb.Length; jd++)
                {
                    if (jc > ss) ss = jc;
                    sr.Add(jb[jd], jc);
                }
            }
            public static float sv(char je)
            {
                float jf;
                if (!sr.TryGetValue(je, out jf))
                    return ss;
                return jf;
            }
            public static float sw(string jg)
            {
                if (su > 0)
                    return jg.Length * ss;
                float jh = 0;
                for (int ji = 0; ji < jg.Length; ji++)
                    jh += sv(jg[ji]);
                return jh;
            }
            public static string sx(string jj, float jk)
            {
                if (jk / ss >= jj.Length)
                    return jj;
                float jl = sw(jj);
                if (jl <= jk)
                    return jj;
                float jm = jl / jj.Length;
                jk -= st;
                int jn = (int)(jk / jm);
                jj = jj.Substring(0, jn); jl = sw(jj);
                while (jl > jk)
                {
                    jn--; jl -= sv(jj[jn]);
                    jj.Remove(jn);
                }
                return jj + "..";
            }
            public static void sy()
            {
                if (sr.Count > 0) return;
                if (su > 0)
                {
                    ss = su + 1;
                    st = 2 * ss;
                    return;
                }

                // Font characters width (font "aw" values here)
                AddCharsSize("3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ", 17f);
                AddCharsSize("#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€", 19f);
                AddCharsSize("$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡", 20f);
                AddCharsSize("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 21f);
                AddCharsSize("(),.1:;[]ft{}·ţťŧț", 9f);
                AddCharsSize("+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−", 18f);
                AddCharsSize(" !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙", 8f);
                AddCharsSize("7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ", 16f);
                AddCharsSize("L_vx«»ĹĻĽĿŁГгзлхчҐ–•", 15f);
                AddCharsSize("\"-rª­ºŀŕŗř", 10f);
                AddCharsSize("mw¼ŵЮщ", 27f);
                AddCharsSize("MМШ", 26f);
                AddCharsSize("WÆŒŴ—…‰", 31f);
                AddCharsSize("'|¦ˉ‘’‚", 6f);
                AddCharsSize("*²³¹", 11f);
                AddCharsSize("\\°“”„", 12f);
                AddCharsSize("/ĳтэє", 14f);
                AddCharsSize("%ĲЫ", 24f);
                AddCharsSize("@©®мшњ", 25f);
                AddCharsSize("\n", 0f);
                AddCharsSize("¾æœЉ", 28f);
                AddCharsSize("½Щ", 29f);
                AddCharsSize("ј", 7f);
                AddCharsSize("љ", 22f);
                AddCharsSize("ґ", 13f);
                AddCharsSize("ю", 23f);
                AddCharsSize("™", 30f);
                AddCharsSize("", 34f);
                AddCharsSize("", 32f);
                AddCharsSize("", 41f);
                // End of font characters width
                st = sv('.') * 2;
            }
        }


        #endregion
    }
}
