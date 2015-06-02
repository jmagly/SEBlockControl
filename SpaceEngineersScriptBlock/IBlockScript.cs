using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEngineersScriptBlock
{
    public interface IBlockScript
    {
        void MainMethod(string argument);
        void CleanUp();
    }
}
