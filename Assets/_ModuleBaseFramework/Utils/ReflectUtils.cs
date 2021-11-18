using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModuleBased.Utils
{
    public static class ReflectUtil
    {
        public static IEnumerable<MemberInfo> Members(this Type type, BindingFlags flags)
        {
            return type.GetMembers(flags);
        }

        public static IEnumerable<FieldInfo> Fields(this Type type, BindingFlags flags)
        {
            return type.GetFields(flags);
        }

        public static IEnumerable<PropertyInfo> Properties(this Type type, BindingFlags flags)
        {
            return type.GetProperties(flags);
        }

        public static IEnumerable<MethodInfo> Methods(this Type type, BindingFlags flags)
        {
            return type.GetMethods(flags);
        }

        public static IEnumerable<TSource> WhereAttr<TSource, TAttr>(this IEnumerable<TSource> sources, bool inherit)
        {
            List<TSource> list = new List<TSource>();
            foreach (var source in sources)
            {
                if (source.GetType().IsDefined(typeof(TAttr)))
                {
                    list.Add(source);
                }
            }
            return list;
        }

        public static IEnumerable<MemberInfo> WhereAttr(this IEnumerable<MemberInfo> members, Type attrType, bool inherit)
        {
            return members.Where(member =>
            {
                return member.IsDefined(attrType, inherit);
            });
        }

        public static IEnumerable<MemberInfo> WhereAttr<T>(this IEnumerable<MemberInfo> members, bool inherit)
        {
            return members.WhereAttr(typeof(T), inherit);
        }

        public static IEnumerable<FieldInfo> WhereAttr(this IEnumerable<FieldInfo> fields, Type attrType, bool inherit)
        {
            return fields.Where(field =>
            {
                return field.IsDefined(attrType, inherit);
            });
        }

        public static IEnumerable<FieldInfo> WhereAttr<T>(this IEnumerable<FieldInfo> fields, bool inherit) where T : Attribute
        {
            return fields.WhereAttr(typeof(T), inherit);
        }

        public static IEnumerable<PropertyInfo> WhereAttr(this IEnumerable<PropertyInfo> props, Type attrType, bool inherit)
        {
            return props.Where(prop =>
            {
                return prop.IsDefined(attrType, inherit);
            });
        }

        public static IEnumerable<PropertyInfo> WhereAttr<T>(this IEnumerable<PropertyInfo> props, bool inherit)
        {
            return props.WhereAttr(typeof(T), inherit);
        }

        public static IEnumerable<MethodInfo> WhereAttr(this IEnumerable<MethodInfo> methods, Type attrType, bool inherit)
        {
            return methods.Where(method =>
            {
                return method.IsDefined(attrType, inherit);
            });
        }

        public static IEnumerable<MethodInfo> WhereAttr<T>(this IEnumerable<MethodInfo> methods, bool inherit)
        {
            return methods.WhereAttr(typeof(T), inherit);
        }

        public static IEnumerable<KeyValuePair<TSource, Attribute>> WithAttr<TSource>(this IEnumerable<TSource> sources, Type attrType, bool inherit)
        {
            List<KeyValuePair<TSource, Attribute>> list = new List<KeyValuePair<TSource, Attribute>>();
            foreach (var source in sources)
            {
                var type = source.GetType();
                if (type.IsDefined(attrType, inherit))
                {
                    foreach (var attr in type.GetCustomAttributes(attrType, inherit))
                    {
                        var pair = new KeyValuePair<TSource, Attribute>(source, (Attribute)attr);
                        list.Add(pair);
                    }
                }
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<TSource, TAttr>> WithAttr<TSource, TAttr>(this IEnumerable<TSource> sources, bool inherit)
        {
            List<KeyValuePair<TSource, TAttr>> list = new List<KeyValuePair<TSource, TAttr>>();
            foreach (var source in sources)
            {
                var type = source.GetType();
                var attrType = typeof(TAttr);
                if (type.IsDefined(attrType, inherit))
                {
                    foreach (var attr in type.GetCustomAttributes(attrType, inherit))
                    {
                        var pair = new KeyValuePair<TSource, TAttr>(source, (TAttr)attr);
                        list.Add(pair);
                    }
                }
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<FieldInfo, T>> WithAttr<T>(this IEnumerable<FieldInfo> fields, bool inherit) where T : Attribute
        {
            return fields
                .WhereAttr<T>(inherit)
                .Select(field =>
                {
                    var attr = field.GetCustomAttribute<T>();
                    return new KeyValuePair<FieldInfo, T>(field, attr);
                });
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, T>> WithAttr<T>(this IEnumerable<PropertyInfo> props, bool inherit) where T : Attribute
        {
            return props
                .WhereAttr<T>(inherit)
                .Select(prop =>
                {
                    var attr = prop.GetCustomAttribute<T>();
                    return new KeyValuePair<PropertyInfo, T>(prop, attr);
                });
        }

        public static IEnumerable<KeyValuePair<MethodInfo, T>> WithAttr<T>(this IEnumerable<MethodInfo> methods, bool inherit) where T : Attribute
        {
            return methods
                .WhereAttr<T>(inherit)
                .Select(method =>
                {
                    var attr = method.GetCustomAttribute<T>();
                    return new KeyValuePair<MethodInfo, T>(method, attr);
                });
        }

        public static IEnumerable<KeyValuePair<MemberInfo, T>> WithAttr<T>(this IEnumerable<MemberInfo> members, bool inherit) where T : Attribute
        {
            return members
                .WhereAttr<T>(inherit)
                .Select(member =>
                {
                    var attr = member.GetCustomAttribute<T>();
                    return new KeyValuePair<MemberInfo, T>(member, attr);
                });
        }

        public static IEnumerable<FieldInfo> ToFields(this IEnumerable<MemberInfo> members)
        {
            List<FieldInfo> fieldList = new List<FieldInfo>();
            MemberInfo member;
            var enumerator = members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                member = enumerator.Current;
                if (member == null)
                    continue;
                if (member.MemberType == MemberTypes.Field)
                    fieldList.Add((FieldInfo)member);
            }
            return fieldList;
        }

        public static IEnumerable<PropertyInfo> ToProperties(this IEnumerable<MemberInfo> members)
        {
            List<PropertyInfo> propList = new List<PropertyInfo>();
            MemberInfo member;
            var enumerator = members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                member = enumerator.Current;
                if (member == null)
                    continue;
                if (member.MemberType == MemberTypes.Property)
                    propList.Add((PropertyInfo)member);
            }
            return propList;
        }

        public static IEnumerable<MethodInfo> ToMethods(this IEnumerable<MemberInfo> members)
        {
            List<MethodInfo> methodList = new List<MethodInfo>();
            MemberInfo member;
            var enumerator = members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                member = enumerator.Current;
                if (member == null)
                    continue;
                if (member.MemberType == MemberTypes.Method)
                    methodList.Add((MethodInfo)member);
            }
            return methodList;
        }

        public static IEnumerable<object> GetValues(this FieldInfo field, IEnumerable<object> instances)
        {
            var valueList = new List<object>();
            foreach (var instance in instances)
            {
                valueList.Add(field.GetValue(instance));
            }
            return valueList;
        }

        public static IEnumerable<T> GetValues<T>(this FieldInfo field, IEnumerable<object> instances)
        {
            var valueList = new List<T>();
            foreach (var instance in instances)
            {
                valueList.Add((T)field.GetValue(instance));
            }
            return valueList;
        }

        public static IEnumerable<object> GetValues(this PropertyInfo prop, IEnumerable<object> instances)
        {
            var valueList = new List<object>();
            foreach (var instance in instances)
            {
                valueList.Add(prop.GetValue(instance));
            }
            return valueList;
        }

        public static IEnumerable<T> GetValues<T>(this PropertyInfo prop, IEnumerable<object> instances)
        {
            var valueList = new List<T>();
            foreach (var instance in instances)
            {
                valueList.Add((T)prop.GetValue(instance));
            }
            return valueList;
        }

        public static void SetValue(this FieldInfo field, IEnumerable<object> instances, object arg)
        {
            foreach (var instance in instances)
            {
                field.SetValue(instance, arg);
            }
        }

        public static void SetValue(this PropertyInfo prop, IEnumerable<object> instances, object arg)
        {
            foreach (var instance in instances)
            {
                prop.SetValue(instance, arg);
            }
        }

        public static IEnumerable<FieldInfo> ToFieldWithType(this IEnumerable<MemberInfo> members, Type fieldType)
        {
            FieldInfo field;
            var fieldList = new List<FieldInfo>();
            foreach (var member in members)
            {
                if (member.MemberType != MemberTypes.Field)
                    continue;
                field = (FieldInfo)member;
                if (field.FieldType == fieldType)
                    fieldList.Add(field);
            }
            return fieldList;
        }

        public static IEnumerable<FieldInfo> ToFieldWithType<T>(this IEnumerable<MemberInfo> members)
        {
            return members.ToFieldWithType(typeof(T));
        }

        public static IEnumerable<PropertyInfo> ToPropertyWithType(this IEnumerable<MemberInfo> members, Type propType)
        {
            PropertyInfo prop;
            var propList = new List<PropertyInfo>();
            foreach (var member in members)
            {
                if (member.MemberType != MemberTypes.Property)
                    continue;
                prop = (PropertyInfo)member;
                if (prop.PropertyType == propType)
                    propList.Add(prop);
            }
            return propList;
        }

        public static IEnumerable<PropertyInfo> ToPropertyWithType<T>(this IEnumerable<MemberInfo> members)
        {
            return members.ToPropertyWithType(typeof(T));
        }

        public static IEnumerable<MethodInfo> ToMethodWithReturnType(this IEnumerable<MemberInfo> members, Type returnType)
        {
            MethodInfo method;
            var methodList = new List<MethodInfo>();
            foreach (var member in members)
            {
                if (member.MemberType != MemberTypes.Method)
                    continue;
                method = (MethodInfo)member;
                if (method.ReturnType == returnType)
                    methodList.Add(method);
            }
            return methodList;
        }

        public static IEnumerable<MethodInfo> ToMethodWithReturnType<T>(this IEnumerable<MemberInfo> members)
        {
            return members.ToMethodWithReturnType(typeof(T));
        }

        public static IEnumerable<FieldInfo> WhereType(this IEnumerable<FieldInfo> fields, Type fieldType)
        {
            return fields.Where(field =>
            {
                return field.FieldType == fieldType;
            });
        }

        public static IEnumerable<FieldInfo> WhereType<T>(this IEnumerable<FieldInfo> fields)
        {
            return fields.WhereType(typeof(T));
        }

        public static IEnumerable<PropertyInfo> WhereType(this IEnumerable<PropertyInfo> props, Type propType)
        {
            return props.Where(prop =>
            {
                return prop.PropertyType == propType;
            });
        }

        public static IEnumerable<PropertyInfo> WhereType<T>(this IEnumerable<PropertyInfo> props)
        {
            return props.WhereType(typeof(T));
        }

        public static IEnumerable<MethodInfo> WhereReturnType(this IEnumerable<MethodInfo> methods, Type returnType)
        {
            return methods.Where(method =>
            {
                return method.ReturnType == returnType;
            });
        }

        public static IEnumerable<MethodInfo> WhereReturnType<T>(this IEnumerable<MethodInfo> methods)
        {
            return methods.WhereReturnType(typeof(T));
        }

        public static IEnumerable<FieldInfo> FieldsWithType(this IEnumerable<Type> types, Type fieldType, BindingFlags flags)
        {
            var fieldList = new List<FieldInfo>();
            foreach (var type in types)
            {
                foreach (var field in type.GetFields(flags))
                {
                    if (field.FieldType == fieldType)
                    {
                        fieldList.Add(field);
                    }
                }
            }
            return fieldList;
        }

        public static IEnumerable<FieldInfo> FieldsWithType<T>(this IEnumerable<Type> types, BindingFlags flags)
        {
            return types.FieldsWithType(typeof(T), flags);
        }

        public static IEnumerable<PropertyInfo> PropertiesWithType(this IEnumerable<Type> types, Type propType, BindingFlags flags)
        {
            var propList = new List<PropertyInfo>();
            foreach (var type in types)
            {
                foreach (var prop in type.GetProperties(flags))
                {
                    if (prop.PropertyType == propType)
                    {
                        propList.Add(prop);
                    }
                }
            }
            return propList;
        }

        public static IEnumerable<PropertyInfo> PropertiesWithType<T>(this IEnumerable<Type> types, BindingFlags flags)
        {
            return types.PropertiesWithType(typeof(T), flags);
        }

        public static IEnumerable<MethodInfo> MethodsWithReturnType(this IEnumerable<Type> types, Type returnType, BindingFlags flags)
        {
            var methodList = new List<MethodInfo>();
            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(flags))
                {
                    if (method.ReturnType == returnType)
                    {
                        methodList.Add(method);
                    }
                }
            }
            return methodList;
        }

        public static IEnumerable<MethodInfo> MethodsWithReturnType<T>(this IEnumerable<Type> types, BindingFlags flags)
        {
            return types.MethodsWithReturnType(typeof(T), flags);
        }

        public static IEnumerable<MemberInfo> IfFieldThen(this IEnumerable<MemberInfo> members, Action<FieldInfo> action)
        {
            if (action == null)
                return members;
            foreach (var member in members)
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    action.Invoke((FieldInfo)member);
                }
            }
            return members;
        }

        public static IEnumerable<MemberInfo> IfPropertyThen(this IEnumerable<MemberInfo> members, Action<PropertyInfo> action)
        {
            if (action == null)
                return members;
            foreach (var member in members)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    action.Invoke((PropertyInfo)member);
                }
            }
            return members;
        }

        public static IEnumerable<MemberInfo> IfMethodThen(this IEnumerable<MemberInfo> members, Action<MethodInfo> action)
        {
            if (action == null)
                return members;
            foreach (var member in members)
            {
                if (member.MemberType == MemberTypes.Method)
                {
                    action.Invoke((MethodInfo)member);
                }
            }
            return members;
        }

        public static IEnumerable<T> WhereAs<T>(this IEnumerable<object> targets, bool inherit = true, bool implement = true)
        {
            return targets.Where(target =>
            {
                Type asType = typeof(T);
                Type type = target.GetType();
                return (type == asType) ||
                    (inherit && type.IsSubclassOf(asType)) ||
                    (implement && asType.IsAssignableFrom(type));
            }).OfType<T>();
        }

        public static void ForEachAs<T>(this IEnumerable<object> targets, Action<T> action, bool inherit = true, bool implement = true)
        {
            foreach (var target in targets)
            {
                if (target == null)
                    continue;
                Type asType = typeof(T);
                Type type = target.GetType();
                if ((type == asType) ||
                    (inherit && type.IsSubclassOf(asType)) ||
                    (implement && asType.IsAssignableFrom(type)))
                {
                    action?.Invoke((T)target);
                }
            }
        }
    }
}
