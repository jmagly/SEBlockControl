﻿namespace SpaceEngineersScriptBlock
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
    /// Control code to allow for forwarding of a received argument list to a specified programmable block(s)
    /// </summary>
    public class CommandForwardControl : BlockScriptBase
    {
        public CommandForwardControl(IMyGridTerminalSystem gts, IMyProgrammableBlock executingBlock) : base(gts, executingBlock) { }

        public override void MainMethod(string argument)
        {
            Main(argument);
        }

        #region Game Code
        private const string ArgPrefix = "--";
        private const string KeyValuePairSeparator = "::";

        private const string RunActionName = "Run";

        private const string ForwardToKeyName = "forwardTo";

        private string args;

        List<TerminalActionParameter> terminalParams = new List<TerminalActionParameter>();

        private class ExecutionContext
        {
            public string Name { get; set; }
            public string Severity { get; set; }
            public string Message { get; set; }
        }

        void Main(string argument)
        {
            args = argument;
            ExecuteBlockScript(ProcessArgs(argument));
        }

        private void ExecuteBlockScript(Dictionary<string, string> context)
        {
            string forwardTo;

            if (!context.TryGetValue(ForwardToKeyName, out forwardTo))
            {
                Echo("Parameter \"forwardTo\" must be defined.");
                return;
            }

            context.Remove(ForwardToKeyName);

            var controllers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(controllers);
            controllers = controllers.FindAll(d => d.CustomName.Contains(forwardTo));

            var args = BuildArgs(context);
            foreach (var controller in controllers)
            {
                CallBlockScript(controller, args);
            }
        }

        private void CallBlockScript(IMyTerminalBlock block, string args)
        {
            terminalParams.Clear();
            terminalParams.Add(TerminalActionParameter.Get(args));

            ExecuteAction(block, RunActionName, new List<TerminalActionParameter>() { TerminalActionParameter.Get(args) });
        }

        private string BuildArgs(Dictionary<string, string> data)
        {
            var retVal = "";

            var separator = "";
            foreach (var kvp in data)
            {
                var kvpString = separator + ArgPrefix + kvp.Key;

                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    kvpString += KeyValuePairSeparator + kvp.Value;
                }

                retVal += kvpString;
                separator = " ";
            }

            return retVal;
        }

        private static void ExecuteAction(IMyTerminalBlock block, string action, List<TerminalActionParameter> parameters = null)
        {
            if (parameters == null)
            {
                block.ApplyAction(action);
            }

            block.ApplyAction(action, parameters);
        }

        private static void ExecuteAction(List<IMyTerminalBlock> blocks, string action, List<TerminalActionParameter> parameters = null)
        {
            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                ExecuteAction(block, action);
            }
        }

        private Dictionary<string, string> ProcessArgs(string args)
        {
            var arguments = ParseArguments(args);

            return arguments;
        }

        private Dictionary<string, string> ParseArguments(string args)
        {
            if (args == null || args == "")
            {
                return new Dictionary<string, string>();
            }

            var argPairs = new List<string>(Split(args, ArgPrefix));

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
        #endregion
    }
}
