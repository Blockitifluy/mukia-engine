using CommandLine;

namespace MukiaEngine.NodeSystem;

public class NodeIndex(Node? parent, Node self, IEnumerable<Node> children)
{
	private Node? _PastParent = null;
	private Node? _Parent = parent;

	public Node? Parent
	{
		get => _Parent;
		set
		{
			_PastParent = _Parent;
			_Parent = value;
		}
	}

	public Node? PastParent => _PastParent;

	public Node Self = self;
	public List<Node> Children = [.. children];
}

public class TreeIndexer(Tree tree)
{
	private Tree Tree = tree;

	public Tree GetTree()
	{
		return Tree;
	}

	public NodeIndex AddToIndexer(Node node)
	{
		List<Node> children = GetChildren(node);
		NodeIndex index = new(node.Parent, node, children);

		Indexer.Add(node, index);
		if (node.Parent is not null)
		{
			UpdateToParent(node.NodeIndex);
		}

		return index;
	}

	public static void ThrowIfInvalidParent(Node self, Node? parent)
	{
		if (parent == self)
		{
			throw new TreeException($"Can not parent to self");
		}

		if (parent is not null)
		{
			bool isDescendant = self.IsDescendant(parent);
			if (isDescendant)
			{
				throw new TreeException($"Circular Heiarchry attemped on {self}");
			}
		}
	}

	private void UpdateToParent(NodeIndex index)
	{
		if (index.PastParent is not null)
		{
			NodeIndex indexPast = index.PastParent.NodeIndex;

			indexPast.Children.Remove(indexPast.Self);
		}

		if (index.Parent is not null)
		{
			NodeIndex indexParent = index.Parent.NodeIndex;

			bool contains = indexParent.Children.Contains(index.Self);
			if (!contains)
			{
				indexParent.Children.Add(index.Self);
			}
		}
	}

	public void UpdateIndex(NodeIndex index)
	{
		index.Children = GetChildren(index.Self);
		UpdateToParent(index);
		// TODO - REMOVE FROM CHILDREN WHEN INDEX IS REMOVED

	}

	/// <summary>
	/// Gets all children in the Node.
	/// </summary>
	/// <returns>List of Children.</returns>
	public List<Node> GetChildren(Node node)
	{
		List<Node> nodes = [];
		foreach (Node other in GetTree().GetAllNodes())
		{
			if (other.Parent == node)
			{
				nodes.Add(other);
			}
		}
		return nodes;
	}


	private Dictionary<Node, NodeIndex> Indexer = [];
}