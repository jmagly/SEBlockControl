namespace SpaceEngineersScriptBlockTests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;

    using Moq;

    using SpaceEngineersScriptBlock;
using System.Threading;

    /// <summary>
    /// Summary description for AirlockControlTests
    /// </summary>
    [TestClass]
    public class AirlockControlTests
    {
        private class ExecutedAction
        {
            public string Name { get; set; }
            public string Action {get; set; }

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

        private AutoResetEvent pressureActionComplete = new AutoResetEvent(false);

        private Queue<ExecutedAction> executionLog = new Queue<ExecutedAction>();

        private Dictionary<string, string> displayOutputs = new Dictionary<string, string>();

        private int o2level = 0;

        private AirlockControl airlockControl;

        static string ProgrammableBlockName = "CPU - Airlock Control - Debug";
        static string TimerBlockName = "Timer - Airlock Control";

        static List<string> PanelNames = new List<string>()
            {
                "LCD - Exterior Airlock - Debug Status NoScroll",
                "LCD - Inner Airlock - Debug Status NoScroll",
                "LCD - Interior Airlock - Debug Status NoScroll"
            };

        const string ExteriorDoorName = "Door - Exterior Airlock";
        const string InteriorDoorName = "Door - Interior Airlock";
        static List<string> DoorNames = new List<string>()
            {
                ExteriorDoorName,
                InteriorDoorName
            };

        const string SupplyVentName = "Vent - Supply Airlock";
        const string DrainVentName = "Vent - Drain Airlock";
        static List<string> VentNames = new List<string>()
            {
                SupplyVentName,
                DrainVentName
            };


        public AirlockControlTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup() 
        {
            o2level = 0;

            airlockControl.CleanUp();
            airlockControl = null;

            if (displayOutputs != null)
            {
                displayOutputs.Clear();
                displayOutputs = new Dictionary<string, string>();
            }

            if (executionLog != null)
            {
                executionLog.Clear();
                executionLog = new Queue<ExecutedAction>();
            }
        }
        //
        #endregion

        [TestMethod]
        public void Initialize_Control_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var gts = CreateGrid();
            var control = this.airlockControl = new AirlockControl(gts);
            control.Main("");

            /*foreach (var display in displayOutputs)
            {
                Assert.AreEqual(exepctedStartLog.Trim(), display.Value.Trim());
            }*/

            var expectedQueue = CreateStartQueue();

            while (expectedQueue.Count > 0)
            {
                var expected = expectedQueue.Dequeue();
                var actual = executionLog.Dequeue();

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Exterior_Access_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var gts = CreateGrid();
            var control = this.airlockControl = new AirlockControl(gts);
            control.Main(AirlockControl.ExteriorAccessName);

            var expectedQueue = CreateStartQueue();

            foreach (var vent in VentNames)
            {
                expectedQueue.Enqueue(new ExecutedAction() { Name = vent, Action = AirlockControl.OffAction });
            }

            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.OpenAction });

            while (expectedQueue.Count > 0)
            {
                var expected = expectedQueue.Dequeue();
                var actual = executionLog.Dequeue();

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Interior_Access_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var gts = CreateGrid();
            var control = this.airlockControl = new AirlockControl(gts);
            control.Main(AirlockControl.InteriorAccessName);

            // Wait for depressurization
            System.Threading.Thread.Sleep(50);

            var expectedQueue = CreateStartQueue();

            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.DepressurizeOffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = TimerBlockName, Action = AirlockControl.StartPressureTimeAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = SupplyVentName, Action = AirlockControl.OnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.OpenAction });

            while (expectedQueue.Count > 0)
            {
                var expected = expectedQueue.Dequeue();
                var actual = executionLog.Dequeue();

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Exterior_to_Interior_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var gts = CreateGrid();
            var control = this.airlockControl = new AirlockControl(gts);
            control.Main(AirlockControl.ExteriorAccessName);

            // Wait for depressurization
            System.Threading.Thread.Sleep(50);

            control.Main(AirlockControl.TransferAccessName);

            // Wait for depressurization
            System.Threading.Thread.Sleep(50);

            var expectedQueue = CreateStartQueue();

            expectedQueue.Enqueue(new ExecutedAction() { Name = SupplyVentName, Action = AirlockControl.OffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.OpenAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.DepressurizeOffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = TimerBlockName, Action = AirlockControl.StartPressureTimeAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = SupplyVentName, Action = AirlockControl.OnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.OpenAction });

            while (expectedQueue.Count > 0)
            {
                var expected = expectedQueue.Dequeue();
                var actual = executionLog.Dequeue();

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Interior_to_Exterior_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var gts = CreateGrid();
            var control = this.airlockControl = new AirlockControl(gts);
            control.Main(AirlockControl.InteriorAccessName);

            System.Threading.Thread.Sleep(50);

            control.Main(AirlockControl.TransferAccessName);

            System.Threading.Thread.Sleep(50);

            var expectedQueue = CreateStartQueue();

            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.DepressurizeOffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = TimerBlockName, Action = AirlockControl.StartPressureTimeAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = SupplyVentName, Action = AirlockControl.OnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.OpenAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = SupplyVentName, Action = AirlockControl.OffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.DepressurizeOnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OnAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = TimerBlockName, Action = AirlockControl.StartPressureTimeAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.CloseAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = DrainVentName, Action = AirlockControl.OffAction });
            expectedQueue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.OpenAction });

            while (expectedQueue.Count > 0)
            {
                var expected = expectedQueue.Dequeue();
                var actual = executionLog.Dequeue();

                Assert.AreEqual(expected, actual);
            }
        }

        private Queue<ExecutedAction> CreateStartQueue()
        {
            var queue = new Queue<ExecutedAction>();
            queue.Enqueue(new ExecutedAction() { Name = ExteriorDoorName, Action = AirlockControl.CloseAction });
            queue.Enqueue(new ExecutedAction() { Name = InteriorDoorName, Action = AirlockControl.CloseAction });
            return queue;
        }

        private IMyGridTerminalSystem CreateGrid()
        {
            var mockGrid = new Mock<IMyGridTerminalSystem>();
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

            return mockGrid.Object;
        }

        private Mock<IMyProgrammableBlock> CreateProgrammableBlock(string name)
        {
            var mock = new Mock<IMyProgrammableBlock>();

            SetName(name, mock);
            SetTerminalAction(name, mock);

            return mock;
        }

        static object timerlock = new object();

        private Mock<IMyTimerBlock> CreateTimerBlock(string name)
        {
            var mock = new Mock<IMyTimerBlock>();


            SetName(name, mock);
            SetTerminalAction(name, mock, 
                (an) => 
                    {
                        if (an == AirlockControl.StartPressureTimeAction)
                        {
                            // The game kicks off the timer async
                            new System.Threading.Tasks.Task(
                              () =>
                               {
                                   lock(timerlock)
                                   {
                                        // Minimum wait time 50 milliseconds
                                        System.Threading.Thread.Sleep(10);
                                        this.o2level = this.o2level == 0 ? 100 : 0;
                                        this.airlockControl.Main(string.Empty);
                                   }

                                }, System.Threading.Tasks.TaskCreationOptions.LongRunning).Start();
                        }
                    });

            return mock;
        }

        private Mock<IMyTextPanel> CreatePanel(string name)
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

        private Mock<IMyDoor> CreateDoor(string name)
        {
            var mock = new Mock<IMyDoor>();
            
            SetName(name, mock);
            SetTerminalAction(name, mock);

            return mock;
        }

        private Mock<IMyAirVent> CreateVent(string name)
        {
            var mock = new Mock<IMyAirVent>();

            SetName(name, mock);
            SetTerminalAction(name, mock);

            mock.Setup(v => v.GetOxygenLevel()).Returns(this.o2level);

            return mock;
        }

        private void SetName(string name, Mock moq)
        {
            var mock = moq.As<IMyTerminalBlock>();
            mock.Setup(p => p.CustomName).Returns(name);
        }

        private void SetTerminalAction(string name, Mock moq, Action<string> action = null)
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

        private void PressureOperationComplete()
        {
            pressureActionComplete.Set();
        }
    }
}
