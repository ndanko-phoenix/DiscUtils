using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DiscUtils.CoreCompat
{
    internal static class ReflectionHelper
    {
        public static bool IsEnum(Type type)
        {
#if NETSTANDARD1_5
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static Attribute GetCustomAttribute(PropertyInfo property, Type attributeType)
        {
#if NETSTANDARD1_5
            return property.GetCustomAttribute(attributeType);
#else
            return Attribute.GetCustomAttribute(property, attributeType);
#endif
        }

        public static Attribute GetCustomAttribute(PropertyInfo property, Type attributeType, bool inherit)
        {
#if NETSTANDARD1_5
            return property.GetCustomAttribute(attributeType, inherit);
#else
            return Attribute.GetCustomAttribute(property, attributeType, inherit);
#endif
        }

        public static Attribute GetCustomAttribute(FieldInfo field, Type attributeType)
        {
#if NETSTANDARD1_5
            return field.GetCustomAttribute(attributeType);
#else
            return Attribute.GetCustomAttribute(field, attributeType);
#endif
        }

        public static Attribute GetCustomAttribute(Type type, Type attributeType)
        {
#if NETSTANDARD1_5
            return type.GetTypeInfo().GetCustomAttribute(attributeType);
#else
            return Attribute.GetCustomAttribute(type, attributeType);
#endif
        }

        public static Attribute GetCustomAttribute(Type type, Type attributeType, bool inherit)
        {
#if NETSTANDARD1_5
            return type.GetTypeInfo().GetCustomAttribute(attributeType, inherit);
#else
            return Attribute.GetCustomAttribute(type, attributeType);
#endif
        }

        public static IEnumerable<Attribute> GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
#if NETSTANDARD1_5
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);
#else
            return Attribute.GetCustomAttributes(type, attributeType);
#endif
        }

        public static Assembly GetAssembly(Type type)
        {
#if NETSTANDARD1_5
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static int SizeOf<T>()
        {
#if NETSTANDARD1_5
            return Marshal.SizeOf<T>();
#else
            return Marshal.SizeOf(typeof(T));
#endif
        }
    }
}