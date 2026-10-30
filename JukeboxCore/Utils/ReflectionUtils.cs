using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;
using static System.Reflection.BindingFlags;

namespace JukeboxCore.Utils;

public abstract class ReflectionUtils
{
    private const BindingFlags PrivateFields = Public | NonPublic | Instance;
    private const BindingFlags StaticFields = Public | NonPublic | Static;
    
    public static T GetPrivate<T>(object instance, Type classType, string field)
    {
        var privateField = classType.GetField(field, PrivateFields);
        return (T)(privateField != null ? privateField.GetValue(instance) : null);
    }

    public static void SetPrivate<T, TV>(T instance, Type classType, string field, TV value)
    {
        var privateField = classType.GetField(field, PrivateFields | SetField);
        if (privateField != null)
            privateField.SetValue(instance, value);
    }

    public static List<FieldInfo> GetAssembliesStaticFields(params Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(static assembly => assembly.GetTypes())
            .SelectMany(static type => type.GetFields(StaticFields))
            .Where(static field => !field.IsInitOnly
                                   && !field.FieldType.IsArray
                                   && typeof(Object).IsAssignableFrom(field.FieldType))
            .ToList();
    }
}