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
    /// Text Panel control code that simulates the behavior of OSS projects like log4net and log4j
    /// </summary>
    public class Log4SEControl : BlockScriptBase 
    {
        public Log4SEControl(IMyGridTerminalSystem gts, IMyProgrammableBlock executingBlock) : base(gts, executingBlock) { }

        public override void MainMethod(string argument)
        {
            Main(argument);
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

        public const string DefaultDisplaySystemName = "LCD";
        public const string ControlDebugDisplayName = "SEBlockControl::Test::Log4SEControl";

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

        private Dictionary<string, bool> logTypeEnabled =
            new Dictionary<string, bool>()
            {
                { MessageSeverity.Debug, false },
                { MessageSeverity.Info, false },
                { MessageSeverity.Warn, true },
                { MessageSeverity.Error, true },
                { MessageSeverity.Fatal, true },
            };
        
        private bool testEnabled;

        private Dictionary<string, List<IMyTerminalBlock>> displays = new Dictionary<string, List<IMyTerminalBlock>>();

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

            WriteLogMessage(severity, message);
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

            logTypeEnabled[MessageSeverity.Debug] = controllerName.Contains((DebugBlockName + KeyValuePairSeparator + EnabledSettingName).ToUpper());
            logTypeEnabled[MessageSeverity.Info] = controllerName.Contains((InfoBlockName + KeyValuePairSeparator + EnabledSettingName).ToUpper());
            logTypeEnabled[MessageSeverity.Warn] = !controllerName.Contains((WarningBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());
            logTypeEnabled[MessageSeverity.Error] = !controllerName.Contains((ErrorBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());
            logTypeEnabled[MessageSeverity.Fatal] = !controllerName.Contains((FatalBlockName + KeyValuePairSeparator + DisabledSettingName).ToUpper());

            if (testEnabled)
            {
                var testMessage = "";
                var separator = "";
                foreach (var kvp in logTypeEnabled)
                {
                    testMessage += string.Format("{0}{1}:{2}", separator, kvp.Key, kvp.Value);
                }

                Test(testMessage);
            }
            
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

            foreach (var kvp in logTypeEnabled)
            {
                if (!kvp.Value)
                {
                    continue;
                }

                var logDisplays = panels.FindAll(d => d.CustomName.Contains(kvp.Key));
                logDisplays.AddRange(defaultDisplays);

                Test("{0} displays found:{1}", kvp.Key, logDisplays.Count);
                WriteNamesToTest(logDisplays);

                displays.Add(kvp.Key, logDisplays);
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

        private void WriteLogMessage(MessageSeverity severity, string message = "", params object[] data)
        {
            if (!logTypeEnabled[(string)severity])
            {
                return;
            }

            message = string.Format(message, data);
            WriteToDisplays(string.Format("{0}: {1}", severity, message), displays[(string)severity]);
        }

        private void ClearDisplays()
        {
            Test("Clearing all displays");

            foreach (var kvp in logTypeEnabled)
            {
                ClearDisplays(kvp.Key);
            }

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

        private void ClearDisplays(MessageSeverity severity)
        {
            if (!logTypeEnabled[(string)severity])
            {
                return;
            }

            for (var i = 0; i < displays[(string)severity].Count; i++)
            {
                ClearDisplay(displays[(string)severity][i]);
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

                separator = " | ";
            }

            Test(message);
        }
        #endregion
    }
}
