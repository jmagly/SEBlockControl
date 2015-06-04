namespace SpaceEngineersScriptBlock
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Text;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.Common.ObjectBuilders;
    using VRage;
    using VRageMath;

    public class DisplayControl : BlockScriptBase
    {
        public DisplayControl(IMyGridTerminalSystem gts) : base(gts) { }

        public override void MainMethod(string argument)
        {
            Main(argument);
        }

        public override void CleanUp()
        {
            base.CleanUp();
        }

        #region Game Code
        private struct MessageSeverity
        {
            public const string Debug = "Debug";
            public const string Info = "Info";
            public const string Warn = "Warn";
            public const string Error = "Error";
            public const string Fatal = "Fatal";

            private string type;

            private MessageSeverity(string type)
            {
                this.type = type;
            }

            public static bool operator ==(string left, MessageSeverity right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(string left, MessageSeverity right)
            {
                return !left.Equals(right);
            }

            public static bool operator ==(MessageSeverity left, string right)
            {
                return right.Equals((string)left);
            }

            public static bool operator !=(MessageSeverity left, string right)
            {
                return !right.Equals((string)left);
            }

            public override string ToString()
            {
                return type;
            }

            public override bool Equals(object obj)
            {
                if (obj == null && (type == null || type == ""))
                {
                    return true;
                }

                if (obj == null || (!(obj is MessageSeverity) && !(obj is string)))
                {
                    return false;
                }

                return obj is string 
                    ? type.ToUpper() == ((string)obj).ToUpper() 
                    : type.ToUpper() == obj.ToString().ToUpper();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        private const int TextLinesPerFontPoint = 17;

        private const string ArgPrefix = "--";
        private const string KeyValuePairSeparator = "::";

        public const string DebugBlockName = MessageSeverity.Debug;
        public const string InfoBlockName = MessageSeverity.Info;
        public const string WarningBlockName = MessageSeverity.Warn;
        public const string ErrorBlockName = MessageSeverity.Error;
        public const string FatalBlockName = MessageSeverity.Fatal;
        
        public const string EnabledSettingName = "Enabled";
        public const string DisabledSettingName = "Disabled";

        public const string NoScrollOption = "NoScroll";

        public const string SystemNameArgKeyName = "name";
        public const string SeverityArgKeyName = "severity";
        public const string MessageArgKeyName = "message";

        private string displaySystemName = "LCD";
        private string severity = WarningBlockName;
        private string message = "";

        private IMyProgrammableBlock Controller;

        private bool debugEnabled;
        private bool infoEnabled;
        private bool warningEnabled = true;
        private bool errorEnabled = true;
        private bool fatalEnabled = true;

        private Dictionary<string, string> arguments;

        private List<IMyTerminalBlock> debugDisplays;
        private List<IMyTerminalBlock> infoDisplays;
        private List<IMyTerminalBlock> errorDisplays;
        private List<IMyTerminalBlock> warningDisplays;
        private List<IMyTerminalBlock> fatalDisplays;

        private List<IMyTerminalBlock> testDisplays;

        // Arg Format
        //
        // --name::<MySystemName> --severity::<Debug|[Info]|Warning|error|fatal> --message::<MyMessage>
        // --name::Airlock 002 --severity::Debug --message::This is my debug message, there are many like it but this one is mine!
        //
        void Main(string args)
        {
            args = args.Trim();

            var panels = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);
            testDisplays = panels.FindAll(d => d.CustomName.Contains("LCD - Airlock 002 Admin"));

            WriteToDisplays("=============================================", testDisplays);
            WriteToDisplays("recievedArgs = " + args, testDisplays);

            arguments = ParseArguments(args);

            Init();

            switch (severity)
            {
                case (MessageSeverity.Debug):
                    {
                        Debug(message);
                        break;
                    }
                case (MessageSeverity.Info):
                    {
                        Info(message);
                        break;
                    }
                case (MessageSeverity.Error):
                    {
                        Error(message);
                        break;
                    }
                case (MessageSeverity.Fatal):
                    {
                        Fatal(message);
                        break;
                    }
                case (MessageSeverity.Warn):
                default:
                    {
                        Warn(message);
                        break;
                    }
            }
        }

        private Dictionary<string, string> ParseArguments(string args)
        {
            if (args == null || args == "")
            {
                return new Dictionary<string,string>();
            }

            WriteToDisplays("Parsing Args", testDisplays);

            var argPairs = new List<string>(Split(args, ArgPrefix));

            WriteToDisplays("Found args " + argPairs.Count, testDisplays);

            var retval = new Dictionary<string, string>();
            foreach (var pair in argPairs)
            {
                WriteToDisplays("Parsing Arg Pair : " + pair, testDisplays);

                var kvp = Split(pair, KeyValuePairSeparator);

                WriteToDisplays("Pair Values : " + kvp.Length, testDisplays);
                WriteToDisplays("Value 1 : " + kvp[0], testDisplays);
                WriteToDisplays("Value 2 : " + kvp[1], testDisplays);

                retval.Add(kvp[0], kvp[1]);
            }

            return retval;
        }

        private string[] Split(string str, string separator)
        {
            WriteToDisplays("Splitting Args", testDisplays);

            var data = str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < data.Length - 1; i++)
            {
                data[i] = data[i].Trim();
            }

            return data;
        }

        private void Init()
        {
            displaySystemName = arguments.ContainsKey(SystemNameArgKeyName) ? arguments[SystemNameArgKeyName] : displaySystemName;
            severity = arguments.ContainsKey(SeverityArgKeyName) ? arguments[SeverityArgKeyName] : severity;
            message = arguments.ContainsKey(MessageArgKeyName) ? arguments[MessageArgKeyName] : message;

            WriteToDisplays("Display System : " + displaySystemName, testDisplays);
            WriteToDisplays("Severity : " + severity, testDisplays);
            WriteToDisplays("Messages : " + message, testDisplays);

            var controllers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(controllers);
            controllers = controllers.FindAll(d => d.CustomName.Contains(displaySystemName));


            Controller = (IMyProgrammableBlock)controllers[0];

            var controllerName = Controller.CustomName.ToUpper();

            debugEnabled = controllerName.Contains((DebugBlockName + KeyValuePairSeparator + EnabledSettingName).ToUpper());
            infoEnabled = controllerName.Contains((InfoBlockName + KeyValuePairSeparator + EnabledSettingName).ToUpper());
            warningEnabled = !controllerName.Contains((WarningBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());
            errorEnabled = !controllerName.Contains((ErrorBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());
            fatalEnabled = !controllerName.Contains((FatalBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());

            WriteToDisplays("debugEnabled: " + debugEnabled, testDisplays);
            WriteToDisplays("infoEnabled: " + infoEnabled, testDisplays);
            WriteToDisplays("warningEnabled: " + warningEnabled, testDisplays);
            WriteToDisplays("errorEnabled: " + errorEnabled, testDisplays);
            WriteToDisplays("fatalEnabled: " + fatalEnabled, testDisplays);

            var panels = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);
            panels = panels.FindAll(d => d.CustomName.Contains(displaySystemName));

            if (debugEnabled)
            {
                debugDisplays = panels.FindAll(d => d.CustomName.Contains(DebugBlockName));
                WriteToDisplays("debug displays found: " + debugDisplays.Count, testDisplays);

            }

            if (infoEnabled)
            {
                infoDisplays = panels.FindAll(d => d.CustomName.Contains(InfoBlockName));
                WriteToDisplays("info displays found: " + infoDisplays.Count, testDisplays);

            }

            if (warningEnabled)
            {
                warningDisplays = panels.FindAll(d => d.CustomName.Contains(WarningBlockName));
                WriteToDisplays("warning displays found: " + warningDisplays.Count, testDisplays);

            }

            if (errorEnabled)
            {
                errorDisplays = panels.FindAll(d => d.CustomName.Contains(ErrorBlockName));
                WriteToDisplays("error displays found: " + errorDisplays.Count, testDisplays);

            }

            if (fatalEnabled)
            {
                fatalDisplays = panels.FindAll(d => d.CustomName.Contains(FatalBlockName));
                WriteToDisplays("fatal displays found: " + fatalDisplays.Count, testDisplays);

            }

        }

        private void Debug(string message = "", params object[] data)
        {
            if (!debugEnabled)
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays("Debug: " + message, debugDisplays);
        }

        private void Info(string message = "", params object[] data)
        {
            if (!infoEnabled)
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays("Info: " + message, infoDisplays);
        }

        private void Warn(string message = "", params object[] data)
        {
            if (!warningEnabled)
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays("Warn: " + message, warningDisplays);
        }

        private void Error(string message = "", params object[] data)
        {
            if (!errorEnabled)
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays("Error: " + message, errorDisplays);
        }

        private void Fatal(string message = "", params object[] data)
        {
            if (!fatalEnabled)
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays("Fatal: " + message, fatalDisplays);
        }

        private void ClearDisplays()
        {
            ClearDebugDisplays();
            ClearInfoDisplays();
            ClearWarningDisplays();
            ClearErrorDisplays();
            ClearFatalDisplays();
        }

        private void ClearDebugDisplays()
        {
            for (var i = 0; i < debugDisplays.Count; i++)
            {
                ClearDisplay(debugDisplays[i]);
            }
        }

        private void ClearInfoDisplays()
        {
            for (var i = 0; i < infoDisplays.Count; i++)
            {
                ClearDisplay(infoDisplays[i]);
            }
        }

        private void ClearWarningDisplays()
        {
            for (var i = 0; i < warningDisplays.Count; i++)
            {
                ClearDisplay(warningDisplays[i]);
            }
        }

        private void ClearErrorDisplays()
        {
            for (var i = 0; i < errorDisplays.Count; i++)
            {
                ClearDisplay(errorDisplays[i]);
            }
        }

        private void ClearFatalDisplays()
        {
            for (var i = 0; i < fatalDisplays.Count; i++)
            {
                ClearDisplay(fatalDisplays[i]);
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
        #endregion
    }
}
