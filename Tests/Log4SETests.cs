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

    public class Log4SETests : BlockScriptTest
    {
        private const string SystemCpuName = "CPU - Log4SETest - Debug::Enabled Info::Enabled";

        [Fact]
        public void Logger_Test()
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
            //control.Main("--clear");

            // Validating tests with asserts here is time consuming - TODO: Better tests.
            var echolog = control.EchoOutput;
        }

        protected override IBlockGridData AcquireTestData()
        {
            var data = base.AcquireTestData();

            // Update any LCDs in the data set to include the default system name for testing - these will end up being "default" displays
            foreach (var blockData in data.Blocks.Where(b => b.BlockType.Equals(typeof(IMyTextPanel))))
            {
                blockData.Name = string.Format("{0} - {1}", Log4SEControl.DefaultDisplaySystemName, blockData.Name);
            }

            // Add A dedicated display for each output type
            var diplayNameFormat = "{0} {1} {2}";
            data.Blocks.Add(new MockBlockData(string.Format(diplayNameFormat, Log4SEControl.DefaultDisplaySystemName, Log4SEControl.DebugBlockName, Log4SEControl.NoScrollOption), typeof(IMyTextPanel)));
            data.Blocks.Add(new MockBlockData(string.Format(diplayNameFormat, Log4SEControl.DefaultDisplaySystemName, Log4SEControl.InfoBlockName, Log4SEControl.NoScrollOption), typeof(IMyTextPanel)));
            data.Blocks.Add(new MockBlockData(string.Format(diplayNameFormat, Log4SEControl.DefaultDisplaySystemName, Log4SEControl.WarningBlockName, Log4SEControl.NoScrollOption), typeof(IMyTextPanel)));
            data.Blocks.Add(new MockBlockData(string.Format(diplayNameFormat, Log4SEControl.DefaultDisplaySystemName, Log4SEControl.ErrorBlockName, Log4SEControl.NoScrollOption), typeof(IMyTextPanel)));
            data.Blocks.Add(new MockBlockData(string.Format(diplayNameFormat, Log4SEControl.DefaultDisplaySystemName, Log4SEControl.FatalBlockName, Log4SEControl.NoScrollOption), typeof(IMyTextPanel)));

            // Add test output display
            data.Blocks.Add(new MockBlockData(string.Format("{0} {1}", Log4SEControl.ControlDebugDisplayName, Log4SEControl.NoScrollOption), typeof(IMyTextPanel)));

            // Add programmable block that "runs" the BlockScript
            data.Blocks.Add(new MockBlockData(SystemCpuName, typeof(IMyProgrammableBlock)));

            return data;
        }

        protected override Moq.Mock<T> CreateMock<T>(string name, Action<ExecutedAction> getActionWithName = null, Action<ExecutedAction> apply = null)
        {
            var blockType = typeof(T);
            var mock = base.CreateMock<T>(name, getActionWithName, apply);

            if (blockType.Equals(typeof(IMyTextPanel)))
            {
                // Add support for font size TerminalProperty
                var termMock = mock.As<IMyTextPanel>();
                termMock.Setup(b => b.GetProperty(It.IsAny<string>())).Returns(() =>
                    {
                        var propertyMock = new Mock<ITerminalProperty>();
                     
                        // Required for property lookup/casting to output value
                        propertyMock.Setup(p => p.Id).Returns("FontSize");
                        propertyMock.Setup(p => p.TypeName).Returns("float");

                        // Required for get FontSize property
                        Single fontSize = 1;
                        var genericPropertySingle = propertyMock.As<ITerminalProperty<Single>>();
                        genericPropertySingle.Setup(g => g.GetValue(It.IsAny<IMyCubeBlock>())).Returns(fontSize);

                        return propertyMock.Object;
                    });
            }

            return mock;
        }

        private Log4SEControl CreateLogControl()
        {
            var control = new Log4SEControl(CurrentGts, (IMyProgrammableBlock)CurrentGts.GetBlockWithName(SystemCpuName));
            return control;
        }
    }
}
