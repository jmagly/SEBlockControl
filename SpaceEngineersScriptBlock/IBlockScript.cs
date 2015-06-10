namespace SpaceEngineersScriptBlock
{
    using System;
    using System.Collections.Generic;
    using System.Collections;

    using Sandbox.ModAPI;

    /// <summary>
    /// Helper interface for running tests while allowing for code to be easily copied and pasted into SE
    /// </summary>
    public interface IBlockScript : IMyGridProgram
    {
        Queue<string> EchoOutput { get; }

        /// <summary>
        /// Wrapper for calling void Main(string)
        /// </summary>
        /// <param name="argument">arg data</param>
        void MainMethod(string argument);

        /// <summary>
        /// Post test cleanup - TODO: Deprecate
        /// </summary>
        void CleanUp();
    }
}
