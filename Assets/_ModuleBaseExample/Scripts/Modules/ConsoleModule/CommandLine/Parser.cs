using ModuleBased.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

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

        public static void Parse<T>(IEnumerable<string> args, Action<T> onOpt, Action<Exception> onError)
        {
            try
            {
                var info = CommandInfo.NewAnalyze<T>();
                var opt = info.Parse(args);
                onOpt?.Invoke(CommandInfoConverter<T>.Convert(opt));
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
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
}
