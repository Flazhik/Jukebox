using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace JukeboxCore.Assets
{
    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    public class AssetsManager : MonoSingleton<AssetsManager>
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private readonly List<AssetBundle> bundles =  new();

        public void LoadAssets(byte[] raw)
        {
            bundles.Add(AssetBundle.LoadFromMemory(raw));
        }

        public void RegisterPrefabs(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                CheckType(type);
        }
        
        private void CheckType(IReflect type)
        {
            type.GetFields(Flags)
                .ToList()
                .ForEach(ProcessField);
        }

        private void ProcessField(FieldInfo field)
        {
            if (field.FieldType.IsArray
                || !typeof(Object).IsAssignableFrom(field.FieldType)
                || !field.IsStatic)
                return;

            var externalAsset = field.GetCustomAttribute<ExternalAsset>();
            var addressableAsset = field.GetCustomAttribute<AddressableAsset>();

            if (externalAsset != null)
                field.SetValue(null, LoadAsset(externalAsset.Path, externalAsset.Type));
            
            if (addressableAsset != null)
                Addressables.LoadAssetAsync<GameObject>(addressableAsset.Path).Completed += value =>
                    field.SetValue(null, Convert.ChangeType(value.Result, addressableAsset.AssetType));
        }

        private Object LoadAsset(string path, Type type)
        {
            return bundles.Select(bundle => bundle.LoadAsset(path, type)).FirstOrDefault();
        }
    }
}