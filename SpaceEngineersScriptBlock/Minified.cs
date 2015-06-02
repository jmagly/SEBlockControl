using System;
using System.Collections.Generic;
using System.Text;
//using Sandbox.ModAPI;  // !!NOT AVAILABLE
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\VRage.Common.dll
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\VRage.Math.dll
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\Sandbox.Common.dll
//Only 5 game namespaces are allowed in Programmable blocks
//http://steamcommunity.com/sharedfiles/filedetails/?id=360966557
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;
namespace SpaceEngineersScriptBlock.Minified
{
            

    class Minified
    {
        static IMyGridTerminalSystem GridTerminalSystem = null;

const string f="Open_Off";void Main(
string a){var j=new List<IMyTerminalBlock>();GridTerminalSystem.GetBlocksOfType<
IMyDoor>(j);j=j.FindAll(k=>k.CustomName.Contains("Airlock"));var l=j.FindAll(k=>
k.CustomName.Contains("Inner"));var m=j.FindAll(k=>k.CustomName.Contains(
"Outter"));var n=new List<IMyTerminalBlock>();GridTerminalSystem.GetBlocksOfType
<IMyAirVent>(n);j=j.FindAll(k=>k.CustomName.Contains("Airlock"));var o=j.FindAll
(k=>k.CustomName.Contains("Supply"));var p=j.FindAll(k=>k.CustomName.Contains(
"Drain"));switch(a){case("InteriorAccess"):{v(l,m,o,p);break;}case("Transfer"):{
w(l,m,o,p);break;}case("ExteriorAccess"):{x(l,m,o,p);break;}}}static void u(List
<IMyTerminalBlock>a,string b){for(int c=0;c<a.Count;c++){var d=a[c];d.
GetActionWithName(b).Apply(d);}}static void v(List<IMyTerminalBlock>a,List<
IMyTerminalBlock>b,List<IMyTerminalBlock>c,List<IMyTerminalBlock>d){u(a,
"Open_On");u(b,f);}static void w(List<IMyTerminalBlock>a,List<IMyTerminalBlock>b
,List<IMyTerminalBlock>c,List<IMyTerminalBlock>d){u(a,f);u(b,f);}static void x(
List<IMyTerminalBlock>a,List<IMyTerminalBlock>b,List<IMyTerminalBlock>c,List<
IMyTerminalBlock>d){u(a,f);u(b,"Open_On");}

    }
}
