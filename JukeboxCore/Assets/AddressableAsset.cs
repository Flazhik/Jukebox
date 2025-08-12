using System;

namespace JukeboxCore.Assets
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AddressableAsset : Attribute
    {
        public string Path { get; }
        
        public Type AssetType { get; }

        public AddressableAsset(string path, Type type)
        {
            Path = path;
            AssetType = type;
        }
    }
}