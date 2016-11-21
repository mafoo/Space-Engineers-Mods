/* v:1.132 [01.144 Stable compatibility, Right command, DetailsX & Dampeners & Occupied commands, Accel, More Power variants, PowerTime, Same grid filter]
* In-game script by MMaster
*
* Last Update: Fix for modded items with space in SubtypeId not showing up
* 01.144 game update compatibility
* Right command
*
* Previous updates: Look at Change notes tab on Steam workshop page.
* Dampeners & Occupied commands, DetailsX which skips "Type:" line
* Performance optimizations when Trigger Now is used
* Fixed \ adding space to command
* Comments in Private text using //
*
* Customize these: (do not report problems with modified values!) */

// Use this tag to identify LCDs managed by this script
// Name filtering rules can be used here so you can use even G:Group or T:[My LCD]
public static string LCD_TAG = "T:[LCD]";

// How many panels to update per one step
public static int PANELS_PER_STEP = 1;
// How many lines to scroll per step
public static int SCROLL_LINES_PER_STEP = 5;

// Enable initial boot sequence (after compile / world load)
public static bool ENABLE_BOOT = true;

// Force redraw of panels? (default = true)
// true - forces redraw of panels (should no longer be needed - its needed again!)
public static bool FORCE_REDRAW = true;

// Tags to remove when displaying names (recommended = "[]")
public static string STRIP_TAGS = "[]";

// (for developer) Enable debug to LCD marked with [DEBUG]
public static bool EnableDebug = false;

/*
READ THIS FULL GUIDE
http://steamcommunity.com/sharedfiles/filedetails/?id=407158161

Basic video guide
Please watch the video guide even if you don't understand my English. You can see how things are done there.

http://www.youtube.com/watch?v=oopzyQ0t6Dk

EXAMPLE WORLD
http://steamcommunity.com/sharedfiles/filedetails/?id=412154340

Read Change Notes (above screenshots) for latest updates and new features.
I notify about updates on twitter so follow if interested.

Please carefully read the FULL GUIDE before asking questions I had to remove guide from here to add more features :(
Please DO NOT publish this script or its derivations without my permission! Feel free to use it in blueprints!

Special Thanks
bssespaceengineers.com - awesome server
Rhedd - for his contribution to modded items entries
Textor and CyberVic for their great script related contributions on Keen forums.

Watch Twitter: https://twitter.com/MattsPlayCorner
and Facebook: https://www.facebook.com/MattsPlayCorner1080p
for more crazy stuff from me in the future :)
*/
void InitBoot() {
if (LCDsProgram.bootFrames == null)
LCDsProgram.bootFrames = new List<string>() {

/* BOOT FRAMES
* Each @"<text>" marks single frame, add as many as you want each will be displayed for one second
* @"" is multiline string so you can write multiple lines
*/
@"
Initializing systems"
,
@"
Verifying connections"
,
@"
Loading commands"
/* END OF BOOT FRAMES */

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// DO NOT MODIFY ANYTHING BELOW THIS
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
};
}

public static string SECONDARY_TAG = "T:!LCD!";
public static int step;
void Main(string argument) {
MM.EnableDebug = EnableDebug;
MM.Me = Me;
MM.Echo = Echo;
MM.ElapsedTime += Runtime.TimeSinceLastRun.TotalSeconds;
InitBoot();
if (!MM.Init(GridTerminalSystem)) return;
MMLCDMgr.forceRedraw = FORCE_REDRAW;
MMLCDMgr.SCROLL_LINES = SCROLL_LINES_PER_STEP;
LCDsProgram.SECONDARY_TAG = SECONDARY_TAG;
LCDsProgram.PANELS_PER_STEP = PANELS_PER_STEP;
if (!ENABLE_BOOT)
LCDsProgram.bootStep = int.MaxValue;
LCDsProgram.bootScreens = ENABLE_BOOT;
List<string> stripPattern = new List<string>();
if (STRIP_TAGS.Length > 0 && STRIP_TAGS.Length % 2 == 0) {
for (int c = 0; c < STRIP_TAGS.Length; c = c + 2) {
stripPattern.Add(string.Concat("\\",STRIP_TAGS.Substring(c,1), ".*\\", STRIP_TAGS.Substring(c + 1)));
}
}
LCDsProgram.stripTags = new System.Text.RegularExpressions.Regex(string.Concat("(?:",string.Join("|", stripPattern.ToArray()), ")"));
LCDsProgram lcdProg = new LCDsProgram(LCD_TAG);
lcdProg.Run(argument.ToLower(), step++);
MM.ElapsedTime = 0;
}
}

public static class MMItems {
public static Dictionary<string, MMItem> items = new Dictionary<string, MMItem>();
public static Dictionary<string, MMItem> itemsBySubtype = new Dictionary<string, MMItem>();
public static List<string> keys = new List<string>();
public static Dictionary<string, MMItem> itemsByShort = new Dictionary<string, MMItem>();

public static void Init() {
if (items.Count > 0) return;

// **************************************************
// OK MAYBE YOU CAN ADD MODDED ITEMS AND MODIFY QUOTAS
//    IF THAT WARNING DIDN'T SCARE YOU
// **************************************************
// ITEMS AND QUOTAS LIST
// (subType, mainType, quota, display name, short name)
// ADD MODDED ITEMS TO THIS LIST
// !! MAIN TYPES MUST GO TOGETHER FOR INV CATEGORIES !!
// VANILLA ITEMS
Add("Stone", "Ore");
Add("Iron", "Ore");
Add("Nickel", "Ore");
Add("Cobalt", "Ore");
Add("Magnesium", "Ore");
Add("Silicon", "Ore");
Add("Silver", "Ore");
Add("Gold", "Ore");
Add("Platinum", "Ore");
Add("Uranium", "Ore");
Add("Ice", "Ore");
Add("Stone", "Ingot", 40000, "Gravel", "gravel");
Add("Iron", "Ingot", 300000);
Add("Nickel", "Ingot", 900000);
Add("Cobalt", "Ingot", 120000);
Add("Magnesium", "Ingot", 80000);
Add("Silicon", "Ingot", 80000);
Add("Silver", "Ingot", 800000);
Add("Gold", "Ingot", 80000);
Add("Platinum", "Ingot", 45000);
Add("Uranium", "Ingot", 12000);
Add("Scrap", "Ingot");
Add("AutomaticRifleItem", "Tool", 0, "Automatic Rifle");
Add("PreciseAutomaticRifleItem", "Tool", 0, "* Precise Rifle");
Add("RapidFireAutomaticRifleItem", "Tool", 0, "** Rapid-Fire Rifle");
Add("UltimateAutomaticRifleItem", "Tool", 0, "*** Elite Rifle");
Add("WelderItem", "Tool", 0, "Welder");
Add("Welder2Item", "Tool", 0, "* Enh. Welder");
Add("Welder3Item", "Tool", 0, "** Prof. Welder");
Add("Welder4Item", "Tool", 0, "*** Elite Welder");
Add("AngleGrinderItem", "Tool", 0, "Angle Grinder");
Add("AngleGrinder2Item", "Tool", 0, "* Enh. Grinder");
Add("AngleGrinder3Item", "Tool", 0, "** Prof. Grinder");
Add("AngleGrinder4Item", "Tool", 0, "*** Elite Grinder");
Add("HandDrillItem", "Tool", 0, "Hand Drill");
Add("HandDrill2Item", "Tool", 0, "* Enh. Drill");
Add("HandDrill3Item", "Tool", 0, "** Prof. Drill");
Add("HandDrill4Item", "Tool", 0, "*** Elite Drill");
Add("Construction", "Component", 50000);
Add("MetalGrid", "Component", 15500, "Metal Grid");
Add("InteriorPlate", "Component", 55000, "Interior Plate");
Add("SteelPlate", "Component", 300000, "Steel Plate");
Add("Girder", "Component", 3500);
Add("SmallTube", "Component", 26000, "Small Tube");
Add("LargeTube", "Component", 6000, "Large Tube");
Add("Motor", "Component", 16000);
Add("Display", "Component", 500);
Add("BulletproofGlass", "Component", 12000, "Bulletp. Glass", "bpglass");
Add("Computer", "Component", 6500);
Add("Reactor", "Component", 10000);
Add("Thrust", "Component", 16000, "Thruster", "thruster");
Add("GravityGenerator", "Component", 250, "GravGen", "gravgen");
Add("Medical", "Component", 120);
Add("RadioCommunication", "Component", 250, "Radio-comm", "radio");
Add("Detector", "Component", 400);
Add("Explosives", "Component", 500);
Add("SolarCell", "Component", 2800, "Solar Cell");
Add("PowerCell", "Component", 2800, "Power Cell");
Add("Superconductor", "Component", 3000);
Add("NATO_5p56x45mm", "Ammo", 8000, "5.56x45mm", "5.56x45mm");
Add("NATO_25x184mm", "Ammo", 2500, "25x184mm", "25x184mm");
Add("Missile200mm", "Ammo", 1600, "200mm Missile", "200mmmissile");
Add("OxygenBottle", "OxygenContainerObject", 5, "Oxygen Bottle");
Add("HydrogenBottle", "GasContainerObject", 5, "Hydrogen Bottle");


// MODDED ITEMS
// (subType, mainType, quota, display name, short name, used)
// * if used is true, item will be shown in inventory even for 0 items
// * if used is false, item will be used only for display name and short name
// AzimuthSupercharger
Add("AzimuthSupercharger", "Component", 1600, "Supercharger", "supercharger", false);
// OKI Ammo
Add("OKI23mmAmmo", "Ammo", 500, "23x180mm", "23x180mm", false);
Add("OKI50mmAmmo", "Ammo", 500, "50x450mm", "50x450mm", false);
Add("OKI122mmAmmo", "Ammo", 200, "122x640mm", "122x640mm", false);
Add("OKI230mmAmmo", "Ammo", 100, "230x920mm", "230x920mm", false);
}

/* REALLY REALLY REALLY
* DO NOT MODIFY ANYTHING BELOW THIS
*/

// displayName - how the item will be displayed
// shortName - how the item can be called in arguments (eg: +supercharger)
public static void Add(string subType, string mainType, int quota = 0, string displayName = "", string shortName = "", bool used = true) {
if (mainType == "Ammo")
mainType = "AmmoMagazine";
else if (mainType == "Tool")
mainType = "PhysicalGunObject";
string fullType = subType + ' ' + mainType;
MMItem item = new MMItem(subType, mainType, quota, displayName, shortName, used);
items.Add(fullType, item);
if (!itemsBySubtype.ContainsKey(subType))
itemsBySubtype.Add(subType, item);
if (shortName != "")
itemsByShort.Add(shortName.ToLower(), item);
keys.Add(fullType);
}

public static MMItem GetItemOfType(string subType = "", string mainType = "") {
if (items.ContainsKey(subType + " " + mainType))
return items[subType + " " + mainType];

if (mainType == "") {
MMItem item = null;
itemsBySubtype.TryGetValue(subType, out item);
return item;
}

if (subType == "")
for (int i = 0; i < items.Count; i++) {
MMItem item = items[keys[i]];
if (mainType == item.mainType)
return item;
}
return null;
}
}

public class LCDsProgram {
// for german clients
public static string SECONDARY_TAG = "";
// approximate width of LCD panel line
public const float LCD_LINE_WIDTH = 730;
// x position of inventory numbers
public const float LCD_LINE_NUMERS_POS = LCD_LINE_WIDTH - 30;

public const float LCD_LINE_INV_NUMBERS_POS = LCD_LINE_WIDTH - 130;
public const float LCD_LINE_INGOT_NUMBERS_POS = 375;
public const float LCD_LINE_DMG_NUMBERS_POS = LCD_LINE_WIDTH - 230;
public const float LCD_LINE_WORK_STATE_POS = LCD_LINE_WIDTH - 30;
public const float LCD_LINE_BLOCK_COUNT_POS = LCD_LINE_WIDTH - 30;

public const float PERCENT_TEXT_SIZE = 110f;

// number of component progress bar characters
public const int INV_PROGRESS_CHARS = 38;
// full line of progress bar
public const int FULL_PROGRESS_CHARS = 116;

public static int PANELS_PER_STEP = 1;

public MMPanelDict panels = new MMPanelDict();
public double velocity = 0;
public static double lastVel = 0;
public double accel = 0;

public static Dictionary<string, int> ptNLtoIdx = new Dictionary<string, int>();
public static List<double> ptElapsedL = new List<double>();
public static VRageMath.Vector3D lastPos;
public static int bootStep = 0;
public static bool bootScreens = true;
public static List<string> bootFrames = null;

public static System.Text.RegularExpressions.Regex stripTags;
public LCDsProgram(string nameLike) {
MMBlkCol lcds = new MMBlkCol();
lcds.AddBofT("textpanel", nameLike);
if (nameLike == "T:[LCD]" && SECONDARY_TAG != "")
lcds.AddBofT("textpanel", SECONDARY_TAG);

int i = 0;
while (i < lcds.Count()) {
IMyTextPanel panel = (lcds.Blocks[i] as IMyTextPanel);
if (!panel.IsWorking) {
lcds.Blocks.RemoveAt(i);
continue;
}
string text = panel.CustomName + " " + panel.NumberInGrid + " " + panel.GetPosition().ToString("F0");
MMPanel p = null;

int joinpos = text.IndexOf("!LINK:");

if (joinpos < 0 || text.Length == joinpos + 6) {
p = new MMPanel();
p.panels.AddItem(text, panel);
panels.AddItem(text, p);
i++;
continue;
}

text = text.Substring(joinpos + 6);

string[] subs = text.Split(' ');
string group = subs[0];
p = panels.GetItem(group);
if (p == null) {
p = new MMPanel();
panels.AddItem(group, p);
}
p.panels.AddItem(text, panel);
i++;
}
}

public void Run(string argument, int step) {
if (panels.CountAll() == 0)
return;

velocity = (MM.Me.GetPosition() - lastPos).Length() / MM.ElapsedTime;
accel = (velocity - lastVel) / MM.ElapsedTime;

for (int ei = 0; ei < ptElapsedL.Count; ei++)
ptElapsedL[ei] += MM.ElapsedTime;

if (argument == "clear") {
bootStep = (bootScreens ? 0 : int.MaxValue);
for (int i = 0; i < panels.CountAll(); i++) {
MMPanel p = panels.GetItemAt(i);
p.SortPanels();
MMLCDMgr.SetupLCDText(p);
MMLCDMgr.ClearText();
MMLCDMgr.UpdatePanel(p);
}
}
else
if (argument == "boot" || bootStep <= bootFrames.Count) {
if (bootStep > bootFrames.Count)
bootStep = 0;

for (int i = 0; i < panels.CountAll(); i++) {
MMPanel p = panels.GetItemAt(i);
p.SortPanels();
MMLCDMgr.SetupLCDText(p);
MMLCDMgr.ClearText();
// don't display on LCDs with really large font
if (p.first.GetValueFloat("FontSize") > 3f)
continue;
MMLCDMgr.AddCenterLn(M.T["B1"], LCD_LINE_WIDTH / 2);
double perc = (double)bootStep / bootFrames.Count * 100;
MMLCDMgr.AddBarLn(perc, FULL_PROGRESS_CHARS);
if (bootStep == bootFrames.Count) {
MMLCDMgr.AddLn("");
MMLCDMgr.AddCenterLn("Configurable Automatic LCDs", LCD_LINE_WIDTH / 2);
MMLCDMgr.AddCenterLn("by MMaster", LCD_LINE_WIDTH / 2);
}
else
MMLCDMgr.AddMultiLn(bootFrames[bootStep]);
MMLCDMgr.UpdatePanel(p);
}
bootStep++;
}
else {
for (int i = 0; i < PANELS_PER_STEP; i++) {
RunSingle(panels.GetItemAt((step * PANELS_PER_STEP + i) % panels.CountAll()));
}
}

lastPos = MM.Me.GetPosition();
lastVel = velocity;
}

public void RunSingle(MMPanel panel) {

bool useText = false;
panel.SortPanels();
MMLCDMgr.SetupLCDText(panel);

string pubText = panel.first.CustomName;
pubText = (pubText.Contains("#") ? pubText.Substring(pubText.LastIndexOf('#') + 1) : "");
MMLCDMgr.ClearText();

if (pubText != "")
MMLCDMgr.AddLn(pubText);

string title = panel.first.GetPrivateTitle();
if (title == "Title" || title == "")
title = panel.first.GetPublicTitle();

if (title.Trim() == "UseTextCommands") {
useText = true;
title = panel.first.GetPrivateText();
title = title.Replace("\\\n", "");
}

if (title.StartsWith("Public") || title == "") {
if (useText)
MMLCDMgr.AddLn(M.T["H1"]);
else
MMLCDMgr.AddLn(M.T["H2"]);
MMLCDMgr.UpdatePanel(panel);
return;
}

char[] delim = { ';', '\n' };
string[] cmds = title.Split(delim);
for (int i = 0; i < cmds.Length; i++) {
MM.Debug("Running command " + cmds[i]);
if (cmds[i].Trim().StartsWith("//"))
continue;

MMCmd cmd = new MMCmd(cmds[i]);

if (cmd.cmdstr.StartsWith("inventory") ||
cmd.cmdstr == "missing" ||
cmd.cmdstr.StartsWith("invlist"))
RunInvListing(cmd);
else
if (cmd.cmdstr.StartsWith("cargo"))
RunCargoStatus(cmd);
else
if (cmd.cmdstr.StartsWith("mass"))
RunMass(cmd);
else
if (cmd.cmdstr == "oxygen")
RunOxygenStatus(cmd);
else
if (cmd.cmdstr == "tanks")
RunTankStatus(cmd);
else
if (cmd.cmdstr.StartsWith("powertime"))
RunPowerTime(cmd);
else
if (cmd.cmdstr.StartsWith("power"))
RunPowerStatus(cmd);
else
if (cmd.cmdstr == "speed")
RunVelocity(cmd);
else
if (cmd.cmdstr.StartsWith("accel"))
RunAccel(cmd);
else
if (cmd.cmdstr.StartsWith("charge"))
RunCharge(cmd);
else
if (cmd.cmdstr.StartsWith("time") ||
cmd.cmdstr.StartsWith("date"))
RunCurrentTime(cmd);
else
if (cmd.cmdstr.StartsWith("countdown"))
RunCountdown(cmd);
else
if (cmd.cmdstr == "echo" ||
cmd.cmdstr == "center" ||
cmd.cmdstr == "right")
RunEcho(cmd);
else
if (cmd.cmdstr.StartsWith("text"))
RunText(cmd);
else
if (cmd.cmdstr.EndsWith("count"))
RunBlockCount(cmd);
else
if (cmd.cmdstr.StartsWith("dampeners") || cmd.cmdstr.StartsWith("occupied"))
RunShipCtrlStatus(cmd);
else
if (cmd.cmdstr.StartsWith("working"))
RunWorkingList(cmd);
else
if (cmd.cmdstr.StartsWith("damage"))
RunDamage(cmd);
else
if (cmd.cmdstr.StartsWith("amount"))
RunItemAmount(cmd);
else
if (cmd.cmdstr.StartsWith("pos"))
RunPosition(cmd);
else
if (cmd.cmdstr.StartsWith("details"))
RunDetails(cmd);
else
MMLCDMgr.AddLn(M.T["UC"] + ": " + cmd.cmdstr);

MM.Debug("Done.");
}

MMLCDMgr.UpdatePanel(panel);

}

public void RunCharge(MMCmd cmd) {
bool simple = cmd.cmdstr.Contains("x");
MMBlkCol blocks = new MMBlkCol();
blocks.AddBofT("jumpdrive", cmd.nameLike);

if (blocks.Count() <= 0) {
MMLCDMgr.AddLn("Charge: " + M.T["D2"]);
return;
}

for (int i = 0; i < blocks.Count(); i++) {
IMyJumpDrive jd = blocks.Blocks[i] as IMyJumpDrive;

double cur, max, perc;
perc = MMStatus.GetJDChargeVals(jd, out cur, out max);

MMLCDMgr.Add(stripTags.Replace(jd.CustomName, ""));
if (!simple) {
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(cur) + "Wh / " + MM.FormatLargeNumber(max) + "Wh", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(perc, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
}
MMLCDMgr.AddRightLn(' ' + perc.ToString("0.0") + "%", LCD_LINE_WIDTH);
}
}

public void RunVelocity(MMCmd cmd) {
MMLCDMgr.Add(M.T["S1"] + " ");
MMLCDMgr.AddRightLn(velocity.ToString("F1") + " m/s ", LCD_LINE_WIDTH);
}

public void RunAccel(MMCmd cmd) {
MMLCDMgr.Add(M.T["AC1"] + " ");
MMLCDMgr.AddRightLn(accel.ToString("F1") + " m/s²", LCD_LINE_WIDTH);
}

public void RunText(MMCmd cmd) {
bool fromLCD = (cmd.cmdstr == "textlcd");

IMyTextPanel p = MMLCDMgr.curP.first;
if (p == null)
return;

string text = p.GetPrivateText();

if (fromLCD) {
if (cmd.nameLike != "" && cmd.nameLike != "*") {
IMyTextPanel tp = MM._GridTerminalSystem.GetBlockWithName(cmd.nameLike) as IMyTextPanel;
if (tp == null) {
MMLCDMgr.AddLn("TextLCD: " + M.T["T1"] + cmd.nameLike);
return;
}

text = tp.GetPublicText();
}
else {
MMLCDMgr.AddLn("TextLCD:" + M.T["T2"]);
return;
}
}

string[] lines = text.Split('\n');
if (lines.Length == 0) {
if (!fromLCD)
MMLCDMgr.AddLn("Text: " + M.T["T3"]);
return;
}

for (int i = 0; i < lines.Length; i++)
MMLCDMgr.AddLn(lines[i]);
}

private void PrintDetails(IMyTerminalBlock block, int first = 0) {
string[] lines = block.DetailedInfo.Split('\n');
for (int j = first; j < lines.Length; j++)
if (lines[j] != "")
MMLCDMgr.AddLn("  " + lines[j]);
}

public void RunDetails(MMCmd cmd) {
if (cmd.nameLike == "" || cmd.nameLike == "*") {
MMLCDMgr.AddLn("Details: " + M.T["D1"]);
return;
}

MMBlkCol blocks = new MMBlkCol();
blocks.AddBOfName(cmd.nameLike);

if (blocks.Count() <= 0) {
MMLCDMgr.AddLn("Details: " + M.T["D2"]);
return;
}

int si = (cmd.cmdstr.EndsWith("x") ? 1 : 0);
IMyTerminalBlock block = blocks.Blocks[0];
MMLCDMgr.AddLn(stripTags.Replace(block.CustomName, ""));
PrintDetails(block, si);

for (int i = 1; i < blocks.Count(); i++) {
block = blocks.Blocks[i];
MMLCDMgr.AddLn("");
MMLCDMgr.AddLn(stripTags.Replace(block.CustomName, ""));
PrintDetails(block, si);
}
}

public void RunPosition(MMCmd cmd) {
bool posxyz = (cmd.cmdstr == "posxyz");
bool gps = (cmd.cmdstr == "posgps");

IMyTerminalBlock block = MMLCDMgr.curP.first;
if (cmd.nameLike != "" && cmd.nameLike != "*") {
block = MM._GridTerminalSystem.GetBlockWithName(cmd.nameLike);
if (block == null) {
MMLCDMgr.AddLn(M.T["P1"] + ": " + cmd.nameLike);
return;
}
}

if (gps) {
VRageMath.Vector3D pos = block.GetPosition();

MMLCDMgr.AddLn("GPS:" + M.T["P2"] + ":" +
pos.GetDim(0).ToString("F2") + ":" +
pos.GetDim(1).ToString("F2") + ":" +
pos.GetDim(2).ToString("F2") + ":");
return;
}

MMLCDMgr.Add(M.T["P2"] + ": ");
if (!posxyz) {
MMLCDMgr.AddRightLn(block.GetPosition().ToString("F0"), LCD_LINE_WORK_STATE_POS);
return;
}

MMLCDMgr.AddLn("");
MMLCDMgr.Add(" X: ");
MMLCDMgr.AddRightLn(block.GetPosition().GetDim(0).ToString("F0"), LCD_LINE_WORK_STATE_POS);
MMLCDMgr.Add(" Y: ");
MMLCDMgr.AddRightLn(block.GetPosition().GetDim(1).ToString("F0"), LCD_LINE_WORK_STATE_POS);
MMLCDMgr.Add(" Z: ");
MMLCDMgr.AddRightLn(block.GetPosition().GetDim(2).ToString("F0"), LCD_LINE_WORK_STATE_POS);
}

private void ShowBlkCntOfType(MMBlkCol blocks, string type, bool enabledCnt, bool producingCnt) {
string name;

if (blocks.Count() == 0) {
name = type.ToLower();
name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
MMLCDMgr.Add(name + " " + M.T["C1"] + " ");
string countstr = (enabledCnt || producingCnt ? "0 / 0" : "0");
MMLCDMgr.AddRightLn(countstr, LCD_LINE_BLOCK_COUNT_POS);
}
else {
Dictionary<string, int> typeCount = new Dictionary<string, int>();
Dictionary<string, int> typeWorkingCount = new Dictionary<string, int>();
List<string> blockTypes = new List<string>();

for (int j = 0; j < blocks.Count(); j++) {
IMyProductionBlock prod = blocks.Blocks[j] as IMyProductionBlock;
name = blocks.Blocks[j].DefinitionDisplayNameText;
if (blockTypes.Contains(name)) {
typeCount[name]++;
if ((enabledCnt && blocks.Blocks[j].IsWorking) ||
(producingCnt && prod != null && prod.IsProducing))
typeWorkingCount[name]++;
}
else {
typeCount.Add(name, 1);
blockTypes.Add(name);
if (enabledCnt || producingCnt)
if ((enabledCnt && blocks.Blocks[j].IsWorking) ||
(producingCnt && prod != null && prod.IsProducing))
typeWorkingCount.Add(name, 1);
else
typeWorkingCount.Add(name, 0);
}
}
for (int j = 0; j < typeCount.Count; j++) {
MMLCDMgr.Add(blockTypes[j] + " " + M.T["C1"] + " ");
string countstr = (enabledCnt || producingCnt ?
typeWorkingCount[blockTypes[j]] + " / " : "") +
typeCount[blockTypes[j]];

MMLCDMgr.AddRightLn(countstr, LCD_LINE_BLOCK_COUNT_POS);
}
}
}

public void RunBlockCount(MMCmd cmd) {
bool enabledCnt = (cmd.cmdstr == "enabledcount");
bool producingCnt = (cmd.cmdstr == "prodcount");

if (cmd.args.Count == 0) {
MMBlkCol blocks = new MMBlkCol();
blocks.AddBOfName(cmd.nameLike);
ShowBlkCntOfType(blocks, "blocks", enabledCnt, producingCnt);
return;
}

for (int i = 0; i < cmd.args.Count; i++) {
MMArg arg = cmd.args[i];
arg.Parse();

for (int subi = 0; subi < arg.sub.Count; subi++) {
MMBlkCol blocks = new MMBlkCol();
blocks.AddBofT(arg.sub[subi], cmd.nameLike);
ShowBlkCntOfType(blocks, arg.sub[subi], enabledCnt, producingCnt);
}
}
}

public string GetWorkingString(IMyTerminalBlock block) {
if (!block.IsWorking)
return M.T["W1"];

IMyProductionBlock prod = block as IMyProductionBlock;
if (prod != null)
if (prod.IsProducing)
return M.T["W2"];
else
return M.T["W3"];

IMyAirVent vent = block as IMyAirVent;
if (vent != null) {
if (vent.CanPressurize)
return (vent.GetOxygenLevel() * 100).ToString("F1") + "%";
else
return M.T["W4"];
}

IMyOxygenTank tank = block as IMyOxygenTank;
if (tank != null)
return (tank.GetOxygenLevel() * 100).ToString("F1") + "%";

IMyBatteryBlock battery = block as IMyBatteryBlock;
if (battery != null)
return MMStatus.GetBatWorkingStr(battery);

IMyJumpDrive jd = block as IMyJumpDrive;
if (jd != null)
return MMStatus.GetJDCharge(jd).ToString("0.0") + "%";

IMyLandingGear gear = block as IMyLandingGear;
if (gear != null)
return MMStatus.GetLGStatus(gear);

IMyDoor door = block as IMyDoor;
if (door != null) {
if (door.Open)
return M.T["W5"];
return M.T["W6"];
}

IMyShipConnector conn = block as IMyShipConnector;
if (conn != null)
if (conn.IsLocked) {
if (conn.IsConnected)
return M.T["W7"];
else
return M.T["W10"];
}
else
return M.T["W8"];

IMyLaserAntenna lasant = block as IMyLaserAntenna;
if (lasant != null)
return MMStatus.GetLAStatus(lasant);

IMyRadioAntenna ant = block as IMyRadioAntenna;
if (ant != null)
return MM.FormatLargeNumber(ant.Radius) + "m";

IMyBeacon beacon = block as IMyBeacon;
if (beacon != null)
return MM.FormatLargeNumber(beacon.Radius) + "m";

return M.T["W9"];
}

private void ShowWorkingOfType(MMBlkCol blocks, bool enabledList, string state) {
for (int j = 0; j < blocks.Count(); j++) {
IMyTerminalBlock block = blocks.Blocks[j];
string onoff = (enabledList ? (block.IsWorking ? M.T["W9"] : M.T["W1"]) : GetWorkingString(block));
if (state != "" && onoff.ToLower() != state)
continue;
if (enabledList)
onoff = GetWorkingString(block);

string blockName = block.CustomName;
blockName = MMStrFn.GetTrimmed(stripTags.Replace(blockName, ""), LCD_LINE_WORK_STATE_POS - 100);
MMLCDMgr.Add(blockName);
MMLCDMgr.AddRightLn(onoff, LCD_LINE_WORK_STATE_POS);
}
}
public void RunShipCtrlStatus(MMCmd cmd) {
MMBlkCol blocks = new MMBlkCol();
blocks.AddBofT("shipctrl", cmd.nameLike);

if (blocks.Count() <= 0) {
if (cmd.nameLike != "" && cmd.nameLike != "*")
MMLCDMgr.AddLn(cmd.cmdstr + ": " + M.T["SC1"] + " (" + cmd.nameLike + ")");
else
MMLCDMgr.AddLn(cmd.cmdstr + ": " + M.T["SC1"]);
return;
}

if (cmd.cmdstr.StartsWith("damp")) {
bool s = (blocks.Blocks[0] as IMyShipController).DampenersOverride;
MMLCDMgr.Add(M.T["SCD"]);
MMLCDMgr.AddRightLn(s ? "ON" : "OFF", LCD_LINE_WORK_STATE_POS);
}
else {
bool s = (blocks.Blocks[0] as IMyShipController).IsUnderControl;
MMLCDMgr.Add(M.T["SCO"]);
MMLCDMgr.AddRightLn(s ? "YES" : "NO", LCD_LINE_WORK_STATE_POS);
}
}
public void RunWorkingList(MMCmd cmd) {
bool enabledList = (cmd.cmdstr == "workingx");

if (cmd.args.Count == 0) {
MMBlkCol blocks = new MMBlkCol();
blocks.AddBOfName(cmd.nameLike);
ShowWorkingOfType(blocks, enabledList, "");
return;
}

for (int i = 0; i < cmd.args.Count; i++) {
MMArg arg = cmd.args[i];
arg.Parse();

for (int subi = 0; subi < arg.sub.Count; subi++) {
MMBlkCol blocks = new MMBlkCol();
if (arg.sub[subi] == "")
continue;
string[] subparts = arg.sub[subi].ToLower().Split(':');
string subargtype = subparts[0];
string subargstate = (subparts.Length > 1 ? subparts[1] : "");
if (subargtype != "")
blocks.AddBofT(subargtype, cmd.nameLike);
else
blocks.AddBOfName(cmd.nameLike);

ShowWorkingOfType(blocks, enabledList, subargstate);
}
}
}

public void RunItemAmount(MMCmd cmd) {
bool progressbars = true;
if (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'x') {
cmd.cmdstr = cmd.cmdstr.Substring(0, cmd.cmdstr.Length - 1);
progressbars = false;
}

if (cmd.args.Count == 0)
cmd.args.Add(new MMArg(
"reactor,gatlingturret,missileturret,interiorturret,gatlinggun,launcherreload,launcher,oxygenerator"));

for (int i = 0; i < cmd.args.Count; i++) {
MMArg arg = cmd.args[i];
arg.Parse();

for (int subi = 0; subi < arg.sub.Count; subi++) {
MMBlkCol blocks = new MMBlkCol();
if (arg.sub[subi] == "")
continue;
string subargtype = arg.sub[subi];
blocks.AddBofT(subargtype, cmd.nameLike);

for (int j = 0; j < blocks.Count(); j++) {
IMyTerminalBlock block = blocks.Blocks[j];
IMyInventory inv = block.GetInventory(0);
if (inv == null)
continue;

double amt = 0;
double maxAmt = 0;
double otherAmt = 0;
List<IMyInventoryItem> items = inv.GetItems();
string itemType = (items.Count > 0 ? items[0].Content.ToString() : "");
for (int idx = 0; idx < items.Count; idx++) {
IMyInventoryItem item = items[idx];

if (item.Content.ToString() != itemType)
otherAmt += (double)item.Amount;
else
amt += (double)item.Amount;
}
string amountStr = M.T["A1"];
string blockName = block.CustomName;

if (amt > 0 && (double)inv.CurrentVolume > 0) {
double otherVol = otherAmt * (double)inv.CurrentVolume / (amt + otherAmt);
maxAmt = Math.Floor(amt * ((double)inv.MaxVolume - otherVol) / (double)inv.CurrentVolume - otherVol);

amountStr = MM.FormatLargeNumber(amt) + " / " + (otherAmt > 0 ? "~" : "") + MM.FormatLargeNumber(maxAmt);
}

blockName = MMStrFn.GetTrimmed(stripTags.Replace(blockName, ""), LCD_LINE_WORK_STATE_POS - 60);
MMLCDMgr.Add(blockName);
MMLCDMgr.AddRightLn(amountStr, LCD_LINE_WORK_STATE_POS);

if (progressbars && maxAmt > 0) {
double perc = 100 * amt / maxAmt;
MMLCDMgr.AddBarLn(perc, FULL_PROGRESS_CHARS);
}
}
}
}
}

public void RunEcho(MMCmd cmd) {
string type = (cmd.cmdstr == "center" ? "c" : (cmd.cmdstr == "right" ? "r" : "n"));
int idx = cmd.cmdLine.IndexOf(' ');
string msg = "";
if (idx >= 0)
msg = cmd.cmdLine.Substring(idx + 1);

switch (type) {
case "c":
MMLCDMgr.AddCenterLn(msg, LCD_LINE_WIDTH / 2);
break;
case "r":
MMLCDMgr.AddRightLn(msg, LCD_LINE_WIDTH);
break;
default:
MMLCDMgr.AddLn(msg);
break;
}
}

public void RunDamage(MMCmd cmd) {
bool simple = cmd.cmdstr.StartsWith("damagex");
bool onlyDmg = cmd.cmdstr.EndsWith("noc"); // no construct

MMBlkCol blocks = new MMBlkCol();
blocks.AddBOfName(cmd.nameLike);
bool found = false;

for (int i = 0; i < blocks.Count(); i++) {
IMyTerminalBlock block = blocks.Blocks[i];
IMySlimBlock slim = block.CubeGrid.GetCubeBlock(block.Position);
float hull = (onlyDmg ? slim.MaxIntegrity : slim.BuildIntegrity) - slim.CurrentDamage;
float perc = 100 * (hull / slim.MaxIntegrity);

if (perc >= 100)
continue;

found = true;

MMLCDMgr.Add(MMStrFn.GetTrimmed(slim.FatBlock.DisplayNameText,
LCD_LINE_DMG_NUMBERS_POS - 70) + " ");
if (!simple) {
MMLCDMgr.AddRight(MM.FormatLargeNumber(hull) + " / ",
LCD_LINE_DMG_NUMBERS_POS);
MMLCDMgr.Add(MM.FormatLargeNumber(slim.MaxIntegrity));
}
MMLCDMgr.AddRightLn(' ' + perc.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(perc, FULL_PROGRESS_CHARS);
}

if (!found)
MMLCDMgr.AddLn(M.T["D3"]);
}

public void RunTankStatus(MMCmd cmd) {
List<MMArg> args = cmd.args;
string tankType;

if (args.Count == 0) {
MMLCDMgr.AddLn(M.T["T4"]);
return;
}

tankType = args[0].arg.Trim().ToLower();

double percent;
MMBlkCol blocks = new MMBlkCol();

blocks.AddBofT("oxytank", cmd.nameLike);

double tankSum = 0;
int cnt = blocks.Count();
for (int i = 0; i < blocks.Count(); i++) {
IMyOxygenTank tank = blocks.Blocks[i] as IMyOxygenTank;
if ((tankType == "oxygen" && tank.BlockDefinition.SubtypeId == "") ||
tank.BlockDefinition.SubtypeId.ToLower().Contains(tankType)) // only selected tank type
tankSum += tank.GetOxygenLevel() * 100;
else
cnt--;
}

if (cnt == 0) {
MMLCDMgr.AddLn(String.Format(M.T["T5"], tankType));
return;
}

percent = tankSum / cnt;

tankType = char.ToUpper(tankType[0]) + tankType.Substring(1);

MMLCDMgr.Add(tankType + " " + M.T["T6"]);
MMLCDMgr.AddRightLn(percent.ToString("F2") + "%", LCD_LINE_WORK_STATE_POS);
MMLCDMgr.AddBarLn(percent, FULL_PROGRESS_CHARS);
}

public void RunOxygenStatus(MMCmd cmd) {
double percent;
MMBlkCol blocks = new MMBlkCol();

blocks.AddBofT("airvent", cmd.nameLike);
bool found = (blocks.Count() > 0);

for (int i = 0; i < blocks.Count(); i++) {
IMyAirVent vent = blocks.Blocks[i] as IMyAirVent;
percent = Math.Max(vent.GetOxygenLevel() * 100, 0f);

MMLCDMgr.Add(stripTags.Replace(vent.CustomName, ""));

if (vent.CanPressurize)
MMLCDMgr.AddRightLn(percent.ToString("F1") + "%", LCD_LINE_WORK_STATE_POS);
else
MMLCDMgr.AddRightLn(M.T["O1"], LCD_LINE_WORK_STATE_POS);
MMLCDMgr.AddBarLn(percent, FULL_PROGRESS_CHARS);
}

blocks.Clear();
blocks.AddBofT("oxyfarm", cmd.nameLike);
int cnt = blocks.Count();
if (cnt > 0) {
double farmSum = 0;
for (int i = 0; i < cnt; i++) {
IMyOxygenFarm farm = blocks.Blocks[i] as IMyOxygenFarm;
farmSum += farm.GetOutput() * 100;
}

percent = farmSum / cnt;

if (found)
MMLCDMgr.AddLn("");

found |= (cnt > 0);

MMLCDMgr.Add(M.T["O2"]);
MMLCDMgr.AddRightLn(percent.ToString("F2") + "%", LCD_LINE_WORK_STATE_POS);
MMLCDMgr.AddBarLn(percent, FULL_PROGRESS_CHARS);
}

blocks.Clear();
blocks.AddBofT("oxytank", cmd.nameLike);
cnt = blocks.Count();

if (cnt == 0) {
if (!found)
MMLCDMgr.AddLn(M.T["O3"]);
return;
}

double tankSum = 0;
int tankCnt = cnt;
for (int i = 0; i < cnt; i++) {
IMyOxygenTank tank = blocks.Blocks[i] as IMyOxygenTank;
if (tank.BlockDefinition.SubtypeId == "" ||
tank.BlockDefinition.SubtypeId.Contains("Oxygen")) // only oxygen tanks
tankSum += tank.GetOxygenLevel() * 100;
else
tankCnt--;
}

if (tankCnt == 0) {
if (!found)
MMLCDMgr.AddLn(M.T["O3"]);
return;
}

percent = tankSum / tankCnt;

if (found)
MMLCDMgr.AddLn("");
MMLCDMgr.Add(M.T["O4"]);
MMLCDMgr.AddRightLn(percent.ToString("F2") + "%", LCD_LINE_WORK_STATE_POS);
MMLCDMgr.AddBarLn(percent, FULL_PROGRESS_CHARS);
}

public void RunMass(MMCmd cmd) {
MMBlkCol blocks = new MMBlkCol();
bool simple = (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'x');
bool progress = (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'p');

blocks.AddBOfName(cmd.nameLike);

double used = blocks.GetMassSummary();
double total = 0;

int argCnt = cmd.args.Count;
if (argCnt > 0) {
cmd.args[0].Parse();
if (cmd.args[0].sub.Count > 0)
double.TryParse(cmd.args[0].sub[0].Trim(), out total);
if (argCnt > 1) {
cmd.args[1].Parse();
if (cmd.args[1].sub.Count > 0) {
string unit = cmd.args[1].sub[0].Trim().ToLower();
if (unit != "")
total *= Math.Pow(1000.0, "kmgtpezy".IndexOf(unit[0]));
}
}
total *= 1000.0;
}

MMLCDMgr.Add(M.T["M1"] + " ");
if (total <= 0) {
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(used), LCD_LINE_WIDTH);
return;
}

double perc = used / total * 100;

if (!simple && !progress) {
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(used) + "g / " + MM.FormatLargeNumber(total) + "g", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(perc, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddLn(' ' + perc.ToString("0.0") + "%");
}
else if (progress) {
MMLCDMgr.AddRightLn(perc.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(perc, FULL_PROGRESS_CHARS);
}
else
MMLCDMgr.AddRightLn(perc.ToString("0.0") + "%", LCD_LINE_WIDTH);
}


class PTD {
public TimeSpan t = new TimeSpan(-1);
public double ct = -1;
public double lastFuel = 0;
}
static Dictionary<string, PTD> savedPT = new Dictionary<string, PTD>();

private double GetPTElapsed(string nameLike) {
if (ptNLtoIdx.ContainsKey(nameLike))
return ptElapsedL[ptNLtoIdx[nameLike]];
ptElapsedL.Add(0);
ptNLtoIdx.Add(nameLike, ptElapsedL.Count - 1);
return 0;
}
private PTD GetSavedPT(string nameLike) {
if (!savedPT.ContainsKey(nameLike))
savedPT[nameLike] = new PTD();
return savedPT[nameLike];
}
private TimeSpan GetPowerTime(string nameLike, out double chargeTime) {
double ptElapsed = GetPTElapsed(nameLike);
PTD pt = GetSavedPT(nameLike);

// at least few seconds to get somehow reliable values
if (ptElapsed <= 3) {
chargeTime = pt.ct;
return pt.t;
}
pt.ct = 0;

MMBlkCol rbs = new MMBlkCol();
rbs.AddBofT("reactor", nameLike);

double fuel = 0;
double rcur = 0, rmax = 0;
for (int bi = 0; bi < rbs.Blocks.Count; bi++) {
IMyReactor block = rbs.Blocks[bi] as IMyReactor;
if (block == null || !block.IsWorking)
continue;
List<double> vals = MMStatus.GetDetailVals(block);
if (vals.Count < 2)
continue;
rmax += vals[0] / 1000000;
rcur += vals[1] / 1000000;
IMyInventory inv = block.GetInventory(0);
fuel += (double)inv.CurrentMass;
}

MMBlkCol bbs = new MMBlkCol();
bbs.AddBofT("battery", nameLike);
double stored = 0;
double bcur = 0, bmax = 0;
for (int bi = 0; bi < bbs.Blocks.Count; bi++) {
IMyBatteryBlock block = bbs.Blocks[bi] as IMyBatteryBlock;
if (block == null || !block.IsWorking)
continue;
List<double> vals = MMStatus.GetDetailVals(block);
if (vals.Count < 6)
continue;
double output = (vals[4] - vals[3]) / 1000000;
double cTime = (output < 0 ? (block.MaxStoredPower - block.CurrentStoredPower) / (-output / 3600) : 0);
if (cTime > pt.ct) pt.ct = cTime;
if (block.GetValueBool("Recharge"))
continue;
bcur += output;
bmax += vals[0] / 1000000;
stored += block.CurrentStoredPower;
}

double usedPower = rcur + bcur;
if (usedPower <= 0)
pt.t = TimeSpan.FromSeconds(-1);
else {
double tPT = pt.t.TotalSeconds;
double sPT;
double deltaF = (pt.lastFuel - fuel) / ptElapsed;
if (rcur <= 0) /* 1000MW per ingot .. better than nothing */
deltaF = Math.Min(usedPower, rmax) / 3600000;

double deltaS = 0;
if (bmax > 0)
deltaS = Math.Min(usedPower, bmax) / 3600;

if (deltaF <= 0 && deltaS <= 0)
sPT = -1;
else
if (deltaF <= 0)
sPT = stored / deltaS;
else
if (deltaS <= 0)
sPT = fuel / deltaF;
else {
double adS = deltaS;
double adF = (rcur <= 0 ? usedPower / 3600 : deltaF * usedPower / rcur);
sPT = stored / adS + fuel / adF;
}
if (tPT <= 0 || sPT < 0)
tPT = sPT;
else
tPT = (tPT + sPT) / 2;
pt.t = TimeSpan.FromSeconds(tPT);
}
ptElapsedL[ptNLtoIdx[nameLike]] = 0;
pt.lastFuel = fuel;
chargeTime = pt.ct;
return pt.t;
}
private string GetRemTStr(TimeSpan remT) {
string text = "";

if (remT.Ticks <= 0)
return "-";
if ((int)remT.TotalDays > 0)
text += (long)remT.TotalDays + " " + M.T["C5"] + " ";
if (remT.Hours > 0 || text != "")
text += remT.Hours + "h ";
if (remT.Minutes > 0 || text != "")
text += remT.Minutes + "m ";
return text + remT.Seconds + "s";
}
public void RunPowerTime(MMCmd cmd) {
bool simple = (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'x');
bool progress = (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'p');

int h = 0, m = 0, s = 0;
int c = 0;
if (cmd.args.Count > 0) {
for (int ai = 0; ai < cmd.args.Count; ai++) {
cmd.args[ai].Parse();
if (cmd.args[ai].sub.Count <= 0)
continue;
string arg = cmd.args[ai].sub[0];
int.TryParse(arg, out c);
if (ai == 0)
h = c;
else if (ai == 1)
m = c;
else if (ai == 2)
s = c;
}
}

double ct = 0, t = 0;
TimeSpan remT = GetPowerTime(cmd.nameLike, out ct);
TimeSpan totalT = new TimeSpan(h, m, s);

string text;
if (remT.TotalSeconds > 0 || ct <= 0) {
MMLCDMgr.Add(M.T["PT1"] + " ");
text = GetRemTStr(remT);
t = remT.TotalSeconds;
}
else {
MMLCDMgr.Add(M.T["PT2"] + " ");
text = GetRemTStr(TimeSpan.FromSeconds(ct));
if (totalT.TotalSeconds >= ct)
t = totalT.TotalSeconds - ct;
else
t = 0;
}

if (totalT.Ticks <= 0) {
MMLCDMgr.AddRightLn(text, LCD_LINE_WIDTH);
return;
}

double perc = t / totalT.TotalSeconds * 100;
if (perc > 100) perc = 100;

if (!simple && !progress) {
MMLCDMgr.AddRightLn(text, LCD_LINE_WIDTH);
MMLCDMgr.AddBar(perc, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddLn(' ' + perc.ToString("0.0") + "%");
}
else if (progress) {
MMLCDMgr.AddRightLn(perc.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(perc, FULL_PROGRESS_CHARS);
}
else
MMLCDMgr.AddRightLn(perc.ToString("0.0") + "%", LCD_LINE_WIDTH);
}

public void RunCargoStatus(MMCmd cmd) {
MMBlkCol blocks = new MMBlkCol();
bool alltypes = cmd.cmdstr.Contains("all");
bool simple = (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'x');
bool progress = (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'p');

if (alltypes)
blocks.AddBOfName(cmd.nameLike);
else
blocks.AddBofT("cargocontainer", cmd.nameLike);

double usedCargo = 0;
double totalCargo = 0;
double percentCargo = blocks.GetCargoSummary(
ref usedCargo, ref totalCargo);

MMLCDMgr.Add(M.T["C2"] + " ");
if (!simple && !progress) {
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(usedCargo) + "L / " + MM.FormatLargeNumber(totalCargo) + "L", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(percentCargo, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddLn(' ' + percentCargo.ToString("0.0") + "%");
}
else if (progress) {
MMLCDMgr.AddRightLn(percentCargo.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(percentCargo, FULL_PROGRESS_CHARS);
}
else
MMLCDMgr.AddRightLn(percentCargo.ToString("0.0") + "%", LCD_LINE_WIDTH);
}

// type: simple, progress, normal
public void ShowPowerOutput(MMBlkCol generators, string title, string type = "n") {
double usedPower = 0, totalPower = 0;
double percentPower = generators.GetPowerOutput(ref usedPower, ref totalPower);

MMLCDMgr.Add(title + ": ");
switch (type) {
case "s":
MMLCDMgr.AddRightLn(' ' + percentPower.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
case "v":
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(usedPower) + "W / " + MM.FormatLargeNumber(totalPower) + "W", LCD_LINE_WIDTH);
break;
case "p":
MMLCDMgr.AddRightLn(' ' + percentPower.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(percentPower, FULL_PROGRESS_CHARS);
break;
default:
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(usedPower) + "W / " + MM.FormatLargeNumber(totalPower) + "W", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(percentPower, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddRightLn(' ' + percentPower.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
}
}

public void ShowSolarOutput(MMBlkCol generators, string title, string type = "n") {
double usedPower = 0, totalPower = 0;
generators.GetPowerOutput(ref usedPower, ref totalPower);
double percentPower = (totalPower > 0 ? (usedPower / totalPower) * 100 : 100);

MMLCDMgr.Add(title + ": ");
switch (type) {
case "s":
MMLCDMgr.AddRightLn(' ' + percentPower.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
case "v":
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(usedPower) + "W / " + MM.FormatLargeNumber(totalPower) + "W", LCD_LINE_WIDTH);
break;
case "p":
MMLCDMgr.AddRightLn(' ' + percentPower.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(percentPower, FULL_PROGRESS_CHARS);
break;
default:
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(usedPower) + "W / " + MM.FormatLargeNumber(totalPower) + "W", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(percentPower, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddRightLn(' ' + percentPower.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
}
}

public void ShowBatteriesInfo(MMBlkCol batteries, string title, string what = "a", string type = "n") {
double output = 0, maxOutput = 0, input = 0, maxInput = 0;
double stored = 0, maxStored = 0;
double percentStored =
batteries.GetBatStats(
ref output, ref maxOutput,
ref input, ref maxInput,
ref stored, ref maxStored);
double percentOutput = (maxOutput > 0 ? (output / maxOutput) * 100 : 100);
double percentInput = (maxInput > 0 ? (input / maxInput) * 100 : 100);
bool showall = what == "a";

if (showall) {
if (type != "p") {
MMLCDMgr.Add(title + ": ");
MMLCDMgr.AddRightLn("(IN " + MM.FormatLargeNumber(input) + "W / OUT " + MM.FormatLargeNumber(output) + "W)", LCD_LINE_WIDTH);
}
else
MMLCDMgr.AddLn(title + ": ");

MMLCDMgr.Add("  " + M.T["P3"] + ": ");
}
else
MMLCDMgr.Add(title + ": ");

if (showall || what == "s")
switch (type) {
case "s":
MMLCDMgr.AddRightLn(' ' + percentStored.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
case "v":
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(stored) + "Wh / " + MM.FormatLargeNumber(maxStored) + "Wh", LCD_LINE_WIDTH);
break;
case "p":
MMLCDMgr.AddRightLn(' ' + percentStored.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(percentStored, FULL_PROGRESS_CHARS);
break;
default:
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(stored) + "Wh / " + MM.FormatLargeNumber(maxStored) + "Wh", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(percentStored, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddRightLn(' ' + percentStored.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
}

if (what == "s")
return;

if (showall)
MMLCDMgr.Add("  " + M.T["P4"] + ": ");

if (showall || what == "o")
switch (type) {
case "s":
MMLCDMgr.AddRightLn(' ' + percentOutput.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
case "v":
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(output) + "W / " + MM.FormatLargeNumber(maxOutput) + "W", LCD_LINE_WIDTH);
break;
case "p":
MMLCDMgr.AddRightLn(' ' + percentOutput.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(percentOutput, FULL_PROGRESS_CHARS);
break;
default:
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(output) + "W / " + MM.FormatLargeNumber(maxOutput) + "W", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(percentOutput, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddRightLn(' ' + percentOutput.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
}

if (what == "o")
return;

if (showall)
MMLCDMgr.Add("  " + M.T["P5"] + ": ");

if (showall || what == "i")
switch (type) {
case "s":
MMLCDMgr.AddRightLn(' ' + percentInput.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
case "v":
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(input) + "W / " + MM.FormatLargeNumber(maxInput) + "W", LCD_LINE_WIDTH);
break;
case "p":
MMLCDMgr.AddRightLn(' ' + percentInput.ToString("0.0") + "%", LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(percentInput, FULL_PROGRESS_CHARS);
break;
default:
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(input) + "W / " + MM.FormatLargeNumber(maxInput) + "W", LCD_LINE_WIDTH);
MMLCDMgr.AddBar(percentInput, FULL_PROGRESS_CHARS, PERCENT_TEXT_SIZE);
MMLCDMgr.AddRightLn(' ' + percentInput.ToString("0.0") + "%", LCD_LINE_WIDTH);
break;
}
}

public void RunPowerStatus(MMCmd cmd) {
MMBlkCol reactors = new MMBlkCol();
MMBlkCol solars = new MMBlkCol();
MMBlkCol batteries = new MMBlkCol();
int got;
string displayType = (cmd.cmdstr.EndsWith("x") ? "s" : (cmd.cmdstr.EndsWith("p") ? "p" : (cmd.cmdstr.EndsWith("v") ? "v" : "n")));
bool issummary = (cmd.cmdstr.StartsWith("powersummary"));
string what = "a";
string titleOvrd = "";

if (cmd.cmdstr.Contains("stored"))
what = "s";
else if (cmd.cmdstr.Contains("in"))
what = "i";
else if (cmd.cmdstr.Contains("out"))
what = "o";

if (what == "a") {
reactors.AddBofT("reactor", cmd.nameLike);
solars.AddBofT("solarpanel", cmd.nameLike);
}
batteries.AddBofT("battery", cmd.nameLike);

got = 0;
int reactCnt = reactors.Count();
int solarCnt = solars.Count();
int batteCnt = batteries.Count();

if (reactCnt > 0) got++;
if (solarCnt > 0) got++;
if (batteCnt > 0) got++;

if (got < 1) {
MMLCDMgr.AddLn(M.T["P6"]);
return;
}

if (cmd.args.Count > 0) {
cmd.args[0].Parse();
if (cmd.args[0].sub.Count > 0 && cmd.args[0].sub[0].Length > 0)
titleOvrd = cmd.args[0].sub[0];
}

if (what != "a") {
ShowBatteriesInfo(batteries, (titleOvrd == "" ? M.T["P7"] : titleOvrd), what, displayType);
return;
}

string title = M.T["P8"];

if (!issummary) {
if (reactCnt > 0)
ShowPowerOutput(reactors, (titleOvrd == "" ? M.T["P9"] : titleOvrd), displayType);
if (solarCnt > 0)
ShowSolarOutput(solars, (titleOvrd == "" ? M.T["P10"] : titleOvrd), displayType);
if (batteCnt > 0)
ShowBatteriesInfo(batteries, (titleOvrd == "" ? M.T["P7"] : titleOvrd), what, displayType);
}
else {
title = M.T["P11"];
got = 10; // hack ;)
}

if (got == 1)
return;

MMBlkCol blocks = new MMBlkCol();
blocks.AddFromBC(reactors);
blocks.AddFromBC(solars);
blocks.AddFromBC(batteries);
ShowPowerOutput(blocks, title, displayType);
}

public void RunCountdown(MMCmd cmd) {
bool center = cmd.cmdstr.EndsWith("c");
bool right = cmd.cmdstr.EndsWith("r");

string input = "";
int firstSpace = cmd.cmdLine.IndexOf(' ');
if (firstSpace >= 0)
input = cmd.cmdLine.Substring(firstSpace + 1).Trim();

DateTime now = DateTime.Now;
DateTime inputt;
// 19:02 3.9.2015
if (!DateTime.TryParseExact(input, "H:mm d.M.yyyy", System.Globalization.CultureInfo.InvariantCulture,
System.Globalization.DateTimeStyles.None, out inputt)) {
MMLCDMgr.AddLn(M.T["C3"]);
MMLCDMgr.AddLn("  Countdown 19:02 28.2.2015");
return;
}

TimeSpan dt = inputt - now;
string text = "";

if (dt.Ticks <= 0)
text = M.T["C4"];
else {
if ((int)dt.TotalDays > 0)
text += (int)dt.TotalDays + " " + M.T["C5"] + " ";
if (dt.Hours > 0 || text != "")
text += dt.Hours + "h ";
if (dt.Minutes > 0 || text != "")
text += dt.Minutes + "m ";
text += dt.Seconds + "s";
}
if (center)
MMLCDMgr.AddCenterLn(text, LCD_LINE_WIDTH / 2);
else if (right)
MMLCDMgr.AddRightLn(text, LCD_LINE_WIDTH);
else
MMLCDMgr.AddLn(text);
}

public void RunCurrentTime(MMCmd cmd) {
bool datetime = (cmd.cmdstr.StartsWith("datetime"));
bool date = (cmd.cmdstr.StartsWith("date"));
bool center = cmd.cmdstr.Contains("c");
int offsetIdx = cmd.cmdstr.IndexOf('+');
if (offsetIdx < 0)
offsetIdx = cmd.cmdstr.IndexOf('-');
float offset = 0;

if (offsetIdx >= 0)
float.TryParse(cmd.cmdstr.Substring(offsetIdx), out offset);

DateTime dt = DateTime.Now.AddHours(offset);

string text = "";
int firstSpace = cmd.cmdLine.IndexOf(' ');
if (firstSpace >= 0)
text = cmd.cmdLine.Substring(firstSpace + 1);
if (!datetime) {
if (!date)
text += dt.ToShortTimeString();
else
text += dt.ToShortDateString();
}
else {
if (text == "")
text = String.Format("{0:d} {0:t}", dt);
else {
// try to prevent exceptions
text = text.Replace("/", "\\/");
text = text.Replace(":", "\\:");
text = text.Replace("\"", "\\\"");
text = text.Replace("'", "\\'");
text = dt.ToString(text + ' ');
text = text.Substring(0, text.Length - 1);
}
}

if (center)
MMLCDMgr.AddCenterLn(text, LCD_LINE_WIDTH / 2);
else
MMLCDMgr.AddLn(text);
}

private void ShowInvLine(string msg, double num, int quota) {
if (quota > 0) {
MMLCDMgr.AddBar(Math.Min(100, 100 * num / quota), INV_PROGRESS_CHARS);
MMLCDMgr.Add(' ' + msg + ' ');
MMLCDMgr.AddRight(MM.FormatLargeNumber(num), LCD_LINE_INV_NUMBERS_POS);
MMLCDMgr.AddLn(" / " + MM.FormatLargeNumber(quota));
}
else {
MMLCDMgr.Add(msg + ':');
MMLCDMgr.AddRightLn(MM.FormatLargeNumber(num), LCD_LINE_NUMERS_POS);
}
}

private void ShowInvIngotLine(string msg, double num, double numOres, int quota) {
if (quota > 0) {
MMLCDMgr.Add(msg + ' ');
MMLCDMgr.AddRight(MM.FormatLargeNumber(num), LCD_LINE_INGOT_NUMBERS_POS);
MMLCDMgr.Add(" / " + MM.FormatLargeNumber(quota));
MMLCDMgr.AddRightLn("+" + MM.FormatLargeNumber(numOres) + " " + M.T["I1"], LCD_LINE_WIDTH);
MMLCDMgr.AddBarLn(Math.Min(100, 100 * num / quota), FULL_PROGRESS_CHARS);
}
else {
MMLCDMgr.Add(msg + ':');
MMLCDMgr.AddRight(MM.FormatLargeNumber(num), LCD_LINE_INGOT_NUMBERS_POS);
MMLCDMgr.AddRightLn("+" + MM.FormatLargeNumber(numOres) + " " + M.T["I1"], LCD_LINE_WIDTH);
}
}

public void PrintItems(MMItemAmounts amounts, bool missing, bool simple, string mainType, string displayType) {
List<MMAmountSpec> items = amounts.GetAmountsOfMain(mainType);
if (items.Count > 0) {
if (!simple) {
if (MMLCDMgr.curLcd.curLine > 0 && MMLCDMgr.curLcd.lines[0] != "")
MMLCDMgr.AddLn("");  // add empty line
MMLCDMgr.AddCenterLn("<< " + displayType + " " + M.T["I2"] + " >>", LCD_LINE_WIDTH / 2);
}

for (int i = 0; i < items.Count; i++) {
double num = items[i].current;

if (missing && num >= items[i].min)
continue;

int quota = items[i].max;
if (missing)
quota = items[i].min;

string msg = MM.ToDisplay(items[i].subType, items[i].mainType);

ShowInvLine(msg, num, quota);
}
}
}

public void RunInvListing(MMCmd cmd) {
MMBlkCol blocks = new MMBlkCol();
bool noexpand = false;
bool simple = false;
if (cmd.cmdstr[cmd.cmdstr.Length - 1] == 's') {
cmd.cmdstr = cmd.cmdstr.Substring(0, cmd.cmdstr.Length - 1);
simple = true;
}
if (cmd.cmdstr[cmd.cmdstr.Length - 1] == 'x') {
cmd.cmdstr = cmd.cmdstr.Substring(0, cmd.cmdstr.Length - 1);
noexpand = true;
}

bool missing = (cmd.cmdstr == "missing");
bool nocats = (cmd.cmdstr == "invlist");

blocks.AddBOfName(cmd.nameLike);

MMItemAmounts amounts = new MMItemAmounts();
List<MMArg> args = cmd.args;
if (args.Count == 0)
args.Add(new MMArg("all"));

for (int i = 0; i < args.Count; i++) {
MMArg arg = args[i];
arg.Parse();
string mainType = arg.main.ToLower();

for (int subi = 0; subi < arg.sub.Count; subi++) {
string[] subs = arg.sub[subi].ToLower().Split(':');
double number;

if (subs[0] == "all")
subs[0] = "";

int min = 1;
int max = -1;
if (subs.Length > 1) {
if (Double.TryParse(subs[1], out number)) {
if (missing)
min = (int)Math.Ceiling(number);
else
max = (int)Math.Ceiling(number);
}
}

string subfulltype = subs[0];
if (mainType != "")
subfulltype += ' ' + mainType;
amounts.AddSpec(subfulltype, (arg.op == "-"), min, max);
}
}

if (!noexpand) {
amounts.ExpandSpecs();
}
amounts.ProcessItemsFromBC(blocks);

PrintItems(amounts, missing, nocats, "Ore", M.T["I3"]);

if (simple)
PrintItems(amounts, missing, nocats, "Ingot", M.T["I4"]);
else {
List<MMAmountSpec> ingots = amounts.GetAmountsOfMain("Ingot");
if (ingots.Count > 0) {
if (!nocats) {
if (MMLCDMgr.curLcd.curLine > 0 && MMLCDMgr.curLcd.lines[0] != "")
MMLCDMgr.AddLn("");  // add empty line
MMLCDMgr.AddCenterLn("<< " + M.T["I4"] + " " + M.T["I2"] + " >>", LCD_LINE_WIDTH / 2);
}

for (int i = 0; i < ingots.Count; i++) {
double num = ingots[i].current;

if (missing && num >= ingots[i].min)
continue;

int quota = ingots[i].max;
if (missing)
quota = ingots[i].min;

string msg = MM.ToDisplay(ingots[i].subType, ingots[i].mainType);
if (ingots[i].subType != "Scrap") {
double numOres = amounts.GetAmountSpec(ingots[i].subType + " Ore", ingots[i].subType, "Ore").current;
ShowInvIngotLine(msg, num, numOres, quota);
}
else
ShowInvLine(msg, num, quota);
}
}
}

PrintItems(amounts, missing, nocats, "Component", M.T["I5"]);
PrintItems(amounts, missing, nocats, "OxygenContainerObject", M.T["I6"]);
PrintItems(amounts, missing, true, "GasContainerObject", "");
PrintItems(amounts, missing, nocats, "AmmoMagazine", M.T["I7"]);
PrintItems(amounts, missing, nocats, "PhysicalGunObject", M.T["I8"]);
}
}

public class MMCmd {
public string cmdstr = "";
public string nameLike = "";
public string cmdLine;
public string cmdLineAfterName = "";

public List<MMArg> args = new List<MMArg>();

public MMCmd(string _cmdLine) {
cmdLine = _cmdLine.TrimStart(' ');
if (cmdLine == "")
return;

string[] targs = cmdLine.Split(' ');
string fullArg = "";
bool multiWord = false;

cmdstr = targs[0].ToLower();

for (int i = 1; i < targs.Length; i++) {
string arg = targs[i];
if (arg == "")
continue;

if (arg[0] == '{' && arg[arg.Length - 1] == '}') {
arg = arg.Substring(1, arg.Length - 2);
if (arg == "")
continue;
if (nameLike == "")
nameLike = arg;
else
args.Add(new MMArg(arg));
continue;
}
if (arg[0] == '{') {
multiWord = true;
fullArg = arg.Substring(1);
continue;
}
if (arg[arg.Length - 1] == '}') {
multiWord = false;
fullArg += ' ' + arg.Substring(0, arg.Length - 1);
if (nameLike == "")
nameLike = fullArg;
else
args.Add(new MMArg(fullArg));
continue;
}

if (multiWord) {
if (fullArg.Length != 0)
fullArg += ' ';
fullArg += arg;
continue;
}

if (nameLike == "")
nameLike = arg;
else
args.Add(new MMArg(arg));
}
}
}

public class MMArg {
public string op = "";
public string main = "";
public string arg = "";
public List<string> sub = new List<string>();

public MMArg(string _arg) {
arg = _arg;
}

public void Parse() {
string cur = arg.Trim();
if (cur[0] == '+' || cur[0] == '-') {
op += cur[0];
cur = arg.Substring(1);
}

string[] parts = cur.Split('/');
string subargs = parts[0];

if (parts.Length > 1) {
main = parts[0];
subargs = parts[1];
}
else
main = "";

if (subargs.Length > 0) {
string[] subs = subargs.Split(',');
for (int i = 0; i < subs.Length; i++)
if (subs[i] != "")
sub.Add(subs[i]);
}
}
}

public class MMBlkCol {
public List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

public double GetCargoSummary(ref double usedAmount, ref double totalAmount) {
for (int i = 0; i < Blocks.Count; i++) {
IMyInventory inv = Blocks[i].GetInventory(0);
if (inv == null)
continue;

usedAmount += (double)inv.CurrentVolume;
totalAmount += (double)inv.MaxVolume;
}
usedAmount *= 1000;
totalAmount *= 1000;
return MM.GetPercent(usedAmount, totalAmount);
}

public double GetMassSummary() {
double used = 0;
for (int i = 0; i < Blocks.Count; i++) {
for (int invId = 0; invId < 2; invId++) {
IMyInventory inv = Blocks[i].GetInventory(invId);
if (inv == null)
continue;

used += (double)inv.CurrentMass;
}
}
return used * 1000;
}

public double GetPowerOutput(ref double current, ref double max) {
for (int i = 0; i < Blocks.Count; i++) {
IMyBatteryBlock bat = (Blocks[i] as IMyBatteryBlock);
List<double> vals = MMStatus.GetDetailVals(Blocks[i]);
if ((bat != null && vals.Count < 6) ||
(bat == null && vals.Count < 2))
continue;

max += vals[0];

if (bat != null)
current += vals[4];
else
current += vals[1];
}
return MM.GetPercent(current, max);
}


// returns percent stored
public double GetBatStats(ref double output, ref double max_output,
ref double input, ref double max_input,
ref double stored, ref double max_stored) {
for (int i = 0; i < Blocks.Count; i++) {
List<double> vals = MMStatus.GetDetailVals(Blocks[i]);
if (vals.Count < 6)
continue;

max_output += vals[0];
max_input += vals[1];
max_stored += vals[2];
input += vals[3];
output += vals[4];
stored += vals[5];
}
return MM.GetPercent(stored, max_stored);
}

private void RemoveDiffGrid() {
int i = 0;
while (i < Blocks.Count) {
if (Blocks[i].CubeGrid != MM.Me.CubeGrid) {
Blocks.RemoveAt(i);
continue;
}
i++;
}
}

public void AddBOfName(string nameLike) {
int sep = nameLike.IndexOf(':');
string modstr = (sep >= 1 && sep <= 2 ? nameLike.Substring(0, sep) : "");
bool thisgrid = modstr.Contains("T");
if (modstr != "")
nameLike = nameLike.Substring(sep + 1);

if (nameLike == "" || nameLike == "*") {
List<IMyTerminalBlock> lBlocks = new List<IMyTerminalBlock>();
MM._GridTerminalSystem.GetBlocks(lBlocks);
Blocks.AddList(lBlocks);
if (thisgrid)
RemoveDiffGrid();
return;
}

string group = (modstr.Contains("G") ? nameLike.Trim().ToLower() : "");
if (group != "") {
List<IMyBlockGroup> BlockGroups = new List<IMyBlockGroup>();
MM._GridTerminalSystem.GetBlockGroups(BlockGroups);
for (int i = 0; i < BlockGroups.Count; i++) {
IMyBlockGroup g = BlockGroups[i];
if (g.Name.ToLower() == group) {
g.GetBlocks(Blocks);
if (thisgrid)
RemoveDiffGrid();
return;
}
}
return;
}
MM._GridTerminalSystem.SearchBlocksOfName(nameLike, Blocks);
if (thisgrid)
RemoveDiffGrid();
}

public void AddBofT(string type, string nameLike = "") {
int sep = nameLike.IndexOf(':');
string modstr = (sep >= 1 && sep <= 2 ? nameLike.Substring(0, sep) : "");
bool thisgrid = modstr.Contains("T");
if (modstr != "")
nameLike = nameLike.Substring(sep + 1);

List<IMyTerminalBlock> blocksOfType = new List<IMyTerminalBlock>();
if (nameLike == "" || nameLike == "*") {
MMBlkMgr.GetBlocksOfType(ref blocksOfType, type);
Blocks.AddList(blocksOfType);
if (thisgrid)
RemoveDiffGrid();
return;
}
string group = (modstr.Contains("G") ? nameLike.Trim().ToLower() : "");
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
if (group != "") {
List<IMyBlockGroup> BlockGroups = new List<IMyBlockGroup>();
MM._GridTerminalSystem.GetBlockGroups(BlockGroups);
for (int i = 0; i < BlockGroups.Count; i++) {
IMyBlockGroup g = BlockGroups[i];
if (g.Name.ToLower() == group) {
blocks.Clear();
g.GetBlocks(blocks);
for (int j = 0; j < blocks.Count; j++) {
if (thisgrid && blocks[j].CubeGrid != MM.Me.CubeGrid)
continue;
if (MMBlkMgr.IsBlockOfType(blocks[j], type))
Blocks.Add(blocks[j]);
}
return;
}
}
return;
}
MMBlkMgr.GetBlocksOfType(ref blocksOfType, type);
for (int i = 0; i < blocksOfType.Count; i++) {
if (thisgrid && blocksOfType[i].CubeGrid != MM.Me.CubeGrid)
continue;
if (blocksOfType[i].CustomName.Contains(nameLike))
Blocks.Add(blocksOfType[i]);
}
}

// add all Blocks from collection col to this collection
public void AddFromBC(MMBlkCol col) {
Blocks.AddList(col.Blocks);
}

// clear all reactors from this collection
public void Clear() {
Blocks.Clear();
}

// number of reactors in collection
public int Count() {
return Blocks.Count;
}
}

public class MMAmountSpec {
public int min;
public int max;
public string subType = "";
public string mainType = "";
public bool ignore;
public double current;

public MMAmountSpec(bool _ignore = false, int _min = 1, int _max = -1) {
min = _min;
ignore = _ignore;
max = _max;
}
}

// Item amounts class
public class MMItemAmounts {
private static Dictionary<string, string> mainTypeMap = new Dictionary<string, string>() {
{ "ingot", "ingot" },
{ "ore", "ore" },
{ "component", "component" },
{ "tool", "physicalgunobject" },
{ "ammo", "ammomagazine"},
{ "oxygen", "oxygencontainerobject" },
{ "gas", "gascontainerobject" }
};

public MMAmountSpecDict specBySubLower;
public MMAmountSpecDict specByMainLower;
public MMAmountSpecDict specByFullLower;
public bool specAll;

public MMAmountSpecDict amountByFullType;

public MMItemAmounts(int size = 20) {
specBySubLower = new MMAmountSpecDict();
specByMainLower = new MMAmountSpecDict();
specByFullLower = new MMAmountSpecDict();
specAll = false;
amountByFullType = new MMAmountSpecDict();
}

public void AddSpec(string subfulltype, bool ignore = false, int min = 1, int max = -1) {
if (subfulltype == "") {
specAll = true;
return;
}

string[] parts = subfulltype.Split(' ');

string mainType = "";
MMAmountSpec spec = new MMAmountSpec(ignore, min, max);

if (parts.Length == 2) {
if (!mainTypeMap.TryGetValue(parts[1], out mainType))
mainType = parts[1];
}

string subType = parts[0];

if (mainTypeMap.TryGetValue(subType, out spec.mainType)) {
specByMainLower.AddItem(spec.mainType, spec);
return;
}

MM.ToInternal(ref subType, ref mainType);
if (mainType == "") {
spec.subType = subType.ToLower();
specBySubLower.AddItem(spec.subType, spec);
return;
}

spec.subType = subType;
spec.mainType = mainType;
specByFullLower.AddItem(subType.ToLower() + ' ' + mainType.ToLower(), spec);
}

public MMAmountSpec GetSpec(string fullType, string subType, string mainType) {
MMAmountSpec spec;

fullType = fullType.ToLower();
spec = specByFullLower.GetItem(fullType);
if (spec != null)
return spec;

subType = subType.ToLower();
spec = specBySubLower.GetItem(subType);
if (spec != null)
return spec;

mainType = mainType.ToLower();
spec = specByMainLower.GetItem(mainType);
if (spec != null)
return spec;

return null;
}

public bool IsIgnored(string fullType, string subType, string mainType) {
MMAmountSpec spec;
bool found = false;

spec = specByMainLower.GetItem(mainType.ToLower());
if (spec != null) {
if (spec.ignore)
return true;
found = true;
}
spec = specBySubLower.GetItem(subType.ToLower());
if (spec != null) {
if (spec.ignore)
return true;
found = true;
}
spec = specByFullLower.GetItem(fullType.ToLower());
if (spec != null) {
if (spec.ignore)
return true;
found = true;
}

return !(specAll || found);
}

public MMAmountSpec CreateAmountSpec(string fullType, string subType, string mainType) {
MMAmountSpec amount = new MMAmountSpec();

fullType = fullType.ToLower();
MMAmountSpec spec = GetSpec(fullType, subType.ToLower(), mainType.ToLower());
if (spec != null) {
amount.min = spec.min;
amount.max = spec.max;
}
amount.subType = subType;
amount.mainType = mainType;

amountByFullType.AddItem(fullType, amount);

return amount;
}

public MMAmountSpec GetAmountSpec(string fullType, string subType, string mainType) {
MMAmountSpec amount = amountByFullType.GetItem(fullType.ToLower());
if (amount == null)
amount = CreateAmountSpec(fullType, subType, mainType);
return amount;
}

public List<MMAmountSpec> GetAmountsOfMain(string mainType) {
List<MMAmountSpec> result = new List<MMAmountSpec>();

for (int i = 0; i < amountByFullType.CountAll(); i++) {
MMAmountSpec spec = amountByFullType.GetItemAt(i);
if (IsIgnored((spec.subType + ' ' + spec.mainType).ToLower(),
spec.subType, spec.mainType))
continue;
if (spec.mainType == mainType)
result.Add(spec);
}

return result;
}

public void ExpandSpecs() {
for (int i = 0; i < MMItems.keys.Count; i++) {
MMItem item = MMItems.items[MMItems.keys[i]];
if (!item.used)
continue;
string fullType = item.subType + ' ' + item.mainType;

if (IsIgnored(fullType, item.subType, item.mainType))
continue;

MMAmountSpec amount = GetAmountSpec(fullType, item.subType, item.mainType);
if (amount.max == -1)
amount.max = item.defQt;
}
}

public void ProcessItemsFromBC(MMBlkCol col) {
for (int i = 0; i < col.Count(); i++) {
for (int invId = 0; invId < col.Blocks[i].GetInventoryCount(); invId++) {
IMyInventory inv = col.Blocks[i].GetInventory(invId);

List<IMyInventoryItem> items = inv.GetItems();
for (int j = 0; j < items.Count; j++) {
IMyInventoryItem item = items[j];
string fullType = MM.GetItemFullType(item);
string fullTypeL = fullType.ToLower();
string subType, mainType;
MM.ParseFullType(fullTypeL, out subType, out mainType);

if (mainType == "ore") {
if (IsIgnored(subType.ToLower() + " ingot", subType, "Ingot") &&
IsIgnored(fullType, subType, mainType))
continue;
}
else {
if (IsIgnored(fullType, subType, mainType))
continue;
}

MM.ParseFullType(fullType, out subType, out mainType);
MMAmountSpec amount = GetAmountSpec(fullTypeL, subType, mainType);
amount.current += (double)item.Amount;
}
}
}
}
}

// MMAPI Helper functions
public static class MMBlkMgr {
public static IMyGridTerminalSystem _GridTerminalSystem;

private static Dictionary<string, Action<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>> BofT = null;

public static void InitBlocksOfExactType() {
if (BofT != null && _GridTerminalSystem.GetBlocksOfType<IMyCargoContainer> == BofT["CargoContainer"])
return;

BofT = new Dictionary<string, Action<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>>() {
{ "CargoContainer", _GridTerminalSystem.GetBlocksOfType<IMyCargoContainer> },
{ "TextPanel", _GridTerminalSystem.GetBlocksOfType<IMyTextPanel> },
{ "Assembler", _GridTerminalSystem.GetBlocksOfType<IMyAssembler> },
{ "Refinery", _GridTerminalSystem.GetBlocksOfType<IMyRefinery> },
{ "Reactor", _GridTerminalSystem.GetBlocksOfType<IMyReactor> },
{ "SolarPanel", _GridTerminalSystem.GetBlocksOfType<IMySolarPanel> },
{ "BatteryBlock", _GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock> },
{ "Beacon", _GridTerminalSystem.GetBlocksOfType<IMyBeacon> },
{ "RadioAntenna", _GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna> },
{ "AirVent", _GridTerminalSystem.GetBlocksOfType<IMyAirVent> },
{ "ConveyorSorter", _GridTerminalSystem.GetBlocksOfType<IMyConveyorSorter> },
{ "OxygenTank", _GridTerminalSystem.GetBlocksOfType<IMyOxygenTank> },
{ "OxygenGenerator", _GridTerminalSystem.GetBlocksOfType<IMyOxygenGenerator> },
{ "OxygenFarm", _GridTerminalSystem.GetBlocksOfType<IMyOxygenFarm> },
{ "LaserAntenna", _GridTerminalSystem.GetBlocksOfType<IMyLaserAntenna> },
{ "Thrust", _GridTerminalSystem.GetBlocksOfType<IMyThrust> },
{ "Gyro", _GridTerminalSystem.GetBlocksOfType<IMyGyro> },
{ "SensorBlock", _GridTerminalSystem.GetBlocksOfType<IMySensorBlock> },
{ "ShipConnector", _GridTerminalSystem.GetBlocksOfType<IMyShipConnector> },
{ "ReflectorLight", _GridTerminalSystem.GetBlocksOfType<IMyReflectorLight> },
{ "InteriorLight", _GridTerminalSystem.GetBlocksOfType<IMyInteriorLight> },
{ "LandingGear", _GridTerminalSystem.GetBlocksOfType<IMyLandingGear> },
{ "ProgrammableBlock", _GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock> },
{ "TimerBlock", _GridTerminalSystem.GetBlocksOfType<IMyTimerBlock> },
{ "MotorStator", _GridTerminalSystem.GetBlocksOfType<IMyMotorStator> },
{ "PistonBase", _GridTerminalSystem.GetBlocksOfType<IMyPistonBase> },
{ "Projector", _GridTerminalSystem.GetBlocksOfType<IMyProjector> },
{ "ShipMergeBlock", _GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock> },
{ "SoundBlock", _GridTerminalSystem.GetBlocksOfType<IMySoundBlock> },
{ "Collector", _GridTerminalSystem.GetBlocksOfType<IMyCollector> },
{ "JumpDrive", _GridTerminalSystem.GetBlocksOfType<IMyJumpDrive> },
{ "Door", _GridTerminalSystem.GetBlocksOfType<IMyDoor> },
{ "GravityGeneratorSphere", _GridTerminalSystem.GetBlocksOfType<IMyGravityGeneratorSphere> },
{ "GravityGenerator", _GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator> },
{ "ShipDrill", _GridTerminalSystem.GetBlocksOfType<IMyShipDrill> },
{ "ShipGrinder", _GridTerminalSystem.GetBlocksOfType<IMyShipGrinder> },
{ "ShipWelder", _GridTerminalSystem.GetBlocksOfType<IMyShipWelder> },
{ "LargeGatlingTurret", _GridTerminalSystem.GetBlocksOfType<IMyLargeGatlingTurret> },
{ "LargeInteriorTurret", _GridTerminalSystem.GetBlocksOfType<IMyLargeInteriorTurret> },
{ "LargeMissileTurret", _GridTerminalSystem.GetBlocksOfType<IMyLargeMissileTurret> },
{ "SmallGatlingGun", _GridTerminalSystem.GetBlocksOfType<IMySmallGatlingGun> },
{ "SmallMissileLauncherReload", _GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncherReload> },
{ "SmallMissileLauncher", _GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher> },
{ "VirtualMass", _GridTerminalSystem.GetBlocksOfType<IMyVirtualMass> },
{ "Warhead", _GridTerminalSystem.GetBlocksOfType<IMyWarhead> },
{ "FunctionalBlock", _GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock> },
{ "LightingBlock", _GridTerminalSystem.GetBlocksOfType<IMyLightingBlock> },
{ "ControlPanel", _GridTerminalSystem.GetBlocksOfType<IMyControlPanel> },
{ "Cockpit", _GridTerminalSystem.GetBlocksOfType<IMyCockpit> },
{ "MedicalRoom", _GridTerminalSystem.GetBlocksOfType<IMyMedicalRoom> },
{ "RemoteControl", _GridTerminalSystem.GetBlocksOfType<IMyRemoteControl> },
{ "ButtonPanel", _GridTerminalSystem.GetBlocksOfType<IMyButtonPanel> },
{ "CameraBlock", _GridTerminalSystem.GetBlocksOfType<IMyCameraBlock> },
{ "OreDetector", _GridTerminalSystem.GetBlocksOfType<IMyOreDetector> },
{ "ShipController", _GridTerminalSystem.GetBlocksOfType<IMyShipController> }
};
}

private static bool IsCryoChamber(IMyTerminalBlock block) {
return block.BlockDefinition.ToString().Contains("Cryo");
}

public static void GetBlocksOfExactType(ref List<IMyTerminalBlock> blocks, string exact) {
Action<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>> fn = null;
if (BofT.TryGetValue(exact, out fn))
fn(blocks, null);
else {
if (exact == "CryoChamber") {
_GridTerminalSystem.GetBlocksOfType<IMyCockpit>(blocks, IsCryoChamber);
return;
}
}
}

public static void GetBlocksOfType(ref List<IMyTerminalBlock> blocks, string typestr) {
GetBlocksOfExactType(ref blocks, ToExactBlockType(typestr.Trim()));
}

public static bool IsBlockOfType(IMyTerminalBlock block, string typestr) {
string et = ToExactBlockType(typestr);
switch (et) {
case "FunctionalBlock":
return true;
case "ShipController":
return (block as IMyShipController != null);
default:
return block.BlockDefinition.ToString().Contains(ToExactBlockType(typestr));
}
}

public static string ToExactBlockType(string typeInStr) {
typeInStr = typeInStr.ToLower();

if (typeInStr.StartsWith("carg")
|| typeInStr.StartsWith("conta")) return "CargoContainer";
if (typeInStr.StartsWith("text")
|| typeInStr.StartsWith("lcd")) return "TextPanel";
if (typeInStr.StartsWith("ass")) return "Assembler";
if (typeInStr.StartsWith("refi")) return "Refinery";
if (typeInStr.StartsWith("reac")) return "Reactor";
if (typeInStr.StartsWith("solar")) return "SolarPanel";
if (typeInStr.StartsWith("bat")) return "BatteryBlock";
if (typeInStr.StartsWith("bea")) return "Beacon";
if (typeInStr.Contains("vent")) return "AirVent";
if (typeInStr.Contains("sorter")) return "ConveyorSorter";
if (typeInStr.Contains("tank")) return "OxygenTank";
if (typeInStr.Contains("farm")
&& typeInStr.Contains("oxy")) return "OxygenFarm";
if (typeInStr.Contains("gene")
&& typeInStr.Contains("oxy")) return "OxygenGenerator";
if (typeInStr.Contains("cryo")) return "CryoChamber";
if (typeInStr == "laserantenna") return "LaserAntenna";
if (typeInStr.Contains("antenna")) return "RadioAntenna";
if (typeInStr.StartsWith("thrust")) return "Thrust";
if (typeInStr.StartsWith("gyro")) return "Gyro";
if (typeInStr.StartsWith("sensor")) return "SensorBlock";
if (typeInStr.Contains("connector")) return "ShipConnector";
if (typeInStr.StartsWith("reflector")) return "ReflectorLight";
if ((typeInStr.StartsWith("inter")
&& typeInStr.EndsWith("light"))) return "InteriorLight";
if (typeInStr.StartsWith("land")) return "LandingGear";
if (typeInStr.StartsWith("program")) return "ProgrammableBlock";
if (typeInStr.StartsWith("timer")) return "TimerBlock";
if (typeInStr.StartsWith("motor")) return "MotorStator";
if (typeInStr.StartsWith("piston")) return "PistonBase";
if (typeInStr.StartsWith("proj")) return "Projector";
if (typeInStr.Contains("merge")) return "ShipMergeBlock";
if (typeInStr.StartsWith("sound")) return "SoundBlock";
if (typeInStr.StartsWith("col")) return "Collector";
if (typeInStr.Contains("jump")) return "JumpDrive";
if (typeInStr == "door") return "Door";
if ((typeInStr.Contains("grav")
&& typeInStr.Contains("sphe"))) return "GravityGeneratorSphere";
if (typeInStr.Contains("grav")) return "GravityGenerator";
if (typeInStr.EndsWith("drill")) return "ShipDrill";
if (typeInStr.Contains("grind")) return "ShipGrinder";
if (typeInStr.EndsWith("welder")) return "ShipWelder";
if ((typeInStr.Contains("turret")
&& typeInStr.Contains("gatl"))) return "LargeGatlingTurret";
if ((typeInStr.Contains("turret")
&& typeInStr.Contains("inter"))) return "LargeInteriorTurret";
if ((typeInStr.Contains("turret")
&& typeInStr.Contains("miss"))) return "LargeMissileTurret";
if (typeInStr.Contains("gatl")) return "SmallGatlingGun";
if ((typeInStr.Contains("launcher")
&& typeInStr.Contains("reload"))) return "SmallMissileLauncherReload";
if ((typeInStr.Contains("launcher"))) return "SmallMissileLauncher";
if (typeInStr.Contains("mass")) return "VirtualMass";
if (typeInStr == "warhead") return "Warhead";
if (typeInStr.StartsWith("func")) return "FunctionalBlock";
if (typeInStr == "shipctrl") return "ShipController";
if (typeInStr.StartsWith("light")) return "LightingBlock";
if (typeInStr.StartsWith("contr")) return "ControlPanel";
if (typeInStr.StartsWith("coc")) return "Cockpit";
if (typeInStr.StartsWith("medi")) return "MedicalRoom";
if (typeInStr.StartsWith("remote")) return "RemoteControl";
if (typeInStr.StartsWith("but")) return "ButtonPanel";
if (typeInStr.StartsWith("cam")) return "CameraBlock";
if (typeInStr.Contains("detect")) return "OreDetector";
return "Unknown";
}
}

public static class MMStatus {
public static List<double> GetDetailVals(IMyTerminalBlock block, int lines = -1) {
List<double> result = new List<double>();

string[] attrLines = block.DetailedInfo.Split('\n');
int max = Math.Min(attrLines.Length, (lines > 0 ? lines : attrLines.Length));

for (int i = 0; i < max; i++) {
string[] parts = attrLines[i].Split(':');
// broken line? (try German)
if (parts.Length < 2) {
parts = attrLines[i].Split('r');
// still broken line? (try Portuguese)
if (parts.Length < 2)
parts = attrLines[i].Split('x');
}

string valStr = (parts.Length < 2 ? parts[0] : parts[1]);
string[] valParts = valStr.Trim().Split(' ');
string strVal = valParts[0].Trim();
char strUnit = (valParts.Length > 1 && valParts[1].Length > 1 ? valParts[1][0] : '.');

double val;
if (Double.TryParse(strVal, out val)) {
double finalVal = val * Math.Pow(1000.0, ".kMGTPEZY".IndexOf(strUnit));
result.Add(finalVal);
}
}
return result;
}

public static string GetBatWorkingStr(IMyBatteryBlock battery) {
string prefix = "";
if (battery.GetValueBool("Recharge"))
prefix = "(+) ";
else
if (battery.GetValueBool("Discharge"))
prefix = "(-) ";
else
prefix = "(±) ";
return prefix + ((battery.CurrentStoredPower / battery.MaxStoredPower) * 100.0f).ToString("0.0") + "%";
}

// Get laser antenna status
public static string GetLAStatus(IMyLaserAntenna gear) {
string[] infoLines = gear.DetailedInfo.Split('\n');
return infoLines[infoLines.Length - 1].Split(' ')[0].ToUpper();
}

public static double GetJDChargeVals(IMyJumpDrive jd, out double cur, out double max) {
List<double> vals = GetDetailVals(jd, 5);
if (vals.Count < 4) {
max = 0;
cur = 0;
return 0;
}

max = vals[1];
cur = vals[3];

return MM.GetPercent(cur, max);
}

public static double GetJDCharge(IMyJumpDrive jd) {
List<double> vals = GetDetailVals(jd, 5);
double max = 0, cur = 0;
if (vals.Count < 4)
return 0;

max = vals[1];
cur = vals[3];

return MM.GetPercent(cur, max);
}

// Get landing gear status
public static string GetLGStatus(IMyLandingGear gear) {
string unlockchars = "udoesnp";

string[] infoLines = gear.DetailedInfo.Split('\n');
string attrLine = infoLines[infoLines.Length - 1];

string[] attr = attrLine.Split(':');

string state;
if (attr.Length < 2) {
state = attr[0].Trim().ToLower();
// Nederlands language (broken)
if (state.StartsWith("slot status"))
state = state.Substring(11);
}
else
state = attr[1].Trim().ToLower();

if (state == "")
return M.T["W8"];

// hope it will be more words in other langs too
if (state.Split(' ').Length > 1)
return M.T["W10"];

if ((unlockchars.IndexOf(state[0]) < 0) && !state.StartsWith("au"))
return M.T["W7"];

return M.T["W8"];
}
}

public static class MM {
public static bool EnableDebug;
public static IMyGridTerminalSystem _GridTerminalSystem;
public static MMBlkCol _DebugTextPanels;
public static IMyProgrammableBlock Me;
public static Action<string> Echo;
public static double ElapsedTime = 0;

public static bool Init(IMyGridTerminalSystem gridSystem) {
_GridTerminalSystem = gridSystem;
MMBlkMgr._GridTerminalSystem = gridSystem;
if (MM.ElapsedTime < 0.2f) return false;
_DebugTextPanels = new MMBlkCol();
MMBlkMgr.InitBlocksOfExactType();
MMStrFn.InitCharSizes();
if (MM.EnableDebug) {
_DebugTextPanels.AddBofT("textpanel", "[DEBUG]");
Debug("DEBUG Panel started.", false, "DEBUG PANEL");
}

MMItems.Init();
return true;
}

public static double GetPercent(double current, double max) {
return (max > 0 ? (current / max) * 100 : 100);
}

// return full type of item
public static string GetItemFullType(IMyInventoryItem item) {
string typeid = item.Content.TypeId.ToString();
typeid = typeid.Substring(typeid.LastIndexOf('_') + 1);
return item.Content.SubtypeName + " " + typeid;
}

// parse full type into subType and mainType
public static void ParseFullType(string fullType, out string subType, out string mainType) {
int idx = fullType.LastIndexOf(' ');

if (idx >= 0) {
subType = fullType.Substring(0, idx);
mainType = fullType.Substring(idx + 1);
return;
}
subType = fullType;
mainType = "";
}

public static string ToDisplay(string fullType) {
string subType, mainType;
ParseFullType(fullType, out subType, out mainType);

return ToDisplay(subType, mainType);
}

public static string ToDisplay(string subType, string mainType) {
MMItem item = MMItems.GetItemOfType(subType, mainType);
if (item != null) {
if (item.dName != "")
return item.dName;

return item.subType;
}

return System.Text.RegularExpressions.Regex.Replace(
subType, "([a-z])([A-Z])", "$1 $2");
}

public static void ToInternal(ref string subType, ref string mainType) {
string shortName = subType.ToLower();
MMItem item;

if (MMItems.itemsByShort.TryGetValue(shortName, out item)) {
subType = item.subType;
mainType = item.mainType;
return;
}

item = MMItems.GetItemOfType(subType, mainType);
if (item != null) {
subType = item.subType;
if (mainType == "Ore" || mainType == "Ingot")
return;
mainType = item.mainType;
}
}

public static string FormatLargeNumber(double number, bool compress = true) {
if (!compress)
return number.ToString(
"#,###,###,###,###,###,###,###,###,###");

string ordinals = " kMGTPEZY";
double compressed = number;

var ordinal = 0;

while (compressed >= 1000) {
compressed /= 1000;
ordinal++;
}

string res = Math.Round(compressed, 1, MidpointRounding.AwayFromZero).ToString();

if (ordinal > 0)
res += " " + ordinals[ordinal];

return res;
}

public static void WriteLine(IMyTextPanel textpanel, string message, bool append = true, string title = "") {
textpanel.WritePublicText(message + "\n", append);
if (title != "")
textpanel.WritePublicTitle(title);
textpanel.ShowTextureOnScreen();
textpanel.ShowPublicTextOnScreen();
}

public static void Debug(string message, bool append = true, string title = "") {
if (EnableDebug && _DebugTextPanels != null && _DebugTextPanels.Count() != 0)
DebugTextPanel(message, append, title);
}

public static void DebugTextPanel(string message, bool append = true, string title = "") {
for (int i = 0; i < _DebugTextPanels.Count(); i++) {
IMyTextPanel debugpanel = _DebugTextPanels.Blocks[i] as IMyTextPanel;
debugpanel.SetCustomName("[DEBUG] Prog: " + message);
WriteLine(debugpanel, message, append, title);
}
}
}

public class MMPanel {
// approximate width of LCD panel line
public const float LCD_LINE_WIDTH = 730;

public MMTextPanelDict panels = new MMTextPanelDict();
public MMLCDMgr.MMLCDText text;
public IMyTextPanel first;

public void SetFontSize(float size) {
for (int i = 0; i < panels.CountAll(); i++)
panels.GetItemAt(i).SetValueFloat("FontSize", size);
}

public void SortPanels() {
panels.SortAll();
first = panels.GetItemAt(0);
}

public bool IsWide() {
return (first.BlockDefinition.SubtypeId.Contains("Wide")
|| first.DefinitionDisplayNameText == "Computer Monitor");
}

public void Update() {
if (text == null || first == null)
return;

float size = first.GetValueFloat("FontSize");

for (int i = 0; i < panels.CountAll(); i++) {
IMyTextPanel panel = panels.GetItemAt(i);
if (i > 0)
panel.SetValueFloat("FontSize", size);
panel.WritePublicText(text.GetDisplayString(i));
if (MMLCDMgr.forceRedraw) {
panel.ShowTextureOnScreen();
}
panel.ShowPublicTextOnScreen();
}
}
}

public static class MMLCDMgr {
private static Dictionary<IMyTextPanel, MMLCDText> texts = new Dictionary<IMyTextPanel, MMLCDText>();
public static bool forceRedraw = true;
public static int SCROLL_LINES = 5;
public static MMLCDText curLcd = null;
public static MMPanel curP = null;

public static void SetupLCDText(MMPanel p) {
MMLCDText lcdText = GetLCDText(p);
lcdText.SetFontSize(p.first.GetValueFloat("FontSize"));
lcdText.SetNrScreens(p.panels.CountAll());
lcdText.widthMod = (p.IsWide() ? 2.0f : 1.0f) * (0.8f / lcdText.fontSize);
curLcd = lcdText;
curP = p;
}

public static MMLCDText GetLCDText(MMPanel p) {
MMLCDText lcdText;
IMyTextPanel panel = p.first;

if (!texts.TryGetValue(panel, out lcdText)) {
lcdText = new MMLCDText();
p.text = lcdText;
texts.Add(panel, lcdText);
}
else
p.text = lcdText;
return lcdText;
}

public static void AddLn(string line) {
curLcd.AddLine(line);
}

public static void AddMultiLn(string multiline) {
curLcd.AddMultiLine(multiline);
}

public static void Add(string text) {
curLcd.AddFast(text);
curLcd.curWidth += MMStrFn.GetSize(text);
}

public static void AddRightLn(string text, float endScreenX) {
AddRight(text, endScreenX);
AddLn("");
}

public static void AddRight(string text, float endScreenX) {
float textWidth = MMStrFn.GetSize(text);
endScreenX = endScreenX * curLcd.widthMod - curLcd.curWidth;

if (endScreenX < textWidth) {
curLcd.AddFast(text);
curLcd.curWidth += textWidth;
return;
}

endScreenX -= textWidth;
int fillchars = (int)Math.Round(endScreenX / MMStrFn.WHITESPACE_WIDTH, MidpointRounding.AwayFromZero);
float fillWidth = fillchars * MMStrFn.WHITESPACE_WIDTH;

curLcd.AddFast(new String(' ', fillchars) + text);
curLcd.curWidth += fillWidth + textWidth;
}

public static void AddCenterLn(string text, float screenX) {
AddCenter(text, screenX);
AddLn("");
}

public static void AddCenter(string text, float screenX) {
float textWidth = MMStrFn.GetSize(text);
screenX = screenX * curLcd.widthMod - curLcd.curWidth;

if (screenX < textWidth / 2) {
curLcd.AddFast(text);
curLcd.curWidth += textWidth;
return;
}

screenX -= textWidth / 2;
int fillchars = (int)Math.Round(screenX / MMStrFn.WHITESPACE_WIDTH, MidpointRounding.AwayFromZero);
float fillWidth = fillchars * MMStrFn.WHITESPACE_WIDTH;

curLcd.AddFast(new String(' ', fillchars) + text);
curLcd.curWidth += fillWidth + textWidth;
}

public static void AddBarLn(double percent, int width = 22, float leaveSpaceX = 0f) {
if (Double.IsNaN(percent)) percent = 0;
int totalBars = (int)(width * curLcd.widthMod) - 2 - (int)(leaveSpaceX / MMStrFn.PROGRESSCHAR_WIDTH);
int fill = Math.Min((int)(percent * totalBars) / 100, totalBars);
curLcd.AddLine("[" + new String('|', fill) + new String('\'', totalBars - fill) + "]");
}

public static void AddBar(double percent, int width = 22, float leaveSpaceX = 0f) {
if (Double.IsNaN(percent)) percent = 0;
int totalBars = (int)(width * curLcd.widthMod) - 2 - (int)(leaveSpaceX / MMStrFn.PROGRESSCHAR_WIDTH);
int fill = Math.Min((int)(percent * totalBars) / 100, totalBars);
curLcd.AddFast("[" + new String('|', fill) + new String('\'', totalBars - fill) + "]");
curLcd.curWidth += MMStrFn.PROGRESSCHAR_WIDTH * totalBars + 18f;
}

public static void ClearText() {
curLcd.ClearText();
}

public static void UpdatePanel(MMPanel panel) {
panel.Update();
curLcd.Scroll();
}

public class MMLCDText {
public float fontSize = 0.8f;
public float widthMod = 1.0f;
public int scrollPos;
public int scrollDir = 1;
public int DisplayLines = 22; // 22 for font size 0.8
public int screens = 1;

public List<string> lines = new List<string>();
public int curLine;
public float curWidth;

public MMLCDText(float fontSize = 0.8f) {
SetFontSize(fontSize);
lines.Add("");
}

public void SetFontSize(float size) {
fontSize = size;
DisplayLines = (int)Math.Round(22 * (0.8 / fontSize) * screens);
}

public void SetNrScreens(int cnt) {
screens = cnt;
DisplayLines = (int)Math.Round(22 * (0.8 / fontSize) * screens);
}

public void AddFast(string text) {
lines[curLine] += text;
}

public void AddMultiLine(string multiline) {
string[] lines = multiline.Split('\n');

for (int i = 0; i < lines.Length; i++)
AddLine(lines[i]);
}

public void AddLine(string line) {
lines[curLine] += line;
lines.Add("");
curLine++;
curWidth = 0;
}

public void ClearText() {
lines.Clear();
lines.Add("");
curWidth = 0;
curLine = 0;
}

public string GetFullString() {
return String.Join("\n", lines);
}

// Display only X lines from scrollPos
public string GetDisplayString(int screenidx = 0) {
if (lines.Count < DisplayLines / screens) {
if (screenidx != 0) return "";
scrollPos = 0;
scrollDir = 1;
return GetFullString();
}

int pos = scrollPos + screenidx * (DisplayLines / screens);
if (pos > lines.Count)
pos = lines.Count;

List<string> display =
lines.GetRange(pos,
Math.Min(lines.Count - pos, DisplayLines / screens));

return String.Join("\n", display);
}

public void Scroll() {
int linesCnt = lines.Count - 1;
if (linesCnt <= DisplayLines) {
scrollPos = 0;
scrollDir = 1;
return;
}

if (scrollDir > 0) {
if (scrollPos + SCROLL_LINES + DisplayLines > linesCnt) {
scrollDir = -1;
scrollPos = Math.Max(linesCnt - DisplayLines, 0);
return;
}

scrollPos += SCROLL_LINES;
}
else {
if (scrollPos - SCROLL_LINES < 0) {
scrollPos = 0;
scrollDir = 1;
return;
}

scrollPos -= SCROLL_LINES;
}
}
}
}

public static class MMStrFn {
private static Dictionary<char, float> charSize = new Dictionary<char, float>();

public const float WHITESPACE_WIDTH = 8f;
public const float PROGRESSCHAR_WIDTH = 6.4f;

public static void InitCharSizes() {
if (charSize.Count > 0)
return;

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
}

private static void AddCharsSize(string chars, float size) {
for (int i = 0; i < chars.Length; i++)
charSize.Add(chars[i], size);
}

public static float GetCharSize(char c) {
float width = 40;
charSize.TryGetValue(c, out width);

return width;
}

public static float GetSize(string str) {
float sum = 0;
for (int i = 0; i < str.Length; i++)
sum += GetCharSize(str[i]);

return sum;
}

public static string GetTrimmed(string text, float pixelWidth) {
int trimlen = Math.Min((int)pixelWidth / 14, text.Length - 2);
float stringSize = GetSize(text);
if (stringSize <= pixelWidth)
return text;

while (stringSize > pixelWidth - 20) {
text = text.Substring(0, trimlen);
stringSize = GetSize(text);
trimlen -= 2;
}
return text + "..";
}
}


public class MMItem {
public string subType;
public string mainType;

public int defQt;
public string dName;
public string sName;

public bool used;

public MMItem(string _subType, string _mainType, int _defaultQuota = 0, string _displayName = "", string _shortName = "", bool _used = true) {
subType = _subType;
mainType = _mainType;
defQt = _defaultQuota;
dName = _displayName;
sName = _shortName;
used = _used;
}
}
public class MMPanelDict {
public Dictionary<string, MMPanel> dict = new Dictionary<string, MMPanel>();
public List<string> keys = new List<string>();

public void AddItem(string key, MMPanel item) { if (!dict.ContainsKey(key)) { keys.Add(key); dict.Add(key, item); } }
public int CountAll() { return dict.Count; }
public MMPanel GetItem(string key) { if (dict.ContainsKey(key)) return dict[key]; return null; }
public MMPanel GetItemAt(int index) { return dict[keys[index]]; }
public void ClearAll() { keys.Clear(); dict.Clear(); }
public void SortAll() { keys.Sort(); }
}
public class MMTextPanelDict {
public Dictionary<string, IMyTextPanel> dict = new Dictionary<string, IMyTextPanel>();
public List<string> keys = new List<string>();

public void AddItem(string key, IMyTextPanel item) { if (!dict.ContainsKey(key)) { keys.Add(key); dict.Add(key, item); } }
public int CountAll() { return dict.Count; }
public IMyTextPanel GetItem(string key) { if (dict.ContainsKey(key)) return dict[key]; return null; }
public IMyTextPanel GetItemAt(int index) { return dict[keys[index]]; }
public void ClearAll() { keys.Clear(); dict.Clear(); }
public void SortAll() { keys.Sort(); }
}
public class MMAmountSpecDict {
public Dictionary<string, MMAmountSpec> dict = new Dictionary<string, MMAmountSpec>();
public List<string> keys = new List<string>();

public void AddItem(string key, MMAmountSpec item) { if (!dict.ContainsKey(key)) { keys.Add(key); dict.Add(key, item); } }
public int CountAll() { return dict.Count; }
public MMAmountSpec GetItem(string key) { if (dict.ContainsKey(key)) return dict[key]; return null; }
public MMAmountSpec GetItemAt(int index) { return dict[keys[index]]; }
public void ClearAll() { keys.Clear(); dict.Clear(); }
public void SortAll() { keys.Sort(); }
}

public static class M {
public static readonly Dictionary<string, string> T = new Dictionary<string, string> {
// TRANSLATION STRINGS
// msg id, text
{ "AC1", "Acceleration:" },
{ "A1", "EMPTY" }, // amount
{ "B1", "Booting up..." },
{ "C1", "count:" },
{ "C2", "Cargo Used:" },
{ "C3", "Invalid countdown format, use:" },
{ "C4", "EXPIRED" },
{ "C5", "days" },
{ "D1", "You need to enter name." },
{ "D2", "No blocks found." },
{ "D3", "No damaged blocks found." },
{ "H1", "Write commands to Private Text." },
{ "H2", "Write commands to Public or Private Title." },
// inventory
{ "I1", "ore" },
{ "I2", "summary" },
{ "I3", "Ores" },
{ "I4", "Ingots" },
{ "I5", "Components" },
{ "I6", "Gas" },
{ "I7", "Ammo" },
{ "I8", "Tools" },
{ "M1", "Cargo Mass:" },
// oxygen
{ "O1", "Leaking" },
{ "O2", "Oxygen Farms" },
{ "O3", "No oxygen blocks found." },
{ "O4", "Oxygen Tanks" },
// position
{ "P1", "Block not found" },
{ "P2", "Location" },
// power
{ "P3", "Stored" },
{ "P4", "Output" },
{ "P5", "Input" },
{ "P6", "No power source found!" },
{ "P7", "Batteries" },
{ "P8", "Total Output" },
{ "P9", "Reactors" },
{ "P10", "Solars" },
{ "P11", "Power" },
{ "PT1", "Power Time:" },
{ "PT2", "Charge Time:" },
{ "S1", "Speed:" },
// text
{ "T1", "Source LCD not found: " },
{ "T2", "Missing source LCD name" },
{ "T3", "LCD Private Text is empty" },
// tanks
{ "T4", "Missing tank type. eg: 'Tanks * Hydrogen'" },
{ "T5", "No {0} tanks found." }, // {0} is tank type
{ "T6", "Tanks" },
{ "UC", "Unknown command" },
// occupied & dampeners
{ "SC1", "Cannot find control block." }, // NEW
{ "SCD", "Dampeners: " }, // NEW
{ "SCO", "Occupied: " }, // NEW
// working
{ "W1", "OFF" },
{ "W2", "WORK" },
{ "W3", "IDLE" },
{ "W4", "LEAK" },
{ "W5", "OPEN" },
{ "W6", "CLOSED" },
{ "W7", "LOCK" },
{ "W8", "UNLOCK" },
{ "W9", "ON" },
{ "W10", "READY" }
};