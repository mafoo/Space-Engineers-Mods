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
    public sealed class Stocker : MyGridProgram
    {
#endif
        IMyTextPanel s;
        Dictionary<string, IMyTextPanel> panels = new Dictionary<string, IMyTextPanel>();
        Dictionary<string, IMyButtonPanel> buttons = new Dictionary<string, IMyButtonPanel>();
        List<IMyCargoContainer> containers = new List<IMyCargoContainer>();
        IMyCargoContainer c_container;
        List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
        IMyBlockGroup c_group;
        string c_menu;
        int offset = 0;
        int total = 0;
        int totalContainers = 0;
        int totalGroups = 0;
        Dictionary<string, string> gridNames = new Dictionary<string, string>();
        Dictionary<string, string> timPresets = new Dictionary<string, string>();
        bool filter_staticGrids = true;
        bool add_mode = false;
        bool group_mode = true;
        System.Text.RegularExpressions.Regex rgx_tim = new System.Text.RegularExpressions.Regex(@"\[TIM(.*)]");

        const char green = '\uE001';
        const char blue = '\uE002';
        const char red = '\uE003';
        const char yellow = '\uE004';
        const char white = '\uE006';
        const char lightGray = '\uE00E';
        const char mediumGray = '\uE00D';
        const char darkGray = '\uE00F';
        const char dimGray = '\uE017';

        public Program()
        {
            Echo("initializing..");
            s = GridTerminalSystem.GetBlockWithName("Stocker - Display") as IMyTextPanel;
            s.WritePublicText("Initializing.....");

            gridNames.Add("Grid_D_Small_621", "Cargo Pod 1");
            gridNames.Add("Grid_D_Small_196", "Cargo Ship");
            gridNames.Add("Grid_D_Small_158", "Unknown Ship");
            gridNames.Add("Grid_D_Small_136", "Welding Ship");
            gridNames.Add("Grid_D_Small_133", "Grinding Ship");
            gridNames.Add("Grid_D_Large_1066", "Large Flyer");

            Echo("Init complete");
            Main("reset");
        }

        public void Main(string argument)
        {
            try
            {
                Echo("Called with " + argument);
                clearScreen();
                string modes = "Static Grid:";
                if (filter_staticGrids)
                    modes += green;
                else
                    modes += red;
                modes += " Add mode:";
                if (add_mode)
                    modes += green;
                else
                    modes += red;
                modes += " Group mode:";
                if (group_mode)
                    modes += green;
                else
                    modes += red;
                writeScreen(modes + "\n");
                int opt;
                if (argument == "reset")
                {
                    updatePresets();
                    updateControls();
                    updateContainers();
                    menu("containers");
                }
                else if (argument == "next")
                {
                    if (offset < totalContainers)
                        offset += 6;
                    menu();
                }
                else if (argument == "back")
                {
                    if (offset >= 6)
                        offset -= 6;
                    menu();
                }
                else if (int.TryParse(argument, out opt))
                {

                    if (c_menu == "containers")
                    {
                        if(offset == 0 && opt == 1)
                        {
                            menu("options");
                            return;
                        }
                        else if (offset + opt < totalContainers)
                        {
                            c_container = containers[offset + opt - 2];
                            c_group = null;
                            string name = rgx_tim.Replace(c_container.CustomName, "");
                            writeScreen("Configuring: " + name + "\n");
                            string current = rgx_tim.Match(c_container.CustomName).Groups[1].Value;
                            if (current == "")
                                writeScreen(red + "No TiM\n");
                            else
                                writeScreen("Existing:\n" + yellow + string.Join("\n", current.Split(' ')) + green + "\n");
                        }else
                        {
                            c_group = groups[offset + opt - 2 - totalContainers];
                            c_container = null;
                            writeScreen("Configuring GROUP: " + c_group.Name + "\n");
                            List<IMyCargoContainer> g_containers = new List<IMyCargoContainer>();
                            c_group.GetBlocksOfType<IMyCargoContainer>(g_containers);
                            for (int i = 0; i < g_containers.Count; i++)
                            {
                                string name = rgx_tim.Replace(g_containers[i].CustomName, "");
                                string current = rgx_tim.Match(g_containers[i].CustomName).Groups[1].Value;
                                writeScreen(green + "Item: " + name + "\n");
                                if (current == "")
                                    writeScreen(red + "No TiM\n");
                                else
                                    writeScreen("Existing:"+ yellow + string.Join("\n" + yellow, current.Split(' ')) + "\n");
                            }
                            offset = 0;
                            total = timPresets.Count;
                        }
                        menu("presets");
                    }
                    else if (c_menu == "presets")
                    {
                        if (c_container != null)
                        {
                            string name = rgx_tim.Replace(c_container.CustomName, "");
                            name.Trim();

                            string[] keys = new string[timPresets.Keys.Count];
                            timPresets.Keys.CopyTo(keys, 0);
                            var preset = timPresets[keys[opt + offset - 1]];

                            string current = rgx_tim.Match(c_container.CustomName).Groups[1].Value;
                            writeScreen("Configuring: " + name + "\n");

                            string expanded = "";
                            if (add_mode)
                                expanded = string.Join(" ", current, preset);
                            if (expanded != "")
                                expanded = "[TIM " + expanded + "]";
                            c_container.SetCustomName(name + " " + expanded);

                            if (expanded == "")
                                writeScreen(red + "No TiM\n");
                            else
                                writeScreen("Existing:\n" + yellow + string.Join("\n" + green, expanded.Split(' ')) + "\n");

                        }else
                        {
                            writeScreen("Configuring GROUP: " + c_group.Name + "\n");

                            string[] keys = new string[timPresets.Keys.Count];
                            timPresets.Keys.CopyTo(keys, 0);
                            var preset = timPresets[keys[opt + offset - 1]];

                            List<IMyCargoContainer> g_containers = new List<IMyCargoContainer>();
                            c_group.GetBlocksOfType<IMyCargoContainer>(g_containers);
                            for (int i = 0; i < g_containers.Count; i++)
                            {
                                var container = g_containers[i];
                                string name = rgx_tim.Replace(container.CustomName, "");
                                name.Trim();
                                string current = rgx_tim.Match(container.CustomName).Groups[1].Value;
                                writeScreen(green + "Item: " + name + "\n");

                                string expanded = preset;
                                if (add_mode)
                                    expanded = string.Join(" ", current, preset);
                                if (expanded != "")
                                    expanded = "[TIM " + expanded + "]";
                                container.SetCustomName(name + " " + expanded);

                                if (expanded == "")
                                    writeScreen(red + "No TiM\n");
                                else
                                    writeScreen("Existing:" +yellow + string.Join("\n" + yellow, expanded.Split(' ')) + "\n");

                            }
                        }
                        menu();
                    }
                    else if (c_menu == "options"){
                        if (opt == 1)
                            filter_staticGrids = !filter_staticGrids;
                        if (opt == 2)
                            add_mode = !add_mode;
                        if (opt == 3)
                            group_mode = !group_mode;
                        clearScreen();
                        modes = "Static Grid:";
                        if (filter_staticGrids)
                            modes += green;
                        else
                            modes += red;
                        modes += " Add mode:";
                        if (add_mode)
                            modes += green;
                        else
                            modes += red;
                        modes += " Group mode:";
                        if (group_mode)
                            modes += green;
                        else
                            modes += red;
                        writeScreen(modes + "\n");
                        menu();
                    }
                }
                else
                {
                    menu();
                }
            }
            catch (System.Exception ex)
            {
                writeScreen("I Crashed:\n");
                writeScreen(ex.Message + "\n");
                writeScreen(ex.StackTrace + "\n");
            }
        }

        private void menu(string e_menu = null)
        {
            try
            {
                total = totalContainers;
                if(group_mode)
                    total += totalGroups;
                Echo("Running menu");
                if (e_menu != null)
                {
                    c_menu = e_menu;
                    offset = 0;
                }
                int page = (offset / 6) + 1;
                int pages = (total / 6) + 1;
                writeScreen("\nPage " + page + " of " + pages + "\n");
                if (c_menu == "containers")
                {
                    Echo("Rendering Items");
                    var rgx = new System.Text.RegularExpressions.Regex(@"\[.*");
                    int i = 0;
                    if( offset == 0)
                    {
                        i++;
                        var panel = panels["Stocker Option 1"];
                        var button = buttons["Stocker Button 1"];
                        panel.SetValue<float>("FontSize", 4f);
                        button.SetValueBool("OnOff", true);
                        panel.WritePublicText(lightGray + "Options");
                    }
                    for (; i < 6; i++)
                    {
                        var panel = panels["Stocker Option " + (i + 1)];
                        var button = buttons["Stocker Button " + (i + 1)];
                        panel.SetValue<float>("FontSize", 2f);
                        if (i + offset - 1 < totalContainers)
                        {
                            button.SetValueBool("OnOff", true);
                            IMyCargoContainer container = containers[i + offset - 1];
                            string name = wordWrap(rgx.Replace(container.CustomName, ""));
                            name = name.Trim();
                            string grid = wordWrap(resolveGrid(container.CubeGrid));
                            string current = rgx_tim.Match(container.CustomName).Groups[1].Value;
                            string output = name + ":\n" + blue + grid + "\n";
                            if (current == "")
                                output += red + "No TiM\n";
                            else
                                output += "Existing:\n" + yellow + string.Join("\n"+ yellow, current.Split(' ')) + "\n" ;
                            panel.WritePublicText(output);
                        }
                        else if(i + offset - 1 <= total)
                        {
                            button.SetValueBool("OnOff", true);
                            IMyBlockGroup group = groups[i + offset - totalContainers - 1];
                            string name = group.Name;
                            name = name.Trim();
                            string output = blue + name + "\n";
                            panel.WritePublicText(output);
                        }
                        else
                        {
                            button.SetValueBool("OnOff", false);
                            panel.WritePublicText("");
                        }
                    }
                }
                else if (c_menu == "presets")
                {
                    Echo("Rendering Items");
                    string[] keys = new string[timPresets.Keys.Count];
                    timPresets.Keys.CopyTo(keys, 0);
                    for (int i = 0; i < 6; i++)
                    {
                        var panel = panels["Stocker Option " + (i + 1)];
                        var button = buttons["Stocker Button " + (i + 1)];
                        panel.SetValue<float>("FontSize", 2f);
                        if (i + offset < totalContainers)
                        {
                            button.SetValueBool("OnOff", true);
                            var timPreset = keys[i + offset];
                            panel.WritePublicText(wordWrap(timPreset));
                            panel.WritePublicText(string.Join("\n", timPresets[timPreset].Split(' ')),true);
                        }
                        else
                        {
                            button.SetValueBool("OnOff", false);
                            panel.WritePublicText(" ");
                        }
                    }
                }
                else if(c_menu == "options")
                {
                    var panel = panels["Stocker Option 1"];
                    buttons["Stocker Button 1"].SetValueBool("OnOff", true);
                    panel.SetValue<float>("FontSize", 4f);
                    if (filter_staticGrids)
                        panel.WritePublicText(green + "\n");
                    else
                        panel.WritePublicText(red + "\n");
                    panel.WritePublicText("Filter\nStatic\nGrids", true);

                    panel = panels["Stocker Option 2"];
                    buttons["Stocker Button 2"].SetValueBool("OnOff", true);
                    panel.SetValue<float>("FontSize", 4f);
                    if (add_mode)
                        panel.WritePublicText(green + "\n");
                    else
                        panel.WritePublicText(red + "\n");
                    panel.WritePublicText("Add\nMode", true);

                    panel = panels["Stocker Option 3"];
                    buttons["Stocker Button 3"].SetValueBool("OnOff", true);
                    panel.SetValue<float>("FontSize", 4f);
                    if (group_mode)
                        panel.WritePublicText(green + "\n");
                    else
                        panel.WritePublicText(red + "\n");
                    panel.WritePublicText("Group\nMode", true);
                    panels["Stocker Option 4"].WritePublicText("");
                    buttons["Stocker Button 4"].SetValueBool("OnOff", false);
                    panels["Stocker Option 5"].WritePublicText("");
                    buttons["Stocker Button 5"].SetValueBool("OnOff", false);
                    panels["Stocker Option 6"].WritePublicText("");
                    buttons["Stocker Button 6"].SetValueBool("OnOff", false);
                    buttons["Stocker Button Next"].SetValueBool("OnOff", false);
                    buttons["Stocker Button Back"].SetValueBool("OnOff", false);
                }
                else
                {
                    Echo("Unkown Menu");
                    writeScreen("Unkown Menu " +c_menu);
                }
                bool n_state = false;
                bool b_state = false;
                if (offset + 6 < totalContainers)
                    n_state = true;
                if (offset > 0)
                    b_state = true;
                buttons["Stocker Button Next"].SetValueBool("OnOff", n_state);
                buttons["Stocker Button Back"].SetValueBool("OnOff", b_state);
            }
            catch (System.Exception ex)
            {
                writeScreen("I Crashed in the menu:\n");
                writeScreen(ex.Message + "\n");
                writeScreen(ex.StackTrace + "\n");
            }

        }

        private void resetOptions()
        {
            Echo("reseting Menu");
            for (int i = 1; i <= 6; i++)
            {
                panels["Stocker Option " + i].WritePublicText(" ");
            }
        }

        private void clearScreen()
        {
            s.WritePublicText("");
        }

        private void writeScreen(string text)
        {
            s.WritePublicText(text, true);
        }

        private string resolveGrid(IMyCubeGrid grid)
        {
            var rgx = new System.Text.RegularExpressions.Regex(@"\s+\{.*");
            var rgx_u = new System.Text.RegularExpressions.Regex(@"\w+_\w+_\w+_");
            string name = rgx.Replace(grid.ToString(), "");
            if (gridNames.ContainsKey(name))
                return gridNames[name];
            if (name.Contains("Grid_D_Small"))
                return "Small Ship " + rgx_u.Replace(name, "");
            if (name.Contains("Grid_D_Large"))
                return "Large Ship " + rgx_u.Replace(name, "");
            if (name.Contains("Grid_S_Large"))
                return "Large Station " + rgx_u.Replace(name, "");
            if (name.Contains("Grid_S_Small"))
                return "Small Station " + rgx_u.Replace(name, "");
            return name;
        }

        private void updatePresets()
        {
            timPresets.Clear();
            timPresets.Add("Clear", "");
            Echo(".. Parsing presets");
            var rows = s.GetPrivateText().Split('\n');
            if (rows.Length > 0)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    string[] values = rows[i].Split('|');
                    if (values.Length > 1)
                        timPresets.Add(values[0], values[1]);
                }
            }
        }
        private void updateControls()
        {
            Echo("Finding Screens");
            panels.Clear();
            var panel_list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("Stocker Option", panel_list);
            for (int i = 0; i < panel_list.Count; i++)
            {
                IMyTextPanel panel;
                panel = panel_list[i] as IMyTextPanel;
                panels.Add(panel.CustomName, panel);
                panel.SetValue<float>("FontSize", 5);
                string text = "Init...";
                if (panel.CustomName.Contains("Back"))
                    text = "\n      Back";
                if (panel.CustomName.Contains("Next"))
                    text = "\nNext";
                panel.WritePublicText(text);
                panel.SetValueColor("FontColor", new Color(255,255,255));
            }

            Echo("Finding Buttons");
            buttons.Clear();
            var button_list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("Stocker Button", button_list);
            for (int i = 0; i < button_list.Count; i++)
            {
                IMyButtonPanel button;
                button = button_list[i] as IMyButtonPanel;
                buttons.Add(button.CustomName, button);
            }
            buttons["Stocker Button Next"].SetValueBool("OnOff", false);
            buttons["Stocker Button Back"].SetValueBool("OnOff", false);
        }

        private void updateContainers()
        {
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(containers);
            if (filter_staticGrids)
            {
                /*containers = containers.Where(X => X.CubeGrid.ToString().Contains(ourGrid)).ToList();*/
                containers = containers.FindAll(delegate (IMyCargoContainer c)
                {
                    if (c.CubeGrid.ToString().Contains("Grid_S_"))
                        return false;
                    return true;
                });
            }
            if (group_mode)
            {
                GridTerminalSystem.GetBlockGroups(groups);
                groups = groups.FindAll(delegate (IMyBlockGroup g)
                {
                    List<IMyCargoContainer> g_containers = new List<IMyCargoContainer>();
                    g.GetBlocksOfType<IMyCargoContainer>(g_containers);
                    for(int i=0;i< g_containers.Count; i++)
                    {
                        containers.Remove(g_containers[i]);
                    }
                    if (g_containers.Count > 0)
                        return true;
                    return false;
                });

            }
            containers.Sort(delegate (IMyCargoContainer x, IMyCargoContainer y)
            {
                if (x.CustomName == null && y.CustomName == null) return 0;
                else if (x.CustomName == null) return -1;
                else if (y.CustomName == null) return 1;
                else return x.CustomName.CompareTo(y.CustomName);
            });
            offset = 0;
            totalContainers = containers.Count;
        }

        private string wordWrap(string text, int width = 20)
        {
            string r_text = "";
            do
            {
                int r_width = text.Length;
                if (r_width < width)
                    width = r_width;
                r_text += text.Substring(0, width) + "\n";
                text = text.Substring(width);
            } while (text.Length > width);
            r_text += text;
            return r_text;
        }
#if DEBUG
    }
}
#endif
