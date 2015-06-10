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

    /// <summary>
    /// Text Panel control code that simulates the behavior of OSS projects like log4net and log4j
    /// </summary>
    public class Log4SEControl : BlockScriptBase 
    {
        public Log4SEControl(IMyGridTerminalSystem gts) : base(gts) { }

        public override void MainMethod(string argument)
        {
            Main(argument);
        }

        public override void CleanUp()
        {
            base.CleanUp();
        }

        #region Game Code
        /// <summary>
        /// Data class for passing around message data
        /// </summary>
        private class ExecutionContext
        {
            public string Name { get; set; }
            public string Severity { get; set; }
            public string Message { get; set; }
            public bool Clear { get; set; }
            public bool ClearOnly { get; set; }

            public override string ToString()
            {
                return (Clear ? "Clear " : "")
                    + (ClearOnly ? "Only " : "") 
                    + string.Format("Name:{0};Severity:{1};Message:{2}", Name, Severity, Message);
            }
        }

        /// <summary>
        /// Psudeo-Enum for message severity
        /// </summary>
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

            public static implicit operator MessageSeverity(string type)
            {
                return new MessageSeverity(type);
            }

            public static explicit operator string(MessageSeverity type)
            {
                return type.ToString();
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
        
        private const string emptyString = "";
        private const int TextLinesPerFontPoint = 17;

        private const string ArgPrefix = "--";
        private const string KeyValuePairSeparator = "::";

        private const string InitializeParameterName = "--initialize";

        private const string ControlDebugDisplayName = "SEBlockControl::Test::Log4SEControl";

        private const string DefaultDisplaySystemName = "LCD";

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
        public const string ClearKeyName = "clear";

        private bool debugEnabled;
        private bool infoEnabled;
        private bool warningEnabled = true;
        private bool errorEnabled = true;
        private bool fatalEnabled = true;
        
        private bool testEnabled;

        private List<IMyTerminalBlock> debugDisplays;
        private List<IMyTerminalBlock> infoDisplays;
        private List<IMyTerminalBlock> errorDisplays;
        private List<IMyTerminalBlock> warningDisplays;
        private List<IMyTerminalBlock> fatalDisplays;

        private List<IMyTerminalBlock> testDisplays;

        private bool initialized;

        private int executionCount;

        private IMyProgrammableBlock Controller
        {
            get { return Me; }
        }

        void Main(string args)
        {
            Echo(args);

            try
            {
                ExecuteBlockScript(Init(args));
            }
            catch (Exception ex)
            {
                Echo(ex.Message);
            }
        }

        private void ExecuteBlockScript(ExecutionContext context)
        {
            var severity = context.Severity;
            var message = context.Message;

            if (context.Clear)
            {
                ClearDisplays();
            }

            if (context.ClearOnly)
            {
                return;
            }

            Test("Showing {0} with message {1}", severity, message);

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

        private ExecutionContext Init(string args)
        {
            initialized = initialized && !args.Contains(InitializeParameterName);

            var context = InitializeExecutionContext(args);
            InitializeResources(context);
            initialized = true;

            return context;
        }

        private ExecutionContext InitializeExecutionContext(string args)
        {
            executionCount++;
            SetupTestDisplays();

            var context = ProcessArgs(args);

            PreExeuctionReport(context);

            return context;
        }

        private void PreExeuctionReport(ExecutionContext context)
        {
            Test("Executing process on controller {0}", Controller.CustomName);
            Test("Execution Count:{0}", executionCount);
            Test("Time since last exec:{0}", ElapsedTime.ToString(@"d\.hh\:mm\:ss"));
        }

        /// <summary>
        /// If any test displays are detected they are configured and started. Test displays are purely for troubleshooting the logging system 
        /// and should not be used or enabled during normal operations.
        /// </summary>
        private void SetupTestDisplays()
        {
            if (initialized)
            {
                return;
            }

            var panels = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);
            testDisplays = panels.FindAll(d => d.CustomName.Contains(ControlDebugDisplayName));

            testEnabled = testDisplays.Count > 0;
            
            if (!testEnabled)
            {
                return;
            }

            ClearTestDisplays();

            Test("===========================================================");
            Test("-----==== Initializing Log4SEControl v0.0.9 ALPHA ====-----");
            Test("===========================================================");

            Test("test displays found: {0}", testDisplays.Count);
            WriteNamesToTest(testDisplays);
        }

        /// <summary>
        /// Splits the arg string into context data
        /// </summary>
        /// <param name="args">args to process</param>
        /// <returns>context data</returns>
        private ExecutionContext ProcessArgs(string args)
        {
            Test("recievedArgs = {0}", args);

            var arguments = ParseArguments(args);

            var clear = arguments.ContainsKey(ClearKeyName) ? true : false;

            var context = new ExecutionContext() 
            { 
                Name = arguments.ContainsKey(SystemNameArgKeyName) ? arguments[SystemNameArgKeyName] : DefaultDisplaySystemName, 
                Severity = arguments.ContainsKey(SeverityArgKeyName) ? arguments[SeverityArgKeyName] : WarningBlockName, 
                Message = arguments.ContainsKey(MessageArgKeyName) ? arguments[MessageArgKeyName] : "",
                Clear = clear,
                ClearOnly = clear && arguments.Count == 1
            };

            Test(context.ToString());

            return context;
        }

        /// <summary>
        /// Locates, "Connects" and configures hardware related to the system
        /// </summary>
        /// <param name="context">context to use</param>
        /// <returns>context data</returns>
        private void InitializeResources(ExecutionContext context)
        {
            if (initialized)
            {
                return;
            }

            executionCount = 0;

            var controllerName = Controller.CustomName.ToUpper();
            var systemName = context.Name;

            debugEnabled = controllerName.Contains((DebugBlockName + KeyValuePairSeparator + EnabledSettingName).ToUpper());
            infoEnabled = controllerName.Contains((InfoBlockName + KeyValuePairSeparator + EnabledSettingName).ToUpper());
            warningEnabled = !controllerName.Contains((WarningBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());
            errorEnabled = !controllerName.Contains((ErrorBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());
            fatalEnabled = !controllerName.Contains((FatalBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());

            Test("debug:{0};info:{0};warn:{0};error:{0};fatal:{0}", debugEnabled, infoEnabled, warningEnabled, errorEnabled, fatalEnabled);
            
            var panels = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);
            panels = panels.FindAll(d => d.CustomName.Contains(systemName));

            var defaultDisplays = panels.FindAll(
                d => !d.CustomName.Contains(DebugBlockName)
                && !d.CustomName.Contains(InfoBlockName)
                && !d.CustomName.Contains(WarningBlockName)
                && !d.CustomName.Contains(ErrorBlockName)
                && !d.CustomName.Contains(FatalBlockName)
                && !d.CustomName.Contains(ControlDebugDisplayName));

            Test("Default Displays with System Name {0}:{1}", systemName, defaultDisplays.Count);
            WriteNamesToTest(defaultDisplays);

            if (debugEnabled)
            {
                debugDisplays = panels.FindAll(d => d.CustomName.Contains(DebugBlockName));
                debugDisplays.AddRange(defaultDisplays);

                Test("debug displays found:{0}", debugDisplays.Count);
                WriteNamesToTest(debugDisplays);
            }

            if (infoEnabled)
            {
                infoDisplays = panels.FindAll(d => d.CustomName.Contains(InfoBlockName));
                infoDisplays.AddRange(defaultDisplays);

                Test("info displays found:{0}", infoDisplays.Count);
                WriteNamesToTest(infoDisplays);
            }

            if (warningEnabled)
            {
                warningDisplays = panels.FindAll(d => d.CustomName.Contains(WarningBlockName));
                warningDisplays.AddRange(defaultDisplays);

                Test("warning displays found:{0}", warningDisplays.Count);
                WriteNamesToTest(warningDisplays);
            }

            if (errorEnabled)
            {
                errorDisplays = panels.FindAll(d => d.CustomName.Contains(ErrorBlockName));
                errorDisplays.AddRange(defaultDisplays);

                Test("error displays found:{0}", errorDisplays.Count);
                WriteNamesToTest(errorDisplays);                
            }

            if (fatalEnabled)
            {
                fatalDisplays = panels.FindAll(d => d.CustomName.Contains(FatalBlockName));
                fatalDisplays.AddRange(defaultDisplays);

                Test("fatal displays found:{0}", fatalDisplays.Count);
                WriteNamesToTest(fatalDisplays);
            }

            Test("Log4SEControl Init Complete");
        }

        private Dictionary<string, string> ParseArguments(string args)
        {
            if (args == null || args == "")
            {
                return new Dictionary<string, string>();
            }

            var argPairs = new List<string>(Split(args, ArgPrefix));

            Test("Parsing {0} args ", argPairs.Count);

            var retval = new Dictionary<string, string>();
            foreach (var pair in argPairs)
            {
                var kvp = Split(pair, KeyValuePairSeparator);

                if (kvp.Length == 0)
                {
                    Echo("Invalid Argument String");
                }

                var key = kvp[0];
                var value = kvp.Length == 2 ? kvp[1] : "";

                Test("Key={0} : Value={1}", key, value);

                retval.Add(key, value);
            }

            return retval;
        }

        private string[] Split(string str, string separator)
        {
            var data = str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < data.Length - 1; i++)
            {
                data[i] = data[i].Trim();
            }

            return data;
        }

        private void Test(string message = "", params object[] data)
        {
            if (!testEnabled)
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays("TEST: " + message, testDisplays);
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
            Test("Clearing all displays");
            ClearDebugDisplays();
            ClearInfoDisplays();
            ClearWarningDisplays();
            ClearErrorDisplays();
            ClearFatalDisplays();
        }

        private void ClearTestDisplays()
        {
            if (!testEnabled)
            {
                return;
            }

            for (var i = 0; i < testDisplays.Count; i++)
            {
                ClearDisplay(testDisplays[i]);
            }
        }

        private void ClearDebugDisplays()
        {
            if (!debugEnabled)
            {
                return;
            }

            for (var i = 0; i < debugDisplays.Count; i++)
            {
                ClearDisplay(debugDisplays[i]);
            }
        }

        private void ClearInfoDisplays()
        {
            if (!infoEnabled)
            {
                return;
            }

            for (var i = 0; i < infoDisplays.Count; i++)
            {
                ClearDisplay(infoDisplays[i]);
            }
        }

        private void ClearWarningDisplays()
        {
            if (!warningEnabled)
            {
                return;
            }

            for (var i = 0; i < warningDisplays.Count; i++)
            {
                ClearDisplay(warningDisplays[i]);
            }
        }

        private void ClearErrorDisplays()
        {
            if (!errorEnabled)
            {
                return;
            }

            for (var i = 0; i < errorDisplays.Count; i++)
            {
                ClearDisplay(errorDisplays[i]);
            }
        }

        private void ClearFatalDisplays()
        {
            if (!fatalEnabled)
            {
                return;
            }

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

                // Screen scrolling, use "NoScroll" in name of text panel to prevent scrolling. 
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

        /// <summary>
        /// Screen scrolling support - trims the top lines of a string to simulate a rolling screen based on a provided font size calculation
        /// </summary>
        /// <param name="text">text to process</param>
        /// <param name="maxLines">maximum number of lines allowed</param>
        /// <returns>truncated string removing first lines</returns>
        private string RemoveTopLines(string text, int maxLines)
        {
            List<string> lines = new List<string>();
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(
                text, 
                "^.*$", 
                System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.RightToLeft);

            while (match.Success && lines.Count < maxLines)
            {
                lines.Insert(0, match.Value);
                match = match.NextMatch();
            }

            return string.Join("\n", lines);
        }

        private void WriteNamesToTest(List<IMyTerminalBlock> blocks)
        {
            if (!testEnabled || blocks.Count <= 0)
            {
                return;
            }

            var message = "";
            var separator = "";

            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                message = message + separator + block.CustomName;

                separator = " ";
            }

            Test(message);
        }
        #endregion
    }
}
