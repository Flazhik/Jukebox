using System;
using System.IO;
using Newtonsoft.Json;

namespace JukeboxCore.Utils;

public sealed class JsonStore<T> where T : class, new()
{
    private readonly object @lock = new();
    private readonly string path;
    private bool dirty;

    public JsonStore(string path)
    {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
        Value = new T();
        Load();
    }

    public T Value { get; private set; }

    public bool Loaded { get; private set; }

    public void Load()
    {
        lock (@lock)
        {
            Loaded = false;
            if (!File.Exists(path))
                return;

            try
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return;

                if (JsonConvert.DeserializeObject<T>(json) is not { } deserialized)
                    return;
                
                Value = deserialized;
                Loaded = true;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public void SaveIfDirty()
    {
        lock (@lock)
        {
            if (dirty)
                Write();
        }
    }

    public void Save()
    {
        lock (@lock)
        {
            Write();
        }
    }

    public void MarkDirty()
    {
        lock (@lock)
        {
            dirty = true;
        }
    }

    public void Mutate(Action<T> change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        lock (@lock)
        {
            change(Value);
            dirty = true;
        }
    }

    private void Write()
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, JsonConvert.SerializeObject(Value));
        dirty = false;
    }
}