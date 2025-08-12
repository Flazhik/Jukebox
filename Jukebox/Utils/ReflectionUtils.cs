using System;
using System.Reflection;

namespace Jukebox.Utils
{
    public abstract class ReflectionUtils
    {
        private const BindingFlags PrivateFields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static T GetPrivate<T>(object instance, Type classType, string field)
        {
            var privateField = classType.GetField(field, PrivateFields);
            return (T)(privateField != null ? privateField.GetValue(instance) : null);
        }

        public static void SetPrivate<T, TV>(T instance, Type classType, string field, TV value)
        {
            var privateField = classType.GetField(field, PrivateFields | BindingFlags.SetField);
            if (privateField != null)
                privateField.SetValue(instance, value);
        }
    }
}