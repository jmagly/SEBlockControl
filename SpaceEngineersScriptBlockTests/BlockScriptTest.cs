namespace SpaceEngineersScriptBlockTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sandbox.ModAPI.Ingame;
    using System.Collections.Generic;
    using Sandbox.ModAPI.Interfaces;

    public abstract class BlockScriptTest
    {
        private Dictionary<string, string> displayOutputs = new Dictionary<string, string>();

        private Queue<ExecutedAction> executionLog = new Queue<ExecutedAction>();

        protected Dictionary<string, string> DisplayOutputs
        {
            get
            {
                return displayOutputs;
            }
        }

        protected Queue<ExecutedAction> ExecutionLog
        {
            get
            {
                return executionLog;
            }
        }

        protected IMyGridTerminalSystem CreateGrid()
        {
            return CreateGridMock().Object;
        }

        protected Mock<IMyGridTerminalSystem> CreateGridMock()
        {
            var mockGrid = new Mock<IMyGridTerminalSystem>();

            /*
            mockGrid.Setup(s => s.GetBlocksOfType<IMyProgrammableBlock>(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>((blocks, collect) => blocks.Add(CreateProgrammableBlock(ProgrammableBlockName).Object));

            mockGrid.Setup(s => s.GetBlocksOfType<IMyTimerBlock>(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>((blocks, collect) => blocks.Add(CreateTimerBlock(TimerBlockName).Object));

            mockGrid.Setup(g => g.GetBlocksOfType<IMyTextPanel>(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>((blocks, collect) => blocks.AddRange(PanelNames.Select(n => (IMyTerminalBlock)CreatePanel(n).Object)));

            mockGrid.Setup(s => s.GetBlocksOfType<IMyDoor>(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>((blocks, collect) => blocks.AddRange(DoorNames.Select(n => (IMyTerminalBlock)CreateDoor(n).Object)));

            mockGrid.Setup(s => s.GetBlocksOfType<IMyAirVent>(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>((blocks, collect) => blocks.AddRange(VentNames.Select(n => (IMyTerminalBlock)CreateVent(n).Object)));
            */

            return mockGrid;
        }

        protected Mock<IMyProgrammableBlock> CreateProgrammableBlock(string name)
        {
            var mock = new Mock<IMyProgrammableBlock>();

            SetName(name, mock);
            SetTerminalAction(name, mock);

            return mock;
        }

        protected Mock<IMyTimerBlock> CreateTimerBlock(string name)
        {
            var mock = new Mock<IMyTimerBlock>();

            SetName(name, mock);
            SetTerminalAction(name, mock);

            return mock;
        }

        protected Mock<IMyTextPanel> CreatePanel(string name)
        {
            var mock = new Mock<IMyTextPanel>();

            SetName(name, mock);
            SetTerminalAction(name, mock);

            this.displayOutputs[name] = string.Empty;

            mock.Setup(p => p.WritePublicText(It.IsAny<string>(), It.IsAny<bool>())).Callback(new Action<string, bool>((message, append) => this.displayOutputs[name] = message));
            mock.Setup(p => p.GetPublicText()).Returns(
                () =>
                {
                    return this.displayOutputs[name];
                });

            return mock;
        }

        protected Mock<IMyDoor> CreateDoor(string name)
        {
            var mock = new Mock<IMyDoor>();

            SetName(name, mock);
            SetTerminalAction(name, mock);

            return mock;
        }

        protected Mock<IMyAirVent> CreateVent(string name)
        {
            var mock = new Mock<IMyAirVent>();

            SetName(name, mock);
            SetTerminalAction(name, mock);

            //mock.Setup(v => v.GetOxygenLevel()).Returns(this.o2level);

            return mock;
        }

        protected void SetName(string name, Mock moq)
        {
            var mock = moq.As<IMyTerminalBlock>();
            mock.Setup(p => p.CustomName).Returns(name);
        }

        protected void SetTerminalAction(string name, Mock moq, Action<string> action = null)
        {
            var mock = moq.As<IMyTerminalBlock>();

            ExecutedAction executedAction = null;

            var mockAction = new Mock<ITerminalAction>();
            mockAction.Setup(a => a.Apply(It.IsAny<IMyCubeBlock>()))
                .Callback<IMyCubeBlock>((block) => this.executionLog.Enqueue(executedAction));

            mock.Setup(b => b.GetActionWithName(It.IsAny<string>())).Returns<string>((an) =>
            {
                executedAction = new ExecutedAction() { Name = name, Action = an };

                if (action != null)
                {
                    action(an);
                }

                return mockAction.Object;
            });
        }

        protected class ExecutedAction
        {
            public string Name { get; set; }
            public string Action { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is ExecutedAction))
                {
                    return false;
                }

                var action = (ExecutedAction)obj;

                return this.Action.Equals(action.Action) && this.Name.Equals(action.Name);
            }

            public override string ToString()
            {
                return Name + " : " + Action;
            }
        }
    }
}
