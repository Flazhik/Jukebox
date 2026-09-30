using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JukeboxCore.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
using static JukeboxCore.Logging.JukeboxLogger;

namespace JukeboxCore.Assets;

public static class AssetsManager
{
    private static readonly Dictionary<object, AsyncOperationHandle> Handles = [];

    public static void Inject(params Assembly[] assemblies)
    {
        var targets = ReflectionUtils.GetAssembliesStaticFields(assemblies)
            .Select(static field => (field, addressable: field.GetCustomAttribute<AddressableAsset>()))
            .Where(static target => target.addressable != null);

        foreach (var (field, addressable) in targets)
            Assign(field, addressable.Path);
    }

    public static T Load<T>(object key) where T : Object
    {
        var normalized = NormalizeKey(key);
        if (Handles.TryGetValue(normalized, out var cached))
            return cached.Result as T;

        var handle = Addressables.LoadAssetAsync<T>(key);
        handle.WaitForCompletion();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Log.LogError($"Failed to load addressable {key}: {handle.OperationException}");
            Addressables.Release(handle);
            return null;
        }

        Handles[normalized] = handle;
        return handle.Result;
    }

    private static void Assign(FieldInfo field, string key)
    {
        var handle = Addressables.LoadAssetAsync<Object>(key);
        var asset = handle.WaitForCompletion();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Log.LogError($"Failed to load addressable {key}: {handle.OperationException}");
            return;
        }

        if (!field.FieldType.IsInstanceOfType(asset))
        {
            Log.LogError($"Addressable {key} is {asset.GetType().Name}, expected {field.FieldType.Name}");
            return;
        }

        Handles[NormalizeKey(key)] = handle;
        field.SetValue(null, asset);
    }
    
        public static void ReleaseAll()
        {
            foreach (var handle in Handles.Values)
                Addressables.Release(handle);
    
            Handles.Clear();
        }

    private static object NormalizeKey(object key) => key is AssetReference reference ? reference.AssetGUID : key;
}
