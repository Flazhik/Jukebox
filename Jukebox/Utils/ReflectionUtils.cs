using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Jukebox.Utils
{
    public abstract class ReflectionUtils
    {
        private const BindingFlags PrivateFields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static object GetPrivate<T>(T instance, Type classType, string field)
        {
            var privateField = classType.GetField(field, PrivateFields);
            return privateField.GetValue(instance);
        }

        public static void SetPrivate<T, TV>(T instance, Type classType, string field, TV value)
        {
            var privateField = classType.GetField(field, PrivateFields | BindingFlags.SetField);
            privateField.SetValue(instance, value);
        }
        
        public static IEnumerable<CodeInstruction> IL(params (OpCode, object)[] instructions) =>
            instructions.Select(i => new CodeInstruction(i.Item1, i.Item2)).ToList();
    }
}