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
        [Inject]
        private IConfigModule _config;
        #endregion

        private bool open_console;

        #region -- History --
        private int _historyIndex;
        private List<string> _history = new List<string>();
        #endregion

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
            if (open_console && Input.GetKeyDown(KeyCode.UpArrow))
            {
                _historyIndex++;
                if (_historyIndex > _history.Count)
                    _historyIndex = _history.Count;
                if (_history.Count > 0)
                {
                    _view.TypeCommand(_history[_history.Count - _historyIndex]);
                }
            }
            if (open_console && Input.GetKeyDown(KeyCode.DownArrow))
            {
                _historyIndex--;
                if (_historyIndex <= 0)
                {
                    _historyIndex = 0;
                    _view.TypeCommand(string.Empty);
                }
                else if (_history.Count > 0)
                {
                    _view.TypeCommand(_history[_history.Count - _historyIndex]);
                }
            }
        }

        public void ExecuteCommand(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;
            var splited = CommandSpliter.Split(line);
            _view.Log(line, italic: true);
            _history.Add(line);
            _historyIndex = 0;

            Parser.Parse<GetOption, SetOption>(splited,
                (getOpt) =>
                {
                    var keys = getOpt.Value;
                    Dictionary<string, string> kv = new Dictionary<string, string>();
                    foreach (var key in keys)
                    {
                        kv.Add(key, _config.LoadOrDefault(key).ToString());
                    }
                    _view.Log(kv.ToArrayString());
                },
                (setOpt) =>
                {
                    var enumerator = setOpt.Value.GetEnumerator();
                    var key = enumerator.MoveNext() ? enumerator.Current : throw new InvalidOperationException("invalid format: no config key");
                    var value = enumerator.MoveNext() ? enumerator.Current : throw new InvalidOperationException("invalid format: no config value");
                    if (setOpt.IsBool)
                    {
                        if (!bool.TryParse(value, out bool result))
                            throw new InvalidOperationException("invalid formate: the value not a bool");
                        _config.Save<bool>(key, result);
                        return;
                    }
                    if (setOpt.IsFloat)
                    {
                        if (!float.TryParse(value, out float result))
                            throw new InvalidOperationException("invalid formate: the value not a float");
                        _config.Save<float>(key, result);
                        return;
                    }
                    if (setOpt.IsInt)
                    {
                        if (!int.TryParse(value, out int result))
                            throw new InvalidOperationException("invalid formate: the value not a integer");
                        _config.Save<int>(key, result);
                        return;
                    }
                    _config.Save<string>(key, value);
                },
                (err) =>
                {
                    _view.Println(err.ToString(), new Color(0.8f, 0, 0), italic: true);
                    Debug.LogError(err);
                });
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

    [Verb("get")]
    public class GetOption
    {
        [Value("value")]
        public IEnumerable<string> Value { get; set; }

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

