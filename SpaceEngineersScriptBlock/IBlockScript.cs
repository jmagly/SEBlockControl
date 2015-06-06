namespace SpaceEngineersScriptBlock
{
    using System;

    using Sandbox.ModAPI;

    public interface IBlockScript : IMyGridProgram
    {
        void MainMethod(string argument);
        void CleanUp();
    }
}
