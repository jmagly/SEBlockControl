namespace SpaceEngineersScriptBlock
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Text;

    using VRage;
    using VRageMath;

    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.Common.ObjectBuilders;

    using BSET.ScriptDevelopment;

    /// <summary>
    /// Simple airlock control script with 3 state control and pressure checks to prevent bad access. 
    /// </summary>
    public class AirlockControl : BlockScriptBase
    {
        public AirlockControl(IMyGridTerminalSystem gts, IMyProgrammableBlock executingBlock) : base(gts, executingBlock) { }

        public override void MainMethod(string argument)
        {
            Main(argument);
        }

        #region Game Code
        private class AirlockState
        {
            public const string TransferInReady = "TransferInReady";
            public const string TransferOutReady = "TransferOutReady";
            public const string SystemOffline = "SystemOffline";
            public const string Idle = "Idle";

            private string state = SystemOffline;

            private AirlockState(string state)
            {
                this.state = state;
            }

            public static implicit operator AirlockState(string state)
            {
                return new AirlockState(state);
            }

            public static explicit operator string(AirlockState state)
            {
                return state.ToString();
            }

            public static bool operator ==(string left, AirlockState right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(string left, AirlockState right)
            {
                return !left.Equals(right);
            }

            public static bool operator ==(AirlockState left, string right)
            {
                return right.Equals((string)left);
            }

            public static bool operator !=(AirlockState left, string right)
            {
                return !right.Equals((string)left);
            }

            public override string ToString()
            {
                return state;
            }

            public override bool Equals(object obj)
            {
                if (obj == null && (state == null || state == ""))
                {
                    return true;
                }

                if (obj == null || (!(obj is AirlockState) && !(obj is string)))
                {
                    return false;
                }

                return obj is string ? state == (string)obj : state == obj.ToString();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public const string OpenAction = "Open_On";
        public const string CloseAction = "Open_Off";
        public const string ToggleOpenCloseAction = "Open/Closed";

        public const string OnAction = "OnOff_On";
        public const string OffAction = "OnOff_Off";
        public const string ToggleOnOffAction = "OnOff";

        public const string DepressurizeOnAction = "Depressurize_On";
        public const string DepressurizeOffAction = "Depressurize_Off";
        public const string ToggleDepressurizeAction = "Depressurize_Off";

        public const string StartPressureTimeAction = "Start";

        public const string InteriorAccessName = "InteriorAccess";
        public const string TransferAccessName = "Transfer";
        public const string ExteriorAccessName = "ExteriorAccess";
        public const string PressureTimerAccessName = "PressureTimerComplete";
        public const string SystemStatus = "SystemStatus";

        public const string AirlockSystemName = "Airlock";
        public const string InteriorDoorName = "Interior";
        public const string ExteriorDoorName = "Exterior";
        public const string SupplyVentName = "Supply";
        public const string DrainVentName = "Drain";
        public const string DebugBlockName = "Debug";
        public const string StatusBlockName = "Status";

        public const string NoScrollOption = "NoScroll";

        private const int TextLinesPerFontPoint = 17;

        private static AirlockState State = AirlockState.SystemOffline;

        private List<IMyTerminalBlock> InnerDoors;
        private List<IMyTerminalBlock> OutterDoors;

        private List<IMyTerminalBlock> SupplyVents;
        private List<IMyTerminalBlock> DrainVents;

        private List<IMyTerminalBlock> StatusDisplays;
        private List<IMyTerminalBlock> DebugDisplays;

        private IMyTimerBlock PressureTimer;

        private IMyProgrammableBlock Controller;

        private bool debugMode;

        private string argument = "";

        void Main(string argument)
        {
            this.argument = argument;

            Init();

            switch (argument)
            {
                case (InteriorAccessName):
                    {
                        Info("Interior Access Requested");
                        InteriorAccess();
                        break;
                    }
                case (TransferAccessName):
                    {
                        Info("Transfer Requested");
                        Transfer();
                        break;
                    }
                case (ExteriorAccessName):
                    {
                        Info("Exterior Access Requested");
                        ExteriorAccess();
                        break;
                    }
                case (PressureTimerAccessName):
                    {
                        Info("Pressure Timer Trigger Complete");
                        PressureTimerComplete();
                        break;
                    }
                case (SystemStatus):
                    {
                        SystemStatusMessage();
                        break;
                    }
                case (""):
                    {
                        if (State == AirlockState.TransferInReady || State == AirlockState.TransferOutReady)
                        {
                            Info("Pressure Timer Trigger Complete");
                            PressureTimerComplete();
                        }

                        break;
                    }
            }
        }

        private void Init()
        {
            var initialized = State != AirlockState.SystemOffline;

            var panels = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);
            panels = panels.FindAll(d => d.CustomName.Contains(AirlockSystemName));
            StatusDisplays = panels.FindAll(d => d.CustomName.Contains(StatusBlockName));
            DebugDisplays = panels.FindAll(d => d.CustomName.Contains(DebugBlockName));;

            if (!initialized)
            {
                ClearDisplays();
                StartupHeader();
                DisplayStatus();
            }

            var controllers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(controllers);
            controllers = controllers.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            Controller = (IMyProgrammableBlock)controllers[0];

            debugMode = Controller.CustomName.Contains(DebugBlockName);

            if (!initialized)
            {
                ControllerStatus();
            }

            var doors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);
            doors = doors.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            InnerDoors = doors.FindAll(d => d.CustomName.Contains(InteriorDoorName));
            OutterDoors = doors.FindAll(d => d.CustomName.Contains(ExteriorDoorName));
            
            if (!initialized)
            {
                DoorStatus();
            }

            var vents = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyAirVent>(vents);
            vents = vents.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            SupplyVents = vents.FindAll(d => d.CustomName.Contains(SupplyVentName));
            DrainVents = vents.FindAll(d => d.CustomName.Contains(DrainVentName));

            if (!initialized)
            {
                VentStatus();
            }

            var timers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(timers);
            timers = timers.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            PressureTimer = (IMyTimerBlock)timers[0];

            if (!initialized)
            {
                TimerStatus();
            }

            SealAirlock();

            State = State == AirlockState.SystemOffline ? AirlockState.Idle : State;

            if (!initialized)
            {
                Info("--== System Started ==--");
                Info();
            }
        }

        private void InteriorAccess()
        {
            State = AirlockState.TransferOutReady;
            Pressurize();
        }

        private void Transfer()
        {
            switch ((string)State)
            {
                case (AirlockState.TransferInReady):
                    {
                        TransferToInterior();
                        break;
                    }
                case (AirlockState.TransferOutReady):
                    {
                        TransferToExterior();
                        break;
                    }
                default:
                    {
                        Debug("Error Invalid transfer request using state {0}", State);
                        break;
                    }
            }
        }

        private void TransferToInterior()
        {
            Debug("Transfering to Interior");

            Pressurize();
        }

        private void TransferToExterior()
        {
            Debug("Transfering to Exterior");

            Depressurize();
        }

        private void ExteriorAccess()
        {
            State = AirlockState.TransferInReady;
            Depressurize();
        }

        private void Pressurize()
        {
            if (GetIsPressurized())
            {
                Debug("Already Pressurized - Skipping");
                ExecuteAction(SupplyVents, OnAction);
                ExecuteAction(DrainVents, OffAction);
                ExecuteAction(InnerDoors, OpenAction);
                return;
            }

            Debug("Pressurizing");
            ExecuteAction(DrainVents, DepressurizeOffAction);
            ExecuteAction(DrainVents, OnAction);

            ExecuteAction(PressureTimer, StartPressureTimeAction);
        }

        private void Depressurize()
        {
            if (!GetIsPressurized())
            {
                Debug("Already Depressurized - Skipping");
                ExecuteAction(SupplyVents, OffAction);
                ExecuteAction(DrainVents, OffAction);
                ExecuteAction(OutterDoors, OpenAction);
                return;
            }

            Debug("Depressurizing");
            ExecuteAction(SupplyVents, OffAction);
            ExecuteAction(DrainVents, DepressurizeOnAction);
            ExecuteAction(DrainVents, OnAction);

            ExecuteAction(PressureTimer, StartPressureTimeAction);
        }

        private bool GetIsPressurized()
        {
            var level = ((IMyAirVent)SupplyVents[0]).GetOxygenLevel();
            return  level > 0;
        }

        private void PressureTimerComplete()
        {
            if (GetIsPressurized())
            {
                ExecuteAction(DrainVents, OffAction);
                ExecuteAction(SupplyVents, OnAction);
                ExecuteAction(InnerDoors, OpenAction);

                if (State != AirlockState.TransferOutReady)
                {
                    State = AirlockState.Idle;
                }

                Debug("Pressurized");

                return;
            }

            ExecuteAction(DrainVents, OffAction);
            ExecuteAction(OutterDoors, OpenAction);

            if (State != AirlockState.TransferInReady)
            {
                State = AirlockState.Idle;
            }

            Debug("Depressurized");
        }

        private void SealAirlock()
        {
            Debug("Sealing Airlock");

            ExecuteAction(OutterDoors, CloseAction);
            ExecuteAction(InnerDoors, CloseAction);

            Debug("Airlock Sealed");
        }

        private static void ExecuteAction(IMyTerminalBlock block, string action)
        {
            block.GetActionWithName(action).Apply(block);
        }

        private static void ExecuteAction(List<IMyTerminalBlock> blocks, string action)
        {
            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                ExecuteAction(block, action);
            }
        }

        private void Debug(string message = "", params object[] data)
        {
            if (!debugMode)
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays("Debug: " + message, DebugDisplays);
        }

        private void Info(string message = "", params object[] data)
        {
            message = string.Format(message, data);
            WriteToDisplays("Info: " + message, StatusDisplays);
        }

        private void ClearDisplays()
        {
            ClearDebugDisplays();
            ClearStatusDisplays();
        }

        private void ClearDebugDisplays()
        {
            for (var i = 0; i < DebugDisplays.Count; i++)
            {
                ClearDisplay(DebugDisplays[i]);
            }
        }

        private void ClearStatusDisplays()
        {
            for (var i = 0; i < StatusDisplays.Count; i++)
            {
                ClearDisplay(StatusDisplays[i]);
            }
        }

        private static void ClearDisplay(IMyTerminalBlock block)
        {
            var panel = (IMyTextPanel)block;
            panel.WritePublicText("");
            panel.ShowPublicTextOnScreen();
        }

        private void WriteToDisplays(string message, List<IMyTerminalBlock> panels)
        {
            for (var i = 0; i < panels.Count; i++)
            {
                var panel = ((IMyTextPanel)panels[i]);
                var text = panel.GetPublicText();
                
                var newMessage = text == "" ? message : text + "\n" + message;

                if (!panel.CustomName.Contains(NoScrollOption))
                {
                    var fontsize = panel.GetValue<Single>("FontSize");
                    var screenLines = Convert.ToInt32(TextLinesPerFontPoint / fontsize);

                    screenLines = screenLines <= 0 ? 1 : screenLines;

                    newMessage = RemoveTopLines(newMessage, screenLines);

                    panel.WritePublicText(newMessage);
                }

                panel.WritePublicText(newMessage);
                panel.ShowPublicTextOnScreen();
            }
        }

        private string RemoveTopLines(string text, int maxLines)
        {
            List<string> lines = new List<string>();
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text, "^.*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.RightToLeft);

            while (match.Success && lines.Count < maxLines)
            {
                lines.Insert(0, match.Value);
                match = match.NextMatch();
            }

            return string.Join("\n", lines);
        }

        private void WriteNamesToDebug(List<IMyTerminalBlock> blocks)
        {
            if (!debugMode)
            {
                return;
            }

            var message = "";
            var separator = "";
            for (var i = 0; i < blocks.Count; i++)
            {
                separator = i == 0 ? "" : ", ";

                var block = blocks[i];
                message = message + separator + block.CustomName;
            }

            Debug(message);
        }

        private void SystemStatusMessage()
        {
            ClearDisplays();
            StartupHeader();
            ControllerStatus();
            DisplayStatus();
            DoorStatus();
            VentStatus();
        }

        private void StartupHeader()
        {
            Info("============== Initializing Airlock System v0.0.1 - Alpha ==============");
            Info(" ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---");
            Info(" EXTREMELY Unstable test system! Use at your own risk! ");
            Info(" ---=== WARNING - WARNING - WARNING - WARNING - WARNING ===---");
        }

        private void ControllerStatus()
        {
            Debug();
            Debug("Main Airlock Controller");
            Debug(Controller.CustomName);
        }

        private void DisplayStatus()
        {
            Debug();
            Debug("Status Displays");
            WriteNamesToDebug(StatusDisplays);

            Debug();
            Debug("Debug Displays");
            WriteNamesToDebug(DebugDisplays);
        }

        private void DoorStatus()
        {
            Debug();
            Debug("Locating Airlock Doors");

            WriteNamesToDebug(InnerDoors);
            WriteNamesToDebug(OutterDoors);
        }

        private void VentStatus()
        {
            Debug();
            Debug("Locating Airlock Vents");

            WriteNamesToDebug(SupplyVents);
            WriteNamesToDebug(DrainVents);
        }

        private void TimerStatus()
        {
            Debug();
            Debug("Locating Pressurization Timer");
            Debug(PressureTimer.CustomName);
            Debug();
        }
        #endregion
    }
}
