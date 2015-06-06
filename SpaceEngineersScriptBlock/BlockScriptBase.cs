namespace SpaceEngineersScriptBlock
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Text;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.Common;
    using VRage;
    using VRageMath;

    /// <summary>
    /// Base heper for block script testing support
    /// </summary>
    public abstract class BlockScriptBase : IBlockScript
    {
        private bool hasMainMethod = true; 

        public BlockScriptBase(IMyGridTerminalSystem gts): base()
        {
            GridTerminalSystem = gts;
        }

        /// <summary>
        /// Main method wrapper to allow for compliation in IDE without modifying Main(string) signature in implemtors
        /// </summary>
        /// <param name="arg"></param>
        public void Main(string arg) { MainMethod(arg); }

        /// <summary>
        /// Forced impl requirment on inheritors to allow for correct test wireup
        /// </summary>
        /// <param name="argument"></param>
        public abstract void MainMethod(string argument);

        public virtual void CleanUp() { }

        #region IMyGridProgram Members
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
        #endregion
    }
}
