/* v:2.0004 (Update for game v01.172) Fixed LCD linking, LCD_TAG can be set in Custom Data of PB, updated deprecated API stuff
* In-game script by MMaster
*
* Last Update:
*  LCD_TAG can be set in Custom Data of Programmable Block (read more in guide section 'What is LCD_TAG?')
*  Fixed LCD linking
*  Updated deprecated API stuff
*  Fix for LCD showing help text when booting
*
* MAJOR UPDATE:
* Complete rework of script core
*  - automatically updates as many LCDs as possible as fast as possible!
*  - can be used on ships of any size!
*  - never throws "Script too complex" exception!
*  - works optimally with timer block set to Trigger Now
*
* New features (check Full Guide for detailed instructions):
* All commands are now read line by line from Custom Data of LCD panel (LCD Title commands are no longer supported)
* Text command removed, use TextLCD command to display text from other LCD or combination of Echo, Center and Right commands
* Ability to add margin on left and right using "!MARGIN:<number of spaces>" (without quotes) in name of LCD
* Ability to scroll LCDs manually using scrollDown and scrollUp Programmable Block Run arguments
* "!NOSCROLL" in name of LCD disables automatic scrolling
* Easy config for monospace fonts (just set the USE_MONOSPACE value below)
* LCDs now automatically have margin of 1 space (you can override it with "!MARGIN:0" (without quotes))
* PowerUsed & PowerUsedTop command to display blocks power input stats
* Altitude & AltitudeSea command to display altitude above ground & sea
* Gravity command and its variants to display gravitational acceleration
* ShipMass & ShipMassBase command to display total/base ship mass
* StopDistance & StopTime to show estimated distance & time to completely stop ship
* SpeedKmh & SpeedMph to show speed in km per hour or miles per hour
* Distance command to show distance of specified block from specified GPS position
* DamageC command to show only blocks in construction
* Rework to use new more efficient ways to get power
* Details command now works even with modded blocks like Nanite Factory by Tyrsis
* Details command can now start output on line where the user specified text is found
* TanksX variant to show only percentage without progress bar
* Tanks and Oxygen commands universal Hydrogen & Oxygen detection for all tanks (even modded ones)
* Speed and Accel commands progress bar support
* Added config options for LCDs with exotic texture sizing (below)
* Moved items definitions higher in the script for easier access
* Right command which prints text aligned to right
* Lots of optimizations
*
* Disabled for now:
* Tanks exact values & TanksX, TanksP, TanksV variants (waiting for game support)
*
* Previous updates: Look at Change notes tab on Steam workshop page.
*
* Customize these: */

// Use this tag to identify LCDs managed by this script
// Name filtering rules can be used here so you can use even G:Group or T:[My LCD]
public string LCD_TAG = "T:[LCD]";

// How many lines to scroll per step
public const int SCROLL_LINES_PER_STEP = 1;

// if you want to use monospace (on all LCDs) change this to true
public bool USE_MONOSPACE = false;

// Enable initial boot sequence (after compile / world load)
public const bool ENABLE_BOOT = true;

/* READ THIS FULL GUIDE
http://steamcommunity.com/sharedfiles/filedetails/?id=407158161

Basic video guide
Please watch the video guide even if you don't understand my English. You can see how things are done there.

https://youtu.be/vqpPQ_20Xso


Read Change Notes (above screenshots) for latest updates and new features.
I notify about updates on twitter so follow if interested.

Please carefully read the FULL GUIDE before asking questions I had to remove guide from here to add more features :(
Please DO NOT publish this script or its derivations without my permission! Feel free to use it in blueprints!

Special Thanks
Keen Software House for awesome Space Engineers game
Malware for contributing to programmable blocks game code
Textor and CyberVic for their great script related contributions on Keen forums.

Watch Twitter: https://twitter.com/MattsPlayCorner
and Facebook: https://www.facebook.com/MattsPlayCorner1080p
for more crazy stuff from me in the future :)
*/
// Force redraw of panels? (default = false)
// true - forces redraw of panels
public const bool FORCE_REDRAW = false;

// (for developer) Enable debug
public const bool EnableDebug = true;

// (for modded lcds) Affects all LCDs managed by this programmable block
/* LCD height modifier
0.5f makes the LCD have only 1/2 the lines of normal LCD
2.0f makes it fit 2x more lines on LCD */
public const float HEIGHT_MOD = 1.0f;

/* line width modifier
0.5f moves the right edge to 50% of normal LCD width
2.0f makes it fit 200% more text on line */
public const float WIDTH_MOD = 1.0f;

// If you use custom monospace font set this to width of single character
public const float MONOSPACE_WIDTH = 24f;

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

void ItemsConf()
{
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

    // REALLY REALLY REALLY
    // DO NOT MODIFY ANYTHING BELOW THIS (TRANSLATION STRINGS ARE AT THE BOTTOM)
}
void Add(string sT, string mT, int q = 0, string dN = "", string sN = "", bool u = true) { MMItems.Add(sT, mT, q, dN, sN, u); }
tq
jo; t6 jp; tn jq = null; void jr(string a) { }
void js(string b, string d) { var e = b.ToLower(); switch (e) { case "lcd_tag": LCD_TAG = d; break; } }
void jt()
{
    string[] f = Me.CustomData.Split('\n'); for (int g = 0; g < f.Length; g++)
    {
        var i = f[g]; int j = i.IndexOf('='); if (j < 0) { jr(i); continue; }
        var k = i.Substring(0, j).Trim(); var l = i.Substring(j + 1).Trim(); js(k, l);
    }
}
void ju(tq n)
{
    ItemsConf(); jt(); if (USE_MONOSPACE) tr.su =
MONOSPACE_WIDTH; jq = new tn(this, EnableDebug, n); jq.qX = LCD_TAG; jq.qY = SCROLL_LINES_PER_STEP; jq.qZ = ENABLE_BOOT; jq.q_ = BOOT_FRAMES; jq.r0 =
FORCE_REDRAW; jq.r3 = HEIGHT_MOD; jq.r2 = WIDTH_MOD;
}
void jv() { jo.sf = this; jq.rb = this; }
void Main(string o)
{
    if (jo == null)
    {
        jo = new tq(this,
EnableDebug); ju(jo); jp = new t6(jq); jo.sh(jp, 0);
    }
    else { jv(); jq.rd.pw(); }
    if (o != "") { if (jp.nC(o)) return; }
    jp.nn = 0; jo.sk();
}
class sz : t1
{
    public sz() { r_ = 7; rW = "CmdInvList"; }
    float jw = 27; float jx = 118; public override void Init()
    {
        jI = new td(s3, mB.rd); jJ = new th(s3); jx = tr.sw(
" / 888.8 M"); jw = tr.sv(' ');
    }
    void jy(string q, double r, int u)
    {
        if (u > 0)
        {
            mB.ru(Math.Min(100, 100 * r / u), 0.3f); mB.Add(' ' + q + ' '); mB.rq(t8
.nQ(r), 1.0f, jx); mB.rm(" / " + t8.nQ(u));
        }
        else { mB.Add(q + ':'); mB.rp(t8.nQ(r), 1.0f, jw); }
    }
    void jz(string v, double w, double x, int y)
    {
        if (y
> 0)
        {
            mB.Add(v + ' '); mB.rq(t8.nQ(w), 0.51f); mB.Add(" / " + t8.nQ(y)); mB.rp("+" + t8.nQ(x) + " " + M.T["I1"], 1.0f); mB.rt(Math.Min(100, 100 * w / y))
                        ;
        }
        else { mB.Add(v + ':'); mB.rq(t8.nQ(w), 0.51f); mB.rp("+" + t8.nQ(x) + " " + M.T["I1"], 1.0f); }
    }
    List<ti> jA; int jB = 0; int jC = 0; bool jD(bool z,
bool A, string B, string C)
    {
        if (!z) { jC = 0; jB = 0; }
        if (jC == 0) { if ((jA = jJ.q6(B, z)) == null) return false; jC++; z = false; }
        if (jA.Count > 0)
        {
            if (!A && !z
) { if (mB.rj.rG > 0 && mB.rj.rF[0] != "") mB.rm(""); mB.rr("<< " + C + " " + M.T["I2"] + " >>"); }
            for (; jB < jA.Count; jB++)
            {
                if (!s3.sp(30)) return false;
                double D = jA[jB].qe; if (jN && D >= jA[jB].q9) continue; int E = jA[jB].qa; if (jN) E = jA[jB].q9; var F = t8.nO(jA[jB].qb, jA[jB].qc); jy(F, D, E);
            }
        }
        return true;
    }
    List<ti> jE; int jF = 0; int jG = 0; bool jH(bool G)
    {
        if (!G) { jF = 0; jG = 0; }
        if (jG == 0)
        {
            if ((jE = jJ.q6("Ingot", G)) == null) return false;
            jG++; G = false;
        }
        if (jE.Count > 0)
        {
            if (!jO && !G) { if (mB.rj.rG > 0 && mB.rj.rF[0] != "") mB.rm(""); mB.rr("<< " + M.T["I4"] + " " + M.T["I2"] + " >>"); }
            for (
; jF < jE.Count; jF++)
            {
                if (!s3.sp(40)) return false; double H = jE[jF].qe; if (jN && H >= jE[jF].q9) continue; int I = jE[jF].qa; if (jN) I = jE[jF].q9;
                var J = t8.nO(jE[jF].qb, jE[jF].qc); if (jE[jF].qb != "Scrap") { double K = jJ.q3(jE[jF].qb + " Ore", jE[jF].qb, "Ore").qe; jz(J, H, K, I); }
                else jy(J
, H, I);
            }
        }
        return true;
    }
    td jI = null; th jJ; List<tb> jK; bool jL, jM, jN, jO; int jP, jQ; void jR()
    {
        jI.pp(); jL = mA.ol.EndsWith("x") || mA.ol.
EndsWith("xs"); jM = mA.ol.EndsWith("s") || mA.ol.EndsWith("sx"); jN = mA.ol.StartsWith("missing"); jO = mA.ol.StartsWith("invlist"); jJ.pZ();
        jK = mA.os; if (jK.Count == 0) jK.Add(new tb("all"));
    }
    bool jS(bool L)
    {
        if (!L) jP = 0; for (; jP < jK.Count; jP++)
        {
            tb N = jK[jP]; N.oC(); var O = N.oz.
ToLower(); if (!L) jQ = 0; else L = false; for (; jQ < N.oB.Count; jQ++)
            {
                if (!s3.sp(30)) return false; string[] P = N.oB[jQ].ToLower().Split(':');
                double Q; if (P[0] == "all") P[0] = ""; int R = 1; int S = -1; if (P.Length > 1)
                {
                    if (Double.TryParse(P[1], out Q))
                    {
                        if (jN) R = (int)Math.Ceiling(Q);
                        else
                            S = (int)Math.Ceiling(Q);
                    }
                }
                var U = P[0]; if (O != "") U += ' ' + O; jJ.p_(U, (N.oy == "-"), R, S);
            }
        }
        return true;
    }
    int jT = 0; int jU = 0; int jV = 0; bool jW(
bool V)
    {
        td W = jI; if (!V) jT = 0; for (; jT < W.pq(); jT++)
        {
            if (!V) jU = 0; for (; jU < W.p5[jT].InventoryCount; jU++)
            {
                if (!V) jV = 0; else V = false;
                IMyInventory X = W.p5[jT].GetInventory(jU); List<IMyInventoryItem> Y = X.GetItems(); for (; jV < Y.Count; jV++)
                {
                    if (!s3.sp(40)) return false;
                    IMyInventoryItem Z = Y[jV]; var _ = t8.nM(Z); var a0 = _.ToLower(); string a1, a2; t8.nN(a0, out a1, out a2); if (a2 == "ore")
                    {
                        if (jJ.q1(a1.ToLower(
) + " ingot", a1, "Ingot") && jJ.q1(_, a1, a2)) continue;
                    }
                    else { if (jJ.q1(_, a1, a2)) continue; }
                    t8.nN(_, out a1, out a2); ti a3 = jJ.q3(a0, a1, a2); a3.
qe += (double)Z.Amount;
                }
            }
        }
        return true;
    }
    int jX = 0; public override bool RunCmd(bool a4)
    {
        if (!a4) { jR(); jX = 0; }
        for (; jX <= 9; jX++)
        {
            switch (jX)
            {
                case 0: if (!jI.pf(mA.om, a4)) return false; break;
                case 1: if (!jS(a4)) return false; if (!jL) { if (!jJ.q8(a4)) return false; } break;
                case 2:
                    if (!
jW(a4)) return false; break;
                case 3: if (!jD(a4, jO, "Ore", M.T["I3"])) return false; break;
                case 4:
                    if (jM)
                    {
                        if (!jD(a4, jO, "Ingot", M.T["I4"]))
                            return false;
                    }
                    else { if (!jH(a4)) return false; }
                    break;
                case 5: if (!jD(a4, jO, "Component", M.T["I5"])) return false; break;
                case 6:
                    if (!jD(a4,
jO, "OxygenContainerObject", M.T["I6"])) return false; break;
                case 7: if (!jD(a4, true, "GasContainerObject", "")) return false; break;
                case 8:
                    if (!jD(a4, jO, "AmmoMagazine", M.T["I7"])) return false; break;
                case 9: if (!jD(a4, jO, "PhysicalGunObject", M.T["I8"])) return false; break;
            }
            a4 = false;
        }
        return true;
    }
}
class sA : t1
{
    td jY; public sA() { r_ = 2; rW = "CmdCargo"; }
    public override void Init() { jY = new td(s3, mB.rd); }
    bool
jZ = true; bool j_ = false; bool k0 = false; double k1 = 0; double k2 = 0; int k3 = 0; public override bool RunCmd(bool a5)
    {
        if (!a5)
        {
            jY.pp(); jZ = mA.ol
.Contains("all"); j_ = (mA.ol[mA.ol.Length - 1] == 'x'); k0 = (mA.ol[mA.ol.Length - 1] == 'p'); k1 = 0; k2 = 0; k3 = 0;
        }
        if (k3 == 0)
        {
            if (jZ)
            {
                if (!jY.pf(mA.om,
a5)) return false;
            }
            else { if (!jY.pn("cargocontainer", mA.om, a5)) return false; }
            k3++; a5 = false;
        }
        double a6 = jY.p7(ref k1, ref k2, a5); if (
Double.IsNaN(a6)) return false; mB.Add(M.T["C2"] + " "); if (!j_ && !k0)
        {
            mB.rp(t8.nQ(k1) + "L / " + t8.nQ(k2) + "L"); mB.ru(a6, 1.0f, mB.r4); mB.rm(
' ' + t8.nR(a6) + "%");
        }
        else if (k0) { mB.rp(t8.nR(a6) + "%"); mB.rt(a6); } else mB.rp(t8.nR(a6) + "%"); return true;
    }
}
class sB : t1
{
    td k4; public
sB()
    { r_ = 2; rW = "CmdMass"; }
    public override void Init() { k4 = new td(s3, mB.rd); }
    bool k5 = false; bool k6 = false; int k7 = 0; public override bool
RunCmd(bool a7)
    {
        if (!a7) { k4.pp(); k5 = (mA.ol[mA.ol.Length - 1] == 'x'); k6 = (mA.ol[mA.ol.Length - 1] == 'p'); k7 = 0; }
        if (k7 == 0)
        {
            if (!k4.pf(mA.om, a7
)) return false; k7++; a7 = false;
        }
        double a8 = k4.pa(a7); if (Double.IsNaN(a8)) return false; double a9 = 0; int aa = mA.os.Count; if (aa > 0)
        {
            double.
TryParse(mA.os[0].oA.Trim(), out a9); if (aa > 1)
            {
                var ab = mA.os[1].oA.Trim().ToLower(); if (ab != "") a9 *= Math.Pow(1000.0, "kmgtpezy".IndexOf(
ab[0]));
            }
            a9 *= 1000.0;
        }
        mB.Add(M.T["M1"] + " "); if (a9 <= 0) { mB.rp(t8.nQ(a8)); return true; }
        double ac = a8 / a9 * 100; if (!k5 && !k6)
        {
            mB.rp(t8.nQ(a8
) + "g / " + t8.nQ(a9) + "g"); mB.ru(ac, 1.0f, mB.r4); mB.rm(' ' + t8.nR(ac) + "%");
        }
        else if (k6) { mB.rp(t8.nR(ac) + "%"); mB.rt(ac); }
        else mB.rp(t8.
nR(ac) + "%"); return true;
    }
}
class sC : t1
{
    tk k8; td k9; public sC() { r_ = 3; rW = "CmdOxygen"; }
    public override void Init()
    {
        k8 = mB.rc; k9 = new td
(s3, mB.rd);
    }
    int ka = 0; int kb = 0; bool kc = false; int kd = 0; double ke = 0; double kf = 0; double kg; public override bool RunCmd(bool ad)
    {
        if (!ad
) { k9.pp(); ka = 0; kb = 0; kg = 0; }
        if (ka == 0) { if (!k9.pn("airvent", mA.om, ad)) return false; kc = (k9.pq() > 0); ka++; ad = false; }
        if (ka == 1)
        {
            for (; kb < k9.
pq(); kb++)
            {
                if (!s3.sp(8)) return false; IMyAirVent ae = k9.p5[kb] as IMyAirVent; kg = Math.Max(ae.GetOxygenLevel() * 100, 0f); mB.Add(ae.
                  CustomName); if (ae.CanPressurize) mB.rp(t8.nR(kg) + "%"); else mB.rp(M.T["O1"]); mB.rt(kg);
            }
            ka++; ad = false;
        }
        if (ka == 2)
        {
            if (!ad) k9.pp(); if (!
k9.pn("oxyfarm", mA.om, ad)) return false; kd = k9.pq(); ka++; ad = false;
        }
        if (ka == 3)
        {
            if (kd > 0)
            {
                if (!ad) kb = 0; double af = 0; for (; kb < kd; kb++)
                {
                    if (!
s3.sp(4)) return false; IMyOxygenFarm ag = k9.p5[kb] as IMyOxygenFarm; af += ag.GetOutput() * 100;
                }
                kg = af / kd; if (kc) mB.rm(""); kc |= (kd > 0); mB.
Add(M.T["O2"]); mB.rp(t8.nR(kg) + "%"); mB.rt(kg);
            }
            ka++; ad = false;
        }
        if (ka == 4)
        {
            if (!ad) k9.pp(); if (!k9.pn("oxytank", mA.om, ad)) return false;
            kd = k9.pq(); if (kd == 0) { if (!kc) mB.rm(M.T["O3"]); return true; }
            ka++; ad = false;
        }
        if (ka == 5)
        {
            if (!ad) { ke = 0; kf = 0; kb = 0; }
            if (!k8.qw(k9.p5,
"oxygen", ref kf, ref ke, ad)) return false; if (ke == 0) { if (!kc) mB.rm(M.T["O3"]); return true; }
            kg = kf / ke * 100; if (kc) mB.rm(""); mB.Add(M.T[
"O4"]); mB.rp(t8.nR(kg) + "%"); mB.rt(kg); ka++;
        }
        return true;
    }
}
class sD : t1
{
    tk kh; td ki; public sD() { r_ = 2; rW = "CmdTanks"; }
    public override
void Init()
    { kh = mB.rc; ki = new td(s3, mB.rd); }
    int kj = 0; string kk; string kl; double km = 0; double kn = 0; double ko; public override bool
RunCmd(bool ah)
    {
        List<tb> aj = mA.os; if (aj.Count == 0) { mB.rm(M.T["T4"]); return true; }
        if (!ah)
        {
            kk = (mA.ol.EndsWith("x") ? "s" : (mA.ol.EndsWith
("p") ? "p" : (mA.ol.EndsWith("v") ? "v" : "n"))); kj = 0; kl = aj[0].oA.Trim().ToLower(); ki.pp(); km = 0; kn = 0;
        }
        if (kj == 0)
        {
            if (!ki.pn("oxytank", mA.om
, ah)) return false; ah = false; kj++;
        }
        if (kj == 1) { if (!kh.qw(ki.p5, kl, ref km, ref kn, ah)) return false; ah = false; kj++; }
        if (kn == 0)
        {
            mB.rm(String
.Format(M.T["T5"], kl)); return true;
        }
        ko = km / kn * 100; kl = char.ToUpper(kl[0]) + kl.Substring(1); mB.Add(kl + " " + M.T["T6"]); switch (kk)
        {
            case
"s":
                mB.rp(' ' + t8.nR(ko) + "%"); break;
            default: mB.rp(' ' + t8.nR(ko) + "%"); mB.rt(ko); break;
        }
        return true;
    }
}
class sE : t1
{
    public sE()
    {
        r_ = 7;
        rW = "CmdPowerTime";
    }
    class sF { public TimeSpan kO = new TimeSpan(-1); public double kP = -1; public double kQ = 0; }
    sF kp = new sF(); td kq; td
kr; public override void Init() { kq = new td(s3, mB.rd); kr = new td(s3, mB.rd); }
    int ks = 0; double kt = 0; double ku = 0, kv = 0; double kw = 0, kx = 0, ky =
0; double kz = 0, kA = 0; int kB = 0; bool kC(string ak, out TimeSpan al, out double am, bool an)
    {
        MyResourceSourceComponent ao;
        MyResourceSinkComponent ap; double aq = rZ; sF ar = kp; al = ar.kO; am = ar.kP; if (!an)
        {
            kq.pp(); kr.pp(); ar.kP = 0; ks = 0; kt = 0; ku = kv = 0; kw = 0; kx = ky = 0;
            kz = kA = 0; kB = 0;
        }
        if (ks == 0) { if (!kq.pn("reactor", ak, an)) return false; an = false; ks++; }
        if (ks == 1)
        {
            for (; kB < kq.p5.Count; kB++)
            {
                if (!s3.sp(6))
                    return false; IMyReactor at = kq.p5[kB] as IMyReactor; if (at == null || !at.IsWorking) continue; if (at.Components.TryGet<
                                MyResourceSourceComponent>(out ao)) { ku += ao.CurrentOutputByType(mB.rc.qo); kv += ao.MaxOutputByType(mB.rc.qo); }
                kt += (double)at.
GetInventory(0).CurrentMass;
            }
            an = false; ks++;
        }
        if (ks == 2) { if (!kr.pn("battery", ak, an)) return false; an = false; ks++; }
        if (ks == 3)
        {
            if (!an) kB = 0
; for (; kB < kr.p5.Count; kB++)
            {
                if (!s3.sp(15)) return false; IMyBatteryBlock at = kr.p5[kB] as IMyBatteryBlock; if (at == null || !at.IsWorking)
                    continue; if (at.Components.TryGet<MyResourceSourceComponent>(out ao))
                {
                    kx = ao.CurrentOutputByType(mB.rc.qo); ky = ao.MaxOutputByType(mB.
rc.qo);
                }
                if (at.Components.TryGet<MyResourceSinkComponent>(out ap)) { kx -= ap.CurrentInputByType(mB.rc.qo); }
                double au = (kx < 0 ? (at.
MaxStoredPower - at.CurrentStoredPower) / (-kx / 3600) : 0); if (au > ar.kP) ar.kP = au; if (at.GetValueBool("Recharge")) continue; kz += kx; kA += ky; kw
+= at.CurrentStoredPower;
            }
            an = false; ks++;
        }
        double av = ku + kz; if (av <= 0) ar.kO = TimeSpan.FromSeconds(-1);
        else
        {
            double aw = ar.kO.TotalSeconds;
            double ax; double ay = (ar.kQ - kt) / aq; if (ku <= 0) ay = Math.Min(av, kv) / 3600000; double az = 0; if (kA > 0) az = Math.Min(av, kA) / 3600; if (ay <= 0 && az <= 0)
                ax = -1;
            else if (ay <= 0) ax = kw / az; else if (az <= 0) ax = kt / ay; else { double aA = az; double aB = (ku <= 0 ? av / 3600 : ay * av / ku); ax = kw / aA + kt / aB; }
            if (aw <= 0
|| ax < 0) aw = ax;
            else aw = (aw + ax) / 2; try { ar.kO = TimeSpan.FromSeconds(aw); } catch { ar.kO = TimeSpan.FromSeconds(-1); }
        }
        ar.kQ = kt; am = ar.kP; al = ar.
kO; return true;
    }
    string kD(TimeSpan aD)
    {
        var aE = ""; if (aD.Ticks <= 0) return "-"; if ((int)aD.TotalDays > 0) aE += (long)aD.TotalDays + " " + M.T[
"C5"] + " "; if (aD.Hours > 0 || aE != "") aE += aD.Hours + "h "; if (aD.Minutes > 0 || aE != "") aE += aD.Minutes + "m "; return aE + aD.Seconds + "s";
    }
    int kE = 0;
    bool kF = false; bool kG = false; double kH = 0; TimeSpan kI; int kJ = 0, kK = 0, kL = 0; int kM = 0; int kN = 0; public override bool RunCmd(bool aF)
    {
        if (!
aF) { kF = (mA.ol[mA.ol.Length - 1] == 'x'); kG = (mA.ol[mA.ol.Length - 1] == 'p'); kE = 0; kJ = kK = kL = kM = 0; kN = 0; kH = 0; }
        if (kE == 0)
        {
            if (mA.os.Count > 0)
            {
                for (
; kN < mA.os.Count; kN++)
                {
                    if (!s3.sp(100)) return false; mA.os[kN].oC(); if (mA.os[kN].oB.Count <= 0) continue; var aG = mA.os[kN].oB[0]; int.
          TryParse(aG, out kM); if (kN == 0) kJ = kM; else if (kN == 1) kK = kM; else if (kN == 2) kL = kM;
                }
            }
            kE++; aF = false;
        }
        if (kE == 1)
        {
            if (!kC(mA.om, out kI, out kH,
aF)) return false; kE++; aF = false;
        }
        if (!s3.sp(30)) return false; double aH = 0; TimeSpan aI; try { aI = new TimeSpan(kJ, kK, kL); }
        catch
        {
            aI =
TimeSpan.FromSeconds(-1);
        }
        string aJ; if (kI.TotalSeconds > 0 || kH <= 0) { mB.Add(M.T["PT1"] + " "); aJ = kD(kI); aH = kI.TotalSeconds; }
        else
        {
            mB.Add(
M.T["PT2"] + " "); aJ = kD(TimeSpan.FromSeconds(kH)); if (aI.TotalSeconds >= kH) aH = aI.TotalSeconds - kH; else aH = 0;
        }
        if (aI.Ticks <= 0)
        {
            mB.rp(aJ);
            return true;
        }
        double aK = aH / aI.TotalSeconds * 100; if (aK > 100) aK = 100; if (!kF && !kG)
        {
            mB.rp(aJ); mB.ru(aK, 1.0f, mB.r4); mB.rm(' ' + aK.ToString(
"0.0") + "%");
        }
        else if (kG) { mB.rp(aK.ToString("0.0") + "%"); mB.rt(aK); } else mB.rp(aK.ToString("0.0") + "%"); return true;
    }
}
class sG : t1
{
    public sG() { r_ = 7; rW = "CmdPowerUsed"; }
    tk kR; td kS; public override void Init() { kS = new td(s3, mB.rd); kR = mB.rc; }
    string kT; string kU;
    string kV; void kW(double aL, double aM)
    {
        double aN = (aM > 0 ? aL / aM * 100 : 0); switch (kT)
        {
            case "s": mB.rp(aN.ToString("0.0") + "%", 1.0f); break;
            case "v": mB.rp(t8.nQ(aL) + "W / " + t8.nQ(aM) + "W", 1.0f); break;
            case "c": mB.rp(t8.nQ(aL) + "W", 1.0f); break;
            case "p":
                mB.rp(aN.ToString(
"0.0") + "%", 1.0f); mB.rt(aN); break;
            default:
                mB.rp(t8.nQ(aL) + "W / " + t8.nQ(aM) + "W"); mB.ru(aN, 1.0f, mB.r4); mB.rp(' ' + aN.ToString("0.0") +
"%"); break;
        }
    }
    double kX = 0, kY = 0; int kZ = 0; int k_ = 0; sH l0 = new sH(); public override bool RunCmd(bool aO)
    {
        if (!aO)
        {
            kT = (mA.ol.EndsWith("x"
) ? "s" : (mA.ol.EndsWith("usedp") || mA.ol.EndsWith("topp") ? "p" : (mA.ol.EndsWith("v") ? "v" : (mA.ol.EndsWith("c") ? "c" : "n")))); kU = (mA.ol.
Contains("top") ? "top" : ""); kV = (mA.os.Count > 0 ? mA.os[0].oA : M.T["PU1"]); kX = kY = 0; k_ = 0; kZ = 0; kS.pp(); l0.l5();
        }
        if (k_ == 0)
        {
            if (!kS.pf(mA.om,
aO)) return false; aO = false; k_++;
        }
        MyResourceSinkComponent aP; MyResourceSourceComponent aQ; switch (kU)
        {
            case "top":
                if (k_ == 1)
                {
                    for (; kZ < kS
.p5.Count; kZ++)
                    {
                        if (!s3.sp(20)) return false; IMyTerminalBlock aR = kS.p5[kZ]; if (aR.Components.TryGet<MyResourceSinkComponent>(out aP))
                        { ListReader<MyDefinitionId> aS = aP.AcceptedResources; if (aS.IndexOf(kR.qo) < 0) continue; kX = aP.CurrentInputByType(kR.qo) * 0xF4240; }
                        else
                            continue; l0.l2(kX, aR);
                    }
                    aO = false; k_++;
                }
                if (l0.l3() <= 0) { mB.rm("PowerUsedTop: " + M.T["D2"]); return true; }
                int aT = 10; if (mA.os.Count > 0) if (
!int.TryParse(kV, out aT)) { aT = 10; }
                if (aT > l0.l3()) aT = l0.l3(); if (k_ == 2)
                {
                    if (!aO) { kZ = l0.l3() - 1; l0.l6(); }
                    for (; kZ >= l0.l3() - aT; kZ--)
                    {
                        if (!s3
.sp(30)) return false; IMyTerminalBlock aR = l0.l4(kZ); var aU = tr.sx(aR.CustomName, tn.qV * 0.4f * mB.rj.rA); if (aR.Components.TryGet<
MyResourceSinkComponent>(out aP)) { kX = aP.CurrentInputByType(kR.qo) * 0xF4240; kY = aP.MaxRequiredInputByType(kR.qo) * 0xF4240; }
                        mB.Add(aU +
" "); kW(kX, kY);
                    }
                }
                break;
            default:
                for (; kZ < kS.p5.Count; kZ++)
                {
                    if (!s3.sp(10)) return false; double aV; IMyTerminalBlock aR = kS.p5[kZ]; if (aR.
Components.TryGet<MyResourceSinkComponent>(out aP))
                    {
                        ListReader<MyDefinitionId> aS = aP.AcceptedResources; if (aS.IndexOf(kR.qo) < 0)
                            continue; aV = aP.CurrentInputByType(kR.qo); kY += aP.MaxRequiredInputByType(kR.qo);
                    }
                    else continue; if (aR.Components.TryGet<
MyResourceSourceComponent>(out aQ) && (aR as IMyBatteryBlock != null)) { aV -= aQ.CurrentOutputByType(kR.qo); if (aV <= 0) continue; }
                    kX += aV;
                }
                mB
.Add(kV); kW(kX * 0xF4240, kY * 0xF4240); break;
        }
        return true;
    }
    public class sH
    {
        List<KeyValuePair<double, IMyTerminalBlock>> l1 = new List<
KeyValuePair<double, IMyTerminalBlock>>(); public void l2(double aZ, IMyTerminalBlock a_)
        {
            l1.Add(new KeyValuePair<double,
IMyTerminalBlock>(aZ, a_));
        }
        public int l3() { return l1.Count; }
        public IMyTerminalBlock l4(int b0) { return l1[b0].Value; }
        public void l5
()
        { l1.Clear(); }
        public void l6() { l1.Sort((b1, b2) => (b1.Key.CompareTo(b2.Key))); }
    }
}
class sI : t1
{
    public sI() { r_ = 3; rW = "CmdPower"; }
    tk l7
; td l8; td l9; td la; td lb; public override void Init()
    {
        l8 = new td(s3, mB.rd); l9 = new td(s3, mB.rd); la = new td(s3, mB.rd); lb = new td(s3, mB.
rd); l7 = mB.rc;
    }
    string lc; bool ld; string le; string lg; int lh; int li = 0; public override bool RunCmd(bool b3)
    {
        if (!b3)
        {
            lc = (mA.ol.
EndsWith("x") ? "s" : (mA.ol.EndsWith("p") ? "p" : (mA.ol.EndsWith("v") ? "v" : "n"))); ld = (mA.ol.StartsWith("powersummary")); le = "a"; lg = ""; if (
mA.ol.Contains("stored")) le = "s";
            else if (mA.ol.Contains("in")) le = "i"; else if (mA.ol.Contains("out")) le = "o"; li = 0; l8.pp(); l9.pp(); la.
pp();
        }
        if (le == "a")
        {
            if (li == 0) { if (!l8.pn("reactor", mA.om, b3)) return false; b3 = false; li++; }
            if (li == 1)
            {
                if (!l9.pn("solarpanel", mA.om, b3))
                    return false; b3 = false; li++;
            }
        }
        else if (li == 0) li = 2; if (li == 2) { if (!la.pn("battery", mA.om, b3)) return false; b3 = false; li++; }
        int b4 = l8.pq()
; int b5 = l9.pq(); int b6 = la.pq(); if (li == 3)
        {
            lh = 0; if (b4 > 0) lh++; if (b5 > 0) lh++; if (b6 > 0) lh++; if (lh < 1) { mB.rm(M.T["P6"]); return true; }
            if (mA.
os.Count > 0) { if (mA.os[0].oA.Length > 0) lg = mA.os[0].oA; }
            li++; b3 = false;
        }
        if (le != "a")
        {
            if (!lw(la, (lg == "" ? M.T["P7"] : lg), le, lc, b3)) return
false; return true;
        }
        var b7 = M.T["P8"]; if (!ld)
        {
            if (li == 4) { if (b4 > 0) if (!lo(l8, (lg == "" ? M.T["P9"] : lg), lc, b3)) return false; li++; b3 = false; }
            if (li == 5) { if (b5 > 0) if (!lo(l9, (lg == "" ? M.T["P10"] : lg), lc, b3)) return false; li++; b3 = false; }
            if (li == 6)
            {
                if (b6 > 0) if (!lw(la, (lg == "" ? M.T["P7"
] : lg), le, lc, b3)) return false; li++; b3 = false;
            }
        }
        else { b7 = M.T["P11"]; lh = 10; if (li == 4) li = 7; }
        if (lh == 1) return true; if (!b3)
        {
            lb.pp(); lb.po(l8
); lb.po(l9); lb.po(la);
        }
        if (!lo(lb, b7, lc, b3)) return false; return true;
    }
    void lj(double b8, double b9)
    {
        double ba = (b9 > 0 ? b8 / b9 * 100 : 0);
        switch (lc)
        {
            case "s": mB.rp(' ' + ba.ToString("0.0") + "%"); break;
            case "v": mB.rp(t8.nQ(b8) + "W / " + t8.nQ(b9) + "W"); break;
            case "c":
                mB.rp(t8
.nQ(b8) + "W"); break;
            case "p": mB.rp(' ' + ba.ToString("0.0") + "%"); mB.rt(ba); break;
            default:
                mB.rp(t8.nQ(b8) + "W / " + t8.nQ(b9) + "W"); mB.ru(
ba, 1.0f, mB.r4); mB.rp(' ' + ba.ToString("0.0") + "%"); break;
        }
    }
    double lk = 0; double ll = 0, lm = 0; int ln = 0; bool lo(td bb, string bc, string bd,
bool be)
    {
        if (!be) { ll = 0; lm = 0; ln = 0; }
        if (ln == 0) { if (!l7.qu(bb.p5, l7.qo, ref lk, ref lk, ref ll, ref lm, be)) return false; ln++; be = false; }
        if (!
s3.sp(50)) return false; double bf = (lm > 0 ? ll / lm * 100 : 0); mB.Add(bc + ": "); lj(ll * 0xF4240, lm * 0xF4240); return true;
    }
    double lp = 0, lq = 0, lr = 0,
ls = 0; double lt = 0, lu = 0; int lv = 0; bool lw(td bg, string bh, string bi, string bj, bool bk)
    {
        if (!bk) { lp = lq = 0; lr = ls = 0; lt = lu = 0; lv = 0; }
        if (lv == 0
)
        {
            if (!l7.qs(bg.p5, ref lr, ref ls, ref lp, ref lq, ref lt, ref lu, bk)) return false; lr *= 0xF4240; ls *= 0xF4240; lp *= 0xF4240; lq *= 0xF4240; lt *=
                                0xF4240; lu *= 0xF4240; lv++; bk = false;
        }
        double bl = (lu > 0 ? lt / lu * 100 : 0); double bm = (lq > 0 ? lp / lq * 100 : 0); double bn = (ls > 0 ? lr / ls * 100 : 0); var bo =
            bi == "a"; if (lv == 1)
        {
            if (!s3.sp(50)) return false; if (bo)
            {
                if (bj != "p")
                {
                    mB.Add(bh + ": "); mB.rp("(IN " + t8.nQ(lr) + "W / OUT " + t8.nQ(lp) + "W)");
                }
                else mB.rm(bh + ": "); mB.Add("  " + M.T["P3"] + ": ");
            }
            else mB.Add(bh + ": "); if (bo || bi == "s") switch (bj)
                {
                    case "s":
                        mB.rp(' ' + bl.ToString(
"0.0") + "%"); break;
                    case "v": mB.rp(t8.nQ(lt) + "Wh / " + t8.nQ(lu) + "Wh"); break;
                    case "p":
                        mB.rp(' ' + bl.ToString("0.0") + "%"); mB.rt(bl);
                        break;
                    default: mB.rp(t8.nQ(lt) + "Wh / " + t8.nQ(lu) + "Wh"); mB.ru(bl, 1.0f, mB.r4); mB.rp(' ' + bl.ToString("0.0") + "%"); break;
                }
            if (bi == "s")
                return true; lv++; bk = false;
        }
        if (lv == 2)
        {
            if (!s3.sp(50)) return false; if (bo) mB.Add("  " + M.T["P4"] + ": "); if (bo || bi == "o") switch (bj)
                {
                    case
"s":
                        mB.rp(' ' + bm.ToString("0.0") + "%"); break;
                    case "v": mB.rp(t8.nQ(lp) + "W / " + t8.nQ(lq) + "W"); break;
                    case "p":
                        mB.rp(' ' + bm.ToString(
"0.0") + "%"); mB.rt(bm); break;
                    default:
                        mB.rp(t8.nQ(lp) + "W / " + t8.nQ(lq) + "W"); mB.ru(bm, 1.0f, mB.r4); mB.rp(' ' + bm.ToString("0.0") + "%");
                        break;
                }
            if (bi == "o") return true; lv++; bk = false;
        }
        if (!s3.sp(50)) return false; if (bo) mB.Add("  " + M.T["P5"] + ": "); if (bo || bi == "i") switch (bj
)
            {
                case "s": mB.rp(' ' + bn.ToString("0.0") + "%"); break;
                case "v": mB.rp(t8.nQ(lr) + "W / " + t8.nQ(ls) + "W"); break;
                case "p":
                    mB.rp(' ' + bn.
ToString("0.0") + "%"); mB.rt(bn); break;
                default:
                    mB.rp(t8.nQ(lr) + "W / " + t8.nQ(ls) + "W"); mB.ru(bn, 1.0f, mB.r4); mB.rp(' ' + bn.ToString(
"0.0") + "%"); break;
            }
        return true;
    }
}
class sJ : t1
{
    public sJ() { r_ = 0.5; rW = "CmdSpeed"; }
    public override bool RunCmd(bool bp)
    {
        double bq = 0;
        double br = 1; var bs = "m/s"; if (mA.ol.Contains("kmh")) { br = 3.6; bs = "km/h"; } else if (mA.ol.Contains("mph")) { br = 2.23694; bs = "mph"; }
        if (mA.om
!= "") double.TryParse(mA.om.Trim(), out bq); mB.Add(M.T["S1"] + " "); mB.rp((mB.re.oH * br).ToString("F1") + " " + bs + " "); if (bq > 0) mB.rt(mB.re
.oH / bq * 100); return true;
    }
}
class sK : t1
{
    public sK() { r_ = 0.5; rW = "CmdAccel"; }
    public override bool RunCmd(bool bt)
    {
        double bu = 0; if (mA.om
!= "") double.TryParse(mA.om.Trim(), out bu); mB.Add(M.T["AC1"] + " "); mB.rp(mB.re.oJ.ToString("F1") + " m/s²"); if (bu > 0)
        {
            double bv = mB.re.
oJ / bu * 100; mB.rt(bv);
        }
        return true;
    }
}
class sL : t1
{
    public sL() { r_ = 30; rW = "CmdEcho"; }
    public override bool RunCmd(bool bw)
    {
        var bx = (mA.ol
== "center" ? "c" : (mA.ol == "right" ? "r" : "n")); switch (bx)
        {
            case "c": mB.rr(mA.oo); break;
            case "r": mB.rp(mA.oo); break;
            default:
                mB.rm(mA.oo);
                break;
        }
        return true;
    }
}
class sM : t1
{
    public sM() { r_ = 3; rW = "CmdCharge"; }
    td lx; public override void Init() { lx = new td(s3, mB.rd); }
    int ly = 0
; int lz = 0; public override bool RunCmd(bool by)
    {
        var bz = mA.ol.Contains("x"); if (!by) { lx.pp(); lz = 0; ly = 0; }
        if (ly == 0)
        {
            if (!lx.pn(
"jumpdrive", mA.om, by)) return false; if (lx.pq() <= 0) { mB.rm("Charge: " + M.T["D2"]); return true; }
            ly++; by = false;
        }
        for (; lz < lx.pq(); lz++)
        {
            if
(!s3.sp(25)) return false; IMyJumpDrive bA = lx.p5[lz] as IMyJumpDrive; double bB, bC, bD; bD = tf.pE(bA, out bB, out bC); mB.Add(bA.CustomName)
; if (!bz) { mB.rp(t8.nQ(bB) + "Wh / " + t8.nQ(bC) + "Wh"); mB.ru(bD, 1.0f, mB.r4); }
            mB.rp(' ' + bD.ToString("0.0") + "%");
        }
        return true;
    }
}
class sN :
t1
{
    public sN() { r_ = 1; rW = "CmdDateTime"; }
    public override bool RunCmd(bool bE)
    {
        var bF = (mA.ol.StartsWith("datetime")); var bG = (mA.ol.
StartsWith("date")); var bH = mA.ol.Contains("c"); int bI = mA.ol.IndexOf('+'); if (bI < 0) bI = mA.ol.IndexOf('-'); float bJ = 0; if (bI >= 0) float.
TryParse(mA.ol.Substring(bI), out bJ); DateTime bK = DateTime.Now.AddHours(bJ); var bL = ""; int bM = mA.on.IndexOf(' '); if (bM >= 0) bL = mA.on.
Substring(bM + 1); if (!bF) { if (!bG) bL += bK.ToShortTimeString(); else bL += bK.ToShortDateString(); }
        else
        {
            if (bL == "") bL = String.Format(
"{0:d} {0:t}", bK);
            else
            {
                bL = bL.Replace("/", "\\/"); bL = bL.Replace(":", "\\:"); bL = bL.Replace("\"", "\\\""); bL = bL.Replace("'", "\\'"); bL = bK
         .ToString(bL + ' '); bL = bL.Substring(0, bL.Length - 1);
            }
        }
        if (bH) mB.rr(bL); else mB.rm(bL); return true;
    }
}
class sO : t1
{
    public sO()
    {
        r_ = 1; rW =
"CmdCountdown";
    }
    public override bool RunCmd(bool bN)
    {
        var bO = mA.ol.EndsWith("c"); var bP = mA.ol.EndsWith("r"); var bQ = ""; int bR = mA.on.
IndexOf(' '); if (bR >= 0) bQ = mA.on.Substring(bR + 1).Trim(); DateTime bS = DateTime.Now; DateTime bT; if (!DateTime.TryParseExact(bQ,
"H:mm d.M.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out bT))
        {
            mB.rm(M.T["C3"
]); mB.rm("  Countdown 19:02 28.2.2015"); return true;
        }
        TimeSpan bU = bT - bS; var bV = ""; if (bU.Ticks <= 0) bV = M.T["C4"];
        else
        {
            if ((int)bU.
TotalDays > 0) bV += (int)bU.TotalDays + " " + M.T["C5"] + " "; if (bU.Hours > 0 || bV != "") bV += bU.Hours + "h "; if (bU.Minutes > 0 || bV != "") bV += bU.Minutes
+ "m "; bV += bU.Seconds + "s";
        }
        if (bO) mB.rr(bV); else if (bP) mB.rp(bV); else mB.rm(bV); return true;
    }
}
class sP : t1
{
    public sP()
    {
        r_ = 1; rW =
"CmdTextLCD";
    }
    public override bool RunCmd(bool bW)
    {
        IMyTextPanel bX = mC.mT.qA; if (bX == null) return true; var bY = ""; if (mA.om != "" && mA.om
!= "*")
        {
            IMyTextPanel bZ = mB.rf.GetBlockWithName(mA.om) as IMyTextPanel; if (bZ == null) { mB.rm("TextLCD: " + M.T["T1"] + mA.om); return true; }
            bY = bZ.GetPublicText();
        }
        else { mB.rm("TextLCD:" + M.T["T2"]); return true; }
        if (bY.Length == 0) return true; mB.rn(bY); return true;
    }
}
class sQ :
t1
{
    public sQ() { r_ = 15; rW = "CmdBlockCount"; }
    td lA; public override void Init() { lA = new td(s3, mB.rd); }
    bool lB; bool lC; int lD = 0; int lE = 0
; public override bool RunCmd(bool b_)
    {
        if (!b_) { lB = (mA.ol == "enabledcount"); lC = (mA.ol == "prodcount"); lD = 0; lE = 0; }
        if (mA.os.Count == 0)
        {
            if (
lE == 0) { if (!b_) lA.pp(); if (!lA.pf(mA.om, b_)) return false; lE++; b_ = false; }
            if (!lO(lA, "blocks", lB, lC, b_)) return false; return true;
        }
        for (;
lD < mA.os.Count; lD++) { tb c0 = mA.os[lD]; if (!b_) c0.oC(); if (!lH(c0, b_)) return false; b_ = false; }
        return true;
    }
    int lF = 0; int lG = 0; bool lH(tb
c1, bool c2)
    {
        if (!c2) { lF = 0; lG = 0; }
        for (; lF < c1.oB.Count; lF++)
        {
            if (lG == 0)
            {
                if (!c2) lA.pp(); if (!lA.pn(c1.oB[lF], mA.om, c2)) return false; lG++;
                c2 = false;
            }
            if (!lO(lA, c1.oB[lF], lB, lC, c2)) return false; lG = 0; c2 = false;
        }
        return true;
    }
    Dictionary<string, int> lI = new Dictionary<string,
int>(); Dictionary<string, int> lJ = new Dictionary<string, int>(); List<string> lK = new List<string>(); int lL = 0; int lM = 0; int lN = 0; bool lO(
td c3, string c4, bool c5, bool c6, bool c7)
    {
        string c8; if (c3.pq() == 0)
        {
            c8 = c4.ToLower(); c8 = char.ToUpper(c8[0]) + c8.Substring(1).ToLower()
; mB.Add(c8 + " " + M.T["C1"] + " "); var c9 = (c5 || c6 ? "0 / 0" : "0"); mB.rp(c9); return true;
        }
        if (!c7)
        {
            lI.Clear(); lJ.Clear(); lK.Clear(); lL = 0; lM =
0; lN = 0;
        }
        if (lN == 0)
        {
            for (; lL < c3.pq(); lL++)
            {
                if (!s3.sp(15)) return false; IMyProductionBlock ca = c3.p5[lL] as IMyProductionBlock; c8 = c3.p5[
lL].DefinitionDisplayNameText; if (lK.Contains(c8)) { lI[c8]++; if ((c5 && c3.p5[lL].IsWorking) || (c6 && ca != null && ca.IsProducing)) lJ[c8]++; }
                else
                {
                    lI.Add(c8, 1); lK.Add(c8); if (c5 || c6) if ((c5 && c3.p5[lL].IsWorking) || (c6 && ca != null && ca.IsProducing)) lJ.Add(c8, 1);
                        else lJ.Add(c8, 0)
;
                }
            }
            lN++; c7 = false;
        }
        for (; lM < lI.Count; lM++)
        {
            if (!s3.sp(8)) return false; mB.Add(lK[lM] + " " + M.T["C1"] + " "); var c9 = (c5 || c6 ? lJ[lK[lM]] +
" / " : "") + lI[lK[lM]]; mB.rp(c9);
        }
        return true;
    }
}
class sR : t1
{
    public sR() { r_ = 5; rW = "CmdShipCtrl"; }
    td lP; public override void Init()
    {
        lP
= new td(s3, mB.rd);
    }
    public override bool RunCmd(bool cc)
    {
        if (!cc) lP.pp(); if (!lP.pn("shipctrl", mA.om, cc)) return false; if (lP.pq() <= 0)
        {
            if (mA.om != "" && mA.om != "*") mB.rm(mA.ol + ": " + M.T["SC1"] + " (" + mA.om + ")"); else mB.rm(mA.ol + ": " + M.T["SC1"]); return true;
        }
        if (mA.ol.
StartsWith("damp")) { var cd = (lP.p5[0] as IMyShipController).DampenersOverride; mB.Add(M.T["SCD"]); mB.rp(cd ? "ON" : "OFF"); }
        else
        {
            var cd = (
lP.p5[0] as IMyShipController).IsUnderControl; mB.Add(M.T["SCO"]); mB.rp(cd ? "YES" : "NO");
        }
        return true;
    }
}
class sS : t1
{
    public sS()
    {
        r_ = 5;
        rW = "CmdWorking";
    }
    td lQ; public override void Init() { lQ = new td(s3, mB.rd); }
    int lR = 0; int lS = 0; bool lT; public override bool RunCmd(bool
cf)
    {
        if (!cf) { lR = 0; lT = (mA.ol == "workingx"); lS = 0; }
        if (mA.os.Count == 0)
        {
            if (lR == 0)
            {
                if (!cf) lQ.pp(); if (!lQ.pf(mA.om, cf)) return false; lR++; cf
= false;
            }
            if (!m1(lQ, lT, "", cf)) return false; return true;
        }
        for (; lS < mA.os.Count; lS++)
        {
            tb cg = mA.os[lS]; if (!cf) cg.oC(); if (!lZ(cg, cf))
                return false; cf = false;
        }
        return true;
    }
    int lU = 0; int lV = 0; string[] lW; string lX; string lY; bool lZ(tb ch, bool ci)
    {
        if (!ci) { lU = 0; lV = 0; }
        for
(; lV < ch.oB.Count; lV++)
        {
            if (lU == 0)
            {
                if (!ci)
                {
                    if (ch.oB[lV] == "") continue; lQ.pp(); lW = ch.oB[lV].ToLower().Split(':'); lX = lW[0]; lY = (lW.
Length > 1 ? lW[1] : "");
                }
                if (lX != "") { if (!lQ.pn(lX, mA.om, ci)) return false; } else { if (!lQ.pf(mA.om, ci)) return false; }
                lU++; ci = false;
            }
            if (!m1(
lQ, lT, lY, ci)) return false; lU = 0; ci = false;
        }
        return true;
    }
    string l_(IMyTerminalBlock cj)
    {
        if (!cj.IsWorking) return M.T["W1"];
        IMyProductionBlock ck = cj as IMyProductionBlock; if (ck != null) if (ck.IsProducing) return M.T["W2"]; else return M.T["W3"]; IMyAirVent cl =
                   cj as IMyAirVent; if (cl != null) { if (cl.CanPressurize) return (cl.GetOxygenLevel() * 100).ToString("F1") + "%"; else return M.T["W4"]; }
        IMyGasTank cm = cj as IMyGasTank; if (cm != null) return (cm.FilledRatio * 100).ToString("F1") + "%"; IMyBatteryBlock cn = cj as IMyBatteryBlock;
        if (cn != null) return tf.pC(cn); IMyJumpDrive co = cj as IMyJumpDrive; if (co != null) return tf.pF(co).ToString("0.0") + "%"; IMyLandingGear cp
                       = cj as IMyLandingGear; if (cp != null) return tf.pG(cp); IMyDoor cq = cj as IMyDoor; if (cq != null)
        {
            if (cq.Status == DoorStatus.Open) return M.T[
"W5"]; return M.T["W6"];
        }
        IMyShipConnector cr = cj as IMyShipConnector; if (cr != null)
        {
            if (cr.Status == MyShipConnectorStatus.Unconnected)
                return M.T["W8"]; if (cr.Status == MyShipConnectorStatus.Connected) return M.T["W7"]; else return M.T["W10"];
        }
        IMyLaserAntenna cs = cj as
IMyLaserAntenna; if (cs != null) return tf.pD(cs); IMyRadioAntenna cu = cj as IMyRadioAntenna; if (cu != null) return t8.nQ(cu.Radius) + "m";
        IMyBeacon cv = cj as IMyBeacon; if (cv != null) return t8.nQ(cv.Radius) + "m"; return M.T["W9"];
    }
    int m0 = 0; bool m1(td cw, bool cx, string cy,
bool cz)
    {
        if (!cz) m0 = 0; for (; m0 < cw.pq(); m0++)
        {
            if (!s3.sp(20)) return false; IMyTerminalBlock cA = cw.p5[m0]; var cB = (cx ? (cA.IsWorking ? M.T[
"W9"] : M.T["W1"]) : l_(cA)); if (cy != "" && cB.ToLower() != cy) continue; if (cx) cB = l_(cA); var cC = cA.CustomName; cC = tr.sx(cC, tn.qV * 0.7f * mB.rj.rA
); mB.Add(cC); mB.rp(cB);
        }
        return true;
    }
}
class sT : t1
{
    public sT() { r_ = 5; rW = "CmdDamage"; }
    td m2; public override void Init()
    {
        m2 = new td(s3
, mB.rd);
    }
    bool m3 = false; int m4 = 0; public override bool RunCmd(bool cD)
    {
        var cE = mA.ol.StartsWith("damagex"); var cF = mA.ol.EndsWith(
"noc"); var cG = (!cF && mA.ol.EndsWith("c")); float cH = 100; if (!cD) { m2.pp(); m3 = false; m4 = 0; }
        if (!m2.pf(mA.om, cD)) return false; if (mA.os.
Count > 0) { if (!float.TryParse(mA.os[0].oA, out cH)) cH = 100; }
        for (; m4 < m2.pq(); m4++)
        {
            if (!s3.sp(30)) return false; IMyTerminalBlock cI = m2.p5
[m4]; IMySlimBlock cJ = cI.CubeGrid.GetCubeBlock(cI.Position); if (cJ == null) continue; float cK = (cF ? cJ.MaxIntegrity : cJ.BuildIntegrity); if
(!cG) cK -= cJ.CurrentDamage; float cL = 100 * (cK / cJ.MaxIntegrity); if (cL >= cH) continue; m3 = true; mB.Add(tr.sx(cJ.FatBlock.DisplayNameText, tn
.qV * 0.69f * mB.rj.rA - 70) + " "); if (!cE) { mB.rq(t8.nQ(cK) + " / ", 0.69f); mB.Add(t8.nQ(cJ.MaxIntegrity)); }
            mB.rp(' ' + cL.ToString("0.0") + "%")
; mB.rt(cL);
        }
        if (!m3) mB.rm(M.T["D3"]); return true;
    }
}
class sU : t1
{
    public sU() { r_ = 2; rW = "CmdAmount"; }
    td m5; public override void Init()
    {
        m5 = new td(s3, mB.rd);
    }
    bool m6; int m7 = 0; int m8 = 0; int m9 = 0; public override bool RunCmd(bool cM)
    {
        if (!cM)
        {
            m6 = !mA.ol.EndsWith("x"); if (mA
.os.Count == 0) mA.os.Add(new tb("reactor,gatlingturret,missileturret,interiorturret,gatlinggun,launcherreload,launcher,oxygenerator"
)); m8 = 0;
        }
        for (; m8 < mA.os.Count; m8++)
        {
            tb cN = mA.os[m8]; if (!cM) { cN.oC(); m7 = 0; m9 = 0; }
            for (; m9 < cN.oB.Count; m9++)
            {
                if (m7 == 0)
                {
                    if (!cM)
                    {
                        if (cN.oB
[m9] == "") continue; m5.pp();
                    }
                    var cO = cN.oB[m9]; if (!m5.pn(cO, mA.om, cM)) return false; m7++; cM = false;
                }
                if (!mk(cM)) return false; cM = false; m7
= 0;
            }
        }
        return true;
    }
    int ma = 0; int mb = 0; double mc = 0; double md = 0; double me = 0; int mf = 0; IMyTerminalBlock mg; IMyInventory mh; List<
      IMyInventoryItem> mi; string mj = ""; bool mk(bool cP)
    {
        if (!cP) { ma = 0; mb = 0; }
        for (; ma < m5.pq(); ma++)
        {
            if (mb == 0)
            {
                if (!s3.sp(50)) return false; mg
= m5.p5[ma]; mh = mg.GetInventory(0); if (mh == null) continue; mb++; cP = false;
            }
            if (!cP)
            {
                mi = mh.GetItems(); mj = (mi.Count > 0 ? mi[0].Content.
ToString() : ""); mf = 0; mc = 0; md = 0; me = 0;
            }
            for (; mf < mi.Count; mf++)
            {
                if (!s3.sp(30)) return false; IMyInventoryItem cQ = mi[mf]; if (cQ.Content.
ToString() != mj) me += (double)cQ.Amount;
                else mc += (double)cQ.Amount;
            }
            var cR = M.T["A1"]; var cS = mg.CustomName; if (mc > 0 && (double)mh.
CurrentVolume > 0)
            {
                double cT = me * (double)mh.CurrentVolume / (mc + me); md = Math.Floor(mc * ((double)mh.MaxVolume - cT) / (double)mh.CurrentVolume
        - cT); cR = t8.nQ(mc) + " / " + (me > 0 ? "~" : "") + t8.nQ(md);
            }
            cS = tr.sx(cS, tn.qV * 0.8f * mB.rj.rA); mB.Add(cS); mB.rp(cR); if (m6 && md > 0)
            {
                double cU = 100 *
mc / md; mB.rt(cU);
            }
            mb = 0; cP = false;
        }
        return true;
    }
}
class sV : t1
{
    public sV() { r_ = 1; rW = "CmdPosition"; }
    public override bool RunCmd(bool cV)
    {
        var cW = (mA.ol == "posxyz"); var cX = (mA.ol == "posgps"); IMyTerminalBlock cY = mC.mT.qA; if (mA.om != "" && mA.om != "*")
        {
            cY = mB.rf.
GetBlockWithName(mA.om); if (cY == null) { mB.rm("Pos: " + M.T["P1"] + ": " + mA.om); return true; }
        }
        if (cX)
        {
            VRageMath.Vector3D cZ = cY.GetPosition
(); mB.rm("GPS:" + M.T["P2"] + ":" + cZ.GetDim(0).ToString("F2") + ":" + cZ.GetDim(1).ToString("F2") + ":" + cZ.GetDim(2).ToString("F2") + ":");
            return true;
        }
        mB.Add(M.T["P2"] + ": "); if (!cW) { mB.rp(cY.GetPosition().ToString("F0")); return true; }
        mB.rm(""); mB.Add(" X: "); mB.rp(cY.
GetPosition().GetDim(0).ToString("F0")); mB.Add(" Y: "); mB.rp(cY.GetPosition().GetDim(1).ToString("F0")); mB.Add(" Z: "); mB.rp(cY.
GetPosition().GetDim(2).ToString("F0")); return true;
    }
}
class sW : t1
{
    public sW() { r_ = 5; rW = "CmdDetails"; }
    string ml = ""; td mm; public
override void Init()
    { mm = new td(s3, mB.rd); if (mA.os.Count > 0) ml = mA.os[0].oA.Trim(); }
    int mn = 0; int mo = 1; IMyTerminalBlock mp; public
override bool RunCmd(bool c_)
    {
        if (mA.om == "" || mA.om == "*") { mB.rm("Details: " + M.T["D1"]); return true; }
        if (!c_) { mm.pp(); mn = 0; mo = 1; }
        if (mn
== 0) { if (!mm.pf(mA.om, c_)) return true; if (mm.pq() <= 0) { mB.rm("Details: " + M.T["D2"]); return true; } mn++; c_ = false; }
        int d0 = (mA.ol.
EndsWith("x") ? 1 : 0); if (mn == 1) { if (!c_) { mp = mm.p5[0]; mB.rm(mp.CustomName); } if (!mt(mp, d0, c_)) return false; mn++; c_ = false; }
        for (; mo < mm.pq(
); mo++) { if (!c_) { mp = mm.p5[mo]; mB.rm(""); mB.rm(mp.CustomName); } if (!mt(mp, d0, c_)) return false; c_ = false; }
        return true;
    }
    string[] mq; int
mr = 0; bool ms = false; bool mt(IMyTerminalBlock d1, int d2, bool d3)
    {
        if (!d3)
        {
            mq = (d1.DetailedInfo + "\n" + d1.CustomInfo).Split('\n'); mr = d2;
            ms = (ml == "");
        }
        for (; mr < mq.Length; mr++)
        {
            if (!s3.sp(5)) return false; if (mq[mr] == "") continue; if (!ms)
            {
                if (!mq[mr].Contains(ml)) continue; ms =
true;
            }
            mB.rm("  " + mq[mr]);
        }
        return true;
    }
}
class sX : t1
{
    public sX() { r_ = 1; rW = "CmdShipMass"; }
    public override bool RunCmd(bool d4)
    {
        var
d5 = mA.ol.EndsWith("base"); double d6 = 0; if (mA.om != "") double.TryParse(mA.om.Trim(), out d6); int d7 = mA.os.Count; if (d7 > 0)
        {
            var d8 = mA.os[0
].oA.Trim().ToLower(); if (d8 != "") d6 *= Math.Pow(1000.0, "kmgtpezy".IndexOf(d8[0]));
        }
        double d9 = (d5 ? mB.re.oR : mB.re.oQ); if (!d5) mB.Add(M.T
["SM1"] + " ");
        else mB.Add(M.T["SM2"] + " "); mB.rp(t8.nQ(d9, true, 'k') + "g "); if (d6 > 0) mB.rt(d9 / d6 * 100); return true;
    }
}
class sY : t1
{
    public
sY()
    { r_ = 1; rW = "CmdDistance"; }
    string mu = ""; string[] mv; Vector3D mw; string mx = ""; bool my = false; public override void Init()
    {
        my = false; if
(mA.os.Count <= 0) return; mu = mA.os[0].oA.Trim(); mv = mu.Split(':'); if (mv.Length < 5 || mv[0] != "GPS") return; double da, db, dc; if (!double.
TryParse(mv[2], out da)) return; if (!double.TryParse(mv[3], out db)) return; if (!double.TryParse(mv[4], out dc)) return; mw = new Vector3D(da
, db, dc); mx = mv[1]; my = true;
    }
    public override bool RunCmd(bool dd)
    {
        if (!my) { mB.rm("Distance: " + M.T["DTU"] + " '" + mu + "'."); return true; }
        IMyTerminalBlock de = mC.mT.qA; if (mA.om != "" && mA.om != "*")
        {
            de = mB.rf.GetBlockWithName(mA.om); if (de == null)
            {
                mB.rm("Distance: " + M.T["P1"] +
": " + mA.om); return true;
            }
        }
        double df = Vector3D.Distance(de.GetPosition(), mw); mB.Add(mx + ": "); mB.rp(t8.nQ(df) + "m "); return true;
    }
}
class sZ : t1
{
    public sZ() { r_ = 1; rW = "CmdAltitude"; }
    public override bool RunCmd(bool dg)
    {
        var dh = (mA.ol.EndsWith("sea") ? "sea" : "ground")
; switch (dh)
        {
            case "sea": mB.Add(M.T["ALT1"]); mB.rp(mB.re.oT.ToString("F0") + " m"); break;
            default:
                mB.Add(M.T["ALT2"]); mB.rp(mB.re.oV.
ToString("F0") + " m"); break;
        }
        return true;
    }
}
class s_ : t1
{
    public s_() { r_ = 1; rW = "CmdStopTask"; }
    public override bool RunCmd(bool di)
    {
        double dj = 0; if (mA.ol.Contains("best")) dj = mB.re.oH / mB.re.oL; else dj = mB.re.oH / mB.re.oO; double dk = mB.re.oH / 2 * dj; if (mA.ol.Contains(
                              "time"))
        {
            mB.Add(M.T["ST"]); if (double.IsNaN(dj)) { mB.rp("N/A"); return true; }
            var dl = ""; try
            {
                TimeSpan dm = TimeSpan.FromSeconds(dj); if ((
int)dm.TotalDays > 0) dl = " > 24h";
                else
                {
                    if (dm.Hours > 0) dl = dm.Hours + "h "; if (dm.Minutes > 0 || dl != "") dl += dm.Minutes + "m "; dl += dm.Seconds + "s";
                }
            }
            catch { dl = "N/A"; }
            mB.rp(dl); return true;
        }
        mB.Add(M.T["SD"]); if (!double.IsNaN(dk) && !double.IsInfinity(dk)) mB.rp(t8.nQ(dk) + "m ");
        else
            mB.rp("N/A"); return true;
    }
}
class t0 : t1
{
    public t0() { r_ = 1; rW = "CmdGravity"; }
    public override bool RunCmd(bool dn)
    {
        var dp = (mA.ol.
Contains("nat") ? "n" : (mA.ol.Contains("art") ? "a" : (mA.ol.Contains("tot") ? "t" : "s"))); Vector3D dq; switch (dp)
        {
            case "n":
                mB.Add(M.T["G2"] +
" "); dq = mB.re.oY.GetNaturalGravity(); mB.rp(dq.Length().ToString("F1") + " m/s²"); break;
            case "a":
                mB.Add(M.T["G3"] + " "); dq = mB.re.oY.
GetArtificialGravity(); mB.rp(dq.Length().ToString("F1") + " m/s²"); break;
            case "t":
                mB.Add(M.T["G1"] + " "); dq = mB.re.oY.GetTotalGravity(
); mB.rp(dq.Length().ToString("F1") + " m/s²"); break;
            default:
                mB.Add(M.T["GN"]); mB.rq(" | ", 0.33f); mB.rq(M.T["GA"] + " | ", 0.66f); mB.rp(
M.T["GT"], 1.0f); mB.Add(""); dq = mB.re.oY.GetNaturalGravity(); mB.rq(dq.Length().ToString("F1") + " | ", 0.33f); dq = mB.re.oY.
GetArtificialGravity(); mB.rq(dq.Length().ToString("F1") + " | ", 0.66f); dq = mB.re.oY.GetTotalGravity(); mB.rp(dq.Length().ToString("F1"
) + " "); break;
        }
        return true;
    }
}
class t1 : tp
{
    public to mz = null; protected ta mA; protected tn mB; protected t4 mC; public t1()
    {
        r_ = 3600; rW =
"CommandTask";
    }
    public void mD(t4 dr, ta ds) { mC = dr; mB = mC.mS; mA = ds; }
    public virtual bool RunCmd(bool dt)
    {
        mB.rm(M.T["UC"] + ": '" + mA.on +
"'"); return true;
    }
    public override bool Run(bool du) { mz = mB.rk(mz, mC.mT); if (!du) mB.rv(); return RunCmd(du); }
}
class t2 : tp
{
    t6 mE; tn mF
; string mG = ""; public t2(tn dv, t6 dw, string dx) { r_ = -1; rW = "ArgScroll"; mG = dx; mE = dw; mF = dv; }
    int mH; td mI; public override void Init()
    {
        mI
= new td(s3, mF.rd);
    }
    int mJ = 0; int mK = 0; ta mL; public override bool Run(bool dy)
    {
        if (!dy) { mK = 0; mI.pp(); mL = new ta(s3); mJ = 0; }
        if (mK == 0)
        {
            if
(!mL.ox(mG, dy)) return false; if (mL.os.Count > 0) { if (!int.TryParse(mL.os[0].oA, out mH)) mH = 1; else if (mH < 1) mH = 1; }
            if (mL.ol.EndsWith("up")
) mH = -mH;
            else if (!mL.ol.EndsWith("down")) mH = 0; mK++; dy = false;
        }
        if (mK == 1) { if (!mI.pn("textpanel", mL.om, dy)) return false; mK++; dy = false; }
        tl dz; for (; mJ < mI.pq(); mJ++)
        {
            if (!s3.sp(20)) return false; IMyTextPanel dA = mI.p5[mJ] as IMyTextPanel; if (!mE.nt.TryGetValue(dA, out dz))
                continue; if (dz == null || dz.qA != dA) continue; if (dz.qE) dz.qz.rU = 10; if (mH > 0) dz.qz.rT(mH); else if (mH < 0) dz.qz.rS(-mH); else dz.qz.rV(); dz.
                                          qJ();
        }
        return true;
    }
}
class t3 : tp
{
    tn mM; t6 mN; public int mO = 0; public t3(tn dB, t6 dC)
    {
        rW = "BootPanelsTask"; r_ = 1; mM = dB; mN = dC; if (!mM.qZ
) mO = int.MaxValue;
    }
    public override bool Run(bool dD)
    {
        if (mO > mM.q_.Count) { s7(); return true; }
        if (mO == 0) { mN.nu = false; }
        mQ(); mO++; return
true;
    }
    public override void End() { mN.nu = true; }
    public void mP()
    {
        t7 dE = mN.np; for (int dF = 0; dF < dE.nG(); dF++)
        {
            tl dG = dE.nI(dF); mM.rk(dG.
qz, dG); mM.rv(); mM.rw(dG);
        }
        mO = (mM.qZ ? 0 : int.MaxValue);
    }
    public void mQ()
    {
        t7 dH = mN.np; for (int dI = 0; dI < dH.nG(); dI++)
        {
            tl dJ = dH.nI(dI); mM
.rk(dJ.qz, dJ); mM.rv(); if (dJ.qA.GetValueFloat("FontSize") > 3f) continue; mM.rr(M.T["B1"]); double dK = (double)mO / mM.q_.Count * 100; mM.rt(
dK); if (mO == mM.q_.Count) { mM.rm(""); mM.rr("Automatic LCDs 2"); mM.rr("by MMaster"); } else mM.rn(mM.q_[mO]); mM.rw(dJ);
        }
    }
    public bool mR(
)
    { return mO <= mM.q_.Count; }
}
class t4 : tp
{
    public tn mS; public tl mT; public t5 mU = null; string mV = "N/A"; public Dictionary<string, t1> mW
= new Dictionary<string, t1>(); public List<string> mX = null; public t6 mY; public bool mZ { get { return mY.nu; } }
    public t4(t6 dL, tl dM)
    {
        r_ = 5
; mT = dM; mY = dL; mS = dL.no; rW = "PanelProcess";
    }
    t1 m_(int dN)
    {
        switch (dN)
        {
            case t9.nU: return new sL();
            case t9.nV: return new sz();
            case t9.nW
:
                return new sA();
            case t9.nX: return new sB();
            case t9.nY: return new sC();
            case t9.nZ: return new sD();
            case t9.n_: return new sE();
            case
t9.o0:
                return new sG();
            case t9.o1: return new sI();
            case t9.o2: return new sJ();
            case t9.o3: return new sK();
            case t9.o4: return new sM();
            case t9.o5: return new sN();
            case t9.o6: return new sO();
            case t9.o7: return new sP();
            case t9.o8: return new sQ();
            case t9.o9:
                return new
sR();
            case t9.oa: return new sS();
            case t9.ob: return new sT();
            case t9.oc: return new sU();
            case t9.od: return new sV();
            case t9.oe:
                return
new sW();
            case t9.of: return new sX();
            case t9.og: return new sY();
            case t9.oh: return new sZ();
            case t9.oi: return new s_();
            case t9.oj:
                return new t0();
            default: return new t1();
        }
    }
    ta n0 = null; t1 n1(string dO, bool dP)
    {
        if (!dP) n0 = new ta(s3); if (!n0.ox(dO, dP)) return null; t1
dQ = m_(n0.oq); dQ.mD(this, n0); s3.sh(dQ, 0); return dQ;
    }
    string n2 = ""; void n3()
    {
        try { n2 = mT.qA.CustomData; }
        catch
        {
            n2 = ""; mT.qA.CustomData = ""
;
        }
        n2 = n2.Replace("\\\n", "");
    }
    int n4 = 0; int n5 = 0; List<string> n6 = null; HashSet<string> n7 = new HashSet<string>(); int n8 = 0; bool n9(bool dR
)
    {
        if (!dR)
        {
            char[] dS = { ';', '\n' }; var dT = n2.Replace("\\;", "\f"); n6 = new List<string>(dT.Split(dS, StringSplitOptions.RemoveEmptyEntries
            )); n7.Clear(); n4 = 0; n5 = 0; n8 = 0;
        } while (n4 < n6.Count)
        {
            if (!s3.sp(500)) return false; if (n6[n4].StartsWith("//"))
            {
                n6.RemoveAt(n4); continue;
            }
            n6[n4] = n6[n4].Replace('\f', ';'); if (!mW.ContainsKey(n6[n4]))
            {
                if (n8 != 1) dR = false; n8 = 1; t1 dU = n1(n6[n4], dR); if (dU == null) return false;
                dR = false; mW.Add(n6[n4], dU); n8 = 0;
            }
            if (!n7.Contains(n6[n4])) n7.Add(n6[n4]); n4++;
        }
        if (mX != null)
        {
            t1 dV; while (n5 < mX.Count)
            {
                if (!s3.sp(7))
                    return false; if (!n7.Contains(mX[n5])) if (mW.TryGetValue(mX[n5], out dV)) { dV.s7(); mW.Remove(mX[n5]); }
                n5++;
            }
        }
        mX = n6; return true;
    }
    public
override void End()
    {
        if (mX != null) { t1 dW; for (int dX = 0; dX < mX.Count; dX++) { if (mW.TryGetValue(mX[dX], out dW)) dW.s7(); } mX = null; }
        if (mU !=
null) { mU.s7(); mU = null; }
    }
    string na = ""; bool nb = false; public override bool Run(bool dY)
    {
        if (mT.qy.qO() <= 0) { s7(); return true; }
        if (!dY)
        {
            mT.qz = mS.rk(mT.qz, mT); n3(); if (mT.qA.CustomName != na) nb = true; else nb = false; na = mT.qA.CustomName;
        }
        if (n2 != mV)
        {
            if (!n9(dY)) return false;
            if (n2 == "") { if (mY.nu) { mS.rv(); mS.rm(M.T["H1"]); mS.rw(mT); return true; } return this.s5(2); }
            nb = true;
        }
        mV = n2; if (mU != null && nb)
        {
            s3.si(mU);
            mU.ng(); s3.sh(mU, 0);
        }
        else if (mU == null) { mU = new t5(this); s3.sh(mU, 0); }
        return true;
    }
}
class t5 : tp
{
    public tn nc; public tl nd; t4 ne;
    public t5(t4 dZ) { ne = dZ; nc = ne.mS; nd = ne.mT; r_ = 0.5; rW = "PanelDisplay"; }
    double nf = 0; public void ng() { nf = 0; }
    int nh = 0; int ni = 0; bool nj =
true; double nk = double.MaxValue; int nl = 0; public override bool Run(bool d_)
    {
        t1 e0; if (!d_ && (!ne.mZ || ne.mX == null || ne.mX.Count <= 0))
            return true; if (ne.mY.nn > 5) return s5(0); if (!d_) { ni = 0; nj = false; nk = double.MaxValue; nl = 0; }
        if (nl == 0)
        {
            while (ni < ne.mX.Count)
            {
                if (!s3.sp(5)
) return false; if (ne.mW.TryGetValue(ne.mX[ni], out e0))
                {
                    if (!e0.s1) return s5(e0.rX - s3.sb + 0.001); if (e0.rY > nf) nj = true; if (e0.rX < nk) nk = e0
.rX;
                }
                ni++;
            }
            nl++; d_ = false;
        }
        double e1 = nk - s3.sb + 0.001; if (!nj && !nd.qF()) return s5(e1); nc.rl(nd); if (nj)
        {
            if (!d_)
            {
                nf = s3.sb; nc.rv(); var e2
= nd.qA.CustomName; e2 = (e2.Contains("#") ? e2.Substring(e2.LastIndexOf('#') + 1) : ""); if (e2 != "") nc.rm(e2); nh = 0;
            } while (nh < ne.mX.Count)
            {
                if (
!s3.sp(7)) return false; if (!ne.mW.TryGetValue(ne.mX[nh], out e0)) { nc.rm("ERR: No cmd task (" + ne.mX[nh] + ")"); nh++; continue; }
                nc.ro(e0.
mz.rM()); nh++;
            }
        }
        nc.rw(nd); ne.mY.nn++; if (r_ < e1 && !nd.qF()) return s5(e1); return true;
    }
}
class t6 : tp
{
    public int nn = 0; public tn no;
    public t7 np = new t7(); td nq; td nr; Dictionary<tl, t4> ns = new Dictionary<tl, t4>(); public Dictionary<IMyTextPanel, tl> nt = new Dictionary<
               IMyTextPanel, tl>(); public bool nu = false; t3 nv = null; public t6(tn e3) { r_ = 5; no = e3; rW = "ProcessPanels"; }
    public override void Init()
    {
        nq =
new td(s3, no.rd); nr = new td(s3, no.rd); nv = new t3(no, this);
    }
    int nw = 0; bool nx(bool e4)
    {
        if (!e4) nw = 0; if (nw == 0)
        {
            if (!nq.pn("textpanel", no.
qX, e4)) return false; nw++; e4 = false;
        }
        if (nw == 1)
        {
            if (no.qX == "T:[LCD]" && "T:!LCD!" != "") if (!nq.pn("textpanel", "T:!LCD!", e4)) return false;
            nw++; e4 = false;
        }
        return true;
    }
    string ny(IMyTextPanel e5)
    {
        return e5.CustomName + " " + e5.NumberInGrid + " " + e5.GetPosition().ToString("F0"
);
    }
    void nz(IMyTextPanel e6)
    {
        tl e7 = null; if (!nt.TryGetValue(e6, out e7)) return; e7.qy.qN(e7.qD); nt.Remove(e6); if (e7.qy.qO() <= 0)
        {
            t4 e8;
            if (ns.TryGetValue(e7, out e8)) { np.nJ(e7.qD); ns.Remove(e7); e8.s7(); }
        }
    }
    int nA = 0; int nB = 0; public override bool Run(bool e9)
    {
        if (!e9)
        {
            nq
.pp(); nA = 0; nB = 0;
        }
        if (!nx(e9)) return false; while (nA < nq.pq())
        {
            if (!s3.sp(20)) return false; IMyTextPanel ea = (nq.p5[nA] as IMyTextPanel);
            if (ea == null || !ea.IsWorking) { nq.p5.RemoveAt(nA); continue; }
            tl eb = null; var ec = false; var ed = ny(ea); int ee = ed.IndexOf("!LINK:"); if (ee >=
0 && ed.Length > ee + 6) { ed = ed.Substring(ee + 6); ec = true; }
            if (nt.ContainsKey(ea)) { eb = nt[ea]; if (eb.qD == ed) { nA++; continue; } this.nz(ea); }
            if (!
ec) { eb = new tl(no, ed); eb.qy.qM(ed, ea); t4 ef = new t4(this, eb); s3.sh(ef, 0); ns.Add(eb, ef); np.nF(ed, eb); nt.Add(ea, eb); nA++; continue; }
            string[] eg = ed.Split(' '); var eh = eg[0]; eb = np.nH(eh); if (eb == null)
            {
                eb = new tl(no, eh); np.nF(eh, eb); t4 ef = new t4(this, eb); s3.sh(ef, 0); ns
.Add(eb, ef);
            }
            eb.qy.qM(ed, ea); nt.Add(ea, eb); nA++;
        } while (nB < nr.pq())
        {
            if (!s3.sp(300)) return false; IMyTextPanel ea = nr.p5[nB] as
IMyTextPanel; if (ea == null) continue; if (!nq.p5.Contains(ea)) this.nz(ea); nB++;
        }
        nr.pp(); nr.po(nq); if (!nv.s0 && nv.mR()) s3.sh(nv, 0); return
true;
    }
    public bool nC(string ek)
    {
        s3.sj("RunCommand: " + ek); var el = ek.ToLower(); if (el == "clear")
        {
            s3.sj("ClearLCDs"); nv.mP(); if (!nv.s0)
                s3.sh(nv, 0); return true;
        }
        if (el == "boot") { s3.sj("Boot, isRunning" + nv.s0); nv.mO = 0; if (!nv.s0) s3.sh(nv, 0); return true; }
        if (el.StartsWith
("scroll")) { t2 em = new t2(no, this, ek); s3.sh(em, 0); return true; }
        return false;
    }
}
public class t7
{
    Dictionary<string, tl> nD = new
Dictionary<string, tl>(); List<string> nE = new List<string>(); public void nF(string en, tl eo)
    {
        if (!nD.ContainsKey(en))
        {
            nE.Add(en); nD.
Add(en, eo);
        }
    }
    public int nG() { return nD.Count; }
    public tl nH(string ep) { if (nD.ContainsKey(ep)) return nD[ep]; return null; }
    public tl
nI(int eq)
    { return nD[nE[eq]]; }
    public void nJ(string er) { nD.Remove(er); nE.Remove(er); }
    public void nK() { nE.Clear(); nD.Clear(); }
    public void nL() { nE.Sort(); }
}
public static class t8
{
    public static string nM(IMyInventoryItem es)
    {
        var et = es.Content.TypeId.
ToString(); et = et.Substring(et.LastIndexOf('_') + 1); return es.Content.SubtypeName + " " + et;
    }
    public static void nN(string eu, out string
ev, out string ew)
    { int ex = eu.LastIndexOf(' '); if (ex >= 0) { ev = eu.Substring(0, ex); ew = eu.Substring(ex + 1); return; } ev = eu; ew = ""; }
    public
static string nO(string ey)
    { string ez, eA; nN(ey, out ez, out eA); return nO(ez, eA); }
    public static string nO(string eB, string eC)
    {
        tg eD
= MMItems.pL(eB, eC); if (eD != null) { if (eD.pP != "") return eD.pP; return eD.pM; }
        return System.Text.RegularExpressions.Regex.Replace(eB,
"([a-z])([A-Z])", "$1 $2");
    }
    public static void nP(ref string eE, ref string eF)
    {
        var eG = eE.ToLower(); tg eH; if (MMItems.pK.TryGetValue(
eG, out eH)) { eE = eH.pM; eF = eH.pN; return; }
        eH = MMItems.pL(eE, eF); if (eH != null) { eE = eH.pM; if (eF == "Ore" || eF == "Ingot") return; eF = eH.pN; }
    }
    public static string nQ(double eI, bool eJ = true, char eK = ' ')
    {
        if (!eJ) return eI.ToString("#,###,###,###,###,###,###,###,###,###"); var
eL = " kMGTPEZY"; double eM = eI; int eN = eL.IndexOf(eK); int eO = (eN < 0 ? 0 : eN); while (eM >= 1000 && eO + 1 < eL.Length) { eM /= 1000; eO++; }
        var eP = Math.
Round(eM, 1, MidpointRounding.AwayFromZero).ToString(); if (eO > 0) eP += " " + eL[eO]; return eP;
    }
    public static string nR(double eQ)
    {
        return (
Math.Floor(eQ * 10) / 10).ToString("F1");
    }
}
public static class t9
{
    public const int nU = 1; public const int nV = 2; public const int nW = 3;
    public const int nX = 4; public const int nY = 5; public const int nZ = 6; public const int n_ = 7; public const int o0 = 8; public const int o1 =
                   9; public const int o2 = 10; public const int o3 = 11; public const int o4 = 12; public const int o5 = 13; public const int o6 = 14; public const
                                   int o7 = 15; public const int o8 = 16; public const int o9 = 17; public const int oa = 18; public const int ob = 19; public const int oc = 20;
    public const int od = 21; public const int oe = 22; public const int of = 23; public const int og = 24; public const int oh = 25; public const
                   int oi = 26; public const int oj = 27;
}
public class ta
{
    tq ok; public string ol = ""; public string om = ""; public string on = ""; public string
oo = ""; public int oq = 0; int or()
    {
        if (ol.StartsWith("inventory") || ol == "missing" || ol.StartsWith("invlist")) return t9.nV; if (ol.
StartsWith("cargo")) return t9.nW; if (ol.StartsWith("mass")) return t9.nX; if (ol.StartsWith("shipmass")) return t9.of; if (ol == "oxygen")
            return t9.nY; if (ol.StartsWith("tanks")) return t9.nZ; if (ol.StartsWith("powertime")) return t9.n_; if (ol.StartsWith("powerused"))
            return t9.o0; if (ol.StartsWith("power")) return t9.o1; if (ol.StartsWith("speed")) return t9.o2; if (ol.StartsWith("accel")) return t9.o3;
        if (ol.StartsWith("alti")) return t9.oh; if (ol.StartsWith("charge")) return t9.o4; if (ol.StartsWith("time") || ol.StartsWith("date"))
            return t9.o5; if (ol.StartsWith("countdown")) return t9.o6; if (ol == "echo" || ol == "center" || ol == "right") return t9.nU; if (ol.StartsWith(
                             "textlcd")) return t9.o7; if (ol.EndsWith("count")) return t9.o8; if (ol.StartsWith("dampeners") || ol.StartsWith("occupied")) return t9.o9
                                      ; if (ol.StartsWith("working")) return t9.oa; if (ol.StartsWith("damage")) return t9.ob; if (ol.StartsWith("amount")) return t9.oc; if (ol.
                                                StartsWith("pos")) return t9.od; if (ol.StartsWith("distance")) return t9.og; if (ol.StartsWith("details")) return t9.oe; if (ol.StartsWith
                                                        ("stop")) return t9.oi; if (ol.StartsWith("gravity")) return t9.oj; return 0;
    }
    public ta(tq eR) { ok = eR; }
    public List<tb> os = new List<tb>();
    string[] ot = null; string ou = ""; bool ov = false; int ow = 1; public bool ox(string eS, bool eT)
    {
        if (!eT)
        {
            oq = 0; om = ""; ol = ""; on = eS.TrimStart(' '
); os.Clear(); if (on == "") return true; int eU = on.IndexOf(' '); if (eU < 0 || eU >= on.Length - 1) oo = ""; else oo = on.Substring(eU + 1); ot = on.Split(
' '); ou = ""; ov = false; ol = ot[0].ToLower(); ow = 1;
        }
        for (; ow < ot.Length; ow++)
        {
            if (!ok.sp(40)) return false; var eV = ot[ow]; if (eV == "") continue;
            if (eV[0] == '{' && eV[eV.Length - 1] == '}')
            {
                eV = eV.Substring(1, eV.Length - 2); if (eV == "") continue; if (om == "") om = eV; else os.Add(new tb(eV));
                continue;
            }
            if (eV[0] == '{') { ov = true; ou = eV.Substring(1); continue; }
            if (eV[eV.Length - 1] == '}')
            {
                ov = false; ou += ' ' + eV.Substring(0, eV.Length - 1
); if (om == "") om = ou; else os.Add(new tb(ou)); continue;
            }
            if (ov) { if (ou.Length != 0) ou += ' '; ou += eV; continue; }
            if (om == "") om = eV;
            else os.Add(
new tb(eV));
        }
        oq = or(); return true;
    }
}
public class tb
{
    public string oy = ""; public string oz = ""; public string oA = ""; public List<string
> oB = new List<string>(); public tb(string eW) { oA = eW; }
    public void oC()
    {
        if (oA == "" || oy != "" || oz != "" || oB.Count > 0) return; var eX = oA.Trim();
        if (eX[0] == '+' || eX[0] == '-') { oy += eX[0]; eX = oA.Substring(1); }
        string[] eY = eX.Split('/'); var eZ = eY[0]; if (eY.Length > 1) { oz = eY[0]; eZ = eY[1]; }
        else oz = ""; if (eZ.Length > 0) { string[] e_ = eZ.Split(','); for (int f0 = 0; f0 < e_.Length; f0++) if (e_[f0] != "") oB.Add(e_[f0]); }
    }
}
public class tc
: tp
{
    MyShipVelocities oD; public VRageMath.Vector3D oE { get { return oD.LinearVelocity; } }
    public VRageMath.Vector3D oF
    {
        get
        {
            return oD.
AngularVelocity;
        }
    }
    double oG = 0; public double oH { get { if (oW != null) return oW.GetShipSpeed(); else return oG; } }
    double oI = 0; public double
oJ
    { get { return oI; } }
    double oK = 0; public double oL { get { return oK; } }
    double oM = 0; double oN = 0; public double oO { get { return oM; } }
    MyShipMass oP; public double oQ { get { return oP.TotalMass; } }
    public double oR { get { return oP.BaseMass; } }
    double oS = double.NaN; public
double oT
    { get { return oS; } }
    double oU = double.NaN; public double oV { get { return oU; } }
    IMyShipController oW = null; IMySlimBlock oX = null;
    public IMyShipController oY { get { return oW; } }
    VRageMath.Vector3D oZ; public tc(tq f1)
    {
        rW = "ShipMgr"; s3 = f1; oZ = s3.sf.Me.GetPosition(); r_
= 0.5;
    }
    List<IMyTerminalBlock> o_ = new List<IMyTerminalBlock>(); int p0 = 0; public override bool Run(bool f2)
    {
        if (!f2)
        {
            o_.Clear(); s3.sf.
GridTerminalSystem.GetBlocksOfType<IMyShipController>(o_); p0 = 0; if (oW != null && oW.CubeGrid.GetCubeBlock(oW.Position) != oX) oW = null;
        }
        if (
o_.Count > 0)
        {
            for (; p0 < o_.Count; p0++)
            {
                if (!s3.sp(20)) return false; IMyShipController f3 = o_[p0] as IMyShipController; if (f3.GetValueBool(
"MainCockpit") || f3.IsUnderControl)
                {
                    oW = f3; oX = f3.CubeGrid.GetCubeBlock(f3.Position); if (f3.GetValueBool("MainCockpit"))
                    {
                        p0 = o_.Count;
                        break;
                    }
                }
            }
            if (oW == null) { oW = o_[0] as IMyShipController; oX = oW.CubeGrid.GetCubeBlock(oW.Position); }
            oP = oW.CalculateShipMass(); if (!oW.
TryGetPlanetElevation(MyPlanetElevation.Sealevel, out oS)) oS = double.NaN; if (!oW.TryGetPlanetElevation(MyPlanetElevation.Surface, out
oU)) oU = double.NaN; oD = oW.GetShipVelocities();
        }
        double f4 = oG; oG = oE.Length(); oI = (oG - f4) / rZ; if (-oI > oK) oK = -oI; if (-oI > oM)
        {
            oM = -oI; oN = s3.sb
;
        }
        if (s3.sb - oN > 5 && -oI > 0.1) oM -= (oM + oI) * 0.3f; return true;
    }
}
public class td
{
    tq p1 = null; te p2; IMyCubeGrid p3
    {
        get
        {
            return p1.sf.Me.
CubeGrid;
        }
    }
    IMyGridTerminalSystem p4 { get { return p1.sf.GridTerminalSystem; } }
    public List<IMyTerminalBlock> p5 = new List<
IMyTerminalBlock>(); public td(tq f5, te f6) { p1 = f5; p2 = f6; }
    int p6 = 0; public double p7(ref double f7, ref double f8, bool f9)
    {
        if (!f9) p6 = 0
; for (; p6 < p5.Count; p6++)
        {
            if (!p1.sp(4)) return Double.NaN; IMyInventory fa = p5[p6].GetInventory(0); if (fa == null) continue; f7 += (double)fa.
CurrentVolume; f8 += (double)fa.MaxVolume;
        }
        f7 *= 1000; f8 *= 1000; return (f8 > 0 ? f7 / f8 * 100 : 100);
    }
    int p8 = 0; double p9 = 0; public double pa(bool
fb)
    {
        if (!fb) { p8 = 0; p9 = 0; }
        for (; p8 < p5.Count; p8++)
        {
            if (!p1.sp(6)) return Double.NaN; for (int fc = 0; fc < 2; fc++)
            {
                IMyInventory fd = p5[p8].
GetInventory(fc); if (fd == null) continue; p9 += (double)fd.CurrentMass;
            }
        }
        return p9 * 1000;
    }
    int pb = 0; bool pc(bool fe = false)
    {
        if (!fe) pb = 0;
        while (pb < p5.Count) { if (!p1.sp(4)) return false; if (p5[pb].CubeGrid != p3) { p5.RemoveAt(pb); continue; } pb++; }
        return true;
    }
    List<
IMyBlockGroup> pd = new List<IMyBlockGroup>(); int pe = 0; public bool pf(string ff, bool fg)
    {
        int fh = ff.IndexOf(':'); var fi = (fh >= 1 && fh <= 2 ?
ff.Substring(0, fh) : ""); var fj = fi.Contains("T"); if (fi != "") ff = ff.Substring(fh + 1); if (ff == "" || ff == "*")
        {
            if (!fg)
            {
                var fk = new List<
IMyTerminalBlock>(); p4.GetBlocks(fk); p5.AddList(fk);
            }
            if (fj) if (!pc(fg)) return false; return true;
        }
        var fl = (fi.Contains("G") ? ff.Trim()
.ToLower() : ""); if (fl != "")
        {
            if (!fg) { pd.Clear(); p4.GetBlockGroups(pd); pe = 0; }
            for (; pe < pd.Count; pe++)
            {
                IMyBlockGroup fm = pd[pe]; if (fm.Name
.ToLower() == fl) { if (!fg) fm.GetBlocks(p5); if (fj) if (!pc(fg)) return false; return true; }
            }
            return true;
        }
        if (!fg) p4.SearchBlocksOfName(ff,
p5); if (fj) if (!pc(fg)) return false; return true;
    }
    List<IMyBlockGroup> pg = new List<IMyBlockGroup>(); List<IMyTerminalBlock> ph = new List<
IMyTerminalBlock>(); int pi = 0; int pj = 0; public bool pk(string fn, string fo, bool fp, bool fq)
    {
        if (!fq)
        {
            pg.Clear(); p4.GetBlockGroups(pg)
; pi = 0;
        }
        for (; pi < pg.Count; pi++)
        {
            IMyBlockGroup fr = pg[pi]; if (fr.Name.ToLower() == fo)
            {
                if (!fq) { pj = 0; ph.Clear(); fr.GetBlocks(ph); }
                else fq =
false; for (; pj < ph.Count; pj++) { if (!p1.sp(6)) return false; if (fp && ph[pj].CubeGrid != p3) continue; if (p2.pz(ph[pj], fn)) p5.Add(ph[pj]); }
                return true;
            }
        }
        return true;
    }
    List<IMyTerminalBlock> pl = new List<IMyTerminalBlock>(); int pm = 0; public bool pn(string fs, string ft, bool
fu)
    {
        int fv = ft.IndexOf(':'); var fw = (fv >= 1 && fv <= 2 ? ft.Substring(0, fv) : ""); var fx = fw.Contains("T"); if (fw != "") ft = ft.Substring(fv + 1); if (
                                 !fu) { pl.Clear(); pm = 0; }
        var fy = (fw.Contains("G") ? ft.Trim().ToLower() : ""); if (fy != "") { if (!pk(fs, fy, fx, fu)) return false; return true; }
        if
(!fu) p2.py(ref pl, fs); if (ft == "" || ft == "*") { if (!fu) p5.AddList(pl); if (fx) if (!pc(fu)) return false; return true; }
        for (; pm < pl.Count; pm++)
        {
            if (!p1.sp(4)) return false; if (fx && pl[pm].CubeGrid != p3) continue; if (pl[pm].CustomName.Contains(ft)) p5.Add(pl[pm]);
        }
        return true;
    }
    public void po(td fz) { p5.AddList(fz.p5); }
    public void pp() { p5.Clear(); }
    public int pq() { return p5.Count; }
}
public class te
{
    tq pr;
    public MyGridProgram ps { get { return pr.sf; } }
    public IMyGridTerminalSystem pt { get { return pr.sf.GridTerminalSystem; } }
    IMyGridTerminalSystem pu = null; Dictionary<string, Action<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>> pv = null; public te(tq fA
              )
    { pr = fA; pu = pr.sf.GridTerminalSystem; }
    public void pw()
    {
        if (pv != null && pt.GetBlocksOfType<IMyCargoContainer> == pv["CargoContainer"])
            return; pv = new Dictionary<string, Action<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>>(){{"CargoContainer",pt.GetBlocksOfType
<IMyCargoContainer>},{"TextPanel",pt.GetBlocksOfType<IMyTextPanel>},{"Assembler",pt.GetBlocksOfType<IMyAssembler>},{"Refinery",pt.
GetBlocksOfType<IMyRefinery>},{"Reactor",pt.GetBlocksOfType<IMyReactor>},{"SolarPanel",pt.GetBlocksOfType<IMySolarPanel>},{
"BatteryBlock",pt.GetBlocksOfType<IMyBatteryBlock>},{"Beacon",pt.GetBlocksOfType<IMyBeacon>},{"RadioAntenna",pt.GetBlocksOfType<
IMyRadioAntenna>},{"AirVent",pt.GetBlocksOfType<IMyAirVent>},{"ConveyorSorter",pt.GetBlocksOfType<IMyConveyorSorter>},{
"OxygenTank",pt.GetBlocksOfType<IMyGasTank>},{"OxygenGenerator",pt.GetBlocksOfType<IMyGasGenerator>},{"OxygenFarm",pt.
GetBlocksOfType<IMyOxygenFarm>},{"LaserAntenna",pt.GetBlocksOfType<IMyLaserAntenna>},{"Thrust",pt.GetBlocksOfType<IMyThrust>},{
"Gyro",pt.GetBlocksOfType<IMyGyro>},{"SensorBlock",pt.GetBlocksOfType<IMySensorBlock>},{"ShipConnector",pt.GetBlocksOfType<
IMyShipConnector>},{"ReflectorLight",pt.GetBlocksOfType<IMyReflectorLight>},{"InteriorLight",pt.GetBlocksOfType<IMyInteriorLight>}
,{"LandingGear",pt.GetBlocksOfType<IMyLandingGear>},{"ProgrammableBlock",pt.GetBlocksOfType<IMyProgrammableBlock>},{"TimerBlock",
pt.GetBlocksOfType<IMyTimerBlock>},{"MotorStator",pt.GetBlocksOfType<IMyMotorStator>},{"PistonBase",pt.GetBlocksOfType<
IMyPistonBase>},{"Projector",pt.GetBlocksOfType<IMyProjector>},{"ShipMergeBlock",pt.GetBlocksOfType<IMyShipMergeBlock>},{
"SoundBlock",pt.GetBlocksOfType<IMySoundBlock>},{"Collector",pt.GetBlocksOfType<IMyCollector>},{"JumpDrive",pt.GetBlocksOfType<
IMyJumpDrive>},{"Door",pt.GetBlocksOfType<IMyDoor>},{"GravityGeneratorSphere",pt.GetBlocksOfType<IMyGravityGeneratorSphere>},{
"GravityGenerator",pt.GetBlocksOfType<IMyGravityGenerator>},{"ShipDrill",pt.GetBlocksOfType<IMyShipDrill>},{"ShipGrinder",pt.
GetBlocksOfType<IMyShipGrinder>},{"ShipWelder",pt.GetBlocksOfType<IMyShipWelder>},{"LargeGatlingTurret",pt.GetBlocksOfType<
IMyLargeGatlingTurret>},{"LargeInteriorTurret",pt.GetBlocksOfType<IMyLargeInteriorTurret>},{"LargeMissileTurret",pt.
GetBlocksOfType<IMyLargeMissileTurret>},{"SmallGatlingGun",pt.GetBlocksOfType<IMySmallGatlingGun>},{"SmallMissileLauncherReload",
pt.GetBlocksOfType<IMySmallMissileLauncherReload>},{"SmallMissileLauncher",pt.GetBlocksOfType<IMySmallMissileLauncher>},{
"VirtualMass",pt.GetBlocksOfType<IMyVirtualMass>},{"Warhead",pt.GetBlocksOfType<IMyWarhead>},{"FunctionalBlock",pt.GetBlocksOfType
<IMyFunctionalBlock>},{"LightingBlock",pt.GetBlocksOfType<IMyLightingBlock>},{"ControlPanel",pt.GetBlocksOfType<IMyControlPanel>},
{"Cockpit",pt.GetBlocksOfType<IMyCockpit>},{"MedicalRoom",pt.GetBlocksOfType<IMyMedicalRoom>},{"RemoteControl",pt.GetBlocksOfType<
IMyRemoteControl>},{"ButtonPanel",pt.GetBlocksOfType<IMyButtonPanel>},{"CameraBlock",pt.GetBlocksOfType<IMyCameraBlock>},{
"OreDetector",pt.GetBlocksOfType<IMyOreDetector>},{"ShipController",pt.GetBlocksOfType<IMyShipController>}};
    }
    public void px(ref
List<IMyTerminalBlock> fB, string fC)
    {
        Action<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>> fD = null; if (pv.TryGetValue(fC, out fD)
) fD(fB, null);
        else
        {
            if (fC == "CryoChamber")
            {
                pt.GetBlocksOfType<IMyCockpit>(fB, fE => fE.BlockDefinition.ToString().Contains("Cryo"));
                return;
            }
        }
    }
    public void py(ref List<IMyTerminalBlock> fF, string fG) { px(ref fF, pA(fG.Trim())); }
    public bool pz(IMyTerminalBlock fH,
string fI)
    {
        var fJ = pA(fI); switch (fJ)
        {
            case "FunctionalBlock": return true;
            case "ShipController":
                return (fH as IMyShipController != null)
;
            default: return fH.BlockDefinition.ToString().Contains(pA(fI));
        }
    }
    public string pA(string fK)
    {
        fK = fK.ToLower(); if (fK.StartsWith(
"carg") || fK.StartsWith("conta")) return "CargoContainer"; if (fK.StartsWith("text") || fK.StartsWith("lcd")) return "TextPanel"; if (fK.
StartsWith("ass")) return "Assembler"; if (fK.StartsWith("refi")) return "Refinery"; if (fK.StartsWith("reac")) return "Reactor"; if (fK.
StartsWith("solar")) return "SolarPanel"; if (fK.StartsWith("bat")) return "BatteryBlock"; if (fK.StartsWith("bea")) return "Beacon"; if (
fK.Contains("vent")) return "AirVent"; if (fK.Contains("sorter")) return "ConveyorSorter"; if (fK.Contains("tank")) return "OxygenTank";
        if (fK.Contains("farm") && fK.Contains("oxy")) return "OxygenFarm"; if (fK.Contains("gene") && fK.Contains("oxy")) return "OxygenGenerator"
                 ; if (fK.Contains("cryo")) return "CryoChamber"; if (fK == "laserantenna") return "LaserAntenna"; if (fK.Contains("antenna")) return
                            "RadioAntenna"; if (fK.StartsWith("thrust")) return "Thrust"; if (fK.StartsWith("gyro")) return "Gyro"; if (fK.StartsWith("sensor")) return
                                     "SensorBlock"; if (fK.Contains("connector")) return "ShipConnector"; if (fK.StartsWith("reflector")) return "ReflectorLight"; if ((fK.
                                            StartsWith("inter") && fK.EndsWith("light"))) return "InteriorLight"; if (fK.StartsWith("land")) return "LandingGear"; if (fK.StartsWith(
                                                   "program")) return "ProgrammableBlock"; if (fK.StartsWith("timer")) return "TimerBlock"; if (fK.StartsWith("motor")) return "MotorStator"
                                                          ; if (fK.StartsWith("piston")) return "PistonBase"; if (fK.StartsWith("proj")) return "Projector"; if (fK.Contains("merge")) return
                                                                   "ShipMergeBlock"; if (fK.StartsWith("sound")) return "SoundBlock"; if (fK.StartsWith("col")) return "Collector"; if (fK.Contains("jump"))
            return "JumpDrive"; if (fK == "door") return "Door"; if ((fK.Contains("grav") && fK.Contains("sphe"))) return "GravityGeneratorSphere"; if (fK
                       .Contains("grav")) return "GravityGenerator"; if (fK.EndsWith("drill")) return "ShipDrill"; if (fK.Contains("grind")) return
                              "ShipGrinder"; if (fK.EndsWith("welder")) return "ShipWelder"; if ((fK.Contains("turret") && fK.Contains("gatl"))) return
                                      "LargeGatlingTurret"; if ((fK.Contains("turret") && fK.Contains("inter"))) return "LargeInteriorTurret"; if ((fK.Contains("turret") && fK.
                                            Contains("miss"))) return "LargeMissileTurret"; if (fK.Contains("gatl")) return "SmallGatlingGun"; if ((fK.Contains("launcher") && fK.
                                                 Contains("reload"))) return "SmallMissileLauncherReload"; if ((fK.Contains("launcher"))) return "SmallMissileLauncher"; if (fK.Contains(
                                                      "mass")) return "VirtualMass"; if (fK == "warhead") return "Warhead"; if (fK.StartsWith("func")) return "FunctionalBlock"; if (fK == "shipctrl"
                                                                ) return "ShipController"; if (fK.StartsWith("light")) return "LightingBlock"; if (fK.StartsWith("contr")) return "ControlPanel"; if (fK.
                                                                        StartsWith("coc")) return "Cockpit"; if (fK.StartsWith("medi")) return "MedicalRoom"; if (fK.StartsWith("remote")) return "RemoteControl"
                                                                               ; if (fK.StartsWith("but")) return "ButtonPanel"; if (fK.StartsWith("cam")) return "CameraBlock"; if (fK.Contains("detect")) return
                                                                                        "OreDetector"; return "Unknown";
    }
}
public static class tf
{
    public static List<double> pB(IMyTerminalBlock fL, int fM = -1)
    {
        var fN = new
List<double>(); string[] fO = fL.DetailedInfo.Split('\n'); int fP = Math.Min(fO.Length, (fM > 0 ? fM : fO.Length)); for (int fQ = 0; fQ < fP; fQ++)
        {
            string[] fR = fO[fQ].Split(':'); if (fR.Length < 2) { fR = fO[fQ].Split('r'); if (fR.Length < 2) fR = fO[fQ].Split('x'); }
            var fS = (fR.Length < 2 ? fR[0] :
fR[1]); string[] fT = fS.Trim().Split(' '); var fU = fT[0].Trim(); var fV = (fT.Length > 1 && fT[1].Length > 1 ? fT[1][0] : '.'); double fW; if (Double.
TryParse(fU, out fW)) { double fX = fW * Math.Pow(1000.0, ".kMGTPEZY".IndexOf(fV)); fN.Add(fX); }
        }
        return fN;
    }
    public static string pC(
IMyBatteryBlock fY)
    {
        var fZ = ""; if (fY.GetValueBool("Recharge")) fZ = "(+) ";
        else if (fY.GetValueBool("Discharge")) fZ = "(-) ";
        else fZ =
"(±) "; return fZ + t8.nR((fY.CurrentStoredPower / fY.MaxStoredPower) * 100.0f) + "%";
    }
    public static string pD(IMyLaserAntenna f_)
    {
        string[]
g0 = f_.DetailedInfo.Split('\n'); return g0[g0.Length - 1].Split(' ')[0].ToUpper();
    }
    public static double pE(IMyJumpDrive g1, out double
g2, out double g3)
    { List<double> g4 = pB(g1, 5); if (g4.Count < 4) { g3 = 0; g2 = 0; return 0; } g3 = g4[1]; g2 = g4[3]; return (g3 > 0 ? g2 / g3 * 100 : 0); }
    public
static double pF(IMyJumpDrive g5)
    {
        List<double> g6 = pB(g5, 5); double g7 = 0, g8 = 0; if (g6.Count < 4) return 0; g7 = g6[1]; g8 = g6[3]; return (g7 > 0 ? g8
/ g7 * 100 : 0);
    }
    public static string pG(IMyLandingGear g9)
    {
        var ga = "udoesnp"; string[] gb = g9.DetailedInfo.Split('\n'); var gc = gb[gb.Length
- 1]; string[] gd = gc.Split(':'); string ge; if (gd.Length < 2)
        {
            ge = gd[0].Trim().ToLower(); if (ge.StartsWith("slot status")) ge = ge.Substring(
11);
        }
        else ge = gd[1].Trim().ToLower(); if (ge == "") return M.T["W8"]; if (ge.Split(' ').Length > 1) return M.T["W10"]; if ((ga.IndexOf(ge[0]) < 0
                ) && !ge.StartsWith("au")) return M.T["W7"]; return M.T["W8"];
    }
}
public static class MMItems
{
    public static Dictionary<string, tg> pH = new
Dictionary<string, tg>(); static Dictionary<string, tg> pI = new Dictionary<string, tg>(); public static List<string> pJ = new List<string>()
; public static Dictionary<string, tg> pK = new Dictionary<string, tg>(); public static void Add(string gf, string gg, int gh, string gi,
string gj, bool gk)
    {
        if (gg == "Ammo") gg = "AmmoMagazine"; else if (gg == "Tool") gg = "PhysicalGunObject"; var gl = gf + ' ' + gg; tg gm = new tg(gf, gg,
          gh, gi, gj, gk); pH.Add(gl, gm); if (!pI.ContainsKey(gf)) pI.Add(gf, gm); if (gj != "") pK.Add(gj.ToLower(), gm); pJ.Add(gl);
    }
    public static tg pL(
string gn = "", string go = "")
    {
        if (pH.ContainsKey(gn + " " + go)) return pH[gn + " " + go]; if (go == "")
        {
            tg gp = null; pI.TryGetValue(gn, out gp);
            return gp;
        }
        if (gn == "") for (int gq = 0; gq < pH.Count; gq++) { tg gp = pH[pJ[gq]]; if (go == gp.pN) return gp; }
        return null;
    }
}
public class tg
{
    public
string pM; public string pN; public int pO; public string pP; public string pQ; public bool pR; public tg(string gs, string gt, int gu = 0,
string gv = "", string gw = "", bool gx = true)
    { pM = gs; pN = gt; pO = gu; pP = gv; pQ = gw; pR = gx; }
}
public class th
{
    static Dictionary<string, string> pS =
new Dictionary<string, string>(){{"ingot","ingot" },{"ore","ore" },{"component","component" },{"tool","physicalgunobject" },{"ammo"
,"ammomagazine" },{"oxygen","oxygencontainerobject" },{"gas","gascontainerobject" }}; tq pT; tj pU; tj pV; tj pW; bool pX; public tj pY;
    public th(tq gy, int gz = 20) { pU = new tj(); pV = new tj(); pW = new tj(); pX = false; pY = new tj(); pT = gy; }
    public void pZ()
    {
        pW.ql(); pV.ql(); pU.ql(
); pX = false; pY.ql();
    }
    public void p_(string gA, bool gB = false, int gC = 1, int gD = -1)
    {
        if (gA == "") { pX = true; return; }
        string[] gE = gA.Split(' ')
; var gF = ""; ti gG = new ti(gB, gC, gD); if (gE.Length == 2) { if (!pS.TryGetValue(gE[1], out gF)) gF = gE[1]; }
        var gH = gE[0]; if (pS.TryGetValue(gH,
out gG.qc)) { pV.qh(gG.qc, gG); return; }
        t8.nP(ref gH, ref gF); if (gF == "") { gG.qb = gH.ToLower(); pU.qh(gG.qb, gG); return; }
        gG.qb = gH; gG.qc = gF;
        pW.qh(gH.ToLower() + ' ' + gF.ToLower(), gG);
    }
    public ti q0(string gI, string gJ, string gK)
    {
        ti gL; gI = gI.ToLower(); gL = pW.qj(gI); if (gL !=
null) return gL; gJ = gJ.ToLower(); gL = pU.qj(gJ); if (gL != null) return gL; gK = gK.ToLower(); gL = pV.qj(gK); if (gL != null) return gL; return null;
    }
    public bool q1(string gM, string gN, string gO)
    {
        ti gP; var gQ = false; gP = pV.qj(gO.ToLower()); if (gP != null)
        {
            if (gP.qd) return true; gQ = true;
        }
        gP = pU.qj(gN.ToLower()); if (gP != null) { if (gP.qd) return true; gQ = true; }
        gP = pW.qj(gM.ToLower()); if (gP != null)
        {
            if (gP.qd) return true; gQ =
true;
        }
        return !(pX || gQ);
    }
    public ti q2(string gR, string gS, string gT)
    {
        ti gU = new ti(); gR = gR.ToLower(); ti gV = q0(gR, gS.ToLower(), gT.
ToLower()); if (gV != null) { gU.q9 = gV.q9; gU.qa = gV.qa; }
        gU.qb = gS; gU.qc = gT; pY.qh(gR, gU); return gU;
    }
    public ti q3(string gW, string gX, string
gY)
    { ti gZ = pY.qj(gW.ToLower()); if (gZ == null) gZ = q2(gW, gX, gY); return gZ; }
    int q4 = 0; List<ti> q5; public List<ti> q6(string g_, bool h0)
    {
        if (!
h0) { q5 = new List<ti>(); q4 = 0; }
        for (; q4 < pY.qi(); q4++)
        {
            if (!pT.sp(5)) return null; ti h1 = pY.qk(q4); if (q1((h1.qb + ' ' + h1.qc).ToLower(), h1.qb
, h1.qc)) continue; if (h1.qc == g_) q5.Add(h1);
        }
        return q5;
    }
    int q7 = 0; public bool q8(bool h2)
    {
        if (!h2) { q7 = 0; }
        for (; q7 < MMItems.pJ.Count; q7++)
        {
            if (!pT.sp(10)) return false; tg h3 = MMItems.pH[MMItems.pJ[q7]]; if (!h3.pR) continue; var h4 = h3.pM + ' ' + h3.pN; if (q1(h4, h3.pM, h3.pN))
                continue; ti h5 = q3(h4, h3.pM, h3.pN); if (h5.qa == -1) h5.qa = h3.pO;
        }
        return true;
    }
}
public class ti
{
    public int q9; public int qa; public
string qb = ""; public string qc = ""; public bool qd; public double qe; public ti(bool h6 = false, int h7 = 1, int h8 = -1) { q9 = h7; qd = h6; qa = h8; }
}
public class tj
{
    Dictionary<string, ti> qf = new Dictionary<string, ti>(); List<string> qg = new List<string>(); public void qh(string h9, ti
ha)
    { if (!qf.ContainsKey(h9)) { qg.Add(h9); qf.Add(h9, ha); } }
    public int qi() { return qf.Count; }
    public ti qj(string hb)
    {
        if (qf.ContainsKey(
hb)) return qf[hb]; return null;
    }
    public ti qk(int hc) { return qf[qg[hc]]; }
    public void ql() { qg.Clear(); qf.Clear(); }
    public void qm()
    {
        qg
.Sort();
    }
}
public class tk
{
    tq qn; public MyDefinitionId qo = new MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.
MyObjectBuilder_GasProperties), "Electricity"); public MyDefinitionId qp = new MyDefinitionId(typeof(VRage.Game.ObjectBuilders.
Definitions.MyObjectBuilder_GasProperties), "Oxygen"); public MyDefinitionId qq = new MyDefinitionId(typeof(VRage.Game.ObjectBuilders.
Definitions.MyObjectBuilder_GasProperties), "Hydrogen"); public tk(tq hd) { qn = hd; }
    int qr = 0; public bool qs(List<IMyTerminalBlock> he,
ref double hf, ref double hg, ref double hh, ref double hi, ref double hj, ref double hk, bool hl)
    {
        if (!hl) qr = 0; MyResourceSinkComponent
hm; MyResourceSourceComponent hn; for (; qr < he.Count; qr++)
        {
            if (!qn.sp(8)) return false; if (he[qr].Components.TryGet<
MyResourceSinkComponent>(out hm)) { hf += hm.CurrentInputByType(qo); hg += hm.MaxRequiredInputByType(qo); }
            if (he[qr].Components.TryGet<
MyResourceSourceComponent>(out hn)) { hh += hn.CurrentOutputByType(qo); hi += hn.MaxOutputByType(qo); }
            hj += (he[qr] as IMyBatteryBlock).
CurrentStoredPower; hk += (he[qr] as IMyBatteryBlock).MaxStoredPower;
        }
        return true;
    }
    int qt = 0; public bool qu(List<IMyTerminalBlock> ho,
MyDefinitionId hp, ref double hq, ref double hr, ref double hs, ref double ht, bool hu)
    {
        if (!hu) qt = 0; MyResourceSinkComponent hv;
        MyResourceSourceComponent hw; for (; qt < ho.Count; qt++)
        {
            if (!qn.sp(6)) return false; if (ho[qt].Components.TryGet<MyResourceSinkComponent>
(out hv)) { hq += hv.CurrentInputByType(hp); hr += hv.MaxRequiredInputByType(hp); }
            if (ho[qt].Components.TryGet<MyResourceSourceComponent>(
out hw)) { hs += hw.CurrentOutputByType(hp); ht += hw.MaxOutputByType(hp); }
        }
        return true;
    }
    int qv = 0; public bool qw(List<IMyTerminalBlock> hx
, string hy, ref double hz, ref double hA, bool hB)
    {
        hy = hy.ToLower(); if (!hB) { qv = 0; hA = 0; hz = 0; }
        MyResourceSinkComponent hC; for (; qv < hx.
Count; qv++)
        {
            if (!qn.sp(30)) return false; IMyGasTank hD = hx[qv] as IMyGasTank; if (hD == null) continue; double hE = 0; if (hD.Components.TryGet<
               MyResourceSinkComponent>(out hC))
            {
                ListReader<MyDefinitionId> hF = hC.AcceptedResources; int hG = 0; for (; hG < hF.Count; hG++)
                {
                    if (hF[hG].
SubtypeId.ToString().ToLower() == hy) { hE = 100; hA += hE; hz += hE * hD.FilledRatio; break; }
                }
            }
        }
        return true;
    }
}
public class tl
{
    tn qx = null; public
tm qy = new tm(); public to qz = null; public IMyTextPanel qA = null; public int qB = 0; public string qC = ""; public string qD = ""; public bool
qE = true; public tl(tn hH, string hI) { qx = hH; qD = hI; }
    public bool qF() { return qz.rF.Count > qz.rB || qz.rC != 0; }
    public void qG(float hJ)
    {
        for (
int hK = 0; hK < qy.qO(); hK++) qy.qQ(hK).SetValueFloat("FontSize", hJ);
    }
    public void qH()
    {
        qy.qS(); qA = qy.qQ(0); int hL = qA.CustomName.IndexOf
("!MARGIN:"); if (hL < 0 || hL + 8 >= qA.CustomName.Length) { qB = 1; qC = " "; }
        else
        {
            var hM = qA.CustomName.Substring(hL + 8); int hN = hM.IndexOf(" "); if
(hN >= 0) hM = hM.Substring(0, hN); if (!int.TryParse(hM, out qB)) qB = 1; qC = new String(' ', qB);
        }
        if (qA.CustomName.Contains("!NOSCROLL")) qE =
false;
        else qE = true;
    }
    public bool qI()
    {
        return (qA.BlockDefinition.SubtypeId.Contains("Wide") || qA.DefinitionDisplayNameText ==
"Computer Monitor");
    }
    public void qJ()
    {
        if (qz == null || qA == null) return; float hO = qA.GetValueFloat("FontSize"); for (int hP = 0; hP < qy.qO();
hP++)
        {
            IMyTextPanel hQ = qy.qQ(hP); if (hP > 0) hQ.SetValueFloat("FontSize", hO); hQ.WritePublicText(qz.rR(hP)); if (qx.r0) hQ.
                  ShowTextureOnScreen(); hQ.ShowPublicTextOnScreen();
        }
    }
}
public class tm
{
    Dictionary<string, IMyTextPanel> qK = new Dictionary<string,
IMyTextPanel>(); List<string> qL = new List<string>(); public void qM(string hR, IMyTextPanel hS)
    {
        if (!qL.Contains(hR))
        {
            qL.Add(hR); qK.Add
(hR, hS);
        }
    }
    public void qN(string hT) { if (qL.Contains(hT)) { qL.Remove(hT); qK.Remove(hT); } }
    public int qO() { return qK.Count; }
    public
IMyTextPanel qP(string hU)
    { if (qL.Contains(hU)) return qK[hU]; return null; }
    public IMyTextPanel qQ(int hV) { return qK[qL[hV]]; }
    public
void qR()
    { qL.Clear(); qK.Clear(); }
    public void qS() { qL.Sort(); }
}
public class tn
{
    public const float qV = 512 / 0.7783784f; public string
qX = "T:[LCD]"; public int qY = 1; public bool qZ = true; public List<string> q_ = null; public bool r0 = true; public bool r1 = false; public float
r2 = 1.0f; public float r3 = 1.0f; public float r4; float r5; float r6; float r7; int r8 = 0; string r9 = ""; tq ra; public MyGridProgram rb; public
tk rc; public te rd; public tc re; public IMyGridTerminalSystem rf { get { return rb.GridTerminalSystem; } }
    public IMyProgrammableBlock rg
    {
        get { return rb.Me; }
    }
    public Action<string> rh { get { return rb.Echo; } }
    public tn(MyGridProgram hW, bool hX, tq hY)
    {
        ra = hY; r1 = hX; rb = hW; rc = new
tk(hY); rd = new te(hY); rd.pw(); tr.sy(); r5 = tr.sv(' '); r6 = tr.sv('\''); r7 = tr.sv('['); r4 = tr.sw(" 100.0%"); re = new tc(ra); ra.sh(re, 0);
    }
    Dictionary<IMyTextPanel, to> ri = new Dictionary<IMyTextPanel, to>(); public to rj = null; public to rk(to hZ, tl h_)
    {
        h_.qH(); IMyTextPanel
i0 = h_.qA; if (hZ == null) hZ = new to(this, i0.GetValueFloat("FontSize")); else hZ.rI(i0.GetValueFloat("FontSize")); hZ.rJ(h_.qy.qO()); hZ.rA
= (h_.qI() ? 2.0f : 1.0f) * r2 / hZ.rz; r9 = h_.qC; r8 = h_.qB; rj = hZ; return hZ;
    }
    public void rl(tl i1) { rj = i1.qz; }
    public void rm(string i2)
    {
        if (rj.
rH <= 0) i2 = r9 + i2; rj.rO(i2);
    }
    public void rn(string i3) { rj.rN(i3, r9); }
    public void ro(List<string> i4) { rj.rL(i4); }
    public void Add(string
i5)
    { if (rj.rH <= 0) i5 = r9 + i5; rj.rK(i5); rj.rH += tr.sw(i5); }
    public void rp(string i6, float i7 = 1.0f, float i8 = 0f) { rq(i6, i7, i8); rm(""); }
    public void rq(string i9, float ia = 1.0f, float ib = 0f)
    {
        float ic = tr.sw(i9); float id = ia * qV * rj.rA - rj.rH - ib; if (r8 > 0) id -= r5 * r8; if (id < ic)
        {
            rj.rK(i9); rj.rH += ic; return;
        }
        id -= ic; int ie = (int)Math.Floor(id / r5); float ig = ie * r5; rj.rK(new String(' ', ie) + i9); rj.rH += ig + ic;
    }
    public
void rr(string ih)
    { rs(ih); rm(""); }
    public void rs(string ii)
    {
        float ij = tr.sw(ii); float ik = qV / 2 * rj.rA - rj.rH; if (ik < ij / 2)
        {
            rj.rK(ii); rj.
rH += ij; return;
        }
        ik -= ij / 2; int il = (int)Math.Round(ik / r5, MidpointRounding.AwayFromZero); float im = il * r5; rj.rK(new String(' ', il) + ii); rj
             .rH += im + ij;
    }
    public void rt(double io, float ip = 1.0f, float iq = 0f)
    {
        if (r8 > 0) iq += r8 * r5 * ((rj.rH <= 0) ? 2 : 1); float ir = qV * ip * rj.rA - rj.rH - iq;
        if (Double.IsNaN(io)) io = 0; int it = (int)(ir / r6) - 2; if (it <= 0) it = 2; int iu = Math.Min((int)(io * it) / 100, it); if (iu < 0) iu = 0; rj.rO((rj.rH <= 0 ? r9 :
                                          "") + "[" + new String('|', iu) + new String('\'', it - iu) + "]");
    }
    public void ru(double iv, float iw = 1.0f, float ix = 0f)
    {
        if (r8 > 0) ix += r8 * r5 * ((rj
.rH <= 0) ? 2 : 1); float iy = qV * iw * rj.rA - rj.rH - ix; if (Double.IsNaN(iv)) iv = 0; int iz = (int)(iy / r6) - 2; if (iz <= 0) iz = 2; int iA = Math.Min((int)(iv *
iz) / 100, iz); if (iA < 0) iA = 0; rj.rK((rj.rH <= 0 ? r9 : "") + "[" + new String('|', iA) + new String('\'', iz - iA) + "]"); rj.rH += (rj.rH <= 0 ? r8 * r5 : 0) + r6 * iz
+ 2 * r7;
    }
    public void rv() { rj.rP(); }
    public void rw(tl iB) { iB.qJ(); if (iB.qE) rj.rV(); }
    public void rx(string iC, string iD)
    {
        IMyTextPanel
iE = rb.GridTerminalSystem.GetBlockWithName(iC) as IMyTextPanel; if (iE == null) return; iE.WritePublicText(iD + "\n", true);
    }
}
public class to
{
    tn ry = null; public float rz = 1.0f; public float rA = 1.0f; public int rB = 17; public int rC = 0; int rD = 1; int rE = 1; public List<string> rF = new
                        List<string>(); public int rG = 0; public float rH = 0; public to(tn iF, float iG = 1.0f) { ry = iF; rI(iG); rF.Add(""); }
    public void rI(float iH)
    {
        rz = iH; rB = (int)Math.Floor(512 / 0.7783784f * ry.r3 * rE / rz / 37);
    }
    public void rJ(int iI)
    {
        rE = iI; rB = (int)Math.Floor(512 / 0.7783784f * ry.r3 * rE /
rz / 37);
    }
    public void rK(string iJ) { rF[rG] += iJ; }
    public void rL(List<string> iK)
    {
        if (rF[rG] == "") rF.RemoveAt(rG); else rG++; rF.AddList(iK
); rG += iK.Count; rF.Add(""); rH = 0;
    }
    public List<string> rM() { if (rF[rG] == "") return rF.GetRange(0, rG); else return rF; }
    public void rN(
string iL, string iM = "")
    { string[] iN = iL.Split('\n'); for (int iO = 0; iO < iN.Length; iO++) rO(iM + iN[iO]); }
    public void rO(string iP)
    {
        rF[rG] +=
iP; rF.Add(""); rG++; rH = 0;
    }
    public void rP() { rF.Clear(); rF.Add(""); rH = 0; rG = 0; }
    public string rQ() { return String.Join("\n", rF); }
    public
string rR(int iQ = 0)
    {
        if (rF.Count < rB / rE) { if (iQ != 0) return ""; rC = 0; rD = 1; return rQ(); }
        int iR = rC + iQ * (rB / rE); if (iR > rF.Count) iR = rF.Count;
        List<string> iS = rF.GetRange(iR, Math.Min(rF.Count - iR, rB / rE)); return String.Join("\n", iS);
    }
    public bool rS(int iT = -1)
    {
        if (iT <= 0) iT = ry.
qY; if (rC - iT <= 0) { rC = 0; return true; }
        rC -= iT; return false;
    }
    public bool rT(int iU = -1)
    {
        if (iU <= 0) iU = ry.qY; int iV = rF.Count - 1; if (rC + iU + rB >=
iV) { rC = Math.Max(iV - rB, 0); return true; }
        rC += iU; return false;
    }
    public int rU = 0; public void rV()
    {
        if (rU > 0) { rU--; return; }
        if (rF.Count - 1 <=
rB) { rC = 0; rD = 1; return; }
        if (rD > 0) { if (rT()) { rD = -1; rU = 2; } } else { if (rS()) { rD = 1; rU = 2; } }
    }
}
public class tp
{
    public string rW = "MMTask"; public
double rX = 0; public double rY = 0; public double rZ = 0; public double r_ = -1; public bool s0 = false; public bool s1 = false; double s2 = 0;
    protected tq s3; public void s4(tq iW) { s3 = iW; }
    protected bool s5(double iX) { s2 = Math.Max(iX, 0.0001); return true; }
    public bool s6()
    {
        if (
rY > 0) { rZ = s3.sb - rY; s3.sj((s1 ? "Running" : "Resuming") + " task: " + rW); s1 = Run(!s1); }
        else
        {
            rZ = 0; s3.sj("Init task: " + rW); Init(); s3.sj(
"Running.."); s1 = Run(false); if (!s1) rY = 0.001;
        }
        if (s1) { rY = s3.sb; if ((r_ >= 0 || s2 > 0) && s0) s3.sh(this, (s2 > 0 ? s2 : r_)); else { s0 = false; rY = 0; } }
        else { if (s0) s3.sh(this, 0, true); }
        s3.sj("Task " + (s1 ? "" : "NOT ") + "finished. " + (s0 ? (s2 > 0 ? "Postponed by " + s2.ToString("F1") + "s" :
"Scheduled after " + r_.ToString("F1") + "s") : "Stopped.")); s2 = 0; return s1;
    }
    public void s7() { s3.si(this); End(); s0 = false; s1 = false; rY = 0; }
    public virtual void Init() { }
    public virtual bool Run(bool iY) { return true; }
    public virtual void End() { }
}
public class tq
{
    public
double sb
    { get { return sd; } }
    int sc = 1000; double sd = 0; List<tp> se = new List<tp>(100); public MyGridProgram sf; bool sg = true; public tq(
MyGridProgram iZ, bool i_ = true)
    { sf = iZ; sg = i_; }
    public void sh(tp j0, double j1, bool j2 = false)
    {
        sj("Scheduling task: " + j0.rW + " after " +
j1.ToString("F2")); j0.s0 = true; j0.s4(this); if (j2) { j0.rX = sb; se.Insert(0, j0); return; }
        if (j1 <= 0) j1 = 0.001; j0.rX = sb + j1; for (int j3 = 0; j3 < se
.Count; j3++) { if (se[j3].rX > j0.rX) { se.Insert(j3, j0); return; } if (j0.rX - se[j3].rX < 0.05) j0.rX = se[j3].rX + 0.05; }
        se.Add(j0);
    }
    public void si
(tp j4)
    { if (se.Contains(j4)) { se.Remove(j4); j4.s0 = false; } }
    public void sj(string j5) { if (sg) sf.Echo(j5); }
    public void sk()
    {
        double j6 = sf
.Runtime.TimeSinceLastRun.TotalSeconds; sd += j6; sj("Total time: " + sd.ToString("F1") + " Time Step: " + j6.ToString("F2")); sc = (int)Math.
Min((j6 * 60) * 1000, 20000 - 1000); sj("Total tasks: " + se.Count + " InstrPerRun: " + sc); while (se.Count >= 1)
        {
            tp j7 = se[0]; if (sc - sf.Runtime.
CurrentInstructionCount <= 0) break; if (j7.rX > sd) break; se.Remove(j7); if (!j7.s6()) break; sj("Done. NextExecTime: " + j7.rX.ToString("F1"))
; sj("Remaining Instr: " + so().ToString());
        }
    }
    int sl = 0; StringBuilder sm = new StringBuilder(); public void sn()
    {
        double j8 = sf.Runtime.
LastRunTimeMs * 1000; if (sl == 5000)
        {
            IMyTextPanel j9 = sf.GridTerminalSystem.GetBlockWithName("AUTOLCD Profiler") as IMyTextPanel; if (j9 ==
null) return; j9.WritePublicText(sm.ToString()); sl++; return;
        }
        sm.Append(sl).Append(";").AppendLine(j8.ToString("F2")); sl++;
    }
    public
int so()
    { return (20000 - sf.Runtime.CurrentInstructionCount); }
    public bool sp(int ja)
    {
        return ((20000 - sf.Runtime.CurrentInstructionCount
) >= ja);
    }
    public void sq() { sj("Remaining Instr: " + so().ToString()); }
}
public static class tr
{
    static Dictionary<char, float> sr = new
Dictionary<char, float>(); static float ss = 1f; static float st = 10f; public static float su = -1f; static void AddCharsSize(string jb,
float jc)
    { jc += 1; for (int jd = 0; jd < jb.Length; jd++) { if (jc > ss) ss = jc; sr.Add(jb[jd], jc); } }
    public static float sv(char je)
    {
        float jf; if (!sr
.TryGetValue(je, out jf)) return ss; return jf;
    }
    public static float sw(string jg)
    {
        if (su > 0) return jg.Length * ss; float jh = 0; for (int ji = 0
; ji < jg.Length; ji++) jh += sv(jg[ji]); return jh;
    }
    public static string sx(string jj, float jk)
    {
        if (jk / ss >= jj.Length) return jj; float jl = sw
(jj); if (jl <= jk) return jj; float jm = jl / jj.Length; jk -= st; int jn = (int)(jk / jm); jj = jj.Substring(0, jn); jl = sw(jj); while (jl > jk)
        {
            jn--; jl -= sv
(jj[jn]); jj.Remove(jn);
        }
        return jj + "..";
    }
    public static void sy()
    {
        if (sr.Count > 0) return; if (su > 0) { ss = su + 1; st = 2 * ss; return; }

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

public static class M
{
    public static readonly Dictionary<string, string> T = new Dictionary<string, string>
{
// TRANSLATION STRINGS
// msg id, text
{ "AC1", "Acceleration:" },
{ "A1", "EMPTY" }, // amount
{ "ALT1", "Altitude:"}, // NEW
{ "ALT2", "Ground:"}, // NEW
{ "B1", "Booting up..." },
{ "C1", "count:" },
{ "C2", "Cargo Used:" },
{ "C3", "Invalid countdown format, use:" },
{ "C4", "EXPIRED" },
{ "C5", "days" },
{ "D1", "You need to enter name." },
{ "D2", "No blocks found." },
{ "D3", "No damaged blocks found." },
{ "DTU", "Invalid GPS format" }, // NEW
{ "GA", "Artif."}, // NEW (don't use more than 5 characters)
{ "GN", "Natur."}, // NEW (don't use more than 5 characters)
{ "GT", "Total"}, // NEW (don't use more than 5 characters)
{ "G1", "Total Gravity:"}, // NEW
{ "G2", "Natur. Gravity:"}, // NEW
{ "G3", "Artif. Gravity:"}, // NEW
{ "H1", "Write commands to Custom Data of this panel." }, // UPDATED
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
{ "PU1", "Power Used:" }, // NEW
{ "S1", "Speed:" },
{ "SM1", "Ship Mass:" }, // NEW
{ "SM2", "Ship Base Mass:" }, // NEW
{ "SD", "Stop Distance:" }, // NEW
{ "ST", "Stop Time:" }, // NEW
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