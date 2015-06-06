namespace SpaceEngineersScriptBlock
{
    using System;
    using Sandbox.ModAPI;

    /// <summary>
    /// Helper interface for running tests while allowing for code to be easily copied and pasted into SE
    /// </summary>
    public interface IBlockScript : IMyGridProgram
    {
        /// <summary>
        /// Wrapper for calling void Main(string)
        /// </summary>
        /// <param name="argument">arg data</param>
        void MainMethod(string argument);

        /// <summary>
        /// Post test cleanup - TODO: Depracate
        /// </summary>
        void CleanUp();
    }
}
