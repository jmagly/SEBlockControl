namespace SpaceEngineersScriptBlock.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;

    using Moq;

    using Xunit;

    using BSET.Mocks;
    using BSET.Testing;

    public class CommandForwardControlTests : BlockScriptTest
    {
        static List<string> BlockNames = new List<string>()
            {
                "CPU - System A - Subsystem A",
                "CPU - System A - Subsystem B",
                "CPU - System B - Subsystem A",
                "CPU - System B - Subsystem B",
                "CPU - System C - Subsystem A",
                "CPU - System C - Subsystem B",
                "CPU - System C - Subsystem C",
            };

        [Fact]
        public void My_Test_1()
        {
            var intialArguments = "--forwardTo::CPU --testarg1::HELLO WORLD!";
            var forwardedArguments = "--testarg1::HELLO WORLD!";

            var control = CreateControl();

            control.Main(intialArguments);
            //control.Main("--forwardTo::CPU -testarg1::HELLO WORLD!");
            //control.Main("--forwardTo::CPU -testarg1::HELLO WORLD!");

            foreach (var name in BlockNames)
            {
                Assert.True(ExecutionLog.Any(b => b.Name == name));
            }

            foreach (var action in ExecutionLog)
            {
                Assert.True(1 == action.Parameters.Count);
                Assert.True(forwardedArguments.Equals(action.Parameters[0].Value));
            }
        }

        protected override IBlockGridData AcquireTestData()
        {
            var data = base.AcquireTestData();

            foreach (var name in BlockNames)
            {
                data.Blocks.Add(new MockBlockData(name, typeof(IMyProgrammableBlock)));
            }

            return data;
        }

        private CommandForwardControl CreateControl()
        {
            var control = new CommandForwardControl(CurrentGts, ExecutionBlock);

            return control;
        }
    }
}
