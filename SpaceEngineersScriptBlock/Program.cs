using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
//using Sandbox.ModAPI;  // !!NOT AVAILABLE
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\VRage.Common.dll
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\VRage.Math.dll
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\Sandbox.Common.dll
//Only 5 game namespaces are allowed in Programmable blocks
//http://steamcommunity.com/sharedfiles/filedetails/?id=360966557
using Sandbox.ModAPI.Ingame; 
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;
namespace SpaceEngineersScripting
{
    class CodeEditorEmulator
    {
        static IMyGridTerminalSystem GridTerminalSystem = null;

        public static void Main(string[] argument)
        {
        }

        // COPY/PASTE BELOW THIS LINE ----------------------------------------------------
/*
        private class AirlockState
        {
            public const string SealedPressurized = "SealedPressurized";
            public const string SealedDepressurized = "SealedDepressurized";
            public const string InteriorOpen = "InteriorOpen";
            public const string ExteriorOpen = "ExteriorOpen";
            public const string SystemOffline = "SystemOffline";
            public const string Initialized = "Initialized";

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
                return right.Equals(left);
            }

            public static bool operator !=(AirlockState left, string right)
            {
                return !right.Equals(left);
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

        const string OpenAction = "Open_On";
        const string CloseAction = "Open_Off";
        const string ToggleOpenCloseAction = "Open/Closed";

        const string OnAction = "OnOff_On";
        const string OffAction = "OnOff_Off";
        const string ToggleOnOffAction = "OnOff";

        const string DepressurizeOnAction = "Depressurize_On";
        const string DepressurizeOffAction = "Depressurize_Off";
        const string ToggleDepressurizeAction = "Depressurize_Off";

        const string StartPressureTimeAction = "Start";

        private static List<IMyTerminalBlock> InnerDoors;
        private static List<IMyTerminalBlock> OutterDoors;

        private static List<IMyTerminalBlock> SupplyVents;
        private static List<IMyTerminalBlock> DrainVents;

        private static List<IMyTerminalBlock> StatusDisplays;
        private static List<IMyTerminalBlock> DebugDisplays;

        private static IMyTimerBlock PressureTimer;

        private static IMyProgrammableBlock Controller;

        private static AirlockState State = AirlockState.SystemOffline;

        private string argument = "";

        private static bool DebugMode;

        void Main(string argument)
        {
            this.argument = argument;

            const string InteriorAccessName = "InteriorAccess";
            const string TransferAccessName = "Transfer";
            const string ExteriorAccessName = "ExteriorAccess";
            const string PressureTimerAccessName = "PressureTimerComplete";
            const string SystemStatus = "SystemStatus";

            Init();

            switch (argument)
            {
                case (InteriorAccessName):
                    {
                        ClearDisplays();
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
                        ClearDisplays();
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
                        if (State == AirlockState.SealedDepressurized || State == AirlockState.SealedPressurized)
                        {
                            Info("Pressure Timer Trigger Complete");
                            PressureTimerComplete();
                        }
                        else
                        {
                            SystemStatusMessage();
                        }

                        break;
                    }
            }
        }

        private void Init()
        {
            if (State != AirlockState.SystemOffline)
            {
                return;
            }

            const string AirlockSystemName = "Airlock";
            const string InnerDoorName = "Inner";
            const string OutterDoorName = "Outter";
            const string SupplyVentName = "Supply";
            const string DrainVentName = "Drain";

            var panels = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);
            panels = panels.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            StatusDisplays = panels;
            DebugDisplays = panels;
            
            ClearDisplays();
            StartupHeader();
            DisplayStatus();

            var controllers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(controllers);
            controllers = controllers.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            Controller = (IMyProgrammableBlock)controllers[0];

            DebugMode = Controller.CustomName.Contains("DEBUG");

            ControllerStatus();

            var doors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);
            doors = doors.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            InnerDoors = doors.FindAll(d => d.CustomName.Contains(InnerDoorName));
            OutterDoors = doors.FindAll(d => d.CustomName.Contains(OutterDoorName));

            DoorStatus();

            var vents = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyAirVent>(vents);
            vents = vents.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            VentStatus();

            SupplyVents = vents.FindAll(d => d.CustomName.Contains(SupplyVentName));
            DrainVents = vents.FindAll(d => d.CustomName.Contains(DrainVentName));

            var timers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(timers);
            timers = timers.FindAll(d => d.CustomName.Contains(AirlockSystemName));

            PressureTimer = (IMyTimerBlock)timers[0];

            TimerStatus();

            SealAirlock();

            State = AirlockState.Initialized;

            Info("--== System Started ==--");
            Info();
        }

        private void InteriorAccess()
        {
            Pressurize();
        }

        private void Transfer()
        {
           if (GetIsNotPressurized())
           {
               TransferToInterior();
           }
           else
           {
               TransferToExterior(); 
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
            Depressurize();
        }

        private void Pressurize()
        {
            SealAirlock();

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
            SealAirlock();

            if (GetIsNotPressurized())
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
            return ((IMyAirVent)SupplyVents[0]).GetOxygenLevel() != 0;
        }

        private bool GetIsNotPressurized()
        {
            return ((IMyAirVent)SupplyVents[0]).GetOxygenLevel() == 0;
        }

        private void PressureTimerComplete()
        {
            var badTimerMessage = "Invalid Pressurization Timer Activation.";

            if (GetIsPressurized())
            {
                if (State != AirlockState.SealedDepressurized)
                {
                    Debug(badTimerMessage);
                }

                ExecuteAction(SupplyVents, OnAction);
                ExecuteAction(DrainVents, OffAction);
                ExecuteAction(InnerDoors, OpenAction);

                State = AirlockState.InteriorOpen;
                Debug("Pressurized");

                return;
            }

            if (State != AirlockState.SealedPressurized)
            {
                Debug(badTimerMessage);
            }

            ExecuteAction(DrainVents, OffAction);
            ExecuteAction(OutterDoors, OpenAction);

            State = AirlockState.ExteriorOpen;
            Debug("Depressurized");
        }

        private void SealAirlock()
        {
            Debug("Sealing Airlock");

            ExecuteAction(InnerDoors, CloseAction);
            ExecuteAction(OutterDoors, CloseAction);

            State = GetIsPressurized() ? AirlockState.SealedPressurized : AirlockState.SealedDepressurized;

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
            if (!DebugMode)
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

        private static void WriteToDisplays(string message, List<IMyTerminalBlock> panels)
        {
            for (var i = 0; i < panels.Count; i++)
            {
                var panel = ((IMyTextPanel)panels[i]);
                var text = panel.GetPublicText();

                panel.WritePublicText(text + "\n" + message);
                panel.ShowPublicTextOnScreen();
            }
        }

        private void WriteNamesToDebug(List<IMyTerminalBlock> blocks)
        {
            if (!DebugMode)
            {
                return;
            }

            var message = "";
            var separator = "";
            for (var i = 0; i < blocks.Count; i++)
            {
                separator = i == 0  ? "" : ", ";

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
        }*/

        // COPY/PASTE ABOVE THIS LINE ----------------------------------------------------
    }
}