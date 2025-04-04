using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Jukebox.Assets
{
    public class AssetsManager : MonoSingleton<AssetsManager>
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private AssetBundle bundle;

        public void LoadAssets()
        {
            bundle = AssetBundle.LoadFromMemory(Resources.Jukebox);
        }

        public void RegisterPrefabs()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
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
                field.SetValue(null, bundle.LoadAsset(externalAsset.Path, externalAsset.Type));

            if (addressableAsset != null)
                Addressables.LoadAssetAsync<GameObject>(addressableAsset.Path).Completed += value =>
                {
                    field.SetValue(null, Convert.ChangeType(value.Result, addressableAsset.AssetType));
                };
        }
    }
}