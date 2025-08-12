using System;

namespace JukeboxCore.Assets
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExternalAsset : Attribute
    {
        public string Path { get; }
        
        public Type Type { get; }

        public ExternalAsset(string path, Type type)
        {
            Path = path;
            Type = type;
        }
    }
}