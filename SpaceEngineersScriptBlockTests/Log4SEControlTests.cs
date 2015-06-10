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
    public class Log4SEControlTests : BlockScriptTest
    {
        static List<string> PanelNames = new List<string>()
            {
                "LCD - SEBlockControl::Test::Log4SEControl NoScroll",
                "LCD - SEBlockControl::Test::Log4SEControl Debug Info Warning Error Fatal NoScroll",
                "LCD - Debug NoScroll",
                "LCD - Info NoScroll",
                "LCD - Warning NoScroll",
                "LCD - Error NoScroll",
                "LCD - Fatal NoScroll",
                "LCD - NoScroll",
            };

        private List<IMyTerminalBlock> panelBlocks;

        [TestMethod]
        public void Startup_Test()
        {
            var control = CreateLogControl();

            control.Main(string.Empty);

            //control.Main("--clear");
            //control.Main("--clear --severity::Debug --message::I am A post clear message");
            control.Main("--severity::Debug --message::This is my debug message, there are many like it but this one is mine!");
            control.Main("--severity::Info --message::Here is some info, I hope it is what you need!");
            control.Main("--severity::Warn --message::WARNING - something kind of bad has happened - you might want to check this out");
            control.Main("--severity::Error --message::ERROR - something bad has happened - fix it before it gets worse");
            control.Main("--severity::Fatal --message::FATAL - something REALL bas has happened - ABANDON SHIP!");

            // Validating tests with asserts here is time consuming - TODO: Better tests.

            var echolog = control.EchoOutput;
        }

        protected override Mock<IMyGridTerminalSystem> CreateGridMock()
        {
            var gtsMock = base.CreateGridMock();

            gtsMock.Setup(g => g.GetBlocksOfType<IMyTextPanel>(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>(
                    (blocks, collect) =>
                    {
                        if (panelBlocks == null)
                        {
                            panelBlocks = PanelNames.Select(n => (IMyTerminalBlock)CreatePanel(n).Object).ToList();
                        }

                        blocks.AddRange(panelBlocks);
                    });

            return gtsMock;
        }

        private Log4SEControl CreateLogControl()
        {
            var gtsMock = CreateGridMock();
            var control = new Log4SEControl(gtsMock.Object);
            control.Me = CreateProgrammableBlock("CPU - Test - Debug::Enabled Info::Enabled").Object;

            return control;
        }
    }
}
