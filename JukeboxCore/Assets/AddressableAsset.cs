using System;

namespace JukeboxCore.Assets;

[AttributeUsage(AttributeTargets.Field)]
public class AddressableAsset(string path) : Attribute
{
    public string Path { get; } = path;
}
