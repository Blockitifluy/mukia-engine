using MukiaEngine.NodeSystem;

namespace MukiaEngine;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class ExportAttribute : Attribute
{
    public ExportAttribute() { }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class SaveNodeAttribute(string savedName) : Attribute
{
    public readonly string SavedName = savedName;
}

[SaveNode("engine.node")]
public class Node
{
    private Node? _Parent;
    /// <summary>
    /// The hierarchal parent of the Node.
    /// </summary>
    /// <remarks>
    /// Cannot parent itself to: it's self, a descendant or ancestor.
    /// </remarks>
    public Node? Parent
    {
        get => _Parent;
        set
        {
            if (value == this)
            {
                throw new TreeException($"Can not parent to self");
            }

            if (value is not null)
            {
                bool isDescendant = IsDescendant(value),
                isAncestor = IsAncestor(value);
                if (isDescendant || isAncestor)
                {
                    throw new TreeException($"Circular Heiarchry attemped on {this}");
                }
            }

            OnParent(value);
            _Parent = value;
        }
    }
    /// <summary>
    /// The un-unique identifier of the Node. 
    /// </summary>
    [Export]
    public string Name { get; set; } = "";

    /// <summary>
    /// Will this Node be saved, when using <see cref="SceneHandler.SaveScene(Tree, string)"/>?
    /// </summary>
    public bool Archivable { get; set; } = true;
    /// <summary>
    /// Is the node enable
    /// </summary>
    /// <remarks>
    /// If <c>false</c>, then the node will not recieve updates.
    /// </remark>
    [Export]
    public bool Enabled { get; set; } = true;

    internal Guid _ID;
    /// <summary>
    /// The unique identifier of the node.
    /// </summary>
    public Guid ID => _ID;

    /// <summary>
    /// Gets the main tree.
    /// </summary>
    /// <returns>The main tree</returns>
    public static Tree GetTree()
    {
        return Tree.GetCurrentTree();
    }

    public override string ToString()
    {
        return $"{GetType().Name} {Name}";
    }

    /// <summary>
    /// Destorys the node and it's descendants by unregistering them.
    /// </summary>
    public void Destroy()
    {
        Tree tree = GetTree();

        var desendents = GetDescendant();
        foreach (Node node in desendents)
        {
            node.Parent = null;
            tree.UnregisterNode(node);
        }
        OnDestroy();
        Parent = null;
        tree.UnregisterNode(this);
    }

    #region Tags
    internal List<uint> _Tags = [];

    /// <inheritdoc cref="TagSystem.HasTag(Node, string)"/>
    public bool HasTag(string tag)
    {
        return TagSystem.HasTag(this, tag);
    }

    /// <inheritdoc cref="TagSystem.AddTag(Node, string)"/>
    public void AddTag(string tag)
    {
        TagSystem.AddTag(this, tag);
    }

    /// <inheritdoc cref="TagSystem.RemoveTag(Node, string)"/>
    public void RemoveTag(string tag)
    {
        TagSystem.RemoveTag(this, tag);
    }

    /// <inheritdoc cref="TagSystem.GetNameTags(Node)"/>
    public string[] GetTags()
    {
        return TagSystem.GetNameTags(this);
    }
    #endregion

    #region Hierarchary
    /// <summary>
    /// Finds the first child that has the matching <paramref name="name"/> and is an approprate type.
    /// </summary>
    /// <typeparam name="TNode">The type of node being queried.</typeparam>
    /// <param name="name">The name of the Node to be matched.</param>
    /// <returns>The node with the matching <paramref name="name"/> and type.</returns>
    public TNode? FindFirstChild<TNode>(string name) where TNode : Node
    {
        foreach (Node node in GetChildren())
        {
            bool nameMatch = node.Name == name;
            if (nameMatch && node is TNode tNode)
            {
                return tNode;
            }
        }
        return null;
    }

    /// <inheritdoc cref="FindFirstChild{TNode}(string)"/>
    /// <param name="type"><inheritdoc cref="FindFirstChild{TNode}(string)"/></param>
    public Node? FindFirstChild(string name, Type type)
    {
        foreach (Node node in GetChildren())
        {
            bool nameMatch = node.Name == name;
            if (nameMatch && node.GetType().IsAssignableTo(type))
            {
                return node;
            }
        }
        return null;
    }

    /// <inheritdoc cref="FindFirstChild{TNode}(string)" path="/param[@name='name']"/>
    /// <summary>
    /// Finds the first child that has the matching <paramref name="name"/>.
    /// </summary>
    /// <returns>The node with the matching <paramref name="name"/>.</returns>
    public Node? FindFirstChild(string name)
    {
        return FindFirstChild<Node>(name);
    }

    /// <summary>
    /// Finds the first child that matchs the wanted type.
    /// </summary>
    /// <typeparam name="TNode">The node type being queried for.</typeparam>
    /// <returns>The node of the wanted type</returns>
    public TNode? FindFirstChildOfType<TNode>() where TNode : Node
    {
        foreach (Node node in GetChildren())
        {
            if (node is TNode tNode)
            {
                return tNode;
            }
        }
        return null;
    }

    /// <inheritdoc cref="FindFirstChildOfType{TNode}()"/>
    /// <param name="type">The node type being queried for.</param>
    public Node? FindFirstChildOfType(Type type)
    {
        foreach (Node node in GetChildren())
        {
            if (node.GetType().IsAssignableTo(type))
            {
                return node;
            }
        }
        return null;
    }

    /// <summary>
    /// Searchs through the node's self and it's ancestors to see, if it can be archived.
    /// </summary>
    /// <returns><c>true</c>, if the node can be archived.</returns>
    public bool CanBeArchived()
    {
        if (!Archivable)
        {
            return false;
        }

        foreach (Node node in GetAncestors())
        {
            if (!node.Archivable)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets all children in the Node.
    /// </summary>
    /// <returns>List of Children.</returns>
    public List<Node> GetChildren()
    {
        List<Node> nodes = [];
        foreach (Node node in GetTree().GetAllNodes())
        {
            if (node.Parent == this)
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }

    /// <summary>
    /// Checks if the node descendes (hierarchally) from <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Is other an ancestor of this.</param>
    /// <returns><c>true</c>, if this is the ancestor of <paramref name="other"/>.</returns>
    public bool IsDescendant(Node other)
    {
        Node? current = Parent;
        while (current is not null)
        {
            if (other == current)
            {
                return true;
            }
            current = current.Parent;
        }
        return false;
    }

    /// <summary>
    /// Checks if the node is an ancestor (hierarchally) to <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Is other a desendant of this.</param>
    /// <returns><c>true</c>, if this is the ancestor of <paramref name="other"/>.</returns>
    public bool IsAncestor(Node other)
    {
        return IsDescendant(other);
    }

    /// <summary>
    /// Gets a node based on it's unique ID.
    /// </summary>
    /// <typeparam name="TNode">The wanted node type.</typeparam>
    /// <param name="id">The unique ID.</param>
    /// <returns>A node with the ID same as <paramref name="id"/>.</returns>
    public static TNode? GetNodeByID<TNode>(Guid id) where TNode : Node
    {
        foreach (Node node in GetTree().GetAllNodes())
        {
            if (node is TNode tNode && node.ID == id)
            {
                return tNode;
            }
        }
        return null;
    }

    /// <inheritdoc cref="GetNodeByID{TNode}(Guid)"/>
    /// <param name="type">The wanted node type.</param>
    public static Node? GetNodeByID(Guid id, Type type)
    {
        foreach (Node node in GetTree().GetAllNodes())
        {
            if (node.GetType().IsAssignableTo(type) && node.ID == id)
            {
                return node;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all descendants.
    /// </summary>
    /// <returns>A list of nodes.</returns>
    public List<Node> GetDescendant()
    {
        List<Node> nodes = [];
        foreach (Node node in GetTree().GetAllNodes())
        {
            bool isDesendent = node.IsDescendant(this);
            if (isDesendent)
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }

    /// <summary>
    /// Gets all ancestors.
    /// </summary>
    /// <returns>A list of nodes.</returns>
    public List<Node> GetAncestors()
    {
        List<Node> nodes = [];
        Node current = this;
        while (current.Parent is not null)
        {
            Node parent = current.Parent;
            nodes.Add(parent);
            current = parent;
        }
        return nodes;
    }
    #endregion

    #region Custom Methods
    /// <summary>
    /// Runs every frame.
    /// </summary>
    /// <param name="delta">The time between the last frame and the second to last frame.</param>
    public virtual void Update(double delta) { }

    /// <summary>
    /// Runs at a fixed rate.
    /// </summary>
    /// <remarks>
    /// To get the time passed between calls use <seealso cref="Tree.FixedUpdateTime"/>.
    /// </remarks>
    public virtual void UpdateFixed() { }

    /// <summary>
    /// Runs before the node is registered.
    /// </summary>
    public virtual void Awake() { }

    /// <summary>
    /// Run after the node is registered.
    /// </summary>
    public virtual void Start() { }

    /// <summary>
    /// Called before the node is destroyed.
    /// </summary>
    protected virtual void OnDestroy() { }

    /// <summary>
    /// Called before the node is reparented.
    /// </summary>
    /// <param name="futureParent">The node's future parent.</param>
    protected virtual void OnParent(Node? futureParent) { }
    #endregion

    #region Node Creation

    /// <summary>
    /// Creates a new node that is:
    /// <list type="bullet">
    /// <item>Has not has it's <see cref="Node.Awake"/> or <see cref="Node.Start"/> fired</item>
    /// <item>And not registered.</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TNode">The node type to be created.</typeparam>
    /// <param name="parent">The parent of the new node.</param>
    /// <param name="name">The name of the node.</param>
    /// <returns>The disabled node.</returns>
    public static TNode NewDisabled<TNode>(Node? parent = null, string? name = null) where TNode : Node, new()
    {
        return new()
        {
            Parent = parent,
            Name = name ?? typeof(TNode).Name
        };
    }

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <returns>The created node.</returns>
    /// <inheritdoc cref="NewDisabled{TNode}(Node?, string?)"/>
    public static TNode New<TNode>(Node? parent = null, string? name = null) where TNode : Node, new()
    {
        TNode node = NewDisabled<TNode>(parent, name);

        node.Awake();
        node._ID = GetTree().RegisterNode(node);
        node.Start();

        return node;
    }
    #endregion
}