namespace SpaceEngineersScriptBlock.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;    
    using System.Threading.Tasks;

    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;

    using Moq;

    using Xunit;

    using BSET.Mocks;
    using BSET.Testing;

    public class AirLockControlTests : BlockScriptTest
    {
        static object timerlock = new object();

        private AirlockControl airlockControl;

        private AutoResetEvent pressureActionComplete = new AutoResetEvent(false);

        private int o2level = 0;

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

        protected override string ExecutionBlockName
        {
            get
            {
                return "CPU - Airlock Control - Debug";
            }
        }

        [Fact]
        public void Initialize_Control_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var control = new AirlockControl(CurrentGts, ExecutionBlock);
            control.Main("");

            var expectedQueue = CreateStartQueue();

            ValidateActionLog(expectedQueue);
        }

        [Fact]
        public void Exterior_Access_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var control = new AirlockControl(CurrentGts, ExecutionBlock);
            control.Main(AirlockControl.ExteriorAccessName);

            var expectedQueue = CreateStartQueue();

            foreach (var vent in VentNames)
            {
                EnqueueAction(expectedQueue, vent, AirlockControl.OffAction);
            }

            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.OpenAction);

            ValidateActionLog(expectedQueue);
        }

        [Fact]
        public void Interior_Access_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var control = this.airlockControl = new AirlockControl(CurrentGts, ExecutionBlock);
            control.Main(AirlockControl.InteriorAccessName);

            // Wait for depressurization
            System.Threading.Thread.Sleep(50);

            var expectedQueue = CreateStartQueue();

            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.DepressurizeOffAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OnAction);
            EnqueueAction(expectedQueue, TimerBlockName, AirlockControl.StartPressureTimeAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OffAction);
            EnqueueAction(expectedQueue, SupplyVentName, AirlockControl.OnAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.OpenAction);

            ValidateActionLog(expectedQueue);
        }

        [Fact]
        public void Exterior_to_Interior_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var control = this.airlockControl = new AirlockControl(CurrentGts, ExecutionBlock);
            control.Main(AirlockControl.ExteriorAccessName);

            // Wait for depressurization
            System.Threading.Thread.Sleep(50);

            control.Main(AirlockControl.TransferAccessName);

            // Wait for depressurization
            System.Threading.Thread.Sleep(50);

            var expectedQueue = CreateStartQueue();

            EnqueueAction(expectedQueue, SupplyVentName, AirlockControl.OffAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OffAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.OpenAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.DepressurizeOffAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OnAction);
            EnqueueAction(expectedQueue, TimerBlockName, AirlockControl.StartPressureTimeAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OffAction);
            EnqueueAction(expectedQueue, SupplyVentName, AirlockControl.OnAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.OpenAction);

            ValidateActionLog(expectedQueue);
        }

        [Fact]
        public void Interior_to_Exterior_Test()
        {
            const string exepctedStartLog = "Info: ============== Initializing Airlock System v0.0.1 - Alpha ==============\nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nInfo:  EXTREMELY Unstable test system! Use at your own risk! \nInfo:  ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---\nDebug: \nDebug: Main Airlock Controller\nDebug: CPU - Airlock Control DEBUG\nDebug: \nDebug: Locating Airlock Doors\nDebug: Door - Interior Airlock\nDebug: Door - Exterior Airlock\nDebug: \nDebug: Locating Airlock Vents\nDebug: Vent - Supply Airlock\nDebug: Vent - Drain Airlock\nDebug: \nDebug: Locating Pressurization Timer\nDebug: Timer - Airlock Control\nDebug: \nDebug: Sealing Airlock\nDebug: Airlock Sealed\nInfo: --== System Started ==--\nInfo: ";

            var gts = CreateGrid();
            var control = this.airlockControl = new AirlockControl(CurrentGts, ExecutionBlock);
            control.Main(AirlockControl.InteriorAccessName);

            System.Threading.Thread.Sleep(50);

            control.Main(AirlockControl.TransferAccessName);

            System.Threading.Thread.Sleep(50);

            var expectedQueue = CreateStartQueue();

            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.DepressurizeOffAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OnAction);
            EnqueueAction(expectedQueue, TimerBlockName, AirlockControl.StartPressureTimeAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OffAction);
            EnqueueAction(expectedQueue, SupplyVentName, AirlockControl.OnAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.OpenAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, SupplyVentName, AirlockControl.OffAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.DepressurizeOnAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OnAction);
            EnqueueAction(expectedQueue, TimerBlockName, AirlockControl.StartPressureTimeAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, InteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(expectedQueue, DrainVentName, AirlockControl.OffAction);
            EnqueueAction(expectedQueue, ExteriorDoorName, AirlockControl.OpenAction);

            ValidateActionLog(expectedQueue);
        }

        protected override IBlockGridData AcquireTestData()
        {
            var data = base.AcquireTestData();

            AddBlockData<IMyTextPanel>(data, PanelNames);
            AddBlockData<IMyDoor>(data, DoorNames);
            AddBlockData<IMyAirVent>(data, VentNames);
            AddBlockData<IMyTimerBlock>(data, TimerBlockName);

            return data;
        }

        protected override Mock<T> CreateMock<T>(string name, Action<ExecutedAction> getActionWithName = null, Action<ExecutedAction> apply = null)
        {
            if (typeof(T).Equals(typeof(IMyTimerBlock)) && name.Equals(TimerBlockName))
            {
                getActionWithName = (action) =>
                    {
                        var an = action.Action;
                        if (an == AirlockControl.StartPressureTimeAction)
                        {
                            // The game kicks off the timer async
                            new System.Threading.Tasks.Task(
                              () =>
                              {
                                  lock (timerlock)
                                  {
                                      // Minimum wait time 50 milliseconds
                                      System.Threading.Thread.Sleep(10);
                                      this.o2level = this.o2level == 0 ? 100 : 0;
                                      this.airlockControl.Main(string.Empty);
                                  }

                              }, System.Threading.Tasks.TaskCreationOptions.LongRunning).Start();
                        }
                    };
            }

            var mock = base.CreateMock<T>(name, getActionWithName, apply);

            if (typeof(T).Equals(typeof(IMyAirVent)) && VentNames.Contains(((IMyAirVent)mock.Object).CustomName))
            {
                var airVentMock = mock as Mock<IMyAirVent>;
                airVentMock.Setup(v => v.GetOxygenLevel()).Returns(
                    () =>
                    {
                        return this.o2level;
                    });
            }

            return mock;
        }

        private Queue<ExecutedAction> CreateStartQueue()
        {
            var queue = new Queue<ExecutedAction>();
            EnqueueAction(queue, ExteriorDoorName, AirlockControl.CloseAction);
            EnqueueAction(queue, InteriorDoorName, AirlockControl.CloseAction);
            return queue;
        }

        private void AddBlockData<T>(IBlockGridData gridData, List<string> names) where T : class
        {
            foreach (var name in names)
            {
                AddBlockData<T>(gridData, name);
            }
        }

        private void AddBlockData<T>(IBlockGridData gridData, string name) where T : class
        {
            gridData.Blocks.Add(new MockBlockData<T>(name));
        }

        private void ValidateActionLog(Queue<ExecutedAction> expectedQueue)
        {
            while (expectedQueue.Count > 0)
            {
                var expected = expectedQueue.Dequeue();
                var actual = ExecutionLog.Dequeue();

                Assert.True(expected.Equals(actual));
            }
        }
    }
}
