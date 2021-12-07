using ModuleBased.Example.CommandLine;
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
    [Inject]
    [CommandGroup("Console")]
    public class ConsoleModule : MonoBehaviour, IEventInject
    {
        [Inject]
        private IGameCore _core;

        private Dictionary<CommandBinding, Command> _commandDict = new Dictionary<CommandBinding, Command>();

        private bool open_console = false;
        private string current_command = string.Empty;

        private List<string> _commands = new List<string>();

        private List<string> _history = new List<string>();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                open_console = !open_console;
            }
        }

        private void OnGUI()
        {
            if (open_console)
            {
                DrawConsole();
            }
        }

        private void DrawConsole()
        {
            var style = GUI.skin.textField;
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            GUI.SetNextControlName("CommandField");
            current_command = GUI.TextField(new Rect(0, 0, Screen.width, 30), current_command, style);

            if (Event.current.isKey &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                _history.Add(current_command);
                // run list
                if (current_command == "list")
                {
                    _history.AddRange(_commandDict.Keys.Select(k =>
                    {
                        return k.Name;
                    }));
                }
                ExecuteCommand(current_command);
                // clear text field
                current_command = string.Empty;
                if (_history.Count > 10)
                {
                    _history.RemoveRange(0, _history.Count - 10);
                }
            }

            LineDrawer drawer = new LineDrawer("xxxxxxxxxxx\nyyyyyyyyy\nzzzzzz", Color.red);
            drawer.Draw(new Vector2(0, 60));
            GUI.TextArea(new Rect(0, 30, Screen.width, Screen.height / 2), _history.ReverseClone().ToArrayString("\n"), style);

        }

        public void ExecuteCommand(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;
            var splited = CommandSpliter.Split(line);
            //Debug.Log(splited.ToArrayString());
            //TestDT();
            var info = CommandInfo.New("config")
                .Verb("set")
                .Values(typeof(IEnumerable<string>))
                .Option(typeof(bool), "f", "float", helpText: "set float value")
                .Flag("i", "int", "set int value");

            //Debug.Log(info.GetColumns().ToArrayString());
            Debug.Log(info);

            var opt = info.Parse(splited);
            Debug.Log(opt);

            var exp = Parser.Parse<ExpOption>(splited);
            Debug.Log(exp);
            //try
            //{
            //    Parser parser = new Parser();
            //    var opt = parser.Parse<ExpOption>(splited);
            //    Debug.Log(opt);

            //}catch(Exception e)
            //{
            //    Debug.LogError(e);
            //}
        }

        public Command FindCommand(string group, string name, string[] args)
        {
            CommandBinding binding = new CommandBinding
            {
                Group = group,
                Name = name,
                ParamCount = args.Length,
            };
            _commandDict.TryGetValue(binding, out Command cmd);
            return cmd;
        }

        public void OnInject()
        {
            _core.OnLoadedCallback += listen_OnLoadedCallback;
        }

        private void listen_OnLoadedCallback(IEnumerable<Contraction> contractions)
        {
            var contracts = contractions.Where(c =>
            {
                return c.ContractType.IsDefined(typeof(CommandGroupAttribute), false);
            });
            foreach (var contract in contracts)
            {
                var groupAttr = (CommandGroupAttribute)contract.ContractType.GetCustomAttributes(typeof(CommandGroupAttribute), false)[0];
                var methodInfos = contract.ContractType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var methodInfo in methodInfos)
                {
                    if (!methodInfo.IsDefined(typeof(CommandAttribute), false))
                        continue;
                    var cmdAttr = methodInfo.GetCustomAttribute<CommandAttribute>();

                    Command cmd = new Command
                    {
                        Group = groupAttr.Name.ToLower(),
                        Name = cmdAttr.Name.ToLower(),
                        ContractType = contract.ContractType,
                        Desciption = cmdAttr.Description,
                        Method = methodInfo,
                        Parameters = methodInfo.GetParameters()
                    };
                    //Debug.Log($"Group:{cmd.Group}, Name:{cmd.Name}, Params Count:{cmd.Parameters.Length}");
                    //Debug.Log($"Hash:{cmd.GetBinding().GetHashCode()}");
                    _commandDict.Add(cmd.GetBinding(), cmd);
                }
            }
        }
    }

    public struct CommandBinding
    {
        public string Group;
        public string Name;
        public int ParamCount;

        public override string ToString()
        {
            return $"(Group:{Group}, Name:{Name}, ParamCount:{ParamCount})";
        }

        public override int GetHashCode()
        {
            int hash = 7;
            unchecked
            {
                hash *= Group.GetHashCode() * 397;
                hash *= Name.GetHashCode() * 397;
                hash *= ParamCount.GetHashCode() * 397;
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(CommandBinding))
            {
                return false;
            }
            var o = (CommandBinding)obj;
            return o.Group.Equals(Group) && o.Name.Equals(Name) && o.ParamCount == ParamCount;
        }
    }

    public class Command
    {
        public string Group;
        public string Name;
        public Type ContractType;
        public MethodInfo Method;
        public ParameterInfo[] Parameters;
        public string Desciption;

        public void Execute(object target, params object[] args)
        {
            var result = Method.Invoke(target, args);
            if (result != null)
            {
                Debug.Log(result.ToString());
            }
        }

        public CommandBinding GetBinding()
        {
            return new CommandBinding
            {
                Group = Group,
                Name = Name,
                ParamCount = Parameters.Length
            };
        }
    }

    public class LineDrawer
    {
        private const char seperateChar = ' ';

        private string Content;
        private int LineCount;
        private Color Color = Color.white;

        public LineDrawer(string content, Color color)
        {
            Content = content;
            LineCount = content.Split(seperateChar).Length;
            Color = color;
        }

        public void Draw(Vector2 pos)
        {
            GUI.Label(new Rect(pos.x, pos.y, Screen.width, LineCount * 100), Content);
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

