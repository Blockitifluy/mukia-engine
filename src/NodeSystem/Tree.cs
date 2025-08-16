namespace MukiaEngine.NodeSystem;

[Serializable]
public class TreeException : Exception
{
    public TreeException() { }
    public TreeException(string message) : base(message) { }
    public TreeException(string message, Exception inner) : base(message, inner) { }
}

public sealed class Tree : IDisposable
{
    private static Tree? CurrentTree;

    /// <summary>
    /// Gets the current tree.
    /// </summary>
    /// <returns>The current tree</returns>
    /// <exception cref="TreeException">Fired when the Tree has not been initised.</exception>
    public static Tree GetCurrentTree()
    {
        if (CurrentTree is not null)
        {
            return CurrentTree;
        }

        throw new TreeException("Current tree doesn't exist");
    }

    /// <summary>
    /// Switches the CurrentTree.
    /// </summary>
    /// <param name="tree">The new tree</param>
    public static void SwitchTree(Tree tree)
    {
        CurrentTree = tree;
    }

    /// <summary>
    /// Creates a new Tree.
    /// </summary>
    /// <returns>The tree just created.</returns>
    public static Tree InitaliseTree(bool isDefault = false)
    {
        Tree tree = new();

        if (isDefault)
        {
            CurrentTree = tree;
        }

        return tree;
    }

    private readonly List<Node> Nodes = [];

    /// <summary>
    /// Gets all node registered in the Tree.
    /// </summary>
    /// <returns></returns>
    public List<Node> GetAllNodes()
    {
        return [.. Nodes];
    }

    #region Register
    /// <summary>
    /// Is this node registered.
    /// </summary>
    /// <param name="node">The node being checked.</param>
    /// <returns>True, if registered.</returns>
    public bool IsNodeRegistered(Node node)
    {
        return Nodes.Contains(node);
    }

    /// <summary>
    /// Registers a node.
    /// </summary>
    /// <remarks>
    /// Registering means that a node can be updated.
    /// </remarks>
    /// <param name="node">The node being registed.</param>
    /// <returns>The ID to be assigned to the node. Not set by function.</returns>
    /// <exception cref="TreeException">This node is already registered.</exception>
    public Guid RegisterNode(Node node)
    {
        if (IsNodeRegistered(node))
        {
            throw new TreeException("This node is already registered");
        }
        Nodes.Add(node);

        Guid id = Guid.NewGuid();
        return id;
    }

    /// <summary>
    /// Unregisters a node.
    /// </summary>
    /// <param name="node">The node being unregisted.</param>
    /// <exception cref="TreeException">This node is already unregistered.</exception>
    public void UnregisterNode(Node node)
    {
        if (!IsNodeRegistered(node))
        {
            throw new TreeException("This node is not registered");
        }
        Nodes.Remove(node);
    }
    #endregion

    #region Update
    /// <summary>
    /// Updates all nodes' <see cref="Node.Update(double)"/>.
    /// </summary>
    /// <param name="delta"><inheritdoc cref="Node.Update(double)" path="/param[@name='delta']"/></param>
    public void UpdateAllNodes(double delta)
    {
        var nodes = GetAllNodes();

        foreach (Node node in nodes)
        {
            if (!node.Enabled)
            {
                continue;
            }

            try
            {
                node.Update(delta);
            }
            catch (Exception err)
            {
                Console.WriteLine($"Uncaught error in {node}\n{err}");
            }
        }
    }

    /// <summary>
    /// The interval that fixed update is fired in milliseconds.
    /// </summary>
    public const int FixedUpdateTime = 10;

    /// <summary>
    /// The interval that fixed update is fired in seconds.
    /// </summary>
    public const double FixedUpdateSeconds = (double)FixedUpdateTime / 1000;

    /// <summary>
    /// Updates all nodes' <see cref="Node.UpdateFixed"/>.
    /// </summary>
    public void UpdateAllNodesFixed(object? state)
    {
        var nodes = GetAllNodes();

        foreach (Node node in nodes)
        {
            if (!node.Enabled)
            {
                continue;
            }

            try
            {
                node.UpdateFixed();
            }
            catch (Exception err)
            {
                Console.WriteLine($"Uncaught error in {node}\n{err}");
            }
        }
    }

    private readonly Timer FixedUpdateTimer;
    #endregion

    void IDisposable.Dispose()
    {
        FixedUpdateTimer.Dispose();
    }

    internal Tree()
    {
        FixedUpdateTimer = new(UpdateAllNodesFixed, null, 0, FixedUpdateTime);
    }
}