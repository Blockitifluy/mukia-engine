using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MukiaEngine.NodeSystem;

[Serializable]
public class MalformedSceneException : Exception
{
    public MalformedSceneException() { }
    public MalformedSceneException(string message) : base(message) { }
    public MalformedSceneException(string message, Exception inner) : base(message, inner) { }
}

[Serializable]
public class SavingParentingException : Exception
{
    public SavingParentingException() { }
    public SavingParentingException(string message) : base(message) { }
    public SavingParentingException(string message, Exception inner) : base(message, inner) { }
}

[Serializable]
public class NodeRefException : Exception
{
    public NodeRefException() { }
    public NodeRefException(string message) : base(message) { }
    public NodeRefException(string message, Exception inner) : base(message, inner) { }
}

[Serializable]
public class LoadingNodeException : Exception
{
    public static void ThrowIfNull([NotNull] object? obj, string msg)
    {
        if (obj is null)
        {
            throw new LoadingNodeException(msg);
        }
    }
    public LoadingNodeException() { }
    public LoadingNodeException(string message) : base(message) { }
    public LoadingNodeException(string message, System.Exception inner) : base(message, inner) { }
}

[Flags]
public enum SceneLoadingFlags : ulong
{
    None = 0,
    Verbose = 1 << 1,
}

[Flags]
public enum SceneSavingFlags : ulong
{
    None = 0,
    Verbose = 1 << 1,
}

public static partial class SceneHandler
{
    private const string NoNodeRefFound = "null";

    private abstract class FlagHandler
    {
        public abstract char FlagCharacter { get; }
        public abstract PropertyFlag Flag { get; }

        public abstract bool IsFlag(object value, out string valueStr);
        public abstract object? HandleValue(ImportProp import);
    }

    private class ResourceFlagHandler : FlagHandler
    {
        public override char FlagCharacter => 'r';
        public override PropertyFlag Flag => PropertyFlag.Resource;

        public override bool IsFlag(object value, out string valueStr)
        {
            bool isResource = Resource.IsSavedResource(value, out string? path);
            valueStr = path ?? "";
            return isResource;
        }

        public override object? HandleValue(ImportProp import)
        {
            return Resource.LoadResourceFromFile(import.Value, import.ValueType);
        }
    }

    private class NodeRefFlagHandler : FlagHandler
    {
        public override char FlagCharacter => 'n';
        public override PropertyFlag Flag => PropertyFlag.NodeRef;

        public override bool IsFlag(object export, out string valueStr)
        {
            bool isNode = export.GetType() == typeof(Node) || export.GetType().IsSubclassOf(typeof(Node));

            valueStr = "";
            return isNode;
        }

        public override object? HandleValue(ImportProp import)
        {
            return null;
        }
    }

    private static readonly FlagHandler[] FlagHandlers = [new ResourceFlagHandler(), new NodeRefFlagHandler()];

    [Flags]
    public enum PropertyFlag
    {
        None = 0,
        Resource = 1 << 1,
        NodeRef = 1 << 2
    }

    private static PropertyFlag ParseFlags(string flagStr)
    {
        PropertyFlag flags = PropertyFlag.None;
        string[] flagSep = flagStr.Split("");

        foreach (string c in flagSep)
        {
            foreach (FlagHandler handler in FlagHandlers)
            {
                if (handler.FlagCharacter.ToString() != c)
                {
                    continue;
                }
                flags |= handler.Flag;
            }
        }

        return flags;
    }

    #region Saving Scene
    // TODO - Add SaveFlags
    private struct ExportNode
    {
        public Type NodeType;
        public ExportProp[] Exports;
        public Node Node;
        public uint ID;

        public string Save(Dictionary<Node, uint> nodeToLocalID, uint localID)
        {
            var saveNode = Node.GetType().GetCustomAttribute<SaveNodeAttribute>(false);
            if (saveNode is null)
            {
                return "";
            }

            TryToGetParentLocalID(Node, nodeToLocalID, out var parentID);

            StringBuilder builder = new();

            builder.AppendLine($"[{saveNode.SavedName} local-id='{localID}' parent='{parentID}']");

            foreach (ExportProp export in Exports)
            {
                builder.AppendLine(export.Save());
            }
            return builder.ToString();
        }

        public ExportNode(Node node, uint id)
        {
            NodeType = node.GetType();
            Node = node;
            ID = id;

            List<ExportProp> b = [];
            GetExportPropetries(b, node);
            Exports = [.. b];
        }
    }

    private struct ExportProp
    {
        public object? Source;
        public PropertyFlag Flags = PropertyFlag.None;

        public string Name;
        public string Value;

        public Type ValueType;

        private readonly string FlagStr = "";

        public string Save()
        {
            string exportValue = Value;

            StringBuilder builder = new();
            builder.Append($"\t{ValueType} ");

            if (Flags != PropertyFlag.None)
            {
                builder.Append(FlagStr + " ");
            }

            builder.Append($"{Name}={exportValue}");

            return builder.ToString();
        }

        public override readonly string ToString()
        {
            return $"ExportNode {Name}, {ValueType}";
        }

        public ExportProp(PropertyInfo info, Node node)
        {
            Source = info.GetValue(node);

            ValueType = Source is null ? info.PropertyType : Source.GetType();
            Name = info.Name;

            foreach (FlagHandler handler in FlagHandlers)
            {
                if (Source is null)
                {
                    break;
                }

                if (!handler.IsFlag(Source, out string? valueStr))
                {
                    continue;
                }

                Flags |= handler.Flag;
                FlagStr += handler.FlagCharacter;
                Value = valueStr;
            }

            if (Flags == PropertyFlag.None)
            {
                string json = JsonSerializer.Serialize(Source, ValueType, JSONOptions);
                Value = json;
            }
            else
            {
                Value = "";
            }
        }
    }

    private static void GetExportPropetries(List<ExportProp> b, Node node)
    {
        Type type = node.GetType();

        PropertyInfo[] properties = type.GetProperties();

        foreach (var info in properties)
        {
            ExportAttribute? exportAttribute = info.GetCustomAttribute<ExportAttribute>(true);
            if (exportAttribute is null)
            {
                continue;
            }

            ExportProp exportNode = new(info, node);
            b.Add(exportNode);
        }
    }

    private static bool TryToGetParentLocalID(Node child, Dictionary<Node, uint> nodeToID, out uint id)
    {
        if (child.Parent is null)
        {
            id = 0;
            return false;
        }

        if (!nodeToID.TryGetValue(child.Parent, out var _id))
        {
            // Caused when not sorting property (engine's fault when happening)
            // See the SaveScene function, to see if its sorting correctly (or at all)
            throw new SavingParentingException($"Child {child} could not find parent's ID");
        }

        id = _id;
        return true;
    }

    private static void CompleteNodeRefrences(Dictionary<Node, uint> nodeToLocalID, List<ExportNode> exports)
    {
        foreach (ExportNode exp in exports)
        {
            for (int i = 0; i < exp.Exports.Length; i++)
            {
                ExportProp[] props = exp.Exports;
                ref ExportProp prop = ref props[i];

                if (!prop.Flags.HasFlag(PropertyFlag.NodeRef))
                {
                    continue;
                }

                if (prop.Source is not Node refNode)
                {
                    // Internal error, not user's fault
                    // Something has went wrong in Node Reference detection
                    throw new NodeRefException($"Node reference ({prop.Name}) is not a node in {exp.Node}");
                }

                if (!nodeToLocalID.TryGetValue(refNode, out uint id))
                {
                    // Console.WriteLine($"Propetry {prop.Name} depends on Node {refNode}, however it has not been saved.");
                    continue;
                }
                prop.Value = id.ToString();
            }
        }
    }

    private static void GetValidNodesForSaving(List<Node> nodes, List<ExportNode> b, Dictionary<Node, uint> nodeToLocalID)
    {
        uint i = 0;
        foreach (Node node in nodes)
        {
            i++;
            var saveNode = node.GetType().GetCustomAttribute<SaveNodeAttribute>(false);
            if (saveNode is null)
            {
                continue;
            }

            if (!node.CanBeArchived())
            {
                continue;
            }

            ExportNode exportNode = new(node, i);
            b.Add(exportNode);
            nodeToLocalID.Add(node, i);
        }
    }

    private static MemoryStream ExportsToStream(List<ExportNode> exports, Dictionary<Node, uint> nodeToLocalID)
    {
        MemoryStream stream = new();
        foreach (ExportNode export in exports)
        {
            Node node = export.Node;

            string exportText = export.Save(nodeToLocalID, export.ID);
            byte[] bytes = Encoding.UTF8.GetBytes(exportText);

            stream.Write(bytes);
        }
        return stream;
    }

    /// <summary>
    /// Saves a scene from <paramref name="tree"/> into the <paramref name="path"/>.
    /// </summary>
    /// <param name="tree">Where the nodes is saved.</param>
    /// <param name="path">Where the scene data is saved.</param>
    public static void SaveScene(Tree tree, string path, SceneSavingFlags flags)
    {
        bool verbose = flags.HasFlag(SceneSavingFlags.Verbose);

        Stopwatch? timer = null;
        if (verbose)
        {
            timer = new();
            timer.Start();
        }

        var nodes = tree.GetAllNodes();

        if (verbose)
        {
            Console.WriteLine("Sorting Nodes");
        }
        nodes.Sort((x, y) => x.GetAncestors().Count - y.GetAncestors().Count);

        if (verbose)
        {
            Console.WriteLine("Converting Nodes to Exports");
        }
        Dictionary<Node, uint> nodeToLocalID = [];
        List<ExportNode> exports = [];
        GetValidNodesForSaving(nodes, exports, nodeToLocalID);

        if (verbose)
        {
            Console.WriteLine("Completing Node References");
        }
        CompleteNodeRefrences(nodeToLocalID, exports);

        using MemoryStream stream = ExportsToStream(exports, nodeToLocalID);

        if (verbose)
        {
            Console.WriteLine("Loading data in stream");
        }
        stream.Position = 0L;
        using FileStream fileStream = new(path, FileMode.Create);
        stream.CopyTo(fileStream);

        if (timer is not null)
        {
            timer.Stop();

            long msTaken = timer.ElapsedMilliseconds;
            Console.WriteLine($"Saving Scene taken {(double)msTaken / 1000} seconds");
        }
    }

    /// <inheritdoc cref="SaveScene(Tree, string, SceneSavingFlags)"/>
    public static void SaveScene(Tree tree, string path)
    {
        SaveScene(tree, path, SceneSavingFlags.None);
    }

    #endregion

    #region Loading Scene

    private struct ImportProp
    {
        public required string Key;
        public required string Value;
        public required Type ValueType;
        public required PropertyFlag Flags;

        public override readonly string ToString()
        {
            return $"Import Property {ValueType} {Key}={Value}";
        }
    }

    private struct ImportNode
    {
        public required string NodeType;
        public required uint ParentID;
        public required uint LocalID;
        public List<ImportProp> Propetries = [];

        public override readonly string ToString()
        {
            return $"Import Node of {NodeType} with {Propetries.Count} propetry(s).";
        }

        public ImportNode() { }
    }

    #region Parsing

    private static ImportNode ParseImportNode(Match nodeMatch)
    {
        var groups = nodeMatch.Groups;

        string nodeType = groups[1].Value,
        local = groups[2].Value,
        parent = groups[3].Value;

        bool canParseLocal = uint.TryParse(local, out var localID),
        canParseParent = uint.TryParse(parent, out var parentID);
        if (!canParseLocal || !canParseParent)
        {
            throw new MalformedSceneException("Couldn't parse either (or both) local or parent!");
        }

        ImportNode importNode = new()
        {
            NodeType = nodeType,
            LocalID = localID,
            ParentID = parentID
        };

        return importNode;
    }

    private static ImportProp ParseImportProp(Match propMatch)
    {
        var groups = propMatch.Groups;

        string typeName = groups[1].Value,
        flags = groups[2].Value,
        name = groups[3].Value,
        value = groups[4].Value;

        Type? type = Type.GetType(typeName);
        ArgumentNullException.ThrowIfNull(type, nameof(typeName));

        PropertyFlag propFlags = ParseFlags(flags);

        ImportProp importProp = new()
        {
            Key = name,
            Value = value,
            ValueType = type,
            Flags = propFlags
        };

        return importProp;
    }

    private static void ParseLine(List<ImportNode> importNodes, string line, uint i, SceneLoadingFlags flags)
    {
        bool verbose = flags.HasFlag(SceneLoadingFlags.Verbose);

        Match nodeMatch = RegexNode().Match(line),
            propMatch = RegexProp().Match(line),
            commentMatch = RegexComment().Match(line);

        if (nodeMatch.Success)
        {
            ImportNode importNode = ParseImportNode(nodeMatch);

            importNodes.Add(importNode);
        }
        else if (propMatch.Success)
        {
            if (importNodes.Count <= 0)
            {
                throw new MalformedSceneException("Property appeared before first node");
            }
            ImportProp importProp = ParseImportProp(propMatch);

            importNodes[^1].Propetries.Add(importProp);
        }
        else if (commentMatch.Success)
        {
            return;
        }
        else
        {
            throw new MalformedSceneException($"Line {i} is not a property or node!");
        }
    }

    private static void ParseSceneFile(List<ImportNode> b, string path, SceneLoadingFlags flags)
    {
        using StreamReader sceneStream = new(path);

        bool verbose = flags.HasFlag(SceneLoadingFlags.Verbose);
        if (verbose)
        {
            Console.WriteLine($"Parsing Scene Stream from {path}");
        }

        uint i = 0;
        while (sceneStream.Peek() >= 0)
        {
            i++;
            string? line = sceneStream.ReadLine();
            if (line is null)
            {
                continue;
            }
            line = line.Trim();

            try
            {
                ParseLine(b, line, i, flags);
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine($"Error thrown on line {i}: {ex}");
                }
                throw;
            }
        }
    }

    #endregion

    #region Propetries

    private static void SetPropertiesOfNode(List<ImportProp> importProps, Type nodeType, Node node)
    {
        foreach (ImportProp prop in importProps)
        {
            PropertyInfo? info = nodeType.GetProperty(prop.Key);
            LoadingNodeException.ThrowIfNull(info, $"Propetry {prop.Key} doesn't exist on {node}.");

            if (!info.CanWrite)
            {
                throw new LoadingNodeException($"Propetry {prop.Key} does exist but can't be set.");
            }

            object? value = null;
            foreach (FlagHandler handler in FlagHandlers)
            {
                if (!prop.Flags.HasFlag(handler.Flag))
                {
                    continue;
                }
                value = handler.HandleValue(prop);
            }

            if (prop.Flags == PropertyFlag.None)
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(prop.Value);
                value = JsonSerializer.Deserialize(jsonBytes, prop.ValueType, JSONOptions);
            }

            info.SetValue(node, value);
        }
    }

    private static void LoadNodesFromImport(Dictionary<uint, Node> idToNode, List<ImportNode> importNodes, SceneLoadingFlags flags)
    {
        bool verbose = flags.HasFlag(SceneLoadingFlags.Verbose);
        if (verbose)
        {
            Console.WriteLine($"Loading {importNodes.Count} node(s)");
        }

        foreach (ImportNode import in importNodes)
        {
            Type nodeType = NodeNameToType[import.NodeType];
            Node? node = (Node?)Activator.CreateInstance(nodeType);
            LoadingNodeException.ThrowIfNull(node, "Node couldn't be constructed (has no parameterless contructor)");

            if (import.ParentID != 0) // Node has a parent
            {
                Node parentNode = idToNode[import.ParentID];
                node.Parent = parentNode;
            }

            SetPropertiesOfNode(import.Propetries, nodeType, node);

            idToNode.Add(import.LocalID, node);
        }
    }

    private static void LoadPropetries(ImportNode import, Node node, Dictionary<uint, Node> idToNode)
    {
        foreach (ImportProp prop in import.Propetries)
        {
            if (!prop.Flags.HasFlag(PropertyFlag.NodeRef))
            {
                continue;
            }

            uint id = uint.Parse(prop.Value);

            if (!idToNode.TryGetValue(id, out Node? refNode))
            {
                continue;
            }

            PropertyInfo? info = node.GetType().GetProperty(prop.Key);
            ArgumentNullException.ThrowIfNull(info, nameof(info));
            info.SetValue(node, refNode);
        }
    }

    private static void LoadPropetriesForAllNodes(Dictionary<uint, Node> idToNode, List<ImportNode> importNodes, bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine("Loading Properties");
        }

        int i = 0;
        try
        {
            while (i < importNodes.Count)
            {
                ImportNode import = importNodes[i];
                Node node = idToNode[import.LocalID];
                LoadPropetries(import, node, idToNode);
                i++;
            }
        }
        catch (Exception)
        {
            if (verbose)
            {
                ImportNode import = importNodes[i];
                Console.WriteLine($"Error when trying to load propetries of type {import.NodeType}");
            }

            throw;
        }
    }

    #endregion

    /// <summary>
    /// Loads a scene from <paramref name="path"/> into the selected <paramref name="tree"/>.
    /// </summary>
    /// <param name="tree">Where the nodes are loaded.</param>
    /// <param name="path">Where the scene data is loaded.</param>
    /// <param name="flags"Scene loading paths</param>
    public static void LoadScene(Tree tree, string path, SceneLoadingFlags flags)
    {
        bool verbose = flags.HasFlag(SceneLoadingFlags.Verbose);

        Stopwatch? timer = null;
        if (verbose)
        {
            timer = new();
            timer.Start();
        }

        try
        {
            List<ImportNode> importNodes = [];
            ParseSceneFile(importNodes, path, flags);

            Dictionary<uint, Node> idToNode = [];
            LoadNodesFromImport(idToNode, importNodes, flags);

            LoadPropetriesForAllNodes(idToNode, importNodes, verbose);

            if (verbose)
            {
                Console.WriteLine("Registering Nodes");
            }

            foreach (Node node in idToNode.Values)
            {
                node.Awake();
                node._ID = tree.RegisterNode(node);
                node.Start();
            }
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.WriteLine($"Exception occured when loading scene {path}: {ex}");
            }
            throw;
        }
        finally
        {
            if (timer is not null)
            {
                timer.Stop();

                long msTaken = timer.ElapsedMilliseconds;
                Console.WriteLine($"Loading Scene taken {(double)msTaken / 1000} seconds");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="path"></param>
    public static void LoadScene(Tree tree, string path)
    {
        LoadScene(tree, path, SceneLoadingFlags.None);
    }

    const string NodeRegex = @"^\[(.+?) local-id='(\d+?)' parent='(\d+?)'\]$";

    [GeneratedRegex(NodeRegex)]
    private static partial Regex RegexNode();

    const string PropRegex = @"^\s*([\w\d.]+?)\s+(?:(r?n?)\s+)?([\w\d]+?)=(.+?)$";

    [GeneratedRegex(PropRegex)]
    private static partial Regex RegexProp();

    const string CommentRegex = @"^\s*#.*$";

    [GeneratedRegex(CommentRegex)]
    private static partial Regex RegexComment();
    #endregion

    private static readonly Dictionary<string, Type> NodeNameToType = [];

    private static readonly JsonSerializerOptions JSONOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    static SceneHandler()
    {
        Assembly assem = typeof(SceneHandler).Assembly;

        Type baseNodeType = typeof(Node);

        foreach (Type type in assem.GetTypes())
        {
            var saveNode = type.GetCustomAttribute<SaveNodeAttribute>();

            if (saveNode is null)
            {
                continue;
            }

            if (!type.IsSubclassOf(baseNodeType) && type != baseNodeType)
            {
                Console.WriteLine($"{type.FullName} doesn't inherit from Node but has the SaveNode Attribute");
                continue;
            }

            NodeNameToType.Add(saveNode.SavedName, type);
        }
    }
}