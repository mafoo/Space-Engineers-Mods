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
    public sealed class ConfigureableAutomaticLCDs2 : MyGridProgram
    {
#endif
/* v:2.0176 [1.194 compatibility]
* Automatic LCDs 2 - In-game script by MMaster
*
* Last Update: Fixed Corner LCDs displaying just one line
*  Added support for new items: Space Credit, Zone Chip, Datapad, Medkit & Powerkit
*  Added support for new blocks: Safe Zone Block, Store Block (includes ATM), Contract Block & Vending Machine
*    (afaik no extra info from the blocks is provided by the game so only the usual commands like BlockCount, Working, etc will work)
*  Fixed multiline command marker \ at the end of line not working
*  Slower command updates in SlowMode
*  Added NB (no bars), NN (no numbers) & NNB (no bars or numbers) variants of Inventory, InvList, Missing and all their variants (check guide)
*  Cockpit (and other blocks) panels support - read guide section 'How to use with cockpits?'
*  Script now correctly uses the Text Padding property of the LCD (you can set it however you want and the script will work with it)
*  Optimizations for servers running script limiter - use SlowMode!
*  Added SlowMode setting to considerably slow down the script (4-5 times less processing per second)
*  Now using MDK!
*  First part of rewrite of memory management which should optimize memory allocations a lot (more optimizations in progress)
* 
* Previous updates: Look at Change notes tab on Steam workshop page. */

/* Customize these: */

// Use this tag to identify LCDs managed by this script
// Name filtering rules can be used here so you can use even G:Group or T:[My LCD]
public string LCD_TAG = "T:[LCD]";

// Set to true if you want to slow down the script
public const bool SlowMode = false;

// How many lines to scroll per step
public const int SCROLL_LINES_PER_STEP = 1;

// Script automatically figures if LCD is using monospace font
// if you use custom font scroll down to the bottom, then scroll a bit up until you find AddCharsSize lines
// monospace font name and size definition is above those

// Enable initial boot sequence (after compile / world load)
public const bool ENABLE_BOOT = true;

/* READ THIS FULL GUIDE
http://steamcommunity.com/sharedfiles/filedetails/?id=407158161

Basic video guide
Please watch the video guide even if you don't understand my English. You can see how things are done there.

https://youtu.be/vqpPQ_20Xso


Please carefully read the FULL GUIDE before asking questions I had to remove guide from here to add more features :(
Please DO NOT publish this script or its derivations without my permission! Feel free to use it in blueprints!

Special Thanks
Keen Software House for awesome Space Engineers game
Malware for contributing to programmable blocks game code and MDK!

Watch Twitter: https://twitter.com/MattsPlayCorner
and Facebook: https://www.facebook.com/MattsPlayCorner1080p
for more crazy stuff from me in the future :)

If you want to make scripts for Space Engineers check out MDK by Malware:
https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
*/
bool MDK_IS_GREAT = true;
/* Customize characters used by script */
class MMStyle {
    // Monospace font characters (\uXXXX is special character code)
    public const char BAR_MONO_START = '[';
    public const char BAR_MONO_END = ']';
    public const char BAR_MONO_EMPTY = '\u2591'; // 25% rect
    public const char BAR_MONO_FILL = '\u2588'; // full rect

    // Classic (Debug) font characters
    // Start and end characters of progress bar need to be the same width!
    public const char BAR_START = '[';
    public const char BAR_END = ']';
    // Empty and fill characters of progress bar need to be the same width!
    public const char BAR_EMPTY = '\'';
    public const char BAR_FILL = '|';
}
// (for developer) Debug level to show
public const int DebugLevel = 0;

// (for modded lcds) Affects all LCDs managed by this programmable block
/* LCD height modifier
0.5f makes the LCD have only 1/2 the lines of normal LCD
2.0f makes it fit 2x more lines on LCD */
public const float HEIGHT_MOD = 1.0f;

/* line width modifier
0.5f moves the right edge to 50% of normal LCD width
2.0f makes it fit 200% more text on line */
public const float WIDTH_MOD = 1.0f;

List<string> BOOT_FRAMES = new List<string>() {
/* BOOT FRAMES
* Each @"<text>" marks single frame, add as many as you want each will be displayed for one second
* @"" is multiline string so you can write multiple lines */
@"
Initializing systems"
,
@"
Verifying connections"
,
@"
Loading commands"
};

void ItemsConf() {
    // ITEMS AND QUOTAS LIST
    // (subType, mainType, quota, display name, short name)
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
    Add("Scrap", "Ore");
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
    Add("Canvas", "Component", 300);
    Add("ZoneChip", "Component", 100, "Zone Chip");
    Add("Datapad", "Datapad", 0);
    Add("Medkit", "ConsumableItem", 0);
    Add("Powerkit", "ConsumableItem", 0);
    Add("SpaceCredit", "PhysicalObject", 0, "Space Credit");
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

    // REALLY REALLY REALLY
    // DO NOT MODIFY ANYTHING BELOW THIS (TRANSLATION STRINGS ARE AT THE BOTTOM)
}
void Add(string sT, string mT, int q = 0, string dN = "", string sN = "", bool u = true) { Ǝ.ĭ(sT, mT, q, dN, sN, u); }
Ƅ Ǝ;ȑ ƚ;Ġ ϔ;ɬ n=null;void ϓ(string Ƙ){}void ϒ(string ϑ,string ϐ){string Ƕ=ϑ.ToLower();switch(Ƕ){case"lcd_tag":LCD_TAG=ϐ;
break;}}void ϕ(){string[]ǈ=Me.CustomData.Split('\n');for(int E=0;E<ǈ.Length;E++){string ǎ=ǈ[E];int š=ǎ.IndexOf('=');if(š<0){ϓ
(ǎ);continue;}string Ϗ=ǎ.Substring(0,š).Trim();string Ǫ=ǎ.Substring(š+1).Trim();ϒ(Ϗ,Ǫ);}}void ώ(ȑ ƚ){Ǝ=new Ƅ();ItemsConf(
);ϕ();n=new ɬ(this,DebugLevel,ƚ);n.Ǝ=Ǝ;n.ɦ=LCD_TAG;n.ɥ=SCROLL_LINES_PER_STEP;n.ɤ=ENABLE_BOOT;n.ɣ=BOOT_FRAMES;n.ɢ=!
MDK_IS_GREAT;n.ɠ=HEIGHT_MOD;n.ɡ=WIDTH_MOD;n.ǟ();}void ύ(){ƚ.ǂ=this;n.ǂ=this;}Program(){Runtime.UpdateFrequency=UpdateFrequency.
Update1;}void Main(string Ą,UpdateType ϋ){try{if(ƚ==null){ƚ=new ȑ(this,DebugLevel,SlowMode);ώ(ƚ);ϔ=new Ġ(n);ƚ.Ȗ(ϔ,0);}else{ύ();
n.ő.Ѓ();}if(Ą.Length==0&&(ϋ&(UpdateType.Update1|UpdateType.Update10|UpdateType.Update100))==0){ƚ.Ȣ();return;}if(Ą!=""){if
(ϔ.Ć(Ą)){ƚ.Ȣ();return;}}ϔ.Ğ=0;ƚ.ȡ();}catch(Exception ex){Echo("ERROR DESCRIPTION:\n"+ex.ToString());Me.Enabled=false;}}
class η:ɑ{Ġ đ;ɬ n;string Ą="";public η(ɬ Ú,Ġ Đ,string Ĳ){Ɍ=-1;ɐ="ArgScroll";Ą=Ĳ;đ=Đ;n=Ú;}int ŭ;γ ζ;public override void Ƀ(){ζ
=new γ(ƒ,n.ő);}int ε=0;int ë=0;ʣ Ƙ;public override bool ɓ(bool õ){if(!õ){ë=0;ζ.ų();Ƙ=new ʣ(ƒ);ε=0;}if(ë==0){if(!Ƙ.ʚ(Ą,õ))
return false;if(Ƙ.ˇ.Count>0){if(!int.TryParse(Ƙ.ˇ[0].Ĳ,out ŭ))ŭ=1;else if(ŭ<1)ŭ=1;}if(Ƙ.ˏ.EndsWith("up"))ŭ=-ŭ;else if(!Ƙ.ˏ.
EndsWith("down"))ŭ=0;ë++;õ=false;}if(ë==1){if(!ζ.ϼ("textpanel",Ƙ.ˍ,õ))return false;ë++;õ=false;}ç k;for(;ε<ζ.ϲ();ε++){if(!ƒ.ȝ(20
))return false;IMyTextPanel δ=ζ.ΰ[ε]as IMyTextPanel;if(!đ.ć.TryGetValue(δ,out k))continue;if(k==null||k.ä!=δ)continue;if(
k.ß)k.å.ĺ=10;if(ŭ>0)k.å.Ľ(ŭ);else if(ŭ<0)k.å.Ŀ(-ŭ);else k.å.Ĺ();k.L();}return true;}}class γ{ȑ ƒ;ϵ β;IMyCubeGrid α{get{
return ƒ.ǂ.Me.CubeGrid;}}IMyGridTerminalSystem ƾ{get{return ƒ.ǂ.GridTerminalSystem;}}public List<IMyTerminalBlock>ΰ=new List<
IMyTerminalBlock>();public γ(ȑ ƚ,ϵ ί){ƒ=ƚ;β=ί;}int θ=0;public double ϊ(ref double ψ,ref double χ,bool õ){if(!õ)θ=0;for(;θ<ΰ.Count;θ++){
if(!ƒ.ȝ(4))return Double.NaN;IMyInventory ς=ΰ[θ].GetInventory(0);if(ς==null)continue;ψ+=(double)ς.CurrentVolume;χ+=(double
)ς.MaxVolume;}ψ*=1000;χ*=1000;return(χ>0?ψ/χ*100:100);}int φ=0;double υ=0;public double τ(bool õ){if(!õ){φ=0;υ=0;}for(;φ<
ΰ.Count;φ++){if(!ƒ.ȝ(6))return Double.NaN;for(int σ=0;σ<2;σ++){IMyInventory ς=ΰ[φ].GetInventory(σ);if(ς==null)continue;υ
+=(double)ς.CurrentMass;}}return υ*1000;}int ω=0;private bool ρ(bool õ=false){if(!õ)ω=0;while(ω<ΰ.Count){if(!ƒ.ȝ(4))return
false;if(ΰ[ω].CubeGrid!=α){ΰ.RemoveAt(ω);continue;}ω++;}return true;}List<IMyBlockGroup>π=new List<IMyBlockGroup>();List<
IMyTerminalBlock>ο=new List<IMyTerminalBlock>();int ξ=0;public bool ν(string ˍ,bool õ){int μ=ˍ.IndexOf(':');string λ=(μ>=1&&μ<=2?ˍ.
Substring(0,μ):"");bool κ=λ.Contains("T");if(λ!="")ˍ=ˍ.Substring(μ+1);if(ˍ==""||ˍ=="*"){if(!õ){ο.Clear();ƾ.GetBlocks(ο);ΰ.AddList
(ο);}if(κ)if(!ρ(õ))return false;return true;}string ό=(λ.Contains("G")?ˍ.Trim():"");if(ό!=""){if(!õ){π.Clear();ƾ.
GetBlockGroups(π);ξ=0;}for(;ξ<π.Count;ξ++){IMyBlockGroup ϖ=π[ξ];if(string.Compare(ϖ.Name,ό,true)==0){if(!õ){ο.Clear();ϖ.GetBlocks(ο);ΰ
.AddList(ο);}if(κ)if(!ρ(õ))return false;return true;}}return true;}if(!õ){ο.Clear();ƾ.SearchBlocksOfName(ˍ,ο);ΰ.AddList(ο
);}if(κ)if(!ρ(õ))return false;return true;}List<IMyBlockGroup>ϱ=new List<IMyBlockGroup>();List<IMyTerminalBlock>Ͻ=new
List<IMyTerminalBlock>();int ϻ=0;int Ϻ=0;public bool Ϲ(string ʐ,string ό,bool κ,bool õ){if(!õ){ϱ.Clear();ƾ.GetBlockGroups(ϱ)
;ϻ=0;}for(;ϻ<ϱ.Count;ϻ++){IMyBlockGroup ϖ=ϱ[ϻ];if(string.Compare(ϖ.Name,ό,true)==0){if(!õ){Ϻ=0;Ͻ.Clear();ϖ.GetBlocks(Ͻ);}
else õ=false;for(;Ϻ<Ͻ.Count;Ϻ++){if(!ƒ.ȝ(6))return false;if(κ&&Ͻ[Ϻ].CubeGrid!=α)continue;if(β.Ͼ(Ͻ[Ϻ],ʐ))ΰ.Add(Ͻ[Ϻ]);}return
true;}}return true;}List<IMyTerminalBlock>ϸ=new List<IMyTerminalBlock>();int Ϸ=0;public bool ϼ(string ʐ,string ˍ,bool õ){int
μ=ˍ.IndexOf(':');string λ=(μ>=1&&μ<=2?ˍ.Substring(0,μ):"");bool κ=λ.Contains("T");if(λ!="")ˍ=ˍ.Substring(μ+1);if(!õ){ϸ.
Clear();Ϸ=0;}string ό=(λ.Contains("G")?ˍ.Trim():"");if(ό!=""){if(!Ϲ(ʐ,ό,κ,õ))return false;return true;}if(!õ)β.Ͽ(ref ϸ,ʐ);if(
ˍ==""||ˍ=="*"){if(!õ)ΰ.AddList(ϸ);if(κ)if(!ρ(õ))return false;return true;}for(;Ϸ<ϸ.Count;Ϸ++){if(!ƒ.ȝ(4))return false;if(
κ&&ϸ[Ϸ].CubeGrid!=α)continue;if(ϸ[Ϸ].CustomName.Contains(ˍ))ΰ.Add(ϸ[Ϸ]);}return true;}public void ϴ(γ ϳ){ΰ.AddList(ϳ.ΰ);}
public void ų(){ΰ.Clear();}public int ϲ(){return ΰ.Count;}}class ϵ{ȑ ƒ;ɬ n;public MyGridProgram ǂ{get{return ƒ.ǂ;}}public
IMyGridTerminalSystem ƾ{get{return ƒ.ǂ.GridTerminalSystem;}}public ϵ(ȑ ƚ,ɬ Ú){ƒ=ƚ;n=Ú;}void Ї<Ǡ>(List<IMyTerminalBlock>І,Func<
IMyTerminalBlock,bool>Ѕ=null)where Ǡ:class,IMyTerminalBlock{ƾ.GetBlocksOfType<Ǡ>(І,Ѕ);}public Dictionary<string,Action<List<
IMyTerminalBlock>,Func<IMyTerminalBlock,bool>>>Є;public void Ѓ(){if(Є!=null)return;Є=new Dictionary<string,Action<List<IMyTerminalBlock>
,Func<IMyTerminalBlock,bool>>>(){{"CargoContainer",Ї<IMyCargoContainer>},{"TextPanel",Ї<IMyTextPanel>},{"Assembler",Ї<
IMyAssembler>},{"Refinery",Ї<IMyRefinery>},{"Reactor",Ї<IMyReactor>},{"SolarPanel",Ї<IMySolarPanel>},{"BatteryBlock",Ї<
IMyBatteryBlock>},{"Beacon",Ї<IMyBeacon>},{"RadioAntenna",Ї<IMyRadioAntenna>},{"AirVent",Ї<IMyAirVent>},{"ConveyorSorter",Ї<
IMyConveyorSorter>},{"OxygenTank",Ї<IMyGasTank>},{"OxygenGenerator",Ї<IMyGasGenerator>},{"OxygenFarm",Ї<IMyOxygenFarm>},{"LaserAntenna",Ї
<IMyLaserAntenna>},{"Thrust",Ї<IMyThrust>},{"Gyro",Ї<IMyGyro>},{"SensorBlock",Ї<IMySensorBlock>},{"ShipConnector",Ї<
IMyShipConnector>},{"ReflectorLight",Ї<IMyReflectorLight>},{"InteriorLight",Ї<IMyInteriorLight>},{"LandingGear",Ї<IMyLandingGear>},{
"ProgrammableBlock",Ї<IMyProgrammableBlock>},{"TimerBlock",Ї<IMyTimerBlock>},{"MotorStator",Ї<IMyMotorStator>},{"PistonBase",Ї<
IMyPistonBase>},{"Projector",Ї<IMyProjector>},{"ShipMergeBlock",Ї<IMyShipMergeBlock>},{"SoundBlock",Ї<IMySoundBlock>},{"Collector",Ї<
IMyCollector>},{"JumpDrive",Ї<IMyJumpDrive>},{"Door",Ї<IMyDoor>},{"GravityGeneratorSphere",Ї<IMyGravityGeneratorSphere>},{
"GravityGenerator",Ї<IMyGravityGenerator>},{"ShipDrill",Ї<IMyShipDrill>},{"ShipGrinder",Ї<IMyShipGrinder>},{"ShipWelder",Ї<IMyShipWelder>}
,{"Parachute",Ї<IMyParachute>},{"LargeGatlingTurret",Ї<IMyLargeGatlingTurret>},{"LargeInteriorTurret",Ї<
IMyLargeInteriorTurret>},{"LargeMissileTurret",Ї<IMyLargeMissileTurret>},{"SmallGatlingGun",Ї<IMySmallGatlingGun>},{
"SmallMissileLauncherReload",Ї<IMySmallMissileLauncherReload>},{"SmallMissileLauncher",Ї<IMySmallMissileLauncher>},{"VirtualMass",Ї<IMyVirtualMass>}
,{"Warhead",Ї<IMyWarhead>},{"FunctionalBlock",Ї<IMyFunctionalBlock>},{"LightingBlock",Ї<IMyLightingBlock>},{
"ControlPanel",Ї<IMyControlPanel>},{"Cockpit",Ї<IMyCockpit>},{"CryoChamber",Ї<IMyCryoChamber>},{"MedicalRoom",Ї<IMyMedicalRoom>},{
"RemoteControl",Ї<IMyRemoteControl>},{"ButtonPanel",Ї<IMyButtonPanel>},{"CameraBlock",Ї<IMyCameraBlock>},{"OreDetector",Ї<
IMyOreDetector>},{"ShipController",Ї<IMyShipController>},{"SafeZoneBlock",Ї<IMySafeZoneBlock>},{"Decoy",Ї<IMyDecoy>}};}public void Ј(
ref List<IMyTerminalBlock>Ā,string Ђ){Action<List<IMyTerminalBlock>,Func<IMyTerminalBlock,bool>>Ё;if(Ђ=="SurfaceProvider"){
ƾ.GetBlocksOfType<IMyTextSurfaceProvider>(Ā);return;}if(Є.TryGetValue(Ђ,out Ё))Ё(Ā,null);else{if(Ђ=="WindTurbine"){ƾ.
GetBlocksOfType<IMyPowerProducer>(Ā,(Ѐ)=>Ѐ.BlockDefinition.TypeIdString.EndsWith("WindTurbine"));return;}if(Ђ=="HydrogenEngine"){ƾ.
GetBlocksOfType<IMyPowerProducer>(Ā,(Ѐ)=>Ѐ.BlockDefinition.TypeIdString.EndsWith("HydrogenEngine"));return;}if(Ђ=="StoreBlock"){ƾ.
GetBlocksOfType<IMyFunctionalBlock>(Ā,(Ѐ)=>Ѐ.BlockDefinition.TypeIdString.EndsWith("StoreBlock"));return;}if(Ђ=="ContractBlock"){ƾ.
GetBlocksOfType<IMyFunctionalBlock>(Ā,(Ѐ)=>Ѐ.BlockDefinition.TypeIdString.EndsWith("ContractBlock"));return;}if(Ђ=="VendingMachine"){ƾ.
GetBlocksOfType<IMyFunctionalBlock>(Ā,(Ѐ)=>Ѐ.BlockDefinition.TypeIdString.EndsWith("VendingMachine"));return;}}}public void Ͽ(ref List<
IMyTerminalBlock>Ā,string ϡ){Ј(ref Ā,Ϡ(ϡ.Trim()));}public bool Ͼ(IMyTerminalBlock ä,string ϡ){string ϫ=Ϡ(ϡ);switch(ϫ){case
"FunctionalBlock":return true;case"ShipController":return(ä as IMyShipController!=null);default:return ä.BlockDefinition.TypeIdString.
Contains(Ϡ(ϡ));}}public string Ϡ(string ϟ){if(ϟ=="surfaceprovider")return"SurfaceProvider";if(ϟ.Ǭ("carg")||ϟ.Ǭ("conta"))return
"CargoContainer";if(ϟ.Ǭ("text")||ϟ.Ǭ("lcd"))return"TextPanel";if(ϟ.Ǭ("ass"))return"Assembler";if(ϟ.Ǭ("refi"))return"Refinery";if(ϟ.Ǭ(
"reac"))return"Reactor";if(ϟ.Ǭ("solar"))return"SolarPanel";if(ϟ.Ǭ("wind"))return"WindTurbine";if(ϟ.Ǭ("hydro")&&ϟ.Contains(
"eng"))return"HydrogenEngine";if(ϟ.Ǭ("bat"))return"BatteryBlock";if(ϟ.Ǭ("bea"))return"Beacon";if(ϟ.ǫ("vent"))return"AirVent";
if(ϟ.ǫ("sorter"))return"ConveyorSorter";if(ϟ.ǫ("tank"))return"OxygenTank";if(ϟ.ǫ("farm")&&ϟ.ǫ("oxy"))return"OxygenFarm";if
(ϟ.ǫ("gene")&&ϟ.ǫ("oxy"))return"OxygenGenerator";if(ϟ.ǫ("cryo"))return"CryoChamber";if(string.Compare(ϟ,"laserantenna",
true)==0)return"LaserAntenna";if(ϟ.ǫ("antenna"))return"RadioAntenna";if(ϟ.Ǭ("thrust"))return"Thrust";if(ϟ.Ǭ("gyro"))return
"Gyro";if(ϟ.Ǭ("sensor"))return"SensorBlock";if(ϟ.ǫ("connector"))return"ShipConnector";if(ϟ.Ǭ("reflector")||ϟ.Ǭ("spotlight"))
return"ReflectorLight";if((ϟ.Ǭ("inter")&&ϟ.ǩ("light")))return"InteriorLight";if(ϟ.Ǭ("land"))return"LandingGear";if(ϟ.Ǭ(
"program"))return"ProgrammableBlock";if(ϟ.Ǭ("timer"))return"TimerBlock";if(ϟ.Ǭ("motor")||ϟ.Ǭ("rotor"))return"MotorStator";if(ϟ.Ǭ(
"piston"))return"PistonBase";if(ϟ.Ǭ("proj"))return"Projector";if(ϟ.ǫ("merge"))return"ShipMergeBlock";if(ϟ.Ǭ("sound"))return
"SoundBlock";if(ϟ.Ǭ("col"))return"Collector";if(ϟ.ǫ("jump"))return"JumpDrive";if(string.Compare(ϟ,"door",true)==0)return"Door";if((ϟ
.ǫ("grav")&&ϟ.ǫ("sphe")))return"GravityGeneratorSphere";if(ϟ.ǫ("grav"))return"GravityGenerator";if(ϟ.ǩ("drill"))return
"ShipDrill";if(ϟ.ǫ("grind"))return"ShipGrinder";if(ϟ.ǩ("welder"))return"ShipWelder";if(ϟ.Ǭ("parach"))return"Parachute";if((ϟ.ǫ(
"turret")&&ϟ.ǫ("gatl")))return"LargeGatlingTurret";if((ϟ.ǫ("turret")&&ϟ.ǫ("inter")))return"LargeInteriorTurret";if((ϟ.ǫ("turret"
)&&ϟ.ǫ("miss")))return"LargeMissileTurret";if(ϟ.ǫ("gatl"))return"SmallGatlingGun";if((ϟ.ǫ("launcher")&&ϟ.ǫ("reload")))
return"SmallMissileLauncherReload";if((ϟ.ǫ("launcher")))return"SmallMissileLauncher";if(ϟ.ǫ("mass"))return"VirtualMass";if(
string.Compare(ϟ,"warhead",true)==0)return"Warhead";if(ϟ.Ǭ("func"))return"FunctionalBlock";if(string.Compare(ϟ,"shipctrl",true
)==0)return"ShipController";if(ϟ.Ǭ("light"))return"LightingBlock";if(ϟ.Ǭ("contr"))return"ControlPanel";if(ϟ.Ǭ("coc"))
return"Cockpit";if(ϟ.Ǭ("medi"))return"MedicalRoom";if(ϟ.Ǭ("remote"))return"RemoteControl";if(ϟ.Ǭ("but"))return"ButtonPanel";if
(ϟ.Ǭ("cam"))return"CameraBlock";if(ϟ.ǫ("detect"))return"OreDetector";if(ϟ.Ǭ("safe"))return"SafeZoneBlock";if(ϟ.Ǭ("store")
)return"StoreBlock";if(ϟ.Ǭ("contract"))return"ContractBlock";if(ϟ.Ǭ("vending"))return"VendingMachine";if(ϟ.Ǭ("decoy"))
return"Decoy";return"Unknown";}public string Ϟ(IMyBatteryBlock ō){string ϝ="";if(ō.ChargeMode==ChargeMode.Recharge)ϝ="(+) ";
else if(ō.ChargeMode==ChargeMode.Discharge)ϝ="(-) ";else ϝ="(±) ";return ϝ+n.Ǽ((ō.CurrentStoredPower/ō.MaxStoredPower)*
100.0f)+"%";}Dictionary<MyLaserAntennaStatus,string>Ϝ=new Dictionary<MyLaserAntennaStatus,string>(){{MyLaserAntennaStatus.Idle
,"IDLE"},{MyLaserAntennaStatus.Connecting,"CONNECTING"},{MyLaserAntennaStatus.Connected,"CONNECTED"},{
MyLaserAntennaStatus.OutOfRange,"OUT OF RANGE"},{MyLaserAntennaStatus.RotatingToTarget,"ROTATING"},{MyLaserAntennaStatus.
SearchingTargetForAntenna,"SEARCHING"}};public string ϛ(IMyLaserAntenna ŋ){return Ϝ[ŋ.Status];}public double Ϛ(IMyJumpDrive Ō,out double ʖ,out
double ƀ){ʖ=Ō.CurrentStoredPower;ƀ=Ō.MaxStoredPower;return(ƀ>0?ʖ/ƀ*100:0);}public double ϙ(IMyJumpDrive Ō){double ʖ=Ō.
CurrentStoredPower;double ƀ=Ō.MaxStoredPower;return(ƀ>0?ʖ/ƀ*100:0);}}class Ϙ:ɑ{ɬ n;Ġ đ;public int ϗ=0;public Ϙ(ɬ Ú,Ġ j){ɐ="BootPanelsTask"
;Ɍ=1;n=Ú;đ=j;if(!n.ɤ){ϗ=int.MaxValue;đ.ú=true;}}Ǒ Ď;public override void Ƀ(){Ď=n.Ď;}public override bool ɓ(bool õ){if(ϗ>n
.ɣ.Count){Ʉ();return true;}if(!õ&&ϗ==0){đ.ú=false;}if(!Ϯ(õ))return false;ϗ++;return true;}public override void ɕ(){đ.ú=
true;}public void ϰ(){û æ=đ.æ;for(int E=0;E<æ.º();E++){ç k=æ.z(E);k.G();}ϗ=(n.ɤ?0:int.MaxValue);}int E;ŝ ϯ=null;public bool
Ϯ(bool õ){û æ=đ.æ;if(!õ)E=0;int ϭ=0;for(;E<æ.º();E++){if(!ƒ.ȝ(40)||ϭ>5)return false;ç k=æ.z(E);ϯ=n.Ǐ(ϯ,k);float?Ϭ=k.Ý?.
FontSize;if(Ϭ!=null&&Ϭ>3f)continue;if(ϯ.Ŗ.Count<=0)ϯ.ŧ(n.ǋ(null,k));else n.ǋ(ϯ.Ŗ[0],k);n.Ǉ();n.Ƣ(Ď.Ǡ("B1"));double ʒ=(double)ϗ/n
.ɣ.Count*100;n.Ʈ(ʒ);if(ϗ==n.ɣ.Count){n.Ǎ("");n.Ƣ("Automatic LCDs 2");n.Ƣ("by MMaster");}else n.ǌ(n.ɣ[ϗ]);bool ß=k.ß;k.ß=
false;n.ǰ(k,ϯ);k.ß=ß;ϭ++;}return true;}public bool Ϫ(){return ϗ<=n.ɣ.Count;}}public enum ϩ{Ϩ=0,ϧ=1,Ϧ=2,ϥ=3,Ϥ=4,ϣ=5,Ϣ=6,ή=7,ʺ=
8,Β=9,ʸ=10,ʷ=11,ʶ=12,ʵ=13,ʴ=14,ʳ=15,ʲ=16,ʱ=17,ʰ=18,ʯ=19,ʮ=20,ʭ=21,ʬ=22,ʫ=23,ʪ=24,ʩ=25,ʨ=26,ʧ=27,ʦ=28,ʥ=29,ʤ=30,ʹ=31,}
class ʣ{ȑ ƒ;public string ˏ="";public string ˍ="";public string ˌ="";public string ˋ="";public ϩ ˊ=ϩ.Ϩ;public ʣ(ȑ ƚ){ƒ=ƚ;}ϩ ˉ
(){if(ˏ=="echo"||ˏ=="center"||ˏ=="right")return ϩ.ϧ;if(ˏ.StartsWith("hscroll"))return ϩ.ʤ;if(ˏ.StartsWith("inventory")||ˏ
.StartsWith("missing")||ˏ.StartsWith("invlist"))return ϩ.Ϧ;if(ˏ.StartsWith("working"))return ϩ.ʰ;if(ˏ.StartsWith("cargo")
)return ϩ.ϥ;if(ˏ.StartsWith("mass"))return ϩ.Ϥ;if(ˏ.StartsWith("shipmass"))return ϩ.ʫ;if(ˏ=="oxygen")return ϩ.ϣ;if(ˏ.
StartsWith("tanks"))return ϩ.Ϣ;if(ˏ.StartsWith("powertime"))return ϩ.ή;if(ˏ.StartsWith("powerused"))return ϩ.ʺ;if(ˏ.StartsWith(
"power"))return ϩ.Β;if(ˏ.StartsWith("speed"))return ϩ.ʸ;if(ˏ.StartsWith("accel"))return ϩ.ʷ;if(ˏ.StartsWith("alti"))return ϩ.ʩ;
if(ˏ.StartsWith("charge"))return ϩ.ʶ;if(ˏ.StartsWith("docked"))return ϩ.ʹ;if(ˏ.StartsWith("time")||ˏ.StartsWith("date"))
return ϩ.ʵ;if(ˏ.StartsWith("countdown"))return ϩ.ʴ;if(ˏ.StartsWith("textlcd"))return ϩ.ʳ;if(ˏ.EndsWith("count"))return ϩ.ʲ;if(
ˏ.StartsWith("dampeners")||ˏ.StartsWith("occupied"))return ϩ.ʱ;if(ˏ.StartsWith("damage"))return ϩ.ʯ;if(ˏ.StartsWith(
"amount"))return ϩ.ʮ;if(ˏ.StartsWith("pos"))return ϩ.ʭ;if(ˏ.StartsWith("distance"))return ϩ.ʪ;if(ˏ.StartsWith("details"))return
ϩ.ʬ;if(ˏ.StartsWith("stop"))return ϩ.ʨ;if(ˏ.StartsWith("gravity"))return ϩ.ʧ;if(ˏ.StartsWith("customdata"))return ϩ.ʦ;if(
ˏ.StartsWith("prop"))return ϩ.ʥ;return ϩ.Ϩ;}public ƙ ˈ(){switch(ˊ){case ϩ.ϧ:return new ґ();case ϩ.Ϧ:return new Ҟ();case ϩ
.ϥ:return new ʘ();case ϩ.Ϥ:return new Ҽ();case ϩ.ϣ:return new Ҫ();case ϩ.Ϣ:return new щ();case ϩ.ή:return new ц();case ϩ.
ʺ:return new П();case ϩ.Β:return new Ҥ();case ϩ.ʸ:return new я();case ϩ.ʷ:return new ʔ();case ϩ.ʶ:return new Λ();case ϩ.ʵ
:return new ˤ();case ϩ.ʴ:return new Ψ();case ϩ.ʳ:return new ĸ();case ϩ.ʲ:return new ʏ();case ϩ.ʱ:return new ѭ();case ϩ.ʰ:
return new Ķ();case ϩ.ʯ:return new Ή();case ϩ.ʮ:return new Һ();case ϩ.ʭ:return new Ҧ();case ϩ.ʬ:return new ΐ();case ϩ.ʫ:return
new Ѧ();case ϩ.ʪ:return new ͼ();case ϩ.ʩ:return new ʑ();case ϩ.ʨ:return new ь();case ϩ.ʧ:return new Ґ();case ϩ.ʦ:return new
Ρ();case ϩ.ʥ:return new ћ();case ϩ.ʤ:return new ҙ();case ϩ.ʹ:return new Ҕ();default:return new ƙ();}}public List<ʾ>ˇ=new
List<ʾ>();string[]ˆ=null;string ˎ="";bool ˁ=false;int Ĵ=1;public bool ʚ(string ˀ,bool õ){if(!õ){ˊ=ϩ.Ϩ;ˍ="";ˏ="";ˌ=ˀ.
TrimStart(' ');ˇ.Clear();if(ˌ=="")return true;int ʿ=ˌ.IndexOf(' ');if(ʿ<0||ʿ>=ˌ.Length-1)ˋ="";else ˋ=ˌ.Substring(ʿ+1);ˆ=ˌ.Split(
' ');ˎ="";ˁ=false;ˏ=ˆ[0].ToLower();Ĵ=1;}for(;Ĵ<ˆ.Length;Ĵ++){if(!ƒ.ȝ(40))return false;string Ĳ=ˆ[Ĵ];if(Ĳ=="")continue;if(Ĳ[
0]=='{'&&Ĳ[Ĳ.Length-1]=='}'){Ĳ=Ĳ.Substring(1,Ĳ.Length-2);if(Ĳ=="")continue;if(ˍ=="")ˍ=Ĳ;else ˇ.Add(new ʾ(Ĳ));continue;}if
(Ĳ[0]=='{'){ˁ=true;ˎ=Ĳ.Substring(1);continue;}if(Ĳ[Ĳ.Length-1]=='}'){ˁ=false;ˎ+=' '+Ĳ.Substring(0,Ĳ.Length-1);if(ˍ=="")ˍ=
ˎ;else ˇ.Add(new ʾ(ˎ));continue;}if(ˁ){if(ˎ.Length!=0)ˎ+=' ';ˎ+=Ĳ;continue;}if(ˍ=="")ˍ=Ĳ;else ˇ.Add(new ʾ(Ĳ));}ˊ=ˉ();
return true;}}class ʾ{public string ʽ="";public string ʼ="";public string Ĳ="";public List<string>ʻ=new List<string>();public
ʾ(string ʢ){Ĳ=ʢ;}public void ʚ(){if(Ĳ==""||ʽ!=""||ʼ!=""||ʻ.Count>0)return;string ʖ=Ĳ.Trim();if(ʖ[0]=='+'||ʖ[0]=='-'){ʽ+=ʖ
[0];ʖ=Ĳ.Substring(1);}string[]ƞ=ʖ.Split('/');string ʕ=ƞ[0];if(ƞ.Length>1){ʼ=ƞ[0];ʕ=ƞ[1];}else ʼ="";if(ʕ.Length>0){string[
]ą=ʕ.Split(',');for(int E=0;E<ą.Length;E++)if(ą[E]!="")ʻ.Add(ą[E]);}}}class ʔ:ƙ{public ʔ(){Ɍ=0.5;ɐ="CmdAccel";}public
override bool ƕ(bool õ){double ʓ=0;if(Ƙ.ˍ!="")double.TryParse(Ƙ.ˍ.Trim(),out ʓ);n.ĭ(Ď.Ǡ("AC1")+" ");n.Ƴ(n.ǀ.ɿ.ToString("F1")+
" m/s²");if(ʓ>0){double ʒ=n.ǀ.ɿ/ʓ*100;n.Ʈ(ʒ);}return true;}}class ʑ:ƙ{public ʑ(){Ɍ=1;ɐ="CmdAltitude";}public override bool ƕ(
bool õ){string ʐ=(Ƙ.ˏ.EndsWith("sea")?"sea":"ground");switch(ʐ){case"sea":n.ĭ(Ď.Ǡ("ALT1"));n.Ƴ(n.ǀ.ɵ.ToString("F0")+" m");
break;default:n.ĭ(Ď.Ǡ("ALT2"));n.Ƴ(n.ǀ.ɳ.ToString("F0")+" m");break;}return true;}}class ʏ:ƙ{public ʏ(){Ɍ=15;ɐ=
"CmdBlockCount";}γ ĵ;public override void Ƀ(){ĵ=new γ(ƒ,n.ő);}bool ʎ;bool ʍ;int Ĵ=0;int ë=0;public override bool ƕ(bool õ){if(!õ){ʎ=(Ƙ.
ˏ=="enabledcount");ʍ=(Ƙ.ˏ=="prodcount");Ĵ=0;ë=0;}if(Ƙ.ˇ.Count==0){if(ë==0){if(!õ)ĵ.ų();if(!ĵ.ν(Ƙ.ˍ,õ))return false;ë++;õ=
false;}if(!ʡ(ĵ,"blocks",ʎ,ʍ,õ))return false;return true;}for(;Ĵ<Ƙ.ˇ.Count;Ĵ++){ʾ Ĳ=Ƙ.ˇ[Ĵ];if(!õ)Ĳ.ʚ();if(!ŀ(Ĳ,õ))return false
;õ=false;}return true;}int İ=0;int ı=0;bool ŀ(ʾ Ĳ,bool õ){if(!õ){İ=0;ı=0;}for(;İ<Ĳ.ʻ.Count;İ++){if(ı==0){if(!õ)ĵ.ų();if(!
ĵ.ϼ(Ĳ.ʻ[İ],Ƙ.ˍ,õ))return false;ı++;õ=false;}if(!ʡ(ĵ,Ĳ.ʻ[İ],ʎ,ʍ,õ))return false;ı=0;õ=false;}return true;}Dictionary<
string,int>ʠ=new Dictionary<string,int>();Dictionary<string,int>ʟ=new Dictionary<string,int>();List<string>ʞ=new List<string>(
);int ù=0;int ʝ=0;int ʜ=0;ʋ ʛ=new ʋ();bool ʡ(γ Ā,string ʐ,bool ʎ,bool ʍ,bool õ){if(Ā.ϲ()==0){ʛ.ų().ʅ(char.ToUpper(ʐ[0])).
ʅ(ʐ.ToLower(),1,ʐ.Length-1);n.ĭ(ʛ.ʅ(" ").ʅ(Ď.Ǡ("C1")).ʅ(" "));string ʙ=(ʎ||ʍ?"0 / 0":"0");n.Ƴ(ʙ);return true;}if(!õ){ʠ.
Clear();ʟ.Clear();ʞ.Clear();ù=0;ʝ=0;ʜ=0;}if(ʜ==0){for(;ù<Ā.ϲ();ù++){if(!ƒ.ȝ(15))return false;IMyProductionBlock Ő=Ā.ΰ[ù]as
IMyProductionBlock;ʛ.ų().ʅ(Ā.ΰ[ù].DefinitionDisplayNameText);string Ƕ=ʛ.ɔ();if(ʞ.Contains(Ƕ)){ʠ[Ƕ]++;if((ʎ&&Ā.ΰ[ù].IsWorking)||(ʍ&&Ő!=null
&&Ő.IsProducing))ʟ[Ƕ]++;}else{ʠ.Add(Ƕ,1);ʞ.Add(Ƕ);if(ʎ||ʍ)if((ʎ&&Ā.ΰ[ù].IsWorking)||(ʍ&&Ő!=null&&Ő.IsProducing))ʟ.Add(Ƕ,1)
;else ʟ.Add(Ƕ,0);}}ʜ++;õ=false;}for(;ʝ<ʠ.Count;ʝ++){if(!ƒ.ȝ(8))return false;n.ĭ(ʞ[ʝ]+" "+Ď.Ǡ("C1")+" ");string ʙ=(ʎ||ʍ?ʟ[
ʞ[ʝ]]+" / ":"")+ʠ[ʞ[ʝ]];n.Ƴ(ʙ);}return true;}}class ʘ:ƙ{γ ĵ;public ʘ(){Ɍ=2;ɐ="CmdCargo";}public override void Ƀ(){ĵ=new γ
(ƒ,n.ő);}bool ʗ=true;bool ː=false;bool Ο=false;bool Ι=false;double Ξ=0;double Ν=0;int ë=0;public override bool ƕ(bool õ){
if(!õ){ĵ.ų();ʗ=Ƙ.ˏ.Contains("all");Ι=Ƙ.ˏ.EndsWith("bar");ː=(Ƙ.ˏ[Ƙ.ˏ.Length-1]=='x');Ο=(Ƙ.ˏ[Ƙ.ˏ.Length-1]=='p');Ξ=0;Ν=0;ë=0
;}if(ë==0){if(ʗ){if(!ĵ.ν(Ƙ.ˍ,õ))return false;}else{if(!ĵ.ϼ("cargocontainer",Ƙ.ˍ,õ))return false;}ë++;õ=false;}double Μ=ĵ.
ϊ(ref Ξ,ref Ν,õ);if(Double.IsNaN(Μ))return false;if(Ι){n.Ʈ(Μ);return true;}n.ĭ(Ď.Ǡ("C2")+" ");if(!ː&&!Ο){n.Ƴ(n.ȅ(Ξ)+
"L / "+n.ȅ(Ν)+"L");n.ƴ(Μ,1.0f,n.ɗ);n.Ǎ(' '+n.Ǽ(Μ)+"%");}else if(Ο){n.Ƴ(n.Ǽ(Μ)+"%");n.Ʈ(Μ);}else n.Ƴ(n.Ǽ(Μ)+"%");return true;}}
class Λ:ƙ{public Λ(){Ɍ=3;ɐ="CmdCharge";}γ ĵ;public override void Ƀ(){ĵ=new γ(ƒ,n.ő);}int ë=0;int ù=0;bool ː=false;bool Κ=
false;bool Ι=false;Dictionary<long,double>e=new Dictionary<long,double>();Dictionary<long,double>Θ=new Dictionary<long,double
>();Dictionary<long,double>Η=new Dictionary<long,double>();Dictionary<long,double>Ζ=new Dictionary<long,double>();
Dictionary<long,double>Ε=new Dictionary<long,double>();double Δ(long Γ,double ʖ,double ƀ){double Π=0;double Χ=0;double έ=0;double
ά=0;if(Θ.TryGetValue(Γ,out έ)){ά=Ζ[Γ];}if(e.TryGetValue(Γ,out Π)){Χ=Η[Γ];}double Ϋ=(ƒ.ȍ-έ);double Ϊ=0;if(Ϋ>0)Ϊ=(ʖ-ά)/Ϋ;if
(Ϊ<0){if(!Ε.TryGetValue(Γ,out Ϊ))Ϊ=0;}else Ε[Γ]=Ϊ;if(Π>0){Θ[Γ]=e[Γ];Ζ[Γ]=Η[Γ];}e[Γ]=ƒ.ȍ;Η[Γ]=ʖ;return(Ϊ>0?(ƀ-ʖ)/Ϊ:0);}
public override bool ƕ(bool õ){if(!õ){ĵ.ų();Ι=Ƙ.ˏ.EndsWith("bar");ː=Ƙ.ˏ.Contains("x");Κ=Ƙ.ˏ.Contains("time");ù=0;ë=0;}if(ë==0)
{if(!ĵ.ϼ("jumpdrive",Ƙ.ˍ,õ))return false;if(ĵ.ϲ()<=0){n.Ǎ("Charge: "+Ď.Ǡ("D2"));return true;}ë++;õ=false;}for(;ù<ĵ.ϲ();ù
++){if(!ƒ.ȝ(25))return false;IMyJumpDrive Ō=ĵ.ΰ[ù]as IMyJumpDrive;double ʖ,ƀ,ʒ;ʒ=n.ő.Ϛ(Ō,out ʖ,out ƀ);if(Ι){n.Ʈ(ʒ);}else{n
.ĭ(Ō.CustomName+" ");if(Κ){TimeSpan Ω=TimeSpan.FromSeconds(Δ(Ō.EntityId,ʖ,ƀ));n.Ƴ(n.ǁ.ȓ(Ω));if(!ː){n.ƴ(ʒ,1.0f,n.ɗ);n.Ƴ(
' '+ʒ.ToString("0.0")+"%");}}else{if(!ː){n.Ƴ(n.ȅ(ʖ)+"Wh / "+n.ȅ(ƀ)+"Wh");n.ƴ(ʒ,1.0f,n.ɗ);}n.Ƴ(' '+ʒ.ToString("0.0")+"%");}}
}return true;}}class Ψ:ƙ{public Ψ(){Ɍ=1;ɐ="CmdCountdown";}public override bool ƕ(bool õ){bool ˡ=Ƙ.ˏ.EndsWith("c");bool Φ=
Ƙ.ˏ.EndsWith("r");string Υ="";int Α=Ƙ.ˌ.IndexOf(' ');if(Α>=0)Υ=Ƙ.ˌ.Substring(Α+1).Trim();DateTime Τ=DateTime.Now;DateTime
Σ;if(!DateTime.TryParseExact(Υ,"H:mm d.M.yyyy",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.
DateTimeStyles.None,out Σ)){n.Ǎ(Ď.Ǡ("C3"));n.Ǎ("  Countdown 19:02 28.2.2015");return true;}TimeSpan ˑ=Σ-Τ;string ķ="";if(ˑ.Ticks<=0)ķ=
Ď.Ǡ("C4");else{if((int)ˑ.TotalDays>0)ķ+=(int)ˑ.TotalDays+" "+Ď.Ǡ("C5")+" ";if(ˑ.Hours>0||ķ!="")ķ+=ˑ.Hours+"h ";if(ˑ.
Minutes>0||ķ!="")ķ+=ˑ.Minutes+"m ";ķ+=ˑ.Seconds+"s";}if(ˡ)n.Ƣ(ķ);else if(Φ)n.Ƴ(ķ);else n.Ǎ(ķ);return true;}}class Ρ:ƙ{public Ρ(
){Ɍ=1;ɐ="CmdCustomData";}public override bool ƕ(bool õ){string ķ="";if(Ƙ.ˍ!=""&&Ƙ.ˍ!="*"){IMyTerminalBlock Ͷ=n.ƾ.
GetBlockWithName(Ƙ.ˍ)as IMyTerminalBlock;if(Ͷ==null){n.Ǎ("CustomData: "+Ď.Ǡ("CD1")+Ƙ.ˍ);return true;}ķ=Ͷ.CustomData;}else{n.Ǎ(
"CustomData:"+Ď.Ǡ("CD2"));return true;}if(ķ.Length==0)return true;n.ǌ(ķ);return true;}}class Ή:ƙ{public Ή(){Ɍ=5;ɐ="CmdDamage";}γ ĵ;
public override void Ƀ(){ĵ=new γ(ƒ,n.ő);}bool Ɯ=false;int ù=0;public override bool ƕ(bool õ){bool ː=Ƙ.ˏ.StartsWith("damagex");
bool ͳ=Ƙ.ˏ.EndsWith("noc");bool Ͳ=(!ͳ&&Ƙ.ˏ.EndsWith("c"));float ͱ=100;if(!õ){ĵ.ų();Ɯ=false;ù=0;}if(!ĵ.ν(Ƙ.ˍ,õ))return false;
if(Ƙ.ˇ.Count>0){if(!float.TryParse(Ƙ.ˇ[0].Ĳ,out ͱ))ͱ=100;}ͱ-=0.00001f;for(;ù<ĵ.ϲ();ù++){if(!ƒ.ȝ(30))return false;
IMyTerminalBlock ä=ĵ.ΰ[ù];IMySlimBlock Ͱ=ä.CubeGrid.GetCubeBlock(ä.Position);if(Ͱ==null)continue;float ˮ=(ͳ?Ͱ.MaxIntegrity:Ͱ.
BuildIntegrity);if(!Ͳ)ˮ-=Ͱ.CurrentDamage;float ʒ=100*(ˮ/Ͱ.MaxIntegrity);if(ʒ>=ͱ)continue;Ɯ=true;string ˬ=n.Ǜ(Ͱ.FatBlock.
DisplayNameText,n.ɞ*0.69f-n.ɗ);n.ĭ(ˬ+' ');if(!ː){n.Ʃ(n.ȅ(ˮ)+" / ",0.69f);n.ĭ(n.ȅ(Ͱ.MaxIntegrity));}n.Ƴ(' '+ʒ.ToString("0.0")+'%');n.Ʈ(ʒ
);}if(!Ɯ)n.Ǎ(Ď.Ǡ("D3"));return true;}}class ˤ:ƙ{public ˤ(){Ɍ=1;ɐ="CmdDateTime";}public override bool ƕ(bool õ){bool ˣ=(Ƙ.
ˏ.StartsWith("datetime"));bool ˢ=(Ƙ.ˏ.StartsWith("date"));bool ˡ=Ƙ.ˏ.Contains("c");int ˠ=Ƙ.ˏ.IndexOf('+');if(ˠ<0)ˠ=Ƙ.ˏ.
IndexOf('-');float ʹ=0;if(ˠ>=0)float.TryParse(Ƙ.ˏ.Substring(ˠ),out ʹ);DateTime ˑ=DateTime.Now.AddHours(ʹ);string ķ="";int Α=Ƙ.ˌ
.IndexOf(' ');if(Α>=0)ķ=Ƙ.ˌ.Substring(Α+1);if(!ˣ){if(!ˢ)ķ+=ˑ.ToShortTimeString();else ķ+=ˑ.ToShortDateString();}else{if(ķ
=="")ķ=String.Format("{0:d} {0:t}",ˑ);else{ķ=ķ.Replace("/","\\/");ķ=ķ.Replace(":","\\:");ķ=ķ.Replace("\"","\\\"");ķ=ķ.
Replace("'","\\'");ķ=ˑ.ToString(ķ+' ');ķ=ķ.Substring(0,ķ.Length-1);}}if(ˡ)n.Ƣ(ķ);else n.Ǎ(ķ);return true;}}class ΐ:ƙ{public ΐ()
{Ɍ=5;ɐ="CmdDetails";}string Ώ="";γ ĵ;public override void Ƀ(){ĵ=new γ(ƒ,n.ő);if(Ƙ.ˇ.Count>0)Ώ=Ƙ.ˇ[0].Ĳ.Trim();}int ë=0;
int ù=1;bool Ύ=false;IMyTerminalBlock ä;public override bool ƕ(bool õ){if(Ƙ.ˍ==""||Ƙ.ˍ=="*"){n.Ǎ("Details: "+Ď.Ǡ("D1"));
return true;}if(!õ){ĵ.ų();Ύ=Ƙ.ˏ.Contains("non");ë=0;ù=1;}if(ë==0){if(!ĵ.ν(Ƙ.ˍ,õ))return true;if(ĵ.ϲ()<=0){n.Ǎ("Details: "+Ď.Ǡ(
"D2"));return true;}ë++;õ=false;}int Ό=(Ƙ.ˏ.EndsWith("x")?1:0);if(ë==1){if(!õ){ä=ĵ.ΰ[0];if(!Ύ)n.Ǎ(ä.CustomName);}if(!Ά(ä,Ό,õ
))return false;ë++;õ=false;}for(;ù<ĵ.ϲ();ù++){if(!õ){ä=ĵ.ΰ[ù];if(!Ύ){n.Ǎ("");n.Ǎ(ä.CustomName);}}if(!Ά(ä,Ό,õ))return
false;õ=false;}return true;}string[]ǈ;int Ί=0;bool Έ=false;ʋ ƫ=new ʋ();bool Ά(IMyTerminalBlock ä,int ͽ,bool õ){if(!õ){ǈ=ƫ.ų()
.ʅ(ä.DetailedInfo).ʅ('\n').ʅ(ä.CustomInfo).ɔ().Split('\n');Ί=ͽ;Έ=(Ώ.Length==0);}for(;Ί<ǈ.Length;Ί++){if(!ƒ.ȝ(5))return
false;if(ǈ[Ί].Length==0)continue;if(!Έ){if(!ǈ[Ί].Contains(Ώ))continue;Έ=true;}n.Ǎ(ƫ.ų().ʅ("  ").ʅ(ǈ[Ί]));}return true;}}class
ͼ:ƙ{public ͼ(){Ɍ=1;ɐ="CmdDistance";}string ͻ="";string[]ͺ;Vector3D ι;string ͷ="";bool Љ=false;public override void Ƀ(){Љ=
false;if(Ƙ.ˇ.Count<=0)return;ͻ=Ƙ.ˇ[0].Ĳ.Trim();ͺ=ͻ.Split(':');if(ͺ.Length<5||ͺ[0]!="GPS")return;double Ҙ,Җ,ҕ;if(!double.
TryParse(ͺ[2],out Ҙ))return;if(!double.TryParse(ͺ[3],out Җ))return;if(!double.TryParse(ͺ[4],out ҕ))return;ι=new Vector3D(Ҙ,Җ,ҕ);
ͷ=ͺ[1];Љ=true;}public override bool ƕ(bool õ){if(!Љ){n.Ǎ("Distance: "+Ď.Ǡ("DTU")+" '"+ͻ+"'.");return true;}
IMyTerminalBlock ä=j.D.ä;if(Ƙ.ˍ!=""&&Ƙ.ˍ!="*"){ä=n.ƾ.GetBlockWithName(Ƙ.ˍ);if(ä==null){n.Ǎ("Distance: "+Ď.Ǡ("P1")+": "+Ƙ.ˍ);return true;
}}double ъ=Vector3D.Distance(ä.GetPosition(),ι);n.ĭ(ͷ+": ");n.Ƴ(n.ȅ(ъ)+"m ");return true;}}class Ҕ:ƙ{γ ĵ;public Ҕ(){Ɍ=2;ɐ
="CmdDocked";}public override void Ƀ(){ĵ=new γ(ƒ,n.ő);}int ë=0;int ғ=0;bool җ=false;bool Ғ=false;IMyShipConnector Ŋ;
public override bool ƕ(bool õ){if(!õ){if(Ƙ.ˏ.EndsWith("e"))җ=true;if(Ƙ.ˏ.Contains("cn"))Ғ=true;ĵ.ų();ë=0;}if(ë==0){if(!ĵ.ϼ(
"connector",Ƙ.ˍ,õ))return false;ë++;ғ=0;õ=false;}if(ĵ.ϲ()<=0){n.Ǎ("Docked: "+Ď.Ǡ("DO1"));return true;}for(;ғ<ĵ.ϲ();ғ++){Ŋ=ĵ.ΰ[ғ]as
IMyShipConnector;if(Ŋ.Status==MyShipConnectorStatus.Connected){if(Ғ){n.ĭ(Ŋ.CustomName+":");n.Ƴ(Ŋ.OtherConnector.CubeGrid.CustomName);}
else{n.Ǎ(Ŋ.OtherConnector.CubeGrid.CustomName);}}else{if(җ){if(Ғ){n.ĭ(Ŋ.CustomName+":");n.Ƴ("-");}else n.Ǎ("-");}}}return
true;}}class ґ:ƙ{public ґ(){Ɍ=30;ɐ="CmdEcho";}public override bool ƕ(bool õ){string ʐ=(Ƙ.ˏ=="center"?"c":(Ƙ.ˏ=="right"?"r":
"n"));switch(ʐ){case"c":n.Ƣ(Ƙ.ˋ);break;case"r":n.Ƴ(Ƙ.ˋ);break;default:n.Ǎ(Ƙ.ˋ);break;}return true;}}class Ґ:ƙ{public Ґ(){Ɍ=
1;ɐ="CmdGravity";}public override bool ƕ(bool õ){string ʐ=(Ƙ.ˏ.Contains("nat")?"n":(Ƙ.ˏ.Contains("art")?"a":(Ƙ.ˏ.Contains
("tot")?"t":"s")));Vector3D ϖ;if(n.ǀ.ɰ==null){n.Ǎ("Gravity: "+Ď.Ǡ("GNC"));return true;}switch(ʐ){case"n":n.ĭ(Ď.Ǡ("G2")+
" ");ϖ=n.ǀ.ɰ.GetNaturalGravity();n.Ƴ(ϖ.Length().ToString("F1")+" m/s²");break;case"a":n.ĭ(Ď.Ǡ("G3")+" ");ϖ=n.ǀ.ɰ.
GetArtificialGravity();n.Ƴ(ϖ.Length().ToString("F1")+" m/s²");break;case"t":n.ĭ(Ď.Ǡ("G1")+" ");ϖ=n.ǀ.ɰ.GetTotalGravity();n.Ƴ(ϖ.Length().
ToString("F1")+" m/s²");break;default:n.ĭ(Ď.Ǡ("GN"));n.Ʃ(" | ",0.33f);n.Ʃ(Ď.Ǡ("GA")+" | ",0.66f);n.Ƴ(Ď.Ǡ("GT"),1.0f);n.ĭ("");ϖ=n
.ǀ.ɰ.GetNaturalGravity();n.Ʃ(ϖ.Length().ToString("F1")+" | ",0.33f);ϖ=n.ǀ.ɰ.GetArtificialGravity();n.Ʃ(ϖ.Length().
ToString("F1")+" | ",0.66f);ϖ=n.ǀ.ɰ.GetTotalGravity();n.Ƴ(ϖ.Length().ToString("F1")+" ");break;}return true;}}class ҙ:ƙ{public ҙ
(){Ɍ=0.5;ɐ="CmdHScroll";}ʋ Ң=new ʋ();int Ҡ=1;public override bool ƕ(bool õ){if(Ң.ʉ==0){string ķ=Ƙ.ˋ+"  ";if(ķ.Length==0)
return true;float ҟ=n.ɞ;float Ʀ=n.ǝ(ķ,n.Ƹ);float ю=ҟ/Ʀ;if(ю>1)Ң.ʅ(string.Join("",Enumerable.Repeat(ķ,(int)Math.Ceiling(ю))));
else Ң.ʅ(ķ);if(ķ.Length>40)Ҡ=3;else if(ķ.Length>5)Ҡ=2;else Ҡ=1;n.Ǎ(Ң);return true;}bool Φ=Ƙ.ˏ.EndsWith("r");if(Φ){Ң.ƫ.Insert
(0,Ң.ɔ(Ң.ʉ-Ҡ,Ҡ));Ң.ʄ(Ң.ʉ-Ҡ,Ҡ);}else{Ң.ʅ(Ң.ɔ(0,Ҡ));Ң.ʄ(0,Ҡ);}n.Ǎ(Ң);return true;}}class Ҟ:ƙ{public Ҟ(){Ɍ=7;ɐ="CmdInvList";
}float ҝ=-1;float ҡ=-1;public override void Ƀ(){ĵ=new γ(ƒ,n.ő);ѻ=new Ɣ(ƒ,n);}ʋ ƫ=new ʋ(100);Dictionary<string,string>Ҝ=
new Dictionary<string,string>();void қ(string ȥ,double ѯ,int Ô){if(Ô>0){if(!ҁ)n.ƴ(Math.Min(100,100*ѯ/Ô),0.3f);string ˬ;if(Ҝ
.ContainsKey(ȥ)){ˬ=Ҝ[ȥ];}else{if(!Ҋ)ˬ=n.Ǜ(ȥ,n.ɞ*0.5f-ҏ-ҡ);else{if(!ҁ)ˬ=n.Ǜ(ȥ,n.ɞ*0.5f);else ˬ=n.Ǜ(ȥ,n.ɞ*0.9f);}Ҝ[ȥ]=ˬ;}ƫ.
ų();if(!ҁ)ƫ.ʅ(' ');if(!Ҋ){n.ĭ(ƫ.ʅ(ˬ).ʅ(' '));n.Ʃ(n.ȅ(ѯ),1.0f,ҏ+ҡ);n.Ǎ(ƫ.ų().ʅ(" / ").ʅ(n.ȅ(Ô)));}else{n.Ǎ(ƫ.ʅ(ˬ));}}else{
if(!Ҋ){n.ĭ(ƫ.ų().ʅ(ȥ).ʅ(':'));n.Ƴ(n.ȅ(ѯ),1.0f,ҝ);}else n.Ǎ(ƫ.ų().ʅ(ȥ));}}void Қ(string ȥ,double ѯ,double ѹ,int Ô){if(Ô>0){
if(!Ҋ){n.ĭ(ƫ.ų().ʅ(ȥ).ʅ(' '));n.Ʃ(n.ȅ(ѯ),0.51f);n.ĭ(ƫ.ų().ʅ(" / ").ʅ(n.ȅ(Ô)));n.Ƴ(ƫ.ų().ʅ(" +").ʅ(n.ȅ(ѹ)).ʅ(" ").ʅ(Ď.Ǡ(
"I1")),1.0f);}else n.Ǎ(ƫ.ų().ʅ(ȥ));if(!ҁ)n.Ʈ(Math.Min(100,100*ѯ/Ô));}else{if(!Ҋ){n.ĭ(ƫ.ų().ʅ(ȥ).ʅ(':'));n.Ʃ(n.ȅ(ѯ),0.51f);n.
Ƴ(ƫ.ų().ʅ(" +").ʅ(n.ȅ(ѹ)).ʅ(" ").ʅ(Ď.Ǡ("I1")),1.0f);}else{n.Ǎ(ƫ.ų().ʅ(ȥ));}}}float ҏ=0;bool Ѻ(Ŷ ź){int Ô=(Ҍ?ź.ŵ:ź.ƀ);if(Ô
<0)return true;float Ʊ=n.ǝ(n.ȅ(Ô),n.Ƹ);if(Ʊ>ҏ)ҏ=Ʊ;return true;}List<Ŷ>Ѹ;int ѷ=0;int Ѷ=0;bool ѵ(bool õ,bool Ѵ,string Î,
string О){if(!õ){Ѷ=0;ѷ=0;}if(Ѷ==0){if(Ҏ){if((Ѹ=ѻ.ż(Î,õ,Ѻ))==null)return false;}else{if((Ѹ=ѻ.ż(Î,õ))==null)return false;}Ѷ++;õ=
false;}if(Ѹ.Count>0){if(!Ѵ&&!õ){if(!n.Ʒ)n.Ǎ();n.Ƣ(ƫ.ų().ʅ("<< ").ʅ(О).ʅ(" ").ʅ(Ď.Ǡ("I2")).ʅ(" >>"));}for(;ѷ<Ѹ.Count;ѷ++){if(!
ƒ.ȝ(30))return false;double ѯ=Ѹ[ѷ].Ɖ;if(Ҍ&&ѯ>=Ѹ[ѷ].ŵ)continue;int Ô=Ѹ[ѷ].ƀ;if(Ҍ)Ô=Ѹ[ѷ].ŵ;string ȥ=n.ǲ(Ѹ[ѷ].Ï,Ѹ[ѷ].Î);қ(ȥ,
ѯ,Ô);}}return true;}List<Ŷ>ѳ;int Ѳ=0;int ѱ=0;bool Ѱ(bool õ){if(!õ){Ѳ=0;ѱ=0;}if(ѱ==0){if((ѳ=ѻ.ż("Ingot",õ))==null)return
false;ѱ++;õ=false;}if(ѳ.Count>0){if(!ҋ&&!õ){if(!n.Ʒ)n.Ǎ();n.Ƣ(ƫ.ų().ʅ("<< ").ʅ(Ď.Ǡ("I4")).ʅ(" ").ʅ(Ď.Ǡ("I2")).ʅ(" >>"));}for(
;Ѳ<ѳ.Count;Ѳ++){if(!ƒ.ȝ(40))return false;double ѯ=ѳ[Ѳ].Ɖ;if(Ҍ&&ѯ>=ѳ[Ѳ].ŵ)continue;int Ô=ѳ[Ѳ].ƀ;if(Ҍ)Ô=ѳ[Ѳ].ŵ;string ȥ=n.ǲ
(ѳ[Ѳ].Ï,ѳ[Ѳ].Î);if(ѳ[Ѳ].Ï!="Scrap"){double ѹ=ѻ.ſ(ѳ[Ѳ].Ï+" Ore",ѳ[Ѳ].Ï,"Ore").Ɖ;Қ(ȥ,ѯ,ѹ,Ô);}else қ(ȥ,ѯ,Ô);}}return true;}γ
ĵ=null;Ɣ ѻ;List<ʾ>ˇ;bool ҍ,ː,Ҍ,ҋ,Ҋ,ҁ;int Ĵ,İ;string Ҁ="";float ѿ=0;bool Ҏ=true;void Ѿ(){if(n.Ƹ!=Ҁ||ѿ!=n.ɞ){Ҝ.Clear();ѿ=n.
ɞ;}if(n.Ƹ!=Ҁ){ҡ=n.ǝ(" / ",n.Ƹ);ҝ=n.Ǹ(' ',n.Ƹ);Ҁ=n.Ƹ;}ĵ.ų();ҍ=Ƙ.ˏ.EndsWith("x")||Ƙ.ˏ.EndsWith("xs");ː=Ƙ.ˏ.EndsWith("s")||Ƙ
.ˏ.EndsWith("sx");Ҍ=Ƙ.ˏ.StartsWith("missing");ҋ=Ƙ.ˏ.Contains("list");ҁ=Ƙ.ˏ.Contains("nb");Ҋ=Ƙ.ˏ.Contains("nn");ѻ.ų();ˇ=Ƙ.
ˇ;if(ˇ.Count==0)ˇ.Add(new ʾ("all"));}bool ѽ(bool õ){if(!õ)Ĵ=0;for(;Ĵ<ˇ.Count;Ĵ++){ʾ Ĳ=ˇ[Ĵ];Ĳ.ʚ();string Î=Ĳ.ʼ;if(!õ)İ=0;
else õ=false;for(;İ<Ĳ.ʻ.Count;İ++){if(!ƒ.ȝ(30))return false;string[]ą=Ĳ.ʻ[İ].Split(':');double Ȃ;if(string.Compare(ą[0],
"all",true)==0)ą[0]="";int ŵ=1;int ƀ=-1;if(ą.Length>1){if(Double.TryParse(ą[1],out Ȃ)){if(Ҍ)ŵ=(int)Math.Ceiling(Ȃ);else ƀ=(
int)Math.Ceiling(Ȃ);}}string Ɵ=ą[0];if(!string.IsNullOrEmpty(Î))Ɵ+=' '+Î;ѻ.Ơ(Ɵ,Ĳ.ʽ=="-",ŵ,ƀ);}}return true;}int К=0;int σ=0
;int ң=0;List<MyInventoryItem>ƃ=new List<MyInventoryItem>();bool һ(bool õ){γ ϳ=ĵ;if(!õ)К=0;for(;К<ϳ.ΰ.Count;К++){if(!õ)σ=
0;for(;σ<ϳ.ΰ[К].InventoryCount;σ++){IMyInventory ς=ϳ.ΰ[К].GetInventory(σ);if(!õ){ң=0;ƃ.Clear();ς.GetItems(ƃ);}else õ=
false;for(;ң<ƃ.Count;ң++){if(!ƒ.ȝ(40))return false;MyInventoryItem À=ƃ[ң];string Ñ=n.ǵ(À);string Ï,Î;n.ǳ(Ñ,out Ï,out Î);if(
string.Compare(Î,"ore",true)==0){if(ѻ.Ɲ(Ï+" ingot",Ï,"Ingot")&&ѻ.Ɲ(Ñ,Ï,Î))continue;}else{if(ѻ.Ɲ(Ñ,Ï,Î))continue;}n.ǳ(Ñ,out Ï,
out Î);Ŷ ŷ=ѻ.ſ(Ñ,Ï,Î);ŷ.Ɖ+=(double)À.Amount;}}}return true;}int ë=0;public override bool ƕ(bool õ){if(!õ){Ѿ();ë=0;}for(;ë<=
13;ë++){switch(ë){case 0:if(!ĵ.ν(Ƙ.ˍ,õ))return false;break;case 1:if(!ѽ(õ))return false;if(ҍ)ë++;break;case 2:if(!ѻ.Ÿ(õ))
return false;break;case 3:if(!һ(õ))return false;break;case 4:if(!ѵ(õ,ҋ,"Ore",Ď.Ǡ("I3")))return false;break;case 5:if(ː){if(!ѵ(
õ,ҋ,"Ingot",Ď.Ǡ("I4")))return false;}else{if(!Ѱ(õ))return false;}break;case 6:if(!ѵ(õ,ҋ,"Component",Ď.Ǡ("I5")))return
false;break;case 7:if(!ѵ(õ,ҋ,"OxygenContainerObject",Ď.Ǡ("I6")))return false;break;case 8:if(!ѵ(õ,true,"GasContainerObject",
""))return false;break;case 9:if(!ѵ(õ,ҋ,"AmmoMagazine",Ď.Ǡ("I7")))return false;break;case 10:if(!ѵ(õ,ҋ,"PhysicalGunObject"
,Ď.Ǡ("I8")))return false;break;case 11:if(!ѵ(õ,true,"Datapad",""))return false;break;case 12:if(!ѵ(õ,true,
"ConsumableItem",""))return false;break;case 13:if(!ѵ(õ,true,"PhysicalObject",""))return false;break;}õ=false;}Ҏ=false;return true;}}
class Һ:ƙ{public Һ(){Ɍ=2;ɐ="CmdAmount";}γ ĵ;public override void Ƀ(){ĵ=new γ(ƒ,n.ő);}bool ҹ;bool Ҹ=false;int ı=0;int Ĵ=0;int
İ=0;public override bool ƕ(bool õ){if(!õ){ҹ=!Ƙ.ˏ.EndsWith("x");Ҹ=Ƙ.ˏ.EndsWith("bar");if(Ҹ)ҹ=true;if(Ƙ.ˇ.Count==0)Ƙ.ˇ.Add(
new ʾ("reactor,gatlingturret,missileturret,interiorturret,gatlinggun,launcherreload,launcher,oxygenerator"));Ĵ=0;}for(;Ĵ<Ƙ.
ˇ.Count;Ĵ++){ʾ Ĳ=Ƙ.ˇ[Ĵ];if(!õ){Ĳ.ʚ();ı=0;İ=0;}for(;İ<Ĳ.ʻ.Count;İ++){if(ı==0){if(!õ){if(Ĳ.ʻ[İ]=="")continue;ĵ.ų();}string
ľ=Ĳ.ʻ[İ];if(!ĵ.ϼ(ľ,Ƙ.ˍ,õ))return false;ı++;õ=false;}if(!ҿ(õ))return false;õ=false;ı=0;}}return true;}int ҷ=0;int Ī=0;
double ŷ=0;double ҽ=0;double ӄ=0;int ң=0;IMyTerminalBlock ӂ;IMyInventory Ӂ;List<MyInventoryItem>ƃ=new List<MyInventoryItem>();
string Ӏ="";bool ҿ(bool õ){if(!õ){ҷ=0;Ī=0;}for(;ҷ<ĵ.ϲ();ҷ++){if(Ī==0){if(!ƒ.ȝ(50))return false;ӂ=ĵ.ΰ[ҷ];Ӂ=ӂ.GetInventory(0);if
(Ӂ==null)continue;Ī++;õ=false;}if(!õ){ƃ.Clear();Ӂ.GetItems(ƃ);Ӏ=(ƃ.Count>0?ƃ[0].Type.ToString():"");ң=0;ŷ=0;ҽ=0;ӄ=0;}for(
;ң<ƃ.Count;ң++){if(!ƒ.ȝ(30))return false;MyInventoryItem À=ƃ[ң];if(À.Type.ToString()!=Ӏ)ӄ+=(double)À.Amount;else ŷ+=(
double)À.Amount;}string Ҿ=Ď.Ǡ("A1");string Ƌ=ӂ.CustomName;if(ŷ>0&&(double)Ӂ.CurrentVolume>0){double Ӄ=ӄ*(double)Ӂ.
CurrentVolume/(ŷ+ӄ);ҽ=Math.Floor(ŷ*((double)Ӂ.MaxVolume-Ӄ)/((double)Ӂ.CurrentVolume-Ӄ));Ҿ=n.ȅ(ŷ)+" / "+(ӄ>0?"~":"")+n.ȅ(ҽ);}if(!Ҹ||ҽ
<=0){Ƌ=n.Ǜ(Ƌ,n.ɞ*0.8f);n.ĭ(Ƌ);n.Ƴ(Ҿ);}if(ҹ&&ҽ>0){double ʒ=100*ŷ/ҽ;n.Ʈ(ʒ);}Ī=0;õ=false;}return true;}}class Ҽ:ƙ{γ ĵ;public
Ҽ(){Ɍ=2;ɐ="CmdMass";}public override void Ƀ(){ĵ=new γ(ƒ,n.ő);}bool ː=false;bool Ο=false;int ë=0;public override bool ƕ(
bool õ){if(!õ){ĵ.ų();ː=(Ƙ.ˏ[Ƙ.ˏ.Length-1]=='x');Ο=(Ƙ.ˏ[Ƙ.ˏ.Length-1]=='p');ë=0;}if(ë==0){if(!ĵ.ν(Ƙ.ˍ,õ))return false;ë++;õ=
false;}double Ç=ĵ.τ(õ);if(Double.IsNaN(Ç))return false;double ʓ=0;int Ѥ=Ƙ.ˇ.Count;if(Ѥ>0){double.TryParse(Ƙ.ˇ[0].Ĳ.Trim(),out
ʓ);if(Ѥ>1){string ѣ=Ƙ.ˇ[1].Ĳ.Trim();char э=' ';if(ѣ.Length>0)э=Char.ToLower(ѣ[0]);int ё="kmgtpezy".IndexOf(э);if(ё>=0)ʓ*=
Math.Pow(1000.0,ё);}ʓ*=1000.0;}n.ĭ(Ď.Ǡ("M1")+" ");if(ʓ<=0){n.Ƴ(n.ȃ(Ç,false));return true;}double ʒ=Ç/ʓ*100;if(!ː&&!Ο){n.Ƴ(n.
ȃ(Ç)+" / "+n.ȃ(ʓ));n.ƴ(ʒ,1.0f,n.ɗ);n.Ǎ(' '+n.Ǽ(ʒ)+"%");}else if(Ο){n.Ƴ(n.Ǽ(ʒ)+"%");n.Ʈ(ʒ);}else n.Ƴ(n.Ǽ(ʒ)+"%");return
true;}}class Ҫ:ƙ{Ȳ ǁ;γ ĵ;public Ҫ(){Ɍ=3;ɐ="CmdOxygen";}public override void Ƀ(){ǁ=n.ǁ;ĵ=new γ(ƒ,n.ő);}int ë=0;int ù=0;bool Ɯ
=false;int ҩ=0;double ȴ=0;double ȵ=0;double ƭ;public override bool ƕ(bool õ){if(!õ){ĵ.ų();ë=0;ù=0;ƭ=0;}if(ë==0){if(!ĵ.ϼ(
"airvent",Ƙ.ˍ,õ))return false;Ɯ=(ĵ.ϲ()>0);ë++;õ=false;}if(ë==1){for(;ù<ĵ.ϲ();ù++){if(!ƒ.ȝ(8))return false;IMyAirVent ŏ=ĵ.ΰ[ù]as
IMyAirVent;ƭ=Math.Max(ŏ.GetOxygenLevel()*100,0f);n.ĭ(ŏ.CustomName);if(ŏ.CanPressurize)n.Ƴ(n.Ǽ(ƭ)+"%");else n.Ƴ(Ď.Ǡ("O1"));n.Ʈ(ƭ);}
ë++;õ=false;}if(ë==2){if(!õ)ĵ.ų();if(!ĵ.ϼ("oxyfarm",Ƙ.ˍ,õ))return false;ҩ=ĵ.ϲ();ë++;õ=false;}if(ë==3){if(ҩ>0){if(!õ)ù=0;
double Ҩ=0;for(;ù<ҩ;ù++){if(!ƒ.ȝ(4))return false;IMyOxygenFarm ҧ=ĵ.ΰ[ù]as IMyOxygenFarm;Ҩ+=ҧ.GetOutput()*100;}ƭ=Ҩ/ҩ;if(Ɯ)n.Ǎ(
"");Ɯ|=(ҩ>0);n.ĭ(Ď.Ǡ("O2"));n.Ƴ(n.Ǽ(ƭ)+"%");n.Ʈ(ƭ);}ë++;õ=false;}if(ë==4){if(!õ)ĵ.ų();if(!ĵ.ϼ("oxytank",Ƙ.ˍ,õ))return
false;ҩ=ĵ.ϲ();if(ҩ==0){if(!Ɯ)n.Ǎ(Ď.Ǡ("O3"));return true;}ë++;õ=false;}if(ë==5){if(!õ){ȴ=0;ȵ=0;ù=0;}if(!ǁ.ȷ(ĵ.ΰ,"oxygen",ref ȵ
,ref ȴ,õ))return false;if(ȴ==0){if(!Ɯ)n.Ǎ(Ď.Ǡ("O3"));return true;}ƭ=ȵ/ȴ*100;if(Ɯ)n.Ǎ("");n.ĭ(Ď.Ǡ("O4"));n.Ƴ(n.Ǽ(ƭ)+"%");n
.Ʈ(ƭ);ë++;}return true;}}class Ҧ:ƙ{public Ҧ(){Ɍ=1;ɐ="CmdPosition";}public override bool ƕ(bool õ){bool ҥ=(Ƙ.ˏ=="posxyz");
bool ͻ=(Ƙ.ˏ=="posgps");IMyTerminalBlock ä=j.D.ä;if(Ƙ.ˍ!=""&&Ƙ.ˍ!="*"){ä=n.ƾ.GetBlockWithName(Ƙ.ˍ);if(ä==null){n.Ǎ("Pos: "+Ď.
Ǡ("P1")+": "+Ƙ.ˍ);return true;}}if(ͻ){Vector3D Ŧ=ä.GetPosition();n.Ǎ("GPS:"+Ď.Ǡ("P2")+":"+Ŧ.GetDim(0).ToString("F2")+":"+
Ŧ.GetDim(1).ToString("F2")+":"+Ŧ.GetDim(2).ToString("F2")+":");return true;}n.ĭ(Ď.Ǡ("P2")+": ");if(!ҥ){n.Ƴ(ä.GetPosition(
).ToString("F0"));return true;}n.Ǎ("");n.ĭ(" X: ");n.Ƴ(ä.GetPosition().GetDim(0).ToString("F0"));n.ĭ(" Y: ");n.Ƴ(ä.
GetPosition().GetDim(1).ToString("F0"));n.ĭ(" Z: ");n.Ƴ(ä.GetPosition().GetDim(2).ToString("F0"));return true;}}class Ҥ:ƙ{public Ҥ(
){Ɍ=3;ɐ="CmdPower";}Ȳ ǁ;γ ҵ;γ Ҵ;γ ҳ;γ Х;γ Ҳ;γ ĵ;public override void Ƀ(){ҵ=new γ(ƒ,n.ő);Ҵ=new γ(ƒ,n.ő);ҳ=new γ(ƒ,n.ő);Х=
new γ(ƒ,n.ő);Ҳ=new γ(ƒ,n.ő);ĵ=new γ(ƒ,n.ő);ǁ=n.ǁ;}string О;bool ұ;string У;string Ұ;int Ҷ;int ë=0;public override bool ƕ(
bool õ){if(!õ){О=(Ƙ.ˏ.EndsWith("x")?"s":(Ƙ.ˏ.EndsWith("p")?"p":(Ƙ.ˏ.EndsWith("v")?"v":"n")));ұ=(Ƙ.ˏ.StartsWith(
"powersummary"));У="a";Ұ="";if(Ƙ.ˏ.Contains("stored"))У="s";else if(Ƙ.ˏ.Contains("in"))У="i";else if(Ƙ.ˏ.Contains("out"))У="o";ë=0;ҵ.ų
();Ҵ.ų();ҳ.ų();Х.ų();Ҳ.ų();}if(У=="a"){if(ë==0){if(!ҵ.ϼ("reactor",Ƙ.ˍ,õ))return false;õ=false;ë++;}if(ë==1){if(!Ҵ.ϼ(
"hydrogenengine",Ƙ.ˍ,õ))return false;õ=false;ë++;}if(ë==2){if(!ҳ.ϼ("solarpanel",Ƙ.ˍ,õ))return false;õ=false;ë++;}if(ë==3){if(!Ҳ.ϼ(
"windturbine",Ƙ.ˍ,õ))return false;õ=false;ë++;}}else if(ë==0)ë=4;if(ë==4){if(!Х.ϼ("battery",Ƙ.ˍ,õ))return false;õ=false;ë++;}int ү=ҵ.
ϲ();int Ү=Ҵ.ϲ();int ҭ=ҳ.ϲ();int Ҭ=Х.ϲ();int ҫ=Ҳ.ϲ();if(ë==5){Ҷ=0;if(ү>0)Ҷ++;if(Ү>0)Ҷ++;if(ҭ>0)Ҷ++;if(ҫ>0)Ҷ++;if(Ҭ>0)Ҷ++;
if(Ҷ<1){n.Ǎ(Ď.Ǡ("P6"));return true;}if(Ƙ.ˇ.Count>0){if(Ƙ.ˇ[0].Ĳ.Length>0)Ұ=Ƙ.ˇ[0].Ĳ;}ë++;õ=false;}if(У!="a"){if(!Ц(Х,(Ұ==
""?Ď.Ǡ("P7"):Ұ),У,О,õ))return false;return true;}string Ф=Ď.Ǡ("P8");if(!ұ){if(ë==6){if(ү>0)if(!Я(ҵ,(Ұ==""?Ď.Ǡ("P9"):Ұ),О,õ
))return false;ë++;õ=false;}if(ë==7){if(Ү>0)if(!Я(Ҵ,(Ұ==""?Ď.Ǡ("P12"):Ұ),О,õ))return false;ë++;õ=false;}if(ë==8){if(ҭ>0)
if(!Я(ҳ,(Ұ==""?Ď.Ǡ("P10"):Ұ),О,õ))return false;ë++;õ=false;}if(ë==9){if(ҫ>0)if(!Я(Ҳ,(Ұ==""?Ď.Ǡ("P13"):Ұ),О,õ))return false
;ë++;õ=false;}if(ë==10){if(Ҭ>0)if(!Ц(Х,(Ұ==""?Ď.Ǡ("P7"):Ұ),У,О,õ))return false;ë++;õ=false;}}else{Ф=Ď.Ǡ("P11");Ҷ=10;if(ë
==6)ë=11;}if(Ҷ==1)return true;if(!õ){ĵ.ų();ĵ.ϴ(ҵ);ĵ.ϴ(Ҵ);ĵ.ϴ(ҳ);ĵ.ϴ(Ҳ);ĵ.ϴ(Х);}if(!Я(ĵ,Ф,О,õ))return false;return true;}
void С(double ʖ,double ƀ){double Л=(ƀ>0?ʖ/ƀ*100:0);switch(О){case"s":n.Ƴ(ƫ.ų().ʅ(' ').ʅ(Л.ToString("F1")).ʅ("%"));break;case
"v":n.Ƴ(ƫ.ų().ʅ(n.ȅ(ʖ)).ʅ("W / ").ʅ(n.ȅ(ƀ)).ʅ("W"));break;case"c":n.Ƴ(ƫ.ų().ʅ(n.ȅ(ʖ)).ʅ("W"));break;case"p":n.Ƴ(ƫ.ų().ʅ(' '
).ʅ(Л.ToString("F1")).ʅ("%"));n.Ʈ(Л);break;default:n.Ƴ(ƫ.ų().ʅ(n.ȅ(ʖ)).ʅ("W / ").ʅ(n.ȅ(ƀ)).ʅ("W"));n.ƴ(Л,1.0f,n.ɗ);n.Ƴ(ƫ.
ų().ʅ(' ').ʅ(Л.ToString("F1")).ʅ("%"));break;}}double Ѽ=0;double Д=0,И=0;int а=0;bool Я(γ Ю,string Ф,string ʐ,bool õ){if(
!õ){Д=0;И=0;а=0;}if(а==0){if(!ǁ.Ƚ(Ю.ΰ,ǁ.Ȱ,ref Ѽ,ref Ѽ,ref Д,ref И,õ))return false;а++;õ=false;}if(!ƒ.ȝ(50))return false;
double Л=(И>0?Д/И*100:0);n.ĭ(Ф+": ");С(Д*1000000,И*1000000);return true;}double Э=0,Ь=0,Ы=0,б=0;double Ъ=0,Ш=0;int Ч=0;ʋ ƫ=new
ʋ(100);bool Ц(γ Х,string Ф,string У,string ʐ,bool õ){if(!õ){Э=Ь=0;Ы=б=0;Ъ=Ш=0;Ч=0;}if(Ч==0){if(!ǁ.Ȭ(Х.ΰ,ref Ы,ref б,ref Э
,ref Ь,ref Ъ,ref Ш,õ))return false;Ы*=1000000;б*=1000000;Э*=1000000;Ь*=1000000;Ъ*=1000000;Ш*=1000000;Ч++;õ=false;}double
Т=(Ш>0?Ъ/Ш*100:0);double Щ=(Ь>0?Э/Ь*100:0);double в=(б>0?Ы/б*100:0);bool м=У=="a";if(Ч==1){if(!ƒ.ȝ(50))return false;if(м)
{if(ʐ!="p"){n.ĭ(ƫ.ų().ʅ(Ф).ʅ(": "));n.Ƴ(ƫ.ų().ʅ("(IN ").ʅ(n.ȅ(Ы)).ʅ("W / OUT ").ʅ(n.ȅ(Э)).ʅ("W)"));}else n.Ǎ(ƫ.ų().ʅ(Ф).ʅ
(": "));n.ĭ(ƫ.ų().ʅ("  ").ʅ(Ď.Ǡ("P3")).ʅ(": "));}else n.ĭ(ƫ.ų().ʅ(Ф).ʅ(": "));if(м||У=="s")switch(ʐ){case"s":n.Ƴ(ƫ.ų().ʅ(
' ').ʅ(Т.ToString("F1")).ʅ("%"));break;case"v":n.Ƴ(ƫ.ų().ʅ(n.ȅ(Ъ)).ʅ("Wh / ").ʅ(n.ȅ(Ш)).ʅ("Wh"));break;case"p":n.Ƴ(ƫ.ų().ʅ(
' ').ʅ(Т.ToString("F1")).ʅ("%"));n.Ʈ(Т);break;default:n.Ƴ(ƫ.ų().ʅ(n.ȅ(Ъ)).ʅ("Wh / ").ʅ(n.ȅ(Ш)).ʅ("Wh"));n.ƴ(Т,1.0f,n.ɗ);n.Ƴ
(ƫ.ų().ʅ(' ').ʅ(Т.ToString("F1")).ʅ("%"));break;}if(У=="s")return true;Ч++;õ=false;}if(Ч==2){if(!ƒ.ȝ(50))return false;if(
м)n.ĭ(ƫ.ų().ʅ("  ").ʅ(Ď.Ǡ("P4")).ʅ(": "));if(м||У=="o")switch(ʐ){case"s":n.Ƴ(ƫ.ų().ʅ(' ').ʅ(Щ.ToString("F1")).ʅ("%"));
break;case"v":n.Ƴ(ƫ.ų().ʅ(n.ȅ(Э)).ʅ("W / ").ʅ(n.ȅ(Ь)).ʅ("W"));break;case"p":n.Ƴ(ƫ.ų().ʅ(' ').ʅ(Щ.ToString("F1")).ʅ("%"));n.Ʈ(
Щ);break;default:n.Ƴ(ƫ.ų().ʅ(n.ȅ(Э)).ʅ("W / ").ʅ(n.ȅ(Ь)).ʅ("W"));n.ƴ(Щ,1.0f,n.ɗ);n.Ƴ(ƫ.ų().ʅ(' ').ʅ(Щ.ToString("F1")).ʅ(
"%"));break;}if(У=="o")return true;Ч++;õ=false;}if(!ƒ.ȝ(50))return false;if(м)n.ĭ(ƫ.ų().ʅ("  ").ʅ(Ď.Ǡ("P5")).ʅ(": "));if(м
||У=="i")switch(ʐ){case"s":n.Ƴ(ƫ.ų().ʅ(' ').ʅ(в.ToString("F1")).ʅ("%"));break;case"v":n.Ƴ(ƫ.ų().ʅ(n.ȅ(Ы)).ʅ("W / ").ʅ(n.ȅ(
б)).ʅ("W"));break;case"p":n.Ƴ(ƫ.ų().ʅ(' ').ʅ(в.ToString("F1")).ʅ("%"));n.Ʈ(в);break;default:n.Ƴ(ƫ.ų().ʅ(n.ȅ(Ы)).ʅ("W / ")
.ʅ(n.ȅ(б)).ʅ("W"));n.ƴ(в,1.0f,n.ɗ);n.Ƴ(ƫ.ų().ʅ(' ').ʅ(в.ToString("F1")).ʅ("%"));break;}return true;}}class ц:ƙ{public ц()
{Ɍ=7;ɐ="CmdPowerTime";}class ф{public TimeSpan Ċ=new TimeSpan(-1);public double Ѝ=-1;public double у=0;}ф т=new ф();γ с;γ
р;public override void Ƀ(){с=new γ(ƒ,n.ő);р=new γ(ƒ,n.ő);}int п=0;double о=0;double х=0,н=0;double л=0,к=0,й=0;double и=0
,з=0;int ж=0;private bool е(string ˍ,out TimeSpan д,out double г,bool õ){MyResourceSourceComponent ȹ;
MyResourceSinkComponent ȗ;double М=ɍ;ф Ж=т;д=Ж.Ċ;г=Ж.Ѝ;if(!õ){с.ų();р.ų();Ж.Ѝ=0;п=0;о=0;х=н=0;л=0;к=й=0;и=з=0;ж=0;}if(п==0){if(!с.ϼ("reactor",ˍ
,õ))return false;õ=false;п++;}if(п==1){for(;ж<с.ΰ.Count;ж++){if(!ƒ.ȝ(6))return false;IMyReactor ä=с.ΰ[ж]as IMyReactor;if(
ä==null||!ä.IsWorking)continue;if(ä.Components.TryGet<MyResourceSourceComponent>(out ȹ)){х+=ȹ.CurrentOutputByType(n.ǁ.Ȱ);
н+=ȹ.MaxOutputByType(n.ǁ.Ȱ);}о+=(double)ä.GetInventory(0).CurrentMass;}õ=false;п++;}if(п==2){if(!р.ϼ("battery",ˍ,õ))
return false;õ=false;п++;}if(п==3){if(!õ)ж=0;for(;ж<р.ΰ.Count;ж++){if(!ƒ.ȝ(15))return false;IMyBatteryBlock ä=р.ΰ[ж]as
IMyBatteryBlock;if(ä==null||!ä.IsWorking)continue;if(ä.Components.TryGet<MyResourceSourceComponent>(out ȹ)){к=ȹ.CurrentOutputByType(n.ǁ
.Ȱ);й=ȹ.MaxOutputByType(n.ǁ.Ȱ);}if(ä.Components.TryGet<MyResourceSinkComponent>(out ȗ)){к-=ȗ.CurrentInputByType(n.ǁ.Ȱ);}
double Е=(к<0?(ä.MaxStoredPower-ä.CurrentStoredPower)/(-к/3600):0);if(Е>Ж.Ѝ)Ж.Ѝ=Е;if(ä.ChargeMode==ChargeMode.Recharge)
continue;и+=к;з+=й;л+=ä.CurrentStoredPower;}õ=false;п++;}double Д=х+и;if(Д<=0)Ж.Ċ=TimeSpan.FromSeconds(-1);else{double Г=Ж.Ċ.
TotalSeconds;double В;double Б=(Ж.у-о)/М;if(х<=0)Б=Math.Min(Д,н)/3600000;double А=0;if(з>0)А=Math.Min(Д,з)/3600;if(Б<=0&&А<=0)В=-1;
else if(Б<=0)В=л/А;else if(А<=0)В=о/Б;else{double Џ=А;double Ў=(х<=0?Д/3600:Б*Д/х);В=л/Џ+о/Ў;}if(Г<=0||В<0)Г=В;else Г=(Г+В)/
2;try{Ж.Ċ=TimeSpan.FromSeconds(Г);}catch{Ж.Ċ=TimeSpan.FromSeconds(-1);}}Ж.у=о;г=Ж.Ѝ;д=Ж.Ċ;return true;}int ë=0;bool Ι=
false;bool ː=false;bool Ο=false;double Ѝ=0;TimeSpan Ȓ;int Ќ=0,Ћ=0,Њ=0;int ǯ=0;int З=0;public override bool ƕ(bool õ){if(!õ){Ι
=Ƙ.ˏ.EndsWith("bar");ː=(Ƙ.ˏ[Ƙ.ˏ.Length-1]=='x');Ο=(Ƙ.ˏ[Ƙ.ˏ.Length-1]=='p');ë=0;Ќ=Ћ=Њ=ǯ=0;З=0;Ѝ=0;}if(ë==0){if(Ƙ.ˇ.Count>0
){for(;З<Ƙ.ˇ.Count;З++){if(!ƒ.ȝ(100))return false;Ƙ.ˇ[З].ʚ();if(Ƙ.ˇ[З].ʻ.Count<=0)continue;string Ĳ=Ƙ.ˇ[З].ʻ[0];int.
TryParse(Ĳ,out ǯ);if(З==0)Ќ=ǯ;else if(З==1)Ћ=ǯ;else if(З==2)Њ=ǯ;}}ë++;õ=false;}if(ë==1){if(!е(Ƙ.ˍ,out Ȓ,out Ѝ,õ))return false;ë
++;õ=false;}if(!ƒ.ȝ(30))return false;double Ċ=0;TimeSpan Р;try{Р=new TimeSpan(Ќ,Ћ,Њ);}catch{Р=TimeSpan.FromSeconds(-1);}
string ķ;if(Ȓ.TotalSeconds>0||Ѝ<=0){if(!Ι)n.ĭ(Ď.Ǡ("PT1")+" ");ķ=n.ǁ.ȓ(Ȓ);Ċ=Ȓ.TotalSeconds;}else{if(!Ι)n.ĭ(Ď.Ǡ("PT2")+" ");ķ=n.
ǁ.ȓ(TimeSpan.FromSeconds(Ѝ));if(Р.TotalSeconds>=Ѝ)Ċ=Р.TotalSeconds-Ѝ;else Ċ=0;}if(Р.Ticks<=0){n.Ƴ(ķ);return true;}double
ʒ=Ċ/Р.TotalSeconds*100;if(ʒ>100)ʒ=100;if(Ι){n.Ʈ(ʒ);return true;}if(!ː&&!Ο){n.Ƴ(ķ);n.ƴ(ʒ,1.0f,n.ɗ);n.Ǎ(' '+ʒ.ToString(
"0.0")+"%");}else if(Ο){n.Ƴ(ʒ.ToString("0.0")+"%");n.Ʈ(ʒ);}else n.Ƴ(ʒ.ToString("0.0")+"%");return true;}}class П:ƙ{public П()
{Ɍ=7;ɐ="CmdPowerUsed";}Ȳ ǁ;γ ĵ;public override void Ƀ(){ĵ=new γ(ƒ,n.ő);ǁ=n.ǁ;}string О;string Н;string ϝ;void С(double ʖ,
double ƀ){double Л=(ƀ>0?ʖ/ƀ*100:0);switch(О){case"s":n.Ƴ(Л.ToString("0.0")+"%",1.0f);break;case"v":n.Ƴ(n.ȅ(ʖ)+"W / "+n.ȅ(ƀ)+
"W",1.0f);break;case"c":n.Ƴ(n.ȅ(ʖ)+"W",1.0f);break;case"p":n.Ƴ(Л.ToString("0.0")+"%",1.0f);n.Ʈ(Л);break;default:n.Ƴ(n.ȅ(ʖ)+
"W / "+n.ȅ(ƀ)+"W");n.ƴ(Л,1.0f,n.ɗ);n.Ƴ(' '+Л.ToString("0.0")+"%");break;}}double ȱ=0,ȳ=0;int К=0;int ë=0;Ѡ Й=new Ѡ();public
override bool ƕ(bool õ){if(!õ){О=(Ƙ.ˏ.EndsWith("x")?"s":(Ƙ.ˏ.EndsWith("usedp")||Ƙ.ˏ.EndsWith("topp")?"p":(Ƙ.ˏ.EndsWith("v")?"v":
(Ƙ.ˏ.EndsWith("c")?"c":"n"))));Н=(Ƙ.ˏ.Contains("top")?"top":"");ϝ=(Ƙ.ˇ.Count>0?Ƙ.ˇ[0].Ĳ:Ď.Ǡ("PU1"));ȱ=ȳ=0;ë=0;К=0;ĵ.ų();Й
.Ã();}if(ë==0){if(!ĵ.ν(Ƙ.ˍ,õ))return false;õ=false;ë++;}MyResourceSinkComponent ȗ;MyResourceSourceComponent ȹ;switch(Н){
case"top":if(ë==1){for(;К<ĵ.ΰ.Count;К++){if(!ƒ.ȝ(20))return false;IMyTerminalBlock ä=ĵ.ΰ[К];if(ä.Components.TryGet<
MyResourceSinkComponent>(out ȗ)){ListReader<MyDefinitionId>Ȕ=ȗ.AcceptedResources;if(Ȕ.IndexOf(ǁ.Ȱ)<0)continue;ȱ=ȗ.CurrentInputByType(ǁ.Ȱ)*
1000000;}else continue;Й.X(ȱ,ä);}õ=false;ë++;}if(Й.º()<=0){n.Ǎ("PowerUsedTop: "+Ď.Ǡ("D2"));return true;}int ļ=10;if(Ƙ.ˇ.Count>0
)if(!int.TryParse(ϝ,out ļ)){ļ=10;}if(ļ>Й.º())ļ=Й.º();if(ë==2){if(!õ){К=Й.º()-1;Й.v();}for(;К>=Й.º()-ļ;К--){if(!ƒ.ȝ(30))
return false;IMyTerminalBlock ä=Й.z(К);string Ƌ=n.Ǜ(ä.CustomName,n.ɞ*0.4f);if(ä.Components.TryGet<MyResourceSinkComponent>(out
ȗ)){ȱ=ȗ.CurrentInputByType(ǁ.Ȱ)*1000000;ȳ=ȗ.MaxRequiredInputByType(ǁ.Ȱ)*1000000;}n.ĭ(Ƌ+" ");С(ȱ,ȳ);}}break;default:for(;К
<ĵ.ΰ.Count;К++){if(!ƒ.ȝ(10))return false;double ѡ;IMyTerminalBlock ä=ĵ.ΰ[К];if(ä.Components.TryGet<
MyResourceSinkComponent>(out ȗ)){ListReader<MyDefinitionId>Ȕ=ȗ.AcceptedResources;if(Ȕ.IndexOf(ǁ.Ȱ)<0)continue;ѡ=ȗ.CurrentInputByType(ǁ.Ȱ);ȳ+=ȗ.
MaxRequiredInputByType(ǁ.Ȱ);}else continue;if(ä.Components.TryGet<MyResourceSourceComponent>(out ȹ)&&(ä as IMyBatteryBlock!=null)){ѡ-=ȹ.
CurrentOutputByType(ǁ.Ȱ);if(ѡ<=0)continue;}ȱ+=ѡ;}n.ĭ(ϝ);С(ȱ*1000000,ȳ*1000000);break;}return true;}public class Ѡ{List<KeyValuePair<double,
IMyTerminalBlock>>џ=new List<KeyValuePair<double,IMyTerminalBlock>>();public void X(double ў,IMyTerminalBlock ä){џ.Add(new KeyValuePair<
double,IMyTerminalBlock>(ў,ä));}public int º(){return џ.Count;}public IMyTerminalBlock z(int w){return џ[w].Value;}public void
Ã(){џ.Clear();}public void v(){џ.Sort((Ѐ,ќ)=>(Ѐ.Key.CompareTo(ќ.Key)));}}}class ћ:ƙ{γ ĵ;public ћ(){Ɍ=1;ɐ="CmdProp";}
public override void Ƀ(){ĵ=new γ(ƒ,n.ő);}int ë=0;int К=0;bool њ=false;string ѝ=null;string Ѣ=null;string ѧ=null;string Ѯ=null;
public override bool ƕ(bool õ){if(!õ){њ=Ƙ.ˏ.StartsWith("props");ѝ=Ѣ=ѧ=Ѯ=null;К=0;ë=0;}if(Ƙ.ˇ.Count<1){n.Ǎ(Ƙ.ˏ+": "+
"Missing property name.");return true;}if(ë==0){if(!õ)ĵ.ų();if(!ĵ.ν(Ƙ.ˍ,õ))return false;Ѭ();ë++;õ=false;}if(ë==1){int ļ=ĵ.ϲ();if(ļ==0){n.Ǎ(Ƙ.ˏ+
": "+"No blocks found.");return true;}for(;К<ļ;К++){if(!ƒ.ȝ(50))return false;IMyTerminalBlock ä=ĵ.ΰ[К];if(ä.GetProperty(ѝ)!=
null){if(Ѣ==null){string ϝ=n.Ǜ(ä.CustomName,n.ɞ*0.7f);n.ĭ(ϝ);}else n.ĭ(Ѣ);n.Ƴ(ѫ(ä,ѝ,ѧ,Ѯ));if(!њ)return true;}}}return true;}
void Ѭ(){ѝ=Ƙ.ˇ[0].Ĳ;if(Ƙ.ˇ.Count>1){if(!њ)Ѣ=Ƙ.ˇ[1].Ĳ;else ѧ=Ƙ.ˇ[1].Ĳ;if(Ƙ.ˇ.Count>2){if(!њ)ѧ=Ƙ.ˇ[2].Ĳ;else Ѯ=Ƙ.ˇ[2].Ĳ;if(Ƙ.ˇ
.Count>3&&!њ)Ѯ=Ƙ.ˇ[3].Ĳ;}}}string ѫ(IMyTerminalBlock ä,string Ѫ,string ѩ=null,string Ѩ=null){return(ä.GetValue<bool>(Ѫ)?(
ѩ!=null?ѩ:Ď.Ǡ("W9")):(Ѩ!=null?Ѩ:Ď.Ǡ("W1")));}}class ѭ:ƙ{public ѭ(){Ɍ=5;ɐ="CmdShipCtrl";}γ ĵ;public override void Ƀ(){ĵ=
new γ(ƒ,n.ő);}public override bool ƕ(bool õ){if(!õ)ĵ.ų();if(!ĵ.ϼ("shipctrl",Ƙ.ˍ,õ))return false;if(ĵ.ϲ()<=0){if(Ƙ.ˍ!=""&&Ƙ.
ˍ!="*")n.Ǎ(Ƙ.ˏ+": "+Ď.Ǡ("SC1")+" ("+Ƙ.ˍ+")");else n.Ǎ(Ƙ.ˏ+": "+Ď.Ǡ("SC1"));return true;}if(Ƙ.ˏ.StartsWith("damp")){bool Њ
=(ĵ.ΰ[0]as IMyShipController).DampenersOverride;n.ĭ(Ď.Ǡ("SCD"));n.Ƴ(Њ?"ON":"OFF");}else{bool Њ=(ĵ.ΰ[0]as
IMyShipController).IsUnderControl;n.ĭ(Ď.Ǡ("SCO"));n.Ƴ(Њ?"YES":"NO");}return true;}}class Ѧ:ƙ{public Ѧ(){Ɍ=1;ɐ="CmdShipMass";}public
override bool ƕ(bool õ){bool ѥ=Ƙ.ˏ.EndsWith("base");double ʓ=0;if(Ƙ.ˍ!="")double.TryParse(Ƙ.ˍ.Trim(),out ʓ);int Ѥ=Ƙ.ˇ.Count;if(Ѥ
>0){string ѣ=Ƙ.ˇ[0].Ĳ.Trim();char э=' ';if(ѣ.Length>0)э=Char.ToLower(ѣ[0]);int ё="kmgtpezy".IndexOf(э);if(ё>=0)ʓ*=Math.
Pow(1000.0,ё);}double ɹ=(ѥ?n.ǀ.ʂ:n.ǀ.ɸ);if(!ѥ)n.ĭ(Ď.Ǡ("SM1")+" ");else n.ĭ(Ď.Ǡ("SM2")+" ");n.Ƴ(n.ȃ(ɹ,true,'k')+" ");if(ʓ>0)
n.Ʈ(ɹ/ʓ*100);return true;}}class я:ƙ{public я(){Ɍ=0.5;ɐ="CmdSpeed";}public override bool ƕ(bool õ){double ʓ=0;double ю=1;
string э="m/s";if(Ƙ.ˏ.Contains("kmh")){ю=3.6;э="km/h";}else if(Ƙ.ˏ.Contains("mph")){ю=2.23694;э="mph";}if(Ƙ.ˍ!="")double.
TryParse(Ƙ.ˍ.Trim(),out ʓ);n.ĭ(Ď.Ǡ("S1")+" ");n.Ƴ((n.ǀ.ʁ*ю).ToString("F1")+" "+э+" ");if(ʓ>0)n.Ʈ(n.ǀ.ʁ/ʓ*100);return true;}}
class ь:ƙ{public ь(){Ɍ=1;ɐ="CmdStopTask";}public override bool ƕ(bool õ){double ы=0;if(Ƙ.ˏ.Contains("best"))ы=n.ǀ.ʁ/n.ǀ.ɽ;
else ы=n.ǀ.ʁ/n.ǀ.ɺ;double ъ=n.ǀ.ʁ/2*ы;if(Ƙ.ˏ.Contains("time")){n.ĭ(Ď.Ǡ("ST"));if(double.IsNaN(ы)){n.Ƴ("N/A");return true;}
string ķ="";try{TimeSpan ˑ=TimeSpan.FromSeconds(ы);if((int)ˑ.TotalDays>0)ķ=" > 24h";else{if(ˑ.Hours>0)ķ=ˑ.Hours+"h ";if(ˑ.
Minutes>0||ķ!="")ķ+=ˑ.Minutes+"m ";ķ+=ˑ.Seconds+"s";}}catch{ķ="N/A";}n.Ƴ(ķ);return true;}n.ĭ(Ď.Ǡ("SD"));if(!double.IsNaN(ъ)&&!
double.IsInfinity(ъ))n.Ƴ(n.ȅ(ъ)+"m ");else n.Ƴ("N/A");return true;}}class щ:ƙ{Ȳ ǁ;γ ĵ;public щ(){Ɍ=2;ɐ="CmdTanks";}public
override void Ƀ(){ǁ=n.ǁ;ĵ=new γ(ƒ,n.ő);}int ë=0;char О='n';string ш;double ч=0;double ѐ=0;double ƭ;bool Ι=false;public override
bool ƕ(bool õ){List<ʾ>ˇ=Ƙ.ˇ;if(ˇ.Count==0){n.Ǎ(Ď.Ǡ("T4"));return true;}if(!õ){О=(Ƙ.ˏ.EndsWith("x")?'s':(Ƙ.ˏ.EndsWith("p")?
'p':(Ƙ.ˏ.EndsWith("v")?'v':'n')));Ι=Ƙ.ˏ.EndsWith("bar");ë=0;if(ш==null){ш=ˇ[0].Ĳ.Trim();ш=char.ToUpper(ш[0])+ш.Substring(1)
.ToLower();}ĵ.ų();ч=0;ѐ=0;}if(ë==0){if(!ĵ.ϼ("oxytank",Ƙ.ˍ,õ))return false;õ=false;ë++;}if(ë==1){if(!ĵ.ϼ("hydrogenengine",
Ƙ.ˍ,õ))return false;õ=false;ë++;}if(ë==2){if(!ǁ.ȷ(ĵ.ΰ,ш,ref ч,ref ѐ,õ))return false;õ=false;ë++;}if(ѐ==0){n.Ǎ(String.
Format(Ď.Ǡ("T5"),ш));return true;}ƭ=ч/ѐ*100;if(Ι){n.Ʈ(ƭ);return true;}n.ĭ(ш);switch(О){case's':n.Ƴ(' '+n.Ǽ(ƭ)+"%");break;case
'v':n.Ƴ(n.ȅ(ч)+"L / "+n.ȅ(ѐ)+"L");break;case'p':n.Ƴ(' '+n.Ǽ(ƭ)+"%");n.Ʈ(ƭ);break;default:n.Ƴ(n.ȅ(ч)+"L / "+n.ȅ(ѐ)+"L");n.ƴ(
ƭ,1.0f,n.ɗ);n.Ƴ(' '+ƭ.ToString("0.0")+"%");break;}return true;}}class љ{ɬ n=null;public string I="Debug";public float ј=
1.0f;public List<ʋ>ǈ=new List<ʋ>();public int ŕ=0;public float ї=0;public љ(ɬ Ú){n=Ú;ǈ.Add(new ʋ());}public void і(string ķ)
{ǈ[ŕ].ʅ(ķ);}public void і(ʋ ƪ){ǈ[ŕ].ʅ(ƪ);}public void ѕ(){ǈ.Add(new ʋ());ŕ++;ї=0;}public void ѕ(string ǎ){ǈ[ŕ].ʅ(ǎ);ѕ();}
public void є(List<ʋ>ѓ){if(ǈ[ŕ].ʉ==0)ǈ.RemoveAt(ŕ);else ŕ++;ǈ.AddList(ѓ);ŕ+=ѓ.Count-1;ѕ();}public List<ʋ>ţ(){if(ǈ[ŕ].ʉ==0)
return ǈ.GetRange(0,ŕ);else return ǈ;}public void ђ(string Ǌ,string O=""){string[]ǈ=Ǌ.Split('\n');for(int E=0;E<ǈ.Length;E++)ѕ
(O+ǈ[E]);}public void Ǉ(){ǈ.Clear();ѕ();ŕ=0;}public int ť(){return ŕ+(ǈ[ŕ].ʉ>0?1:0);}public string Ť(){return String.Join
("\n",ǈ);}public void ţ(List<ʋ>Ţ,int š,int Š){int ş=š+Š;int Ş=ť();if(ş>Ş)ş=Ş;for(int E=š;E<ş;E++)Ţ.Add(ǈ[E]);}}class ŝ{ɬ
n=null;public float Ŝ=1.0f;public int Ś=17;public int ř=0;int Ř=1;int ŗ=1;public List<љ>Ŗ=new List<љ>();public int ŕ=0;
public ŝ(ɬ Ú){n=Ú;}public void Ŕ(int ļ){ŗ=ļ;}public void ś(){Ś=(int)Math.Floor(ɬ.ɩ*Ŝ*ŗ/ɬ.ɧ);}public void ŧ(љ ķ){Ŗ.Add(ķ);}
public void ų(){Ŗ.Clear();}public int ť(){int ļ=0;foreach(var ķ in Ŗ){ļ+=ķ.ť();}return ļ;}ʋ ű=new ʋ(256);public ʋ Ť(){ű.ų();
int ļ=Ŗ.Count;for(int E=0;E<ļ-1;E++){ű.ʅ(Ŗ[E].Ť());ű.ʅ("\n");}if(ļ>0)ű.ʅ(Ŗ[ļ-1].Ť());return ű;}List<ʋ>Ű=new List<ʋ>(20);
public ʋ ů(int Ů=0){ű.ų();Ű.Clear();if(ŗ<=0)return ű;int Ų=Ŗ.Count;int ŭ=0;int Ŭ=(Ś/ŗ);int ū=(Ů*Ŭ);int Ŧ=ř+ū;int Ū=Ŧ+Ŭ;bool ũ=
false;for(int E=0;E<Ų;E++){љ ķ=Ŗ[E];int Ş=ķ.ť();int Ũ=ŭ;ŭ+=Ş;if(!ũ&&ŭ>Ŧ){int š=Ŧ-Ũ;if(ŭ>=Ū){ķ.ţ(Ű,š,Ū-Ũ-š);break;}ũ=true;ķ.ţ(
Ű,š,Ş);continue;}if(ũ){if(ŭ>=Ū){ķ.ţ(Ű,0,Ū-Ũ);break;}ķ.ţ(Ű,0,Ş);}}int ļ=Ű.Count;for(int E=0;E<ļ-1;E++){ű.ʅ(Ű[E]);ű.ʅ("\n")
;}if(ļ>0)ű.ʅ(Ű[ļ-1]);return ű;}public bool Ŀ(int ļ=-1){if(ļ<=0)ļ=n.ɥ;if(ř-ļ<=0){ř=0;return true;}ř-=ļ;return false;}
public bool Ľ(int ļ=-1){if(ļ<=0)ļ=n.ɥ;int Ļ=ť();if(ř+ļ+Ś>=Ļ){ř=Math.Max(Ļ-Ś,0);return true;}ř+=ļ;return false;}public int ĺ=0;
public void Ĺ(){if(ĺ>0){ĺ--;return;}if(ť()<=Ś){ř=0;Ř=1;return;}if(Ř>0){if(Ľ()){Ř=-1;ĺ=2;}}else{if(Ŀ()){Ř=1;ĺ=2;}}}}class ĸ:ƙ{
public ĸ(){Ɍ=1;ɐ="CmdTextLCD";}public override bool ƕ(bool õ){string ķ="";if(Ƙ.ˍ!=""&&Ƙ.ˍ!="*"){IMyTextPanel ý=n.ƾ.
GetBlockWithName(Ƙ.ˍ)as IMyTextPanel;if(ý==null){n.Ǎ("TextLCD: "+Ď.Ǡ("T1")+Ƙ.ˍ);return true;}ķ=ý.GetText();}else{n.Ǎ("TextLCD:"+Ď.Ǡ("T2"
));return true;}if(ķ.Length==0)return true;n.ǌ(ķ);return true;}}class Ķ:ƙ{public Ķ(){Ɍ=5;ɐ="CmdWorking";}γ ĵ;public
override void Ƀ(){ĵ=new γ(ƒ,n.ő);}int ë=0;int Ĵ=0;bool ĳ;public override bool ƕ(bool õ){if(!õ){ë=0;ĳ=(Ƙ.ˏ=="workingx");Ĵ=0;}if(Ƙ
.ˇ.Count==0){if(ë==0){if(!õ)ĵ.ų();if(!ĵ.ν(Ƙ.ˍ,õ))return false;ë++;õ=false;}if(!ń(ĵ,ĳ,"",õ))return false;return true;}for(
;Ĵ<Ƙ.ˇ.Count;Ĵ++){ʾ Ĳ=Ƙ.ˇ[Ĵ];if(!õ)Ĳ.ʚ();if(!ŀ(Ĳ,õ))return false;õ=false;}return true;}int ı=0;int İ=0;string[]į;string ľ
;string Į;bool ŀ(ʾ Ĳ,bool õ){if(!õ){ı=0;İ=0;}for(;İ<Ĳ.ʻ.Count;İ++){if(ı==0){if(!õ){if(string.IsNullOrEmpty(Ĳ.ʻ[İ]))
continue;ĵ.ų();į=Ĳ.ʻ[İ].Split(':');ľ=į[0];Į=(į.Length>1?į[1]:"");}if(!string.IsNullOrEmpty(ľ)){if(!ĵ.ϼ(ľ,Ƙ.ˍ,õ))return false;}
else{if(!ĵ.ν(Ƙ.ˍ,õ))return false;}ı++;õ=false;}if(!ń(ĵ,ĳ,Į,õ))return false;ı=0;õ=false;}return true;}string Œ(
IMyTerminalBlock ä){ϵ ő=n.ő;if(!ä.IsWorking)return Ď.Ǡ("W1");IMyProductionBlock Ő=ä as IMyProductionBlock;if(Ő!=null)if(Ő.IsProducing)
return Ď.Ǡ("W2");else return Ď.Ǡ("W3");IMyAirVent ŏ=ä as IMyAirVent;if(ŏ!=null){if(ŏ.CanPressurize)return(ŏ.GetOxygenLevel()*
100).ToString("F1")+"%";else return Ď.Ǡ("W4");}IMyGasTank Ŏ=ä as IMyGasTank;if(Ŏ!=null)return(Ŏ.FilledRatio*100).ToString(
"F1")+"%";IMyBatteryBlock ō=ä as IMyBatteryBlock;if(ō!=null)return ő.Ϟ(ō);IMyJumpDrive Ō=ä as IMyJumpDrive;if(Ō!=null)return
ő.ϙ(Ō).ToString("0.0")+"%";IMyLandingGear ŋ=ä as IMyLandingGear;if(ŋ!=null){switch((int)ŋ.LockMode){case 0:return Ď.Ǡ(
"W8");case 1:return Ď.Ǡ("W10");case 2:return Ď.Ǡ("W7");}}IMyDoor œ=ä as IMyDoor;if(œ!=null){if(œ.Status==DoorStatus.Open)
return Ď.Ǡ("W5");return Ď.Ǡ("W6");}IMyShipConnector Ŋ=ä as IMyShipConnector;if(Ŋ!=null){if(Ŋ.Status==MyShipConnectorStatus.
Unconnected)return Ď.Ǡ("W8");if(Ŋ.Status==MyShipConnectorStatus.Connected)return Ď.Ǡ("W7");else return Ď.Ǡ("W10");}IMyLaserAntenna
ŉ=ä as IMyLaserAntenna;if(ŉ!=null)return ő.ϛ(ŉ);IMyRadioAntenna ň=ä as IMyRadioAntenna;if(ň!=null)return n.ȅ(ň.Radius)+
"m";IMyBeacon Ň=ä as IMyBeacon;if(Ň!=null)return n.ȅ(Ň.Radius)+"m";IMyThrust ņ=ä as IMyThrust;if(ņ!=null&&ņ.ThrustOverride>
0)return n.ȅ(ņ.ThrustOverride)+"N";return Ď.Ǡ("W9");}int Ņ=0;bool ń(γ Ā,bool Ń,string ł,bool õ){if(!õ)Ņ=0;for(;Ņ<Ā.ϲ();Ņ
++){if(!ƒ.ȝ(20))return false;IMyTerminalBlock ä=Ā.ΰ[Ņ];string Ŵ=(Ń?(ä.IsWorking?Ď.Ǡ("W9"):Ď.Ǡ("W1")):Œ(ä));if(!string.
IsNullOrEmpty(ł)&&String.Compare(Ŵ,ł,true)!=0)continue;if(Ń)Ŵ=Œ(ä);string Ƌ=ä.CustomName;Ƌ=n.Ǜ(Ƌ,n.ɞ*0.7f);n.ĭ(Ƌ);n.Ƴ(Ŵ);}return true
;}}class ƙ:ɑ{public љ ķ=null;protected ʣ Ƙ;protected ɬ n;protected Ė j;protected Ǒ Ď;public ƙ(){Ɍ=3600;ɐ="CommandTask";}
public void Ɨ(Ė f,ʣ Ɩ){j=f;n=j.n;Ƙ=Ɩ;Ď=n.Ď;}public virtual bool ƕ(bool õ){n.Ǎ(Ď.Ǡ("UC")+": '"+Ƙ.ˌ+"'");return true;}public
override bool ɓ(bool õ){ķ=n.ǋ(ķ,j.D);if(!õ)n.Ǉ();return ƕ(õ);}}class Ɣ{Dictionary<string,string>Ɠ=new Dictionary<string,string>(
StringComparer.InvariantCultureIgnoreCase){{"ingot","ingot"},{"ore","ore"},{"component","component"},{"tool","physicalgunobject"},{
"ammo","ammomagazine"},{"oxygen","oxygencontainerobject"},{"gas","gascontainerobject"}};ȑ ƒ;ɬ n;ƅ Ƒ;ƅ Ɛ;ƅ Ə;Ƅ Ǝ;bool ƍ;public
ƅ ƌ;public Ɣ(ȑ ƚ,ɬ Ú,int J=20){Ƒ=new ƅ();Ɛ=new ƅ();Ə=new ƅ();ƍ=false;ƌ=new ƅ();ƒ=ƚ;n=Ú;Ǝ=n.Ǝ;}public void ų(){Ə.Ã();Ɛ.Ã()
;Ƒ.Ã();ƍ=false;ƌ.Ã();}public void Ơ(string Ɵ,bool Ɗ=false,int ŵ=1,int ƀ=-1){if(string.IsNullOrEmpty(Ɵ)){ƍ=true;return;}
string[]ƞ=Ɵ.Split(' ');string Î="";Ŷ ź=new Ŷ(Ɗ,ŵ,ƀ);if(ƞ.Length==2){if(!Ɠ.TryGetValue(ƞ[1],out Î))Î=ƞ[1];}string Ï=ƞ[0];if(Ɠ.
TryGetValue(Ï,out ź.Î)){Ɛ.X(ź.Î,ź);return;}n.Ǳ(ref Ï,ref Î);if(string.IsNullOrEmpty(Î)){ź.Ï=Ï;Ƒ.X(ź.Ï,ź);return;}ź.Ï=Ï;ź.Î=Î;Ə.X(Ï+
' '+Î,ź);}public Ŷ ơ(string Ñ,string Ï,string Î){Ŷ ź;ź=Ə.µ(Ñ);if(ź!=null)return ź;ź=Ƒ.µ(Ï);if(ź!=null)return ź;ź=Ɛ.µ(Î);if(
ź!=null)return ź;return null;}public bool Ɲ(string Ñ,string Ï,string Î){Ŷ ź;bool Ɯ=false;ź=Ɛ.µ(Î);if(ź!=null){if(ź.Ɗ)
return true;Ɯ=true;}ź=Ƒ.µ(Ï);if(ź!=null){if(ź.Ɗ)return true;Ɯ=true;}ź=Ə.µ(Ñ);if(ź!=null){if(ź.Ɗ)return true;Ɯ=true;}return!(ƍ
||Ɯ);}public Ŷ ƛ(string Ñ,string Ï,string Î){Ŷ ŷ=new Ŷ();Ŷ ź=ơ(Ñ,Ï,Î);if(ź!=null){ŷ.ŵ=ź.ŵ;ŷ.ƀ=ź.ƀ;}ŷ.Ï=Ï;ŷ.Î=Î;ƌ.X(Ñ,ŷ);
return ŷ;}public Ŷ ſ(string Ñ,string Ï,string Î){Ŷ ŷ=ƌ.µ(Ñ);if(ŷ==null)ŷ=ƛ(Ñ,Ï,Î);return ŷ;}int ž=0;List<Ŷ>Ž;public List<Ŷ>ż(
string Î,bool õ,Func<Ŷ,bool>Ż=null){if(!õ){Ž=new List<Ŷ>();ž=0;}for(;ž<ƌ.º();ž++){if(!ƒ.ȝ(5))return null;Ŷ ź=ƌ.z(ž);if(Ɲ(ź.Ï+
' '+ź.Î,ź.Ï,ź.Î))continue;if((string.Compare(ź.Î,Î,true)==0)&&(Ż==null||Ż(ź)))Ž.Add(ź);}return Ž;}int Ź=0;public bool Ÿ(
bool õ){if(!õ){Ź=0;}for(;Ź<Ǝ.A.Count;Ź++){if(!ƒ.ȝ(10))return false;Í À=Ǝ.ƃ[Ǝ.A[Ź]];if(!À.Ç)continue;string Ñ=À.Ì+' '+À.Ë;if(
Ɲ(Ñ,À.Ì,À.Ë))continue;Ŷ ŷ=ſ(Ñ,À.Ì,À.Ë);if(ŷ.ƀ==-1)ŷ.ƀ=À.Ê;}return true;}}class Ŷ{public int ŵ;public int ƀ;public string
Ï="";public string Î="";public bool Ɗ;public double Ɖ;public Ŷ(bool ƈ=false,int Ƈ=1,int Ɔ=-1){ŵ=Ƈ;Ɗ=ƈ;ƀ=Ɔ;}}class ƅ{
Dictionary<string,Ŷ>Ł=new Dictionary<string,Ŷ>(StringComparer.InvariantCultureIgnoreCase);List<string>A=new List<string>();public
void X(string ª,Ŷ À){if(!Ł.ContainsKey(ª)){A.Add(ª);Ł.Add(ª,À);}}public int º(){return Ł.Count;}public Ŷ µ(string ª){if(Ł.
ContainsKey(ª))return Ł[ª];return null;}public Ŷ z(int w){return Ł[A[w]];}public void Ã(){A.Clear();Ł.Clear();}public void v(){A.
Sort();}}class Ƅ{public Dictionary<string,Í>ƃ=new Dictionary<string,Í>(StringComparer.InvariantCultureIgnoreCase);Dictionary
<string,Í>Ƃ=new Dictionary<string,Í>(StringComparer.InvariantCultureIgnoreCase);public List<string>A=new List<string>();
public Dictionary<string,Í>Ɓ=new Dictionary<string,Í>(StringComparer.InvariantCultureIgnoreCase);public void ĭ(string Ï,string
Î,int Ô,string Ó,string Ò,bool Ç){if(Î=="Ammo")Î="AmmoMagazine";else if(Î=="Tool")Î="PhysicalGunObject";string Ñ=Ï+' '+Î;
Í À=new Í(Ï,Î,Ô,Ó,Ò,Ç);ƃ.Add(Ñ,À);if(!Ƃ.ContainsKey(Ï))Ƃ.Add(Ï,À);if(Ò!="")Ɓ.Add(Ò,À);A.Add(Ñ);}public Í Ð(string Ï="",
string Î=""){if(ƃ.ContainsKey(Ï+" "+Î))return ƃ[Ï+" "+Î];if(string.IsNullOrEmpty(Î)){Í À=null;Ƃ.TryGetValue(Ï,out À);return À;
}if(string.IsNullOrEmpty(Ï))for(int E=0;E<ƃ.Count;E++){Í À=ƃ[A[E]];if(string.Compare(Î,À.Ë,true)==0)return À;}return null
;}}class Í{public string Ì;public string Ë;public int Ê;public string É;public string È;public bool Ç;public Í(string Æ,
string Õ,int Å=0,string Ö="",string ê="",bool è=true){Ì=Æ;Ë=Õ;Ê=Å;É=Ö;È=ê;Ç=è;}}class ç{ɬ n=null;public C æ=new C();public ŝ å
;public IMyTerminalBlock ä;public IMyTextSurface ã;public int â=0;public int á=0;public string à="";public string é="";
public bool ß=true;public IMyTextSurface Ý=>(Û?ã:ä as IMyTextSurface);public int Ü=>(Û?(n.ƿ(ä)?0:1):æ.º());public bool Û=false
;public ç(ɬ Ú,string Ù){n=Ú;é=Ù;}public ç(ɬ Ú,string Ù,IMyTerminalBlock Ø,IMyTextSurface F,int Þ){n=Ú;é=Ù;ä=Ø;ã=F;â=Þ;Û=
true;}public bool Ä(){return å.ť()>å.Ś||å.ř!=0;}float r=1.0f;bool U=false;public float S(){if(U)return r;U=true;return r;}
bool R=false;public void Q(){if(R)return;if(!Û){æ.v();ä=æ.z(0);}int P=ä.CustomName.IndexOf("!MARGIN:");if(P<0||P+8>=ä.
CustomName.Length){á=1;à=" ";}else{string O=ä.CustomName.Substring(P+8);int N=O.IndexOf(" ");if(N>=0)O=O.Substring(0,N);if(!int.
TryParse(O,out á))á=1;à=new String(' ',á);}if(ä.CustomName.Contains("!NOSCROLL"))ß=false;else ß=true;R=true;}public void L(ŝ K=
null){if(å==null||ä==null)return;if(K==null)K=å;if(!Û){IMyTextSurface F=ä as IMyTextSurface;if(F!=null){float J=F.FontSize;
string I=F.Font;for(int E=0;E<æ.º();E++){IMyTextSurface D=æ.z(E)as IMyTextSurface;if(D==null)continue;D.Alignment=VRage.Game.
GUI.TextPanel.TextAlignment.LEFT;D.FontSize=J;D.Font=I;string H=K.ů(E).ɔ();D.ContentType=VRage.Game.GUI.TextPanel.
ContentType.TEXT_AND_IMAGE;D.WriteText(H);}}}else{ã.Alignment=VRage.Game.GUI.TextPanel.TextAlignment.LEFT;ã.ContentType=VRage.Game.
GUI.TextPanel.ContentType.TEXT_AND_IMAGE;ã.WriteText(K.ů().ɔ());}R=false;}public void G(){if(ä==null)return;if(Û){ã.
WriteText("");return;}IMyTextSurface F=ä as IMyTextSurface;if(F==null)return;for(int E=0;E<æ.º();E++){IMyTextSurface D=æ.z(E)as
IMyTextSurface;if(D==null)continue;D.WriteText("");}}}class C{Dictionary<string,IMyTerminalBlock>B=new Dictionary<string,
IMyTerminalBlock>();Dictionary<IMyTerminalBlock,string>V=new Dictionary<IMyTerminalBlock,string>();List<string>A=new List<string>();
public void X(string ª,IMyTerminalBlock À){if(!A.Contains(ª)){A.Add(ª);B.Add(ª,À);V.Add(À,ª);}}public void Â(string ª){if(A.
Contains(ª)){A.Remove(ª);V.Remove(B[ª]);B.Remove(ª);}}public void Á(IMyTerminalBlock À){if(V.ContainsKey(À)){A.Remove(V[À]);B.
Remove(V[À]);V.Remove(À);}}public int º(){return B.Count;}public IMyTerminalBlock µ(string ª){if(A.Contains(ª))return B[ª];
return null;}public IMyTerminalBlock z(int w){return B[A[w]];}public void Ã(){A.Clear();B.Clear();V.Clear();}public void v(){A
.Sort();}}class o:ɑ{public ɬ n;public ç k;Ė j;public o(Ė f){j=f;n=j.n;k=j.D;Ɍ=0.5;ɐ="PanelDisplay";}double e=0;public
void b(){e=0;}int a=0;int Z=0;bool W=true;double Y=double.MaxValue;int ë=0;public override bool ɓ(bool õ){ƙ Ę;if(!õ&&(j.ę==
false||j.Ē==null||j.Ē.Count<=0))return true;if(j.đ.Ğ>3)return Ɇ(0);if(!õ){Z=0;W=false;Y=double.MaxValue;ë=0;}if(ë==0){while(Z
<j.Ē.Count){if(!ƒ.ȝ(5))return false;if(j.ē.TryGetValue(j.Ē[Z],out Ę)){if(!Ę.ɉ)return Ɇ(Ę.ɏ-ƒ.ȍ+0.001);if(Ę.Ɏ>e)W=true;if(
Ę.ɏ<Y)Y=Ę.ɏ;}Z++;}ë++;õ=false;}double ė=Y-ƒ.ȍ+0.001;if(!W&&!k.Ä())return Ɇ(ė);n.Ǐ(k.å,k);if(W){if(!õ){e=ƒ.ȍ;k.å.ų();a=0;}
while(a<j.Ē.Count){if(!ƒ.ȝ(7))return false;if(!j.ē.TryGetValue(j.Ē[a],out Ę)){k.å.Ŗ.Add(n.ǋ(null,k));n.Ǉ();n.Ǎ(
"ERR: No cmd task ("+j.Ē[a]+")");a++;continue;}k.å.ŧ(Ę.ķ);a++;}}n.ǰ(k);j.đ.Ğ++;if(Ɍ<ė&&!k.Ä())return Ɇ(ė);return true;}}class Ė:ɑ{public ɬ n
;public ç D;public o ĕ=null;string Ĕ="N/A";public Dictionary<string,ƙ>ē=new Dictionary<string,ƙ>();public List<string>Ē=
null;public Ġ đ;public bool ę{get{return đ.ú;}}public Ė(Ġ Đ,ç ď){Ɍ=5;D=ď;đ=Đ;n=Đ.n;ɐ="PanelProcess";}Ǒ Ď;public override
void Ƀ(){Ď=n.Ď;}ʣ č=null;ƙ Č(string ċ,bool õ){if(!õ)č=new ʣ(ƒ);if(!č.ʚ(ċ,õ))return null;ƙ Ċ=č.ˈ();Ċ.Ɨ(this,č);ƒ.Ȗ(Ċ,0);
return Ċ;}string ĉ="";void Ĉ(){try{ĉ=D.ä.Ǧ(D.â,n.ɦ);}catch{ĉ="";return;}ĉ=ĉ?.Replace("\\\n","");}int a=0;int Ě=0;List<string>ģ
=null;HashSet<string>Ĭ=new HashSet<string>();int Ī=0;bool ĩ(bool õ){if(!õ){char[]Ĩ={';','\n'};string ħ=ĉ.Replace("\\;",
"\f");if(ħ.StartsWith("@")){int Ħ=ħ.IndexOf("\n");if(Ħ<0){ħ="";}else{ħ=ħ.Substring(Ħ+1);}}ģ=new List<string>(ħ.Split(Ĩ,
StringSplitOptions.RemoveEmptyEntries));Ĭ.Clear();a=0;Ě=0;Ī=0;}while(a<ģ.Count){if(!ƒ.ȝ(500))return false;if(ģ[a].StartsWith("//")){ģ.
RemoveAt(a);continue;}ģ[a]=ģ[a].Replace('\f',';');if(!ē.ContainsKey(ģ[a])){if(Ī!=1)õ=false;Ī=1;ƙ Ę=Č(ģ[a],õ);if(Ę==null)return
false;õ=false;ē.Add(ģ[a],Ę);Ī=0;}if(!Ĭ.Contains(ģ[a]))Ĭ.Add(ģ[a]);a++;}if(Ē!=null){ƙ Ċ;while(Ě<Ē.Count){if(!ƒ.ȝ(7))return
false;if(!Ĭ.Contains(Ē[Ě]))if(ē.TryGetValue(Ē[Ě],out Ċ)){Ċ.Ʉ();ē.Remove(Ē[Ě]);}Ě++;}}Ē=ģ;return true;}public override void ɕ(
){if(Ē!=null){ƙ Ċ;for(int ĥ=0;ĥ<Ē.Count;ĥ++){if(ē.TryGetValue(Ē[ĥ],out Ċ))Ċ.Ʉ();}Ē=null;}if(ĕ!=null){ĕ.Ʉ();ĕ=null;}else{}
}ŝ ī=null;string Ĥ="";bool Ģ=false;public override bool ɓ(bool õ){if(D.Ü<=0){Ʉ();return true;}if(!õ){D.å=n.Ǐ(D.å,D);ī=n.Ǐ
(ī,D);Ĉ();if(ĉ==null){if(D.Û){đ.ò(D.ã,D.ä as IMyTextPanel);}else{Ʉ();}return true;}if(D.ä.CustomName!=Ĥ){Ģ=true;}else{Ģ=
false;}Ĥ=D.ä.CustomName;}if(ĉ!=Ĕ){if(!ĩ(õ))return false;if(ĉ==""){Ĕ="";if(đ.ú){if(ī.Ŗ.Count<=0)ī.Ŗ.Add(n.ǋ(null,D));else n.ǋ(
ī.Ŗ[0],D);n.Ǉ();n.Ǎ(Ď.Ǡ("H1"));bool ġ=D.ß;D.ß=false;n.ǰ(D,ī);D.ß=ġ;return true;}return this.Ɇ(2);}Ģ=true;}Ĕ=ĉ;if(ĕ!=null
&&Ģ){ƒ.Ȩ(ĕ);ĕ.b();ƒ.Ȗ(ĕ,0);}else if(ĕ==null){ĕ=new o(this);ƒ.Ȗ(ĕ,0);}return true;}}class Ġ:ɑ{const string ğ="T:!LCD!";
public int Ğ=0;public ɬ n;public û æ=new û();γ ĝ;γ Ĝ;Dictionary<ç,Ė>ě=new Dictionary<ç,Ė>();public Dictionary<IMyTextSurface,ç
>ć=new Dictionary<IMyTextSurface,ç>();public bool ú=false;Ϙ ă=null;public Ġ(ɬ Ú){Ɍ=5;n=Ú;ɐ="ProcessPanels";}public
override void Ƀ(){ĝ=new γ(ƒ,n.ő);Ĝ=new γ(ƒ,n.ő);ă=new Ϙ(n,this);}int ø=0;bool ö(bool õ){if(!õ)ø=0;if(ø==0){if(!ĝ.ν(n.ɦ,õ))return
false;ø++;õ=false;}if(ø==1){if(n.ɦ=="T:[LCD]"&&ğ!="")if(!ĝ.ν(ğ,õ))return false;ø++;õ=false;}return true;}string ô(
IMyTerminalBlock ä){int ó=ä.CustomName.IndexOf("!LINK:");if(ó>=0&&ä.CustomName.Length>ó+6){return ä.CustomName.Substring(ó+6)+' '+ä.
Position.ToString();}return ä.EntityId.ToString();}public void ò(IMyTextSurface F,IMyTextPanel D){ç k;if(F==null)return;if(!ć.
TryGetValue(F,out k))return;if(D!=null){k.æ.Á(D);}ć.Remove(F);if(k.Ü<=0||k.Û){Ė ñ;if(ě.TryGetValue(k,out ñ)){æ.Á(k.é);ě.Remove(k);ñ
.Ʉ();}}}void ð(IMyTerminalBlock ä){IMyTextSurfaceProvider ï=ä as IMyTextSurfaceProvider;IMyTextSurface F=ä as
IMyTextSurface;if(F!=null){ò(F,ä as IMyTextPanel);return;}if(ï==null)return;for(int E=0;E<ï.SurfaceCount;E++){F=ï.GetSurface(E);ò(F,
null);}}string Ù;string î;bool í;int ù=0;int ì=0;public override bool ɓ(bool õ){if(!õ){ĝ.ų();ù=0;ì=0;}if(!ö(õ))return false;
while(ù<ĝ.ϲ()){if(!ƒ.ȝ(20))return false;IMyTerminalBlock ä=(ĝ.ΰ[ù]as IMyTerminalBlock);if(ä==null||!ä.IsWorking){ĝ.ΰ.RemoveAt
(ù);continue;}IMyTextSurfaceProvider ï=ä as IMyTextSurfaceProvider;IMyTextSurface F=ä as IMyTextSurface;IMyTextPanel D=ä
as IMyTextPanel;ç k;Ù=ô(ä);string[]ą=Ù.Split(' ');î=ą[0];í=ą.Length>1;if(D!=null){if(ć.ContainsKey(F)){k=ć[F];if(k.é==Ù+
"@0"||(í&&k.é==î)){ù++;continue;}ð(ä);}if(!í){k=new ç(n,Ù+"@0",ä,F,0);Ė ñ=new Ė(this,k);ƒ.Ȗ(ñ,0);ě.Add(k,ñ);æ.X(k.é,k);ć.Add
(F,k);ù++;continue;}k=æ.µ(î);if(k==null){k=new ç(n,î);æ.X(î,k);Ė ñ=new Ė(this,k);ƒ.Ȗ(ñ,0);ě.Add(k,ñ);}k.æ.X(Ù,ä);ć.Add(F,
k);}else{if(ï==null){ù++;continue;}for(int E=0;E<ï.SurfaceCount;E++){F=ï.GetSurface(E);if(ć.ContainsKey(F)){k=ć[F];if(k.é
==Ù+'@'+E.ToString()){continue;}ò(F,null);}if(ä.Ǧ(E,n.ɦ)==null)continue;k=new ç(n,Ù+"@"+E.ToString(),ä,F,E);Ė ñ=new Ė(this
,k);ƒ.Ȗ(ñ,0);ě.Add(k,ñ);æ.X(k.é,k);ć.Add(F,k);}}ù++;}while(ì<Ĝ.ϲ()){if(!ƒ.ȝ(300))return false;IMyTerminalBlock ä=Ĝ.ΰ[ì];
if(ä==null)continue;if(!ĝ.ΰ.Contains(ä)){ð(ä);}ì++;}Ĝ.ų();Ĝ.ϴ(ĝ);if(!ă.Ɋ&&ă.Ϫ())ƒ.Ȗ(ă,0);return true;}public bool Ć(string
Ą){if(string.Compare(Ą,"clear",true)==0){ă.ϰ();if(!ă.Ɋ)ƒ.Ȗ(ă,0);return true;}if(string.Compare(Ą,"boot",true)==0){ă.ϗ=0;
if(!ă.Ɋ)ƒ.Ȗ(ă,0);return true;}if(Ą.Ǭ("scroll")){η Ă=new η(n,this,Ą);ƒ.Ȗ(Ă,0);return true;}if(string.Compare(Ą,"props",true
)==0){ϵ ā=n.ő;List<IMyTerminalBlock>Ā=new List<IMyTerminalBlock>();List<ITerminalAction>ÿ=new List<ITerminalAction>();
List<ITerminalProperty>þ=new List<ITerminalProperty>();IMyTextPanel ý=ƒ.ǂ.GridTerminalSystem.GetBlockWithName("DEBUG")as
IMyTextPanel;if(ý==null){return true;}ý.WriteText("Properties: ");foreach(var À in ā.Є){ý.WriteText(À.Key+" =============="+"\n",
true);À.Value(Ā,null);if(Ā.Count<=0){ý.WriteText("No blocks\n",true);continue;}Ā[0].GetProperties(þ,(k)=>{return k.Id!=
"Name"&&k.Id!="OnOff"&&!k.Id.StartsWith("Show");});foreach(var ü in þ){ý.WriteText("P "+ü.Id+" "+ü.TypeName+"\n",true);}þ.
Clear();Ā.Clear();}}return false;}}class û{Dictionary<string,ç>Ł=new Dictionary<string,ç>();List<string>A=new List<string>();
public void X(string ª,ç À){if(!Ł.ContainsKey(ª)){A.Add(ª);Ł.Add(ª,À);}}public int º(){return Ł.Count;}public ç µ(string ª){if
(Ł.ContainsKey(ª))return Ł[ª];return null;}public ç z(int w){return Ł[A[w]];}public void Á(string ª){Ł.Remove(ª);A.Remove
(ª);}public void Ã(){A.Clear();Ł.Clear();}public void v(){A.Sort();}}class Ȳ{ȑ ƒ;ɬ n;public MyDefinitionId Ȱ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Electricity");public MyDefinitionId ȯ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Oxygen");public MyDefinitionId Ȯ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Hydrogen");public Ȳ(ȑ ƚ,ɬ Ú){ƒ=ƚ;n=Ú;}int
ȭ=0;public bool Ȭ(List<IMyTerminalBlock>Ā,ref double ȱ,ref double ȳ,ref double Ȼ,ref double Ⱥ,ref double Ɂ,ref double ɀ,
bool õ){if(!õ)ȭ=0;MyResourceSinkComponent ȗ;MyResourceSourceComponent ȹ;for(;ȭ<Ā.Count;ȭ++){if(!ƒ.ȝ(8))return false;if(Ā[ȭ].
Components.TryGet<MyResourceSinkComponent>(out ȗ)){ȱ+=ȗ.CurrentInputByType(Ȱ);ȳ+=ȗ.MaxRequiredInputByType(Ȱ);}if(Ā[ȭ].Components.
TryGet<MyResourceSourceComponent>(out ȹ)){Ȼ+=ȹ.CurrentOutputByType(Ȱ);Ⱥ+=ȹ.MaxOutputByType(Ȱ);}IMyBatteryBlock ȿ=(Ā[ȭ]as
IMyBatteryBlock);Ɂ+=ȿ.CurrentStoredPower;ɀ+=ȿ.MaxStoredPower;}return true;}int Ⱦ=0;public bool Ƚ(List<IMyTerminalBlock>Ā,MyDefinitionId
ȼ,ref double ȱ,ref double ȳ,ref double Ȼ,ref double Ⱥ,bool õ){if(!õ)Ⱦ=0;MyResourceSinkComponent ȗ;
MyResourceSourceComponent ȹ;for(;Ⱦ<Ā.Count;Ⱦ++){if(!ƒ.ȝ(6))return false;if(Ā[Ⱦ].Components.TryGet<MyResourceSinkComponent>(out ȗ)){ȱ+=ȗ.
CurrentInputByType(ȼ);ȳ+=ȗ.MaxRequiredInputByType(ȼ);}if(Ā[Ⱦ].Components.TryGet<MyResourceSourceComponent>(out ȹ)){Ȼ+=ȹ.
CurrentOutputByType(ȼ);Ⱥ+=ȹ.MaxOutputByType(ȼ);}}return true;}int ȸ=0;public bool ȷ(List<IMyTerminalBlock>Ā,string ȶ,ref double ȵ,ref
double ȴ,bool õ){if(!õ){ȸ=0;ȴ=0;ȵ=0;}MyResourceSinkComponent ȗ;for(;ȸ<Ā.Count;ȸ++){if(!ƒ.ȝ(30))return false;IMyGasTank Ŏ=Ā[ȸ]
as IMyGasTank;if(Ŏ==null)continue;double ȕ=0;if(Ŏ.Components.TryGet<MyResourceSinkComponent>(out ȗ)){ListReader<
MyDefinitionId>Ȕ=ȗ.AcceptedResources;int E=0;for(;E<Ȕ.Count;E++){if(string.Compare(Ȕ[E].SubtypeId.ToString(),ȶ,true)==0){ȕ=Ŏ.Capacity;
ȴ+=ȕ;ȵ+=ȕ*Ŏ.FilledRatio;break;}}}}return true;}public string ȓ(TimeSpan Ȓ){string ķ="";if(Ȓ.Ticks<=0)return"-";if((int)Ȓ.
TotalDays>0)ķ+=(long)Ȓ.TotalDays+" "+n.Ď.Ǡ("C5")+" ";if(Ȓ.Hours>0||ķ!="")ķ+=Ȓ.Hours+"h ";if(Ȓ.Minutes>0||ķ!="")ķ+=Ȓ.Minutes+"m ";
return ķ+Ȓ.Seconds+"s";}}class ȑ{public const double Ȑ=0.05;public const int ȏ=1000;public const int Ȏ=10000;public double ȍ{
get{return ȋ;}}int Ȍ=ȏ;double ȋ=0;List<ɑ>Ȋ=new List<ɑ>(100);public MyGridProgram ǂ;public bool ȉ=false;int Ȉ=0;public ȑ(
MyGridProgram ƻ,int ƺ=1,bool ȇ=false){ǂ=ƻ;Ȉ=ƺ;ȉ=ȇ;}public void Ȗ(ɑ ñ,double Ș,bool Ȫ=false){ñ.Ɋ=true;ñ.ɇ(this);if(Ȫ){ñ.ɏ=ȍ;Ȋ.Insert(0
,ñ);return;}if(Ș<=0)Ș=0.001;ñ.ɏ=ȍ+Ș;for(int E=0;E<Ȋ.Count;E++){if(Ȋ[E].ɏ>ñ.ɏ){Ȋ.Insert(E,ñ);return;}if(ñ.ɏ-Ȋ[E].ɏ<Ȑ)ñ.ɏ=Ȋ
[E].ɏ+Ȑ;}Ȋ.Add(ñ);}public void Ȩ(ɑ ñ){if(Ȋ.Contains(ñ)){Ȋ.Remove(ñ);ñ.Ɋ=false;}}public void Ȧ(ʋ ȧ,int Ȥ=1){if(Ȉ==Ȥ)ǂ.Echo
(ȧ.ɔ());}public void Ȧ(string ȥ,int Ȥ=1){if(Ȉ==Ȥ)ǂ.Echo(ȥ);}const double ȣ=(16.66666666/16);double ȩ=0;public void Ȣ(){ȩ
+=ǂ.Runtime.TimeSinceLastRun.TotalSeconds*ȣ;}ʋ ƫ=new ʋ();public void ȡ(){double Ƞ=ǂ.Runtime.TimeSinceLastRun.TotalSeconds*
ȣ+ȩ;ȩ=0;ȋ+=Ƞ;Ȍ=(int)Math.Min((Ƞ*60)*ȏ/(ȉ?5:1),Ȏ-1000);while(Ȋ.Count>=1){ɑ ñ=Ȋ[0];if(Ȍ-ǂ.Runtime.CurrentInstructionCount<=
0)break;if(ñ.ɏ>ȋ){int ȟ=(int)(60*(ñ.ɏ-ȋ));if(ȟ>=100){ǂ.Runtime.UpdateFrequency=UpdateFrequency.Update100;}else{if(ȟ>=10||
ȉ)ǂ.Runtime.UpdateFrequency=UpdateFrequency.Update10;else ǂ.Runtime.UpdateFrequency=UpdateFrequency.Update1;}break;}Ȋ.
Remove(ñ);if(!ñ.Ʌ())break;}}public int Ȟ(){return(Ȏ-ǂ.Runtime.CurrentInstructionCount);}public bool ȝ(int Ȝ){return((Ȍ-ǂ.
Runtime.CurrentInstructionCount)>=Ȝ);}public void ț(){Ȧ(ƫ.ų().ʅ("Remaining Instr: ").ʅ(Ȟ()));}}class Ț:ɑ{MyShipVelocities ȫ;
public Vector3D ɂ{get{return ȫ.LinearVelocity;}}public Vector3D ɭ{get{return ȫ.AngularVelocity;}}double ʃ=0;public double ʁ{
get{if(ɲ!=null)return ɲ.GetShipSpeed();else return ʃ;}}double ʀ=0;public double ɿ{get{return ʀ;}}double ɾ=0;public double ɽ
{get{return ɾ;}}double ɼ=0;double ɻ=0;public double ɺ{get{return ɼ;}}MyShipMass ɹ;public double ɸ{get{return ɹ.TotalMass;
}}public double ʂ{get{return ɹ.BaseMass;}}double ɷ=double.NaN;public double ɵ{get{return ɷ;}}double ɴ=double.NaN;public
double ɳ{get{return ɴ;}}IMyShipController ɲ=null;IMySlimBlock ɱ=null;public IMyShipController ɰ{get{return ɲ;}}Vector3D ɯ;
public Ț(ȑ ƚ){ɐ="ShipMgr";ƒ=ƚ;ɯ=ƒ.ǂ.Me.GetPosition();Ɍ=0.5;}List<IMyTerminalBlock>ɮ=new List<IMyTerminalBlock>();int ɶ=0;
public override bool ɓ(bool õ){if(!õ){ɮ.Clear();ƒ.ǂ.GridTerminalSystem.GetBlocksOfType<IMyShipController>(ɮ);ɶ=0;if(ɲ!=null&&ɲ
.CubeGrid.GetCubeBlock(ɲ.Position)!=ɱ)ɲ=null;}if(ɮ.Count>0){for(;ɶ<ɮ.Count;ɶ++){if(!ƒ.ȝ(20))return false;
IMyShipController ʇ=ɮ[ɶ]as IMyShipController;if(ʇ.IsMainCockpit||ʇ.IsUnderControl){ɲ=ʇ;ɱ=ʇ.CubeGrid.GetCubeBlock(ʇ.Position);if(ʇ.
IsMainCockpit){ɶ=ɮ.Count;break;}}}if(ɲ==null){ɲ=ɮ[0]as IMyShipController;ɱ=ɲ.CubeGrid.GetCubeBlock(ɲ.Position);}ɹ=ɲ.CalculateShipMass
();if(!ɲ.TryGetPlanetElevation(MyPlanetElevation.Sealevel,out ɷ))ɷ=double.NaN;if(!ɲ.TryGetPlanetElevation(
MyPlanetElevation.Surface,out ɴ))ɴ=double.NaN;ȫ=ɲ.GetShipVelocities();}double ʌ=ʃ;ʃ=ɂ.Length();ʀ=(ʃ-ʌ)/ɍ;if(-ʀ>ɾ)ɾ=-ʀ;if(-ʀ>ɼ){ɼ=-ʀ;ɻ=ƒ.ȍ
;}if(ƒ.ȍ-ɻ>5&&-ʀ>0.1)ɼ-=(ɼ+ʀ)*0.3f;return true;}}class ʋ{public StringBuilder ƫ;public ʋ(int ʊ=0){ƫ=new StringBuilder(ʊ);
}public int ʉ{get{return ƫ.Length;}}public ʋ ų(){ƫ.Clear();return this;}public ʋ ʅ(string ħ){ƫ.Append(ħ);return this;}
public ʋ ʅ(double ʈ){ƫ.Append(ʈ);return this;}public ʋ ʅ(char ǯ){ƫ.Append(ǯ);return this;}public ʋ ʅ(ʋ ʆ){ƫ.Append(ʆ.ƫ);return
this;}public ʋ ʅ(string ħ,int Ǿ,int ɒ){ƫ.Append(ħ,Ǿ,ɒ);return this;}public ʋ ʅ(char ǯ,int Š){ƫ.Append(ǯ,Š);return this;}
public ʋ ʄ(int Ǿ,int ɒ){ƫ.Remove(Ǿ,ɒ);return this;}public string ɔ(){return ƫ.ToString();}public string ɔ(int Ǿ,int ɒ){return
ƫ.ToString(Ǿ,ɒ);}public char this[int ª]{get{return ƫ[ª];}}}class ɑ{public string ɐ="MMTask";public double ɏ=0;public
double Ɏ=0;public double ɍ=0;public double Ɍ=-1;double ɋ=-1;public bool Ɋ=false;public bool ɉ=false;double Ɉ=0;protected ȑ ƒ;
public void ɇ(ȑ ƚ){ƒ=ƚ;if(ƒ.ȉ){if(ɋ==-1){ɋ=Ɍ;Ɍ*=2;}else{Ɍ=ɋ*2;}}else{if(ɋ!=-1){Ɍ=ɋ;ɋ=-1;}}}protected bool Ɇ(double Ș){Ɉ=Math.
Max(Ș,0.0001);return true;}public bool Ʌ(){if(Ɏ>0){ɍ=ƒ.ȍ-Ɏ;ƒ.Ȧ((ɉ?"Running":"Resuming")+" task: "+ɐ);ɉ=ɓ(!ɉ);}else{ɍ=0;ƒ.Ȧ(
"Init task: "+ɐ);Ƀ();ƒ.Ȧ("Running..");ɉ=ɓ(false);if(!ɉ)Ɏ=0.001;}if(ɉ){Ɏ=ƒ.ȍ;if((Ɍ>=0||Ɉ>0)&&Ɋ)ƒ.Ȗ(this,(Ɉ>0?Ɉ:Ɍ));else{Ɋ=false;Ɏ=0;}}
else{if(Ɋ)ƒ.Ȗ(this,0,true);}ƒ.Ȧ("Task "+(ɉ?"":"NOT ")+"finished. "+(Ɋ?(Ɉ>0?"Postponed by "+Ɉ.ToString("F1")+"s":
"Scheduled after "+Ɍ.ToString("F1")+"s"):"Stopped."));Ɉ=0;return ɉ;}public void Ʉ(){ƒ.Ȩ(this);ɕ();Ɋ=false;ɉ=false;Ɏ=0;}public virtual void
Ƀ(){}public virtual bool ɓ(bool õ){return true;}public virtual void ɕ(){}}class ɬ{public const float ɫ=512;public const
float ɪ=ɫ/0.7783784f;public const float ɩ=ɫ/0.7783784f;public const float ɨ=ɪ;public const float ɧ=37;public string ɦ=
"T:[LCD]";public int ɥ=1;public bool ɤ=true;public List<string>ɣ=null;public bool ɢ=true;public int Ȉ=0;public float ɡ=1.0f;
public float ɠ=1.0f;public float ɟ{get{return ɨ*ƹ.ј;}}public float ɞ{get{return(float)ɟ-2*ɖ[Ƹ]*á;}}string ɝ;string ɜ;float ɛ=-
1;Dictionary<string,float>ɚ=new Dictionary<string,float>(2);Dictionary<string,float>ə=new Dictionary<string,float>(2);
Dictionary<string,float>ɘ=new Dictionary<string,float>(2);public float ɗ{get{return ɘ[Ƹ];}}Dictionary<string,float>ɖ=new
Dictionary<string,float>(2);Dictionary<string,float>ș=new Dictionary<string,float>(2);Dictionary<string,float>Ȇ=new Dictionary<
string,float>(2);int á=0;string à="";Dictionary<string,char>ǆ=new Dictionary<string,char>(2);Dictionary<string,char>ǅ=new
Dictionary<string,char>(2);Dictionary<string,char>Ǆ=new Dictionary<string,char>(2);Dictionary<string,char>ǃ=new Dictionary<string,
char>(2);public ȑ ƒ;public MyGridProgram ǂ;public Ȳ ǁ;public ϵ ő;public Ț ǀ;public Ƅ Ǝ;public Ǒ Ď;public
IMyGridTerminalSystem ƾ{get{return ǂ.GridTerminalSystem;}}public IMyProgrammableBlock ƽ{get{return ǂ.Me;}}public Action<string>Ƽ{get{return ǂ
.Echo;}}public ɬ(MyGridProgram ƻ,int ƺ,ȑ ƚ){ƒ=ƚ;Ȉ=ƺ;ǂ=ƻ;Ď=new Ǒ();ǁ=new Ȳ(ƚ,this);ő=new ϵ(ƚ,this);ő.Ѓ();ǀ=new Ț(ƒ);ƒ.Ȗ(ǀ,
0);}љ ƹ=null;public string Ƹ{get{return ƹ.I;}}public bool Ʒ{get{return(ƹ.ť()==0);}}public bool ƿ(IMyTerminalBlock ä){if(ä
==null||ä.WorldMatrix==MatrixD.Identity)return true;return ƾ.GetBlockWithId(ä.EntityId)==null;}public љ ǋ(љ ǐ,ç k){k.Q();
IMyTextSurface F=k.Ý;if(ǐ==null)ǐ=new љ(this);ǐ.I=F.Font;if(!ɖ.ContainsKey(ǐ.I))ǐ.I=ɝ;ǐ.ј=(F.SurfaceSize.X/F.TextureSize.X)*(F.
TextureSize.X/F.TextureSize.Y)*ɡ/F.FontSize*(100f-F.TextPadding*2)/100;à=k.à;á=k.á;ƹ=ǐ;return ǐ;}public ŝ Ǐ(ŝ å,ç k){k.Q();
IMyTextSurface F=k.Ý;if(å==null)å=new ŝ(this);å.Ŕ(k.Ü);å.Ŝ=k.S()*(F.SurfaceSize.Y/F.TextureSize.Y)*ɠ/F.FontSize*(100f-F.TextPadding*2)
/100;å.ś();à=k.à;á=k.á;return å;}public void Ǎ(){ƹ.ѕ();}public void Ǎ(ʋ ǎ){if(ƹ.ї<=0)ƹ.і(à);ƹ.і(ǎ);ƹ.ѕ();}public void Ǎ(
string ǎ){if(ƹ.ї<=0)ƹ.і(à);ƹ.ѕ(ǎ);}public void ǌ(string Ǌ){ƹ.ђ(Ǌ,à);}public void ǉ(List<ʋ>ǈ){ƹ.є(ǈ);}public void ĭ(ʋ ƪ,bool Ʋ=
true){if(ƹ.ї<=0)ƹ.і(à);ƹ.і(ƪ);if(Ʋ)ƹ.ї+=ǝ(ƪ,ƹ.I);}public void ĭ(string ķ,bool Ʋ=true){if(ƹ.ї<=0)ƹ.і(à);ƹ.і(ķ);if(Ʋ)ƹ.ї+=ǝ(ķ,
ƹ.I);}public void Ƴ(ʋ ƪ,float ƨ=1.0f,float Ƨ=0f){Ʃ(ƪ,ƨ,Ƨ);ƹ.ѕ();}public void Ƴ(string ķ,float ƨ=1.0f,float Ƨ=0f){Ʃ(ķ,ƨ,Ƨ)
;ƹ.ѕ();}ʋ ƫ=new ʋ();public void Ʃ(ʋ ƪ,float ƨ=1.0f,float Ƨ=0f){float Ʀ=ǝ(ƪ,ƹ.I);float ƥ=ƨ*ɨ*ƹ.ј-ƹ.ї-Ƨ;if(á>0)ƥ-=2*ɖ[ƹ.I]*
á;if(ƥ<Ʀ){ƹ.і(ƪ);ƹ.ї+=Ʀ;return;}ƥ-=Ʀ;int Ƥ=(int)Math.Floor(ƥ/ɖ[ƹ.I]);float ƣ=Ƥ*ɖ[ƹ.I];ƫ.ų().ʅ(' ',Ƥ).ʅ(ƪ);ƹ.і(ƫ);ƹ.ї+=ƣ+Ʀ
;}public void Ʃ(string ķ,float ƨ=1.0f,float Ƨ=0f){float Ʀ=ǝ(ķ,ƹ.I);float ƥ=ƨ*ɨ*ƹ.ј-ƹ.ї-Ƨ;if(á>0)ƥ-=2*ɖ[ƹ.I]*á;if(ƥ<Ʀ){ƹ.і
(ķ);ƹ.ї+=Ʀ;return;}ƥ-=Ʀ;int Ƥ=(int)Math.Floor(ƥ/ɖ[ƹ.I]);float ƣ=Ƥ*ɖ[ƹ.I];ƫ.ų().ʅ(' ',Ƥ).ʅ(ķ);ƹ.і(ƫ);ƹ.ї+=ƣ+Ʀ;}public void
Ƣ(ʋ ƪ){ƶ(ƪ);ƹ.ѕ();}public void Ƣ(string ķ){ƶ(ķ);ƹ.ѕ();}public void ƶ(ʋ ƪ){float Ʀ=ǝ(ƪ,ƹ.I);float Ƶ=ɨ/2*ƹ.ј-ƹ.ї;if(Ƶ<Ʀ/2){
ƹ.і(ƪ);ƹ.ї+=Ʀ;return;}Ƶ-=Ʀ/2;int Ƥ=(int)Math.Round(Ƶ/ɖ[ƹ.I],MidpointRounding.AwayFromZero);float ƣ=Ƥ*ɖ[ƹ.I];ƫ.ų().ʅ(' ',Ƥ
).ʅ(ƪ);ƹ.і(ƫ);ƹ.ї+=ƣ+Ʀ;}public void ƶ(string ķ){float Ʀ=ǝ(ķ,ƹ.I);float Ƶ=ɨ/2*ƹ.ј-ƹ.ї;if(Ƶ<Ʀ/2){ƹ.і(ķ);ƹ.ї+=Ʀ;return;}Ƶ-=Ʀ
/2;int Ƥ=(int)Math.Round(Ƶ/ɖ[ƹ.I],MidpointRounding.AwayFromZero);float ƣ=Ƥ*ɖ[ƹ.I];ƫ.ų().ʅ(' ',Ƥ).ʅ(ķ);ƹ.і(ƫ);ƹ.ї+=ƣ+Ʀ;}
public void ƴ(double ƭ,float Ƭ=1.0f,float Ƨ=0f,bool Ʋ=true){if(á>0)Ƨ+=2*á*ɖ[ƹ.I];float Ʊ=ɨ*Ƭ*ƹ.ј-ƹ.ї-Ƨ;if(Double.IsNaN(ƭ))ƭ=0;
int ư=(int)(Ʊ/ș[ƹ.I])-2;if(ư<=0)ư=2;int Ư=Math.Min((int)(ƭ*ư)/100,ư);if(Ư<0)Ư=0;if(ƹ.ї<=0)ƹ.і(à);ƫ.ų().ʅ(ǆ[ƹ.I]).ʅ(ǃ[ƹ.I],Ư
).ʅ(Ǆ[ƹ.I],ư-Ư).ʅ(ǅ[ƹ.I]);ƹ.і(ƫ);if(Ʋ)ƹ.ї+=ș[ƹ.I]*ư+2*Ȇ[ƹ.I];}public void Ʈ(double ƭ,float Ƭ=1.0f,float Ƨ=0f){ƴ(ƭ,Ƭ,Ƨ,
false);ƹ.ѕ();}public void Ǉ(){ƹ.Ǉ();}public void ǰ(ç D,ŝ K=null){D.L(K);if(D.ß)D.å.Ĺ();}public void Ƿ(string Ƕ,string ķ){
IMyTextPanel D=ǂ.GridTerminalSystem.GetBlockWithName(Ƕ)as IMyTextPanel;if(D==null)return;D.WriteText(ķ+"\n",true);}public string ǵ(
MyInventoryItem À){string Ǵ=À.Type.TypeId.ToString();Ǵ=Ǵ.Substring(Ǵ.LastIndexOf('_')+1);return À.Type.SubtypeId+" "+Ǵ;}public void ǳ(
string Ñ,out string Ï,out string Î){int š=Ñ.LastIndexOf(' ');if(š>=0){Ï=Ñ.Substring(0,š);Î=Ñ.Substring(š+1);return;}Ï=Ñ;Î="";}
public string ǲ(string Ñ){string Ï,Î;ǳ(Ñ,out Ï,out Î);return ǲ(Ï,Î);}public string ǲ(string Ï,string Î){Í À=Ǝ.Ð(Ï,Î);if(À!=
null){if(À.É.Length>0)return À.É;return À.Ì;}return System.Text.RegularExpressions.Regex.Replace(Ï,"([a-z])([A-Z])","$1 $2")
;}public void Ǳ(ref string Ï,ref string Î){Í À;if(Ǝ.Ɓ.TryGetValue(Ï,out À)){Ï=À.Ì;Î=À.Ë;return;}À=Ǝ.Ð(Ï,Î);if(À!=null){Ï=
À.Ì;if((string.Compare(Î,"Ore",true)==0)||(string.Compare(Î,"Ingot",true)==0))return;Î=À.Ë;}}public string ȅ(double Ȃ,
bool ȁ=true,char Ȅ=' '){if(!ȁ)return Ȃ.ToString("#,###,###,###,###,###,###,###,###,###");string Ȁ=" kMGTPEZY";double ǿ=Ȃ;int
Ǿ=Ȁ.IndexOf(Ȅ);var ǽ=(Ǿ<0?0:Ǿ);while(ǿ>=1000&&ǽ+1<Ȁ.Length){ǿ/=1000;ǽ++;}ƫ.ų().ʅ(Math.Round(ǿ,1,MidpointRounding.
AwayFromZero));if(ǽ>0)ƫ.ʅ(" ").ʅ(Ȁ[ǽ]);return ƫ.ɔ();}public string ȃ(double Ȃ,bool ȁ=true,char Ȅ=' '){if(!ȁ)return Ȃ.ToString(
"#,###,###,###,###,###,###,###,###,###");string Ȁ=" ktkMGTPEZY";double ǿ=Ȃ;int Ǿ=Ȁ.IndexOf(Ȅ);var ǽ=(Ǿ<0?0:Ǿ);while(ǿ>=1000&&ǽ+1<Ȁ.Length){ǿ/=1000;ǽ++;}ƫ.ų().ʅ
(Math.Round(ǿ,1,MidpointRounding.AwayFromZero));if(ǽ==1)ƫ.ʅ(" kg");else if(ǽ==2)ƫ.ʅ(" t");else if(ǽ>2)ƫ.ʅ(" ").ʅ(Ȁ[ǽ]).ʅ(
"t");return ƫ.ɔ();}public string Ǽ(double ƭ){return(Math.Floor(ƭ*10)/10).ToString("F1");}Dictionary<char,float>ǻ=new
Dictionary<char,float>();void Ǻ(string ǹ,float J){J+=1;for(int E=0;E<ǹ.Length;E++){if(J>ɚ[ɝ])ɚ[ɝ]=J;ǻ.Add(ǹ[E],J);}}public float Ǹ
(char ǯ,string I){float Ʊ;if(I==ɜ||!ǻ.TryGetValue(ǯ,out Ʊ))return ɚ[I];return Ʊ;}public float ǝ(ʋ Ǟ,string I){if(I==ɜ)
return Ǟ.ʉ*ɚ[I];float ǜ=0;for(int E=0;E<Ǟ.ʉ;E++)ǜ+=Ǹ(Ǟ[E],I);return ǜ;}public float ǝ(string ħ,string I){if(I==ɜ)return ħ.
Length*ɚ[I];float ǜ=0;for(int E=0;E<ħ.Length;E++)ǜ+=Ǹ(ħ[E],I);return ǜ;}public string Ǜ(string ķ,float ǚ){if(ǚ/ɚ[ƹ.I]>=ķ.
Length)return ķ;float Ǚ=ǝ(ķ,ƹ.I);if(Ǚ<=ǚ)return ķ;float ǘ=Ǚ/ķ.Length;ǚ-=ə[ƹ.I];int Ǘ=(int)Math.Max(ǚ/ǘ,1);if(Ǘ<ķ.Length/2){ƫ.ų
().ʅ(ķ,0,Ǘ);Ǚ=ǝ(ƫ,ƹ.I);}else{ƫ.ų().ʅ(ķ);Ǘ=ķ.Length;}while(Ǚ>ǚ&&Ǘ>1){Ǘ--;Ǚ-=Ǹ(ķ[Ǘ],ƹ.I);}if(ƫ.ʉ>Ǘ)ƫ.ʄ(Ǘ,ƫ.ʉ-Ǘ);return ƫ.ʅ(
"..").ɔ();}void ǖ(string Ǖ){ɝ=Ǖ;ǆ[ɝ]=MMStyle.BAR_START;ǅ[ɝ]=MMStyle.BAR_END;Ǆ[ɝ]=MMStyle.BAR_EMPTY;ǃ[ɝ]=MMStyle.BAR_FILL;ɚ[ɝ
]=0f;}void ǔ(string Ǔ,float ǒ){ɜ=Ǔ;ɛ=ǒ;ɚ[ɜ]=ɛ+1;ə[ɜ]=2*(ɛ+1);ǆ[ɜ]=MMStyle.BAR_MONO_START;ǅ[ɜ]=MMStyle.BAR_MONO_END;Ǆ[ɜ]=
MMStyle.BAR_MONO_EMPTY;ǃ[ɜ]=MMStyle.BAR_MONO_FILL;ɖ[ɜ]=Ǹ(' ',ɜ);ș[ɜ]=Ǹ(Ǆ[ɜ],ɜ);Ȇ[ɜ]=Ǹ(ǆ[ɜ],ɜ);ɘ[ɜ]=ǝ(" 100.0%",ɜ);}public void
ǟ(){if(ǻ.Count>0)return;
// Monospace font name, width of single character
// Change this if you want to use different (modded) monospace font
ǔ("Monospace", 24f);

// Classic/Debug font name (uses widths of characters below)
// Change this if you want to use different font name (non-monospace)
ǖ("Debug");
// Font characters width (font "aw" values here)
Ǻ("3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ", 17f);
Ǻ("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 21f);
Ǻ("#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€", 19f);
Ǻ("￥$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡", 20f);
Ǻ("！ !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙", 8f);
Ǻ("？7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ", 16f);
Ǻ("（）：《》，。、；【】(),.1:;[]ft{}·ţťŧț", 9f);
Ǻ("+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−", 18f);
Ǻ("L_vx«»ĹĻĽĿŁГгзлхчҐ–•", 15f);
Ǻ("\"-rª­ºŀŕŗř", 10f);
Ǻ("WÆŒŴ—…‰", 31f);
Ǻ("'|¦ˉ‘’‚", 6f);
Ǻ("@©®мшњ", 25f);
Ǻ("mw¼ŵЮщ", 27f);
Ǻ("/ĳтэє", 14f);
Ǻ("\\°“”„", 12f);
Ǻ("*²³¹", 11f);
Ǻ("¾æœЉ", 28f);
Ǻ("%ĲЫ", 24f);
Ǻ("MМШ", 26f);
Ǻ("½Щ", 29f);
Ǻ("ю", 23f);
Ǻ("ј", 7f);
Ǻ("љ", 22f);
Ǻ("ґ", 13f);
Ǻ("™", 30f);
// End of font characters width
        ɖ[ɝ]=Ǹ(' ',ɝ);ș[ɝ]=Ǹ(Ǆ[ɝ],ɝ);Ȇ[ɝ]=Ǹ(ǆ[ɝ],ɝ);ɘ[ɝ]=ǝ(" 100.0%",ɝ);ə[ɝ]=Ǹ('.',ɝ)*2;}}class Ǒ{public string Ǡ(string
Ǯ){return TT[Ǯ];}
readonly Dictionary<string, string> TT = new Dictionary<string, string>
{
// TRANSLATION STRINGS
// msg id, text
{ "AC1", "Acceleration:" },
// amount
{ "A1", "EMPTY" },
{ "ALT1", "Altitude:"},
{ "ALT2", "Ground:"},
{ "B1", "Booting up..." },
{ "C1", "count:" },
{ "C2", "Cargo Used:" },
{ "C3", "Invalid countdown format, use:" },
{ "C4", "EXPIRED" },
{ "C5", "days" },
// customdata
{ "CD1", "Block not found: " },
{ "CD2", "Missing block name" },
{ "D1", "You need to enter name." },
{ "D2", "No blocks found." },
{ "D3", "No damaged blocks found." },
{ "DO1", "No connectors found." }, // NEW
{ "DTU", "Invalid GPS format" },
{ "GA", "Artif."}, // (not more than 5 characters)
{ "GN", "Natur."}, // (not more than 5 characters)
{ "GT", "Total"}, // (not more than 5 characters)
{ "G1", "Total Gravity:"},
{ "G2", "Natur. Gravity:"},
{ "G3", "Artif. Gravity:"},
{ "GNC", "No cockpit!"},
{ "H1", "Write commands to Custom Data of this panel." },
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
{ "P12", "Engines" }, // NEW!
{ "P13", "Turbines" }, // NEW!
{ "PT1", "Power Time:" },
{ "PT2", "Charge Time:" },
{ "PU1", "Power Used:" },
{ "S1", "Speed:" },
{ "SM1", "Ship Mass:" },
{ "SM2", "Ship Base Mass:" },
{ "SD", "Stop Distance:" },
{ "ST", "Stop Time:" },
// text
{ "T1", "Source LCD not found: " },
{ "T2", "Missing source LCD name" },
// tanks
{ "T4", "Missing tank type. eg: 'Tanks * Hydrogen'" },
{ "T5", "No {0} tanks found." }, // {0} is tank type
{ "UC", "Unknown command" },
// occupied & dampeners
{ "SC1", "Cannot find control block." },
{ "SCD", "Dampeners: " },
{ "SCO", "Occupied: " },
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
    }
}static class ǭ{public static bool Ǭ(this string ħ,string Ǫ){return ħ.StartsWith(Ǫ,StringComparison.
InvariantCultureIgnoreCase);}public static bool ǫ(this string ħ,string Ǫ){if(ħ==null)return false;return ħ.IndexOf(Ǫ,StringComparison.
InvariantCultureIgnoreCase)>=0;}public static bool ǩ(this string ħ,string Ǫ){return ħ.EndsWith(Ǫ,StringComparison.InvariantCultureIgnoreCase);}}
static class Ǩ{public static string ǧ(this IMyTerminalBlock ä){int Ŧ=ä.CustomData.IndexOf("\n---\n");if(Ŧ<0){if(ä.CustomData.
StartsWith("---\n"))return ä.CustomData.Substring(4);return ä.CustomData;}return ä.CustomData.Substring(Ŧ+5);}public static string
Ǧ(this IMyTerminalBlock ä,int š,string ǥ){string Ǥ=ä.ǧ();string ǣ="@"+š.ToString()+" AutoLCD";string Ǣ='\n'+ǣ;int Ŧ=0;if(
!Ǥ.StartsWith(ǣ,StringComparison.InvariantCultureIgnoreCase)){Ŧ=Ǥ.IndexOf(Ǣ,StringComparison.InvariantCultureIgnoreCase);
}if(Ŧ<0){if(š==0){if(Ǥ.Length==0)return"";if(Ǥ[0]=='@')return null;Ŧ=Ǥ.IndexOf("\n@");if(Ŧ<0)return Ǥ;return Ǥ.Substring(
0,Ŧ);}else return null;}int ǡ=Ǥ.IndexOf("\n@",Ŧ+1);if(ǡ<0){if(Ŧ==0)return Ǥ;return Ǥ.Substring(Ŧ+1);}if(Ŧ==0)return Ǥ.
Substring(0,ǡ);return Ǥ.Substring(Ŧ+1,ǡ-Ŧ);}
#if DEBUG
    }
}
#endif
