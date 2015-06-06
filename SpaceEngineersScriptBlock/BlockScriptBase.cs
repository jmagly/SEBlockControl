using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common;
using VRage;
using VRageMath;

namespace SpaceEngineersScriptBlock
{
    public abstract class BlockScriptBase : IBlockScript
    {
        private bool hasMainMethod = true; 

        public BlockScriptBase(IMyGridTerminalSystem gts): base()
        {
            GridTerminalSystem = gts;
        }

        public void Main(string arg) { MainMethod(arg); }

        public abstract void MainMethod(string argument);

        public virtual void CleanUp() { }

        public virtual IMyGridTerminalSystem GridTerminalSystem
        {
            get;
            set;
        }

        public virtual Action<string> Echo
        {
            get;
            set;
        }

        public virtual TimeSpan ElapsedTime
        {
            get;
            set;
        }

        public virtual bool HasMainMethod
        {
            get
            {
                return hasMainMethod;
            }
            set
            {
                hasMainMethod = value;
            }
        }

        public virtual IMyProgrammableBlock Me
        {
            get;
            set;
        }

        public virtual string Storage
        {
            get;
            set;
        }
    }
}
