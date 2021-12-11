using ModuleBased.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModuleBased.Example.CommandLine
{
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
}
