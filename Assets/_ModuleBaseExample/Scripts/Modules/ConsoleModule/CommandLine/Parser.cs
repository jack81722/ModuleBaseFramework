using ModuleBased.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModuleBased.Example.CommandLine
{
    public class Parser
    {
        public static T Parse<T>(IEnumerable<string> args)
        {
            var info = CommandInfo.NewAnalyze<T>();
            var opt = info.Parse(args);
            return CommandInfoConverter<T>.Convert(opt);
        }

        public static void Parse<T1, T2>(IEnumerable<string> args, Action<T1> onOpt1, Action<T2> onOpt2, Action<Exception> onError)
        {
            try
            {
                var enumerator = args.GetEnumerator();
                string verb;
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException($"invalid format : no cmd");
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException($"invalid format : no verb");
                verb = enumerator.Current;
                var info1 = CommandInfoCache<T1>.Info;
                var info2 = CommandInfoCache<T2>.Info;
                if (verb == info1.Verb)
                {
                    var opt = info1.Parse(args);
                    onOpt1.Invoke(CommandInfoConverter<T1>.Convert(opt));
                    return;
                }
                if (verb == info2.Verb)
                {
                    var opt = info2.Parse(args);
                    onOpt2.Invoke(CommandInfoConverter<T2>.Convert(opt));
                    return;
                }
                throw new InvalidOperationException($"invalid format : unknown verb ({verb})");
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }

        public static void Parse<T1, T2, T3>(IEnumerable<string> args, Action<T1> onOpt1, Action<T2> onOpt2, Action<T3> onOpt3, Action<Exception> onError)
        {
            try
            {
                var enumerator = args.GetEnumerator();
                string verb;
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException($"invalid format : no verb");
                verb = enumerator.Current;
                var info1 = CommandInfoCache<T1>.Info;
                var info2 = CommandInfoCache<T2>.Info;
                var info3 = CommandInfoCache<T3>.Info;
                if (verb == info1.Verb)
                {
                    var opt = info1.Parse(args);
                    onOpt1.Invoke(CommandInfoConverter<T1>.Convert(opt));
                    return;
                }
                if (verb == info2.Verb)
                {
                    var opt = info2.Parse(args);
                    onOpt2.Invoke(CommandInfoConverter<T2>.Convert(opt));
                    return;
                }
                if (verb == info3.Verb)
                {
                    var opt = info3.Parse(args);
                    onOpt3.Invoke(CommandInfoConverter<T3>.Convert(opt));
                    return;
                }
                throw new InvalidOperationException($"invalid format : unknown verb ({verb})");
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }

        public static void Parse(IEnumerable<string> args, IDictionary<Type, Action<object>> onOpt, Action<Exception> onError)
        {
            try
            {
                var enumerator = args.GetEnumerator();
                string verb;
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException($"invalid format : no verb");
                verb = enumerator.Current;
                foreach (var pair in onOpt)
                {
                    var info = (CommandInfo)typeof(CommandInfoCache<>).MakeGenericType(pair.Key).GetProperty("Info").GetValue(null);
                    if (verb == info.Verb)
                    {
                        var opt = info.Parse(args);
                        var concrete = typeof(CommandInfoConverter<>).MakeGenericType(pair.Key).GetMethod("Convert").Invoke(null, new object[] { opt });
                        pair.Value.Invoke(concrete);
                        return;
                    }
                }
                throw new InvalidOperationException($"invalid format : unknown verb ({verb})");
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }
    }


    public class CommandInfo : IEnumerable<CommandInfo.OptionInfo>
    {
        #region -- Data table --
        public static readonly string ShortNameColumn = "short_name";
        public static readonly string FullNameColumn = "full_name";
        public static readonly string HelpTextColumn = "help_text";
        public static readonly string ValueTypeColumn = "value_type";
        public static readonly string DefaultValueColumn = "default_value";

        private static DataTable _infoTable;
        public static DataTable InfoTable {
            get
            {
                if (_infoTable == null)
                {
                    _infoTable = new DataTable();
                    _infoTable.Columns.Add(ShortNameColumn, typeof(string));
                    _infoTable.Columns[ShortNameColumn].MaxLength = 1;
                    _infoTable.Columns[ShortNameColumn].AllowDBNull = true;

                    _infoTable.Columns.Add(FullNameColumn, typeof(string));
                    _infoTable.Columns[FullNameColumn].MaxLength = 20;
                    _infoTable.Columns[FullNameColumn].AllowDBNull = false;
                    _infoTable.Columns[FullNameColumn].Unique = true;

                    _infoTable.Columns.Add(HelpTextColumn, typeof(string));
                    _infoTable.Columns[HelpTextColumn].MaxLength = 100;
                    _infoTable.Columns[HelpTextColumn].AllowDBNull = true;
                    _infoTable.Columns[HelpTextColumn].Unique = false;

                    _infoTable.Columns.Add(ValueTypeColumn, typeof(Type));
                    _infoTable.Columns[ValueTypeColumn].AllowDBNull = false;
                    _infoTable.Columns[ValueTypeColumn].Unique = false;

                    _infoTable.Columns.Add(DefaultValueColumn, typeof(object));
                    _infoTable.Columns[DefaultValueColumn].AllowDBNull = true;
                    _infoTable.Columns[DefaultValueColumn].Unique = false;
                }
                return _infoTable;
            }
        }

        #endregion

        public string Verb { get; internal set; }

        public Type ValueType { get; internal set; }

        private DataTable _dt = new DataTable();

        public CommandInfo()
        {
            _dt = InfoTable.Clone();
        }

        #region -- Option info --
        public class OptionInfo
        {
            public string ShortName;
            public string FullName;
            public Type ValueType;
            public bool IsArray => ValueType.IsArray;
            public bool HasDefaultValue => !string.IsNullOrEmpty(DefaultValue as string);
            public object DefaultValue = null;
            public string HelpText = string.Empty;

            public static void WriteDataRow(DataRow row, OptionInfo info)
            {
                row[ShortNameColumn] = info.ShortName;
                row[FullNameColumn] = info.FullName;
                row[HelpTextColumn] = info.HelpText;
                row[ValueTypeColumn] = info.ValueType;
                row[DefaultValueColumn] = info.DefaultValue;
            }

            public static OptionInfo ReadDataRow(DataRow row)
            {
                var info = new OptionInfo
                {
                    ShortName = (string)row[ShortNameColumn],
                    FullName = (string)row[FullNameColumn],
                    HelpText = (string)row[HelpTextColumn],
                    ValueType = (Type)row[ValueTypeColumn],
                    DefaultValue = row[DefaultValueColumn],
                };
                return info;
            }


            public void SetOption(CommandOptions options, IEnumerable<string> args)
            {
                int count = args.Count();

                try
                {
                    if (IsArray)
                    {
                        var elementType = ValueType.GetElementType();
                        var enumerator = args.GetEnumerator();
                        var array = Array.CreateInstance(elementType, count);
                        int i = 0;
                        while (enumerator.MoveNext())
                        {
                            if (elementType == typeof(string))
                            {
                                array.SetValue(enumerator.Current, i++);
                            }
                            else
                            {
                                var element = Convert.ChangeType(enumerator.Current, elementType);
                                array.SetValue(element, i++);
                            }
                        }
                        options.AddOption(ShortName, FullName, array);
                    }
                    else if (ValueType == typeof(bool))
                    {
                        options.AddOption(ShortName, FullName, true);
                    }
                    else
                    {
                        var arg = args.FirstOrDefault();
                        if (arg != null)
                        {
                            options.AddOption(ShortName, FullName, Convert.ChangeType(arg, ValueType));
                        }
                        else if (HasDefaultValue)
                        {
                            options.AddOption(ShortName, FullName, DefaultValue);
                        }
                        else
                        {
                            options.AddOption(ShortName, FullName, Activator.CreateInstance(ValueType));
                        }
                    }
                }
                catch
                {
                    options.AddOption(ShortName, FullName, Activator.CreateInstance(ValueType));
                }
            }

            public override string ToString()
            {
                return $"(short:{ShortName}, full:{FullName}, type:{ValueType.Name})";
            }

        }
        #endregion

        #region -- Static methods --
        public static CommandInfo New()
        {
            return new CommandInfo();
        }

        public static CommandInfo NewAnalyze<T>()
        {
            return NewAnalyze(typeof(T));
        }

        public static CommandInfo NewAnalyze(Type type)
        {
            CommandInfo info = New();
            info.Analyze(type);
            Debug.Log($"new analyze : {info}");
            return info;
        }
        #endregion

        public void Analyze(Type type)
        {
            _dt.Clear();
            if (type.IsDefined(typeof(VerbAttribute), false))
            {
                var verbAttr = type.GetCustomAttribute<VerbAttribute>();
                Verb = verbAttr.Name;
            }
            else
            {
                Verb = type.Name;
            }
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (prop.IsDefined(typeof(ValueAttribute)))
                {
                    ValueType = prop.PropertyType;
                }
                if (prop.IsDefined(typeof(OptionAttribute)))
                {   
                    var optAttr = prop.GetCustomAttribute<OptionAttribute>();
                    var optInfo = new OptionInfo
                    {
                        ShortName = optAttr.ShortName,
                        FullName = optAttr.FullName,
                        DefaultValue = optAttr.Default,
                        HelpText = optAttr.HelpText,
                        ValueType = prop.PropertyType,
                    };
                    Debug.Log(optInfo);
                    CreateOptionInfo(optInfo);
                }
            }
        }

        public void Analyze<T>()
        {
            Analyze(typeof(T));
        }

        public void SetValue(CommandOptions options, IEnumerable<string> args)
        {
            if (ValueType == null)
                return;
            int count = args.Count();
            if (ValueType.IsArray)
            {
                options.Value = InstantiateArray(args);
            }
            else if (ValueType.IsGenericType && ValueType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                options.Value = InstantiateEnumerable(args);
            }
            else
            {
                if (count > 0)
                {
                    options.Value = Convert.ChangeType(args.FirstOrDefault(), ValueType);
                }
            }
        }

        private object InstantiateArray(IEnumerable<string> args)
        {
            int count = args.Count();
            var elementType = ValueType.GetElementType();
            var array = Array.CreateInstance(elementType, count);
            var enumerator = args.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (elementType == typeof(string))
                {
                    array.SetValue(enumerator.Current, i++);
                }
                else
                {
                    var value = Convert.ChangeType(enumerator.Current, elementType);
                    array.SetValue(value, i++);
                }
            }
            return array;
        }

        private object InstantiateEnumerable(IEnumerable<string> args)
        {
            int count = args.Count();
            var elementType = ValueType.GetGenericArguments()[0];
            var array = Array.CreateInstance(elementType, count);
            var enumerator = args.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (elementType == typeof(string))
                {
                    array.SetValue(enumerator.Current, i++);
                }
                else
                {
                    var value = Convert.ChangeType(enumerator.Current, elementType);
                    array.SetValue(value, i++);
                }
            }
            return array;
        }

        public void CreateOptionInfo(OptionInfo info)
        {
            var row = _dt.NewRow();
            OptionInfo.WriteDataRow(row, info);
            _dt.Rows.Add(row);
        }

        public OptionInfo FindByShortName(string shortName)
        {
            var expr = $"{ShortNameColumn} = '{shortName}'";
            var row = _dt.Select(expr).FirstOrDefault();
            if (row == null)
                throw new InvalidExpressionException($"no such short name : {shortName}");
            return OptionInfo.ReadDataRow(row);
        }

        public OptionInfo FindByFullName(string fullName)
        {
            var expr = $"{FullNameColumn} = '{fullName}'";
            var row = _dt.Select(expr).FirstOrDefault();
            if (row == null)
                throw new InvalidExpressionException($"no such short name : {fullName}");
            return OptionInfo.ReadDataRow(row);
        }

        public IEnumerator<OptionInfo> GetEnumerator()
        {
            foreach (DataRow row in _dt.Rows)
            {
                yield return OptionInfo.ReadDataRow(row);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string[] GetColumns()
        {
            string[] col = new string[_dt.Columns.Count];
            for (int i = 0; i < _dt.Columns.Count; i++)
            {
                col[i] = _dt.Columns[i].ColumnName;
            }
            return col;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            bool hasVerb = !string.IsNullOrEmpty(Verb);
            if (hasVerb)
                builder.Append($"Verb:{Verb}");
            if (hasVerb)
                builder.Append($", Opt:{this.ToArrayString()}");
            else
                builder.Append($"Opt:{this.ToArrayString()}");
            builder.Append("}");
            return builder.ToString();
        }

    }

    public class CommandInfo<T> : CommandInfo
    {
        public CommandInfo()
        {
            Analyze<T>();
        }
    }

    public class CommandInfoCache<T>
    {
        static CommandInfo _info;
        public static CommandInfo Info {
            get
            {
                if (_info == null)
                {
                    _info = CommandInfo.NewAnalyze<T>();
                }
                return _info;
            }
        }

        public static string Verb => Info.Verb;
    }

    public static class CommandInfoExtension
    {
        public static CommandInfo Verb(this CommandInfo info, string verb)
        {
            info.Verb = verb;
            return info;
        }

        public static CommandInfo Values(this CommandInfo info, Type valueType)
        {
            info.ValueType = valueType;
            return info;
        }

        public static CommandInfo Option(this CommandInfo info, Type type, string shortName, string fullName, object defaultValue = null, string helpText = "")
        {
            CommandInfo.OptionInfo optInfo = new CommandInfo.OptionInfo
            {
                ShortName = shortName,
                FullName = fullName,
                ValueType = type,
                DefaultValue = defaultValue,
                HelpText = helpText,
            };
            info.CreateOptionInfo(optInfo);
            return info;
        }

        public static CommandInfo Option<T>(this CommandInfo info, string shortName, string fullName, T defaultValue = default(T), string helpText = "")
        {
            return info.Option(typeof(T), shortName, fullName, defaultValue, helpText);
        }

        public static CommandInfo Flag(this CommandInfo info, string shortName, string fullName, string helpText)
        {
            return info.Option(shortName, fullName, false, helpText);
        }

        public static CommandOptions Parse(this CommandInfo info, IEnumerable<string> args)
        {
            Debug.Log(info);
            var enumerator = args.GetEnumerator();
            var options = new CommandOptions();
            List<string> temp_args = new List<string>();
            CommandInfo.OptionInfo optInfo = null;
            bool inOption = false;
            bool inValue = true;
            int i = 0;
            while (tryNext(out string current))
            {
                if (i == 0)
                {
                    options.Name = current;
                    i++;
                    continue;
                }
                if (i == 1 && !string.IsNullOrEmpty(info.Verb))
                {
                    options.Verb = current;
                    i++;
                    continue;
                }
                if (current.StartsWith("--"))
                {
                    if (inOption)
                    {
                        optInfo.SetOption(options, temp_args);
                        inOption = false;
                    }
                    if (inValue)
                    {
                        info.SetValue(options, temp_args);
                        inValue = false;
                    }
                    // full-name option 
                    optInfo = info.FindByFullName(current.TrimStart('-'));
                    inOption = true;
                    temp_args.Clear();
                    i++;
                    continue;
                }
                if (current.StartsWith("-"))
                {
                    if (inOption)
                    {
                        optInfo.SetOption(options, temp_args);
                        inOption = false;
                    }
                    if (inValue)
                    {
                        info.SetValue(options, temp_args);
                        inValue = false;
                    }
                    // short-name option
                    optInfo = info.FindByShortName(current.TrimStart('-'));
                    inOption = true;
                    temp_args.Clear();
                    i++;
                    continue;
                }
                temp_args.Add(current);
                i++;
            }
            if (inOption)
            {
                optInfo.SetOption(options, temp_args);
            }
            return options;

            bool tryNext(out string cur_arg)
            {
                if (enumerator.MoveNext())
                {
                    cur_arg = enumerator.Current;
                    return true;
                }
                cur_arg = null;
                return false;
            }
        }
    }

    public class CommandOptions : IEnumerable<KeyValuePair<string, object>>
    {
        #region -- Data table --
        public static readonly string ShortNameColumn = "short_name";
        public static readonly string FullNameColumn = "full_name";
        public static readonly string ValueColumn = "value";

        private static DataTable _optionTable;
        public static DataTable OptionTable {
            get
            {
                if (_optionTable == null)
                {
                    _optionTable = new DataTable();
                    _optionTable.Columns.Add(ShortNameColumn, typeof(string));
                    _optionTable.Columns[ShortNameColumn].MaxLength = 1;
                    _optionTable.Columns[ShortNameColumn].AllowDBNull = true;
                    _optionTable.Columns[ShortNameColumn].Unique = true;

                    _optionTable.Columns.Add(FullNameColumn, typeof(string));
                    _optionTable.Columns[FullNameColumn].MaxLength = 20;
                    _optionTable.Columns[FullNameColumn].AllowDBNull = true;
                    _optionTable.Columns[FullNameColumn].Unique = true;

                    _optionTable.Columns.Add(ValueColumn, typeof(object));
                    _optionTable.Columns[ValueColumn].AllowDBNull = true;
                }
                return _optionTable;
            }
        }

        private DataTable _dt;
        #endregion

        public string Name { get; set; }
        public string Verb { get; set; }

        public object Value { get; set; }

        public CommandOptions()
        {
            _dt = OptionTable.Clone();
        }

        public void AddOption(string shortName, string fullName, object value)
        {
            var row = _dt.NewRow();
            row[ShortNameColumn] = shortName;
            row[FullNameColumn] = fullName;
            row[ValueColumn] = value;
            _dt.Rows.Add(row);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.Append($"Name:{Name}");
            if (!string.IsNullOrEmpty(Verb))
                builder.Append($", Verb:{Verb}");
            var array = Value as IEnumerable;
            if (array == null)
                builder.Append($", Value:{Value}");
            else
                builder.Append($", Value:{array.ToArrayString()}");
            foreach (var pair in this)
            {
                builder.Append($", {pair.Key}:{pair.Value}");
            }
            builder.Append(")");
            return builder.ToString();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (DataRow row in _dt.Rows)
            {
                var key = (string)(row[FullNameColumn] != null ? row[FullNameColumn] : row[ShortNameColumn]);
                if (string.IsNullOrEmpty(key))
                    continue;
                var value = row[ValueColumn];
                yield return new KeyValuePair<string, object>(key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CommandInfoConverter
    {
        private Type _targetType;

        public ConvertInfo _valueInfo;
        public Dictionary<string, ConvertInfo> _dict;

        public CommandInfoConverter(Type targetType)
        {
            _targetType = targetType;
            var valueProp = (PropertyInfo)_targetType.Members(BindingFlags.Public | BindingFlags.Instance).WhereAttr(typeof(ValueAttribute), false).FirstOrDefault();
            if (valueProp != null)
            {
                _valueInfo = new ConvertInfo(valueProp);
            }
            _dict = new Dictionary<string, ConvertInfo>();
            var optProps = _targetType.Members(BindingFlags.Public | BindingFlags.Instance).WithAttr<OptionAttribute>(false);
            foreach (var optProp in optProps)
            {
                var shortName = optProp.Value.ShortName;
                var fullName = optProp.Value.FullName;
                if (!string.IsNullOrEmpty(shortName))
                    _dict.Add(shortName, new ConvertInfo((PropertyInfo)optProp.Key));
                if (!string.IsNullOrEmpty(fullName))
                    _dict.Add(fullName, new ConvertInfo((PropertyInfo)optProp.Key));
            }
        }

        public object Convert(CommandOptions options)
        {
            var target = Activator.CreateInstance(_targetType);
            if (_valueInfo != null)
                _valueInfo.Set(target, options.Value);
            foreach (var pair in options)
            {
                if (_dict.TryGetValue(pair.Key, out ConvertInfo convertInfo))
                {
                    convertInfo.Set(target, pair.Value);
                }
            }
            return target;
        }

        public class ConvertInfo
        {
            private PropertyInfo _prop;

            public ConvertInfo(PropertyInfo propInfo)
            {
                _prop = propInfo;
            }

            public void Set(object target, object value)
            {
                _prop.SetValue(target, value);
            }
        }
    }

    public class CommandInfoConverter<T>
    {
        static CommandInfoConverter _converter;

        public static T Convert(CommandOptions options)
        {
            if (_converter == null)
                _converter = new CommandInfoConverter(typeof(T));
            return (T)_converter.Convert(options);
        }
    }

    // verb
    [Verb("exp")]
    public class ExpOption
    {
        ////value
        //[Value("values")]
        //public IEnumerable<string> Values { get; set; }

        //options
        [Option("f", "float", Default: true)]
        public bool IsFloat { get; set; }

        public override string ToString()
        {
            //return $"{Values.ToArrayString()}, IsFloat:{IsFloat}";
            return $"IsFloat:{IsFloat}";
        }
    }
}
