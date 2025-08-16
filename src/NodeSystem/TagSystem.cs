using System.Text.Json;

namespace MukiaEngine.NodeSystem;

[Serializable]
public class TagException : Exception
{
    public TagException() { }
    public TagException(string message) : base(message) { }
    public TagException(string message, Exception inner) : base(message, inner) { }
}

internal class Tag(uint id, string name, IEnumerable<Node> tagged)
{
    public uint ID { get; set; } = id;
    public string Name { get; set; } = name;
    public List<Node> Tagged { get; set; } = [.. tagged];
}

[SavableSingleton]
/// <summary>
/// Used to quickly query nodes.
/// </summary>
public static class TagSystem
{
    internal static readonly Dictionary<string, Tag> NameToTag = [];
    private static uint TagIndex = 0;

    internal static Tag GetTagFromName(string tagName)
    {
        if (!NameToTag.TryGetValue(tagName, out Tag? tag))
        {
            throw new TagException($"Tag {tagName} does not exist");
        }

        return tag;
    }

    /// <summary>
    /// Get all nodes that have the <paramref name="tagName"/>.
    /// </summary>
    /// <param name="tagName">The tag name</param>
    /// <returns>Array of nodes</returns>
    /// <exception cref="TagException"></exception>
    public static Node[] GetTagged(string tagName)
    {
        Tag tag = GetTagFromName(tagName);

        return [.. tag.Tagged];
    }

    /// <summary>
    /// Checks if a <paramref name="tag"/> is valid.
    /// </summary>
    /// <param name="tag">The tag name</param>
    /// <returns><c>true</c>, if the <paramref name="tag"/> is valid.</returns>
    public static bool IsTag(string tag)
    {
        return NameToTag.ContainsKey(tag);
    }

    /// <summary>
    /// Does the <paramref name="node"/> has a <paramref name="tag"/>.
    /// </summary>
    /// <param name="node">The node checked</param>
    /// <param name="tag">The tag name</param>
    /// <returns><c>true</c>, if the <paramref name="tag"/> is valid.</returns>
    public static bool HasTag(Node node, string tag)
    {
        Node[] tagged = GetTagged(tag);

        return tagged.Contains(node);
    }

    /// <summary>
    /// Adds the <paramref name="tagName"/> to the <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The node to add</param>
    /// <param name="tagName">The tag name</param>
    public static void AddTag(Node node, string tagName)
    {
        bool isTag = IsTag(tagName);

        if (isTag)
        {
            Tag tag = NameToTag[tagName];
            tag.Tagged.Add(node);
            node._Tags.Add(tag.ID);
        }
        else
        {
            Tag tag = new(TagIndex, tagName, [node]);
            NameToTag[tagName] = tag;
            node._Tags.Add(TagIndex);

            TagIndex++;
        }
    }

    /// <summary>
    /// Removes the <paramref name="tagName"/> from the <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The node that the tag is removed.</param>
    /// <param name="tagName">The tag name</param>
    /// <returns><c>true</c>, if the <paramref name="tagName"/> did exist on the tag.</returns>
    public static void RemoveTag(Node node, string tagName)
    {
        bool isATag = IsTag(tagName);
        if (isATag)
        {
            return;
        }

        Tag tag = GetTagFromName(tagName);

        tag.Tagged.Remove(node);
        node._Tags.Remove(tag.ID);
        if (tag.Tagged.Count <= 0)
        {
            NameToTag.Remove(tagName);
        }
    }

    /// <summary>
    /// Gets the tags on the <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>The tags attached to the <paramref name="node"/>.</returns>
    public static uint[] GetTagIDs(Node node)
    {
        return [.. node._Tags];
    }

    /// <inheritdoc cref="GetTagIDs"/>
    public static string[] GetNameTags(Node node)
    {
        Dictionary<uint, string> conv = [];
        foreach (var (_, tag) in NameToTag)
        {
            conv.Add(tag.ID, tag.Name);
        }

        string[] r = new string[node._Tags.Count];
        for (int i = 0; i < node._Tags.Count; i++)
        {
            uint id = node._Tags[i];
            r[i] = conv[id];
        }

        return [.. r];
    }

    /// <inheritdoc cref="GetAllTagIDs"/>
    public static string[] GetAllTags()
    {
        Dictionary<uint, string> conv = [];
        foreach (var (_, tag) in NameToTag)
        {
            conv.Add(tag.ID, tag.Name);
        }

        string[] r = new string[NameToTag.Count];
        Tag[] keys = [.. NameToTag.Values];
        for (int i = 0; i < keys.Length; i++)
        {
            uint id = keys[i].ID;
            r[i] = conv[id];
        }

        return r;
    }

    public static byte[] Save()
    {
        Tag[] tags = [.. NameToTag.Values];

        byte[] b = JsonSerializer.SerializeToUtf8Bytes(tags);

        return b;
    }

    public static void Load(byte[] b)
    {
        Tag[]? tags = JsonSerializer.Deserialize<Tag[]>(b);

        ArgumentNullException.ThrowIfNull(tags, nameof(b));

        TagIndex = (uint)tags.Length;
        foreach (Tag tag in tags)
        {
            NameToTag.Add(tag.Name, tag);
        }
    }
}