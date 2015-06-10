namespace SpaceEngineersScriptBlockTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Sandbox.ModAPI.Ingame;

    using Moq;

    using SpaceEngineersScriptBlock;

    [TestClass]
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

        private List<IMyTerminalBlock> programmableBlocks;

        [TestMethod]
        public void Forward_Simple_Command_Test()
        {
            var intialArguments = "--forwardTo::CPU --testarg1::HELLO WORLD!";
            var forwardedArguments = "--testarg1::HELLO WORLD!";

            var control = CreateControl();

            control.Main(intialArguments);
            //control.Main("--forwardTo::CPU -testarg1::HELLO WORLD!");
            //control.Main("--forwardTo::CPU -testarg1::HELLO WORLD!");

            foreach (var name in BlockNames)
            {
                Assert.IsTrue(ExecutionLog.Any(b => b.Name == name));
            }

            foreach (var action in ExecutionLog)
            {
                Assert.AreEqual(1, action.Parameters.Count);
                Assert.AreEqual(forwardedArguments, action.Parameters[0].Value);
            }
        }

        protected override Mock<IMyGridTerminalSystem> CreateGridMock()
        {
            var gtsMock = base.CreateGridMock();

            gtsMock.Setup(g => g.GetBlocksOfType<IMyProgrammableBlock>(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>(
                    (blocks, collect) =>
                    {
                        if (programmableBlocks == null)
                        {
                            programmableBlocks = BlockNames.Select(n => (IMyTerminalBlock)CreateProgrammableBlock(n).Object).ToList();
                        }

                        blocks.AddRange(programmableBlocks);
                    });

            return gtsMock;
        }

        private CommandForwardControl CreateControl()
        {
            var gtsMock = CreateGridMock();
            var control = new CommandForwardControl(gtsMock.Object);
            control.Me = CreateProgrammableBlock("CPU - Test - Debug::Enabled Info::Enabled").Object;

            return control;
        }

    }
}
