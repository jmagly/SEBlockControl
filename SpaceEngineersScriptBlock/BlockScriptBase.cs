using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace SpaceEngineersScriptBlock
{
    public abstract class BlockScriptBase : IBlockScript
    {
        public BlockScriptBase(IMyGridTerminalSystem gts)
        {
            GridTerminalSystem = gts;
        }

        protected static IMyGridTerminalSystem GridTerminalSystem = null;

        public abstract void MainMethod(string argument);

        public virtual void CleanUp()
        {
            
        }
    }
}
