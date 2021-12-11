using ModuleBased.Example.CommandLine;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ModuleBased.Example
{
    [Injectable(typeof(ConsoleModule))]
    [CommandGroup("Console")]
    public class ConsoleModule : MonoBehaviour
    {
        #region -- Require --
        [Inject]
        private IGameCore _core;
        [Inject]
        private ConsoleView _view;
        #endregion

        private bool open_console;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                open_console = !open_console;
                if (open_console)
                    _view.Display();
                else
                    _view.Hide();
            }
        }

        public void ExecuteCommand(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;
            var splited = CommandSpliter.Split(line);

            string result = string.Empty;
            Parser.Parse<GetOption, SetOption>(splited,
                (g) =>
                {
                    _view.Log(g.ToString());
                },
                (s) =>
                {
                    _view.Log(s.ToString());
                },
                (e) =>
                {
                    _view.Log(e.ToString(), new Color(0.8f, 0, 0), italic: true);
                });
        }

    }




    [Verb("get")]
    public class GetOption
    {
        [Value("value")]
        public IEnumerable<string> Value { get; set; }

        [Option("f", "float", false, false, "get by float")]
        public bool IsFloat { get; set; }

        [Option("i", "int", false, false, "get by int")]
        public bool IsInt { get; set; }

        [Option("b", "bool", false, false, "get by bool")]
        public bool IsBool { get; set; }

        public override string ToString()
        {
            return $"Get value:[{Value.ToArrayString()}], f:{IsFloat}, i:{IsInt}, b:{IsBool}";
        }
    }

    [Verb("set")]
    public class SetOption
    {
        [Value("value")]
        public IEnumerable<string> Value { get; set; }

        [Option("f", "float", false, false, "set by float")]
        public bool IsFloat { get; set; }

        [Option("i", "int", false, false, "set by int")]
        public bool IsInt { get; set; }

        [Option("b", "bool", false, false, "set by bool")]
        public bool IsBool { get; set; }

        public override string ToString()
        {
            return $"Set value:[{Value.ToArrayString()}], f:{IsFloat}, i:{IsInt}, b:{IsBool}";
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandAttribute(string desc = "", [CallerMemberName] string name = null)
        {
            Name = name;
            Description = desc;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class CommandGroupAttribute : Attribute
    {
        public string Name { get; }

        public CommandGroupAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class CommandFlagAttribute : Attribute
    {

    }
}

