using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MukiaEngine;

/// <summary>
/// Thrown when resource couldn't be recieved.
/// </summary>
[Serializable]
public class ResourceException : Exception
{
    public ResourceException() { }
    public ResourceException(string message) : base(message) { }
    public ResourceException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Allows for a class to be able to be saved into the file system.
/// Automatically loaded in <see cref="NodeSystem.SceneHandler"/>.
/// </summary>
public abstract class Resource
{
    private bool _SavedToFile = false;

    /// <summary>
    /// Is a resource saved to a file.
    /// </summary>
    [JsonIgnore]
    public bool SavedToFile => _SavedToFile;

    private string? _FilePath = null;

    /// <summary>
    /// Where the resource saved to.
    /// </summary>
    [JsonIgnore]
    public string FilePath
    {
        get
        {
            if (!_SavedToFile || _FilePath is null)
            {
                throw new ResourceException("Tried to get file path, however the resource isn't saved.");
            }
            return _FilePath;
        }
    }

    private static readonly JsonSerializerOptions JSONOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    /// <summary>
    /// Saves a resource to a file.
    /// </summary>
    /// <param name="path">Resource path location</param>
    public void SaveResource(string path)
    {
        _FilePath = path;
        _SavedToFile = true;

        string jsonStr = JsonSerializer.Serialize(this, GetType());

        byte[] bJson = Encoding.UTF8.GetBytes(jsonStr);

        using FileStream file = File.OpenWrite(path);
        file.Write(bJson);
    }

    /// <summary>
    /// Load a JSON string into a resource.
    /// </summary>
    /// <param name="json">The JSON string</param>
    /// <param name="type">The type of resource</param>
    /// <returns>The loaded resource</returns>
    public static Resource? LoadResource(string json, Type type)
    {
        var resource = JsonSerializer.Deserialize(json, type, JSONOptions) as Resource;

        return resource;
    }

    /// <inheritdoc cref="LoadResource(string, Type)"/>
    /// <typeparam name="TResource">The type of resource</typeparram>
    public static TResource? LoadResource<TResource>(string json) where TResource : Resource
    {
        return (TResource?)LoadResource(json, typeof(TResource));
    }

    /// <summary>
    /// Loads a resource file into the resource class.
    /// </summary>
    /// <param name="path">Path to the resource path</param>
    /// <param name="type">The type of resource</param>
    /// <returns>The loaded resource</returns>
    public static Resource? LoadResourceFromFile(string path, Type type)
    {
        using FileStream fileStream = File.OpenRead(path);
        int length = (int)fileStream.Length;

        byte[] b = new byte[length];
        fileStream.ReadExactly(b, 0, length);

        string json = Encoding.UTF8.GetString(b);

        Resource? res = LoadResource(json, type);
        if (res is not null)
        {
            res._FilePath = path;
            res._SavedToFile = true;
        }

        return res;
    }

    /// <inheritdoc cref="LoadResourceFromFile(string, Type)"/>
    /// <typeparam name="TResource">The type of resource</typeparram>
    public static TResource? LoadResourceFromFile<TResource>(string path) where TResource : Resource
    {
        return (TResource?)LoadResourceFromFile(path, typeof(TResource));
    }

    /// <summary>
    /// Is the <paramref name="obj"/> a saved resource?
    /// </summary>
    /// <typeparam name="T">Type of object (needs to a class)</typeparam>
    /// <param name="obj">The object checked</param>
    /// <param name="path">Path to the resource path</param>
    /// <returns>Is a saved resource</returns>
    public static bool IsSavedResource<T>(T? obj, out string? path) where T : class
    {
        if (obj is not Resource res)
        {
            path = null;
            return false;
        }

        bool isSaved = res.SavedToFile;

        if (!isSaved)
        {
            path = null;
            return false;
        }

        path = res.FilePath;
        return true;
    }
}
