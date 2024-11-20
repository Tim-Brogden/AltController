/*
Alt Controller
--------------
Copyright 2013 Tim Brogden
http://altcontroller.net

Description
-----------
A free program for mapping computer inputs, such as pointer movements and button presses, 
to actions, such as key presses. The aim of this program is to help make third-party programs,
such as computer games, more accessible to users with physical difficulties.

License
-------
This file is part of Alt Controller. 
Alt Controller is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Alt Controller is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Alt Controller.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using AltController.Config;

namespace AltController.Core
{
    /// <summary>
    /// Check whether commands are allowed to be executed
    /// </summary>
    public class CommandRuleManager
    {
        // Fields
        private List<CommandRuleItem> _commandRules = new List<CommandRuleItem>();

        // Properties
        public List<CommandRuleItem> Rules { get { return _commandRules; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appConfig"></param>
        public CommandRuleManager()
        {
        }

        /// <summary>
        /// Constructor from rules
        /// </summary>
        /// <param name="rulesList"></param>
        public CommandRuleManager(IEnumerable<CommandRuleItem> rulesList)
        {
            _commandRules.AddRange(rulesList);
        }

        /// <summary>
        /// Check a command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public CommandRuleItem ApplyRules(string command)
        {
            CommandRuleItem matchingRule;

            // First see if there is an exact rule
            matchingRule = FindRule(command);

            // Otherwise, look for regex rules
            if (matchingRule == null)
            {
                matchingRule = ApplyRulesOfType(command, ECommandAction.Run);
            }
            if (matchingRule == null)
            {
                matchingRule = ApplyRulesOfType(command, ECommandAction.AskMe);
            }
            if (matchingRule == null)
            {
                matchingRule = ApplyRulesOfType(command, ECommandAction.DontRun);
            }
            
            return matchingRule;
        }

        /// <summary>
        /// Apply the rules of a particular type to the specified command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="eCommandRule"></param>
        /// <returns></returns>
        private CommandRuleItem ApplyRulesOfType(string command, ECommandAction actionType)
        {
            CommandRuleItem matchingRule = null;

            foreach (CommandRuleItem rule in _commandRules)
            {
                if (rule.ActionType == actionType && IsMatch(command, rule))
                {
                    matchingRule = rule;
                    break;
                }
            }

            return matchingRule;
        }

        /// <summary>
        /// Apply a particular rule to a command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private bool IsMatch(string command, CommandRuleItem rule)
        {
            bool isMatch;

            string matchPattern = rule.Command;
            if (matchPattern.StartsWith("^"))
            {
                // Regex
                try
                {
                    Regex regex = new Regex(rule.Command, RegexOptions.IgnoreCase);
                    isMatch = regex.IsMatch(command);
                }
                catch (Exception)
                {
                    isMatch = false;
                }
            }
            else
            {
                // Exact
                isMatch = command.Equals(matchPattern);
            }

            return isMatch;
        }

        /// <summary>
        /// Find the first rule for the specified command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public CommandRuleItem FindRule(string command)
        {
            CommandRuleItem matchingRule = null;
            foreach (CommandRuleItem rule in _commandRules)
            {
                if (rule.Command.Equals(command))
                {
                    matchingRule = rule;
                    break;
                }
            }

            return matchingRule;
        }
        
        /// <summary>
        /// Read rules from app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void FromConfig(AppConfig appConfig)
        {
            _commandRules.Clear();
            string allowedCommandsList = appConfig.GetStringVal(Constants.ConfigAllowedCommandsList, "");
            _commandRules.AddRange(ParseCommandRules(allowedCommandsList, ECommandAction.Run));
            string askMeCommandsList = appConfig.GetStringVal(Constants.ConfigAskMeCommandsList, "");
            _commandRules.AddRange(ParseCommandRules(askMeCommandsList, ECommandAction.AskMe));
            string disallowedCommandsList = appConfig.GetStringVal(Constants.ConfigDisallowedCommandsList, "");
            _commandRules.AddRange(ParseCommandRules(disallowedCommandsList, ECommandAction.DontRun));               
        }

        /// <summary>
        /// Write rules to app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void ToConfig(AppConfig appConfig)
        {
            StringBuilder allowedCommands = new StringBuilder();
            StringBuilder askMeCommands = new StringBuilder();
            StringBuilder disallowedCommands = new StringBuilder();
            foreach (CommandRuleItem rule in _commandRules)
            {
                switch (rule.ActionType)
                {
                    case ECommandAction.Run:
                        allowedCommands.AppendLine(rule.Command); break;
                    case ECommandAction.DontRun:
                        disallowedCommands.AppendLine(rule.Command); break;
                    case ECommandAction.AskMe:
                        askMeCommands.AppendLine(rule.Command); break;
                }
            }
            appConfig.SetStringVal(Constants.ConfigAllowedCommandsList, allowedCommands.ToString());
            appConfig.SetStringVal(Constants.ConfigAskMeCommandsList, askMeCommands.ToString());
            appConfig.SetStringVal(Constants.ConfigDisallowedCommandsList, disallowedCommands.ToString());
        }

        /// <summary>
        /// Read a list of command lines from a string
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        private List<CommandRuleItem> ParseCommandRules(string lines, ECommandAction ruleType)
        {
            List<CommandRuleItem> rules = new List<CommandRuleItem>();

            string command;
            using (StringReader sr = new StringReader(lines))
            {
                while ((command = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        rules.Add(new CommandRuleItem(command, ruleType));
                    }
                }
            }

            return rules;
        }


    }
}
