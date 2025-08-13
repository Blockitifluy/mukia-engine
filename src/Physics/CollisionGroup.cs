namespace MukiaEngine.Physics;

[Serializable]
public class CollisionGroupException : Exception
{
    public CollisionGroupException() { }
    public CollisionGroupException(string message) : base(message) { }
    public CollisionGroupException(string message, Exception inner) : base(message, inner) { }
}

internal struct CollisionGroup(string name = Physics.DefaultCollisionGroup, ulong collidesWith = ulong.MaxValue)
{
    public string Name = name;
    public ulong CollidesWith = collidesWith;
}

public static partial class Physics
{
    /// <summary>
    /// The maxiumum amount of Collision Groups allowed.
    /// </summary>
    public const int MaxGroups = 64;
    /// <summary>
    /// The name of the default collision group.
    /// </summary>
    public const string DefaultCollisionGroup = "Default";

    private static readonly CollisionGroup[] CollisionGroups = new CollisionGroup[MaxGroups];
    private static int CollisionGroupCount = 0;

    private static ref CollisionGroup GetCollisionGroup(string name, out int i)
    {
        for (int j = 0; j < CollisionGroups.Length; j++)
        {
            ref CollisionGroup group = ref CollisionGroups[j];
            if (group.Name == name)
            {
                i = j;
                return ref group;
            }
        }

        throw new CollisionGroupException();
    }

    #region Set Status
    /// <summary>
    /// Set the collision status of two collision groups.
    /// </summary>
    /// <param name="group0">The first collision group</param>
    /// <param name="group1">The second collision group</param>
    /// <param name="canCollide">The new collision status</param>
    public static void SetCollisionStatus(string group0, string group1, bool canCollide)
    {
        ref CollisionGroup left = ref GetCollisionGroup(group0, out int i),
        right = ref GetCollisionGroup(group1, out int j);

        SetCollisionStatus(ref left, ref right, canCollide, i, j);
    }

    private static void SetCollisionStatus(ref CollisionGroup left, ref CollisionGroup right, bool canCollide, int i, int j)
    {

        if (canCollide)
        {
            left.CollidesWith |= 1ul << j;
            right.CollidesWith |= 1ul << i;
        }
        else
        {
            left.CollidesWith &= ~(1ul << j);
            right.CollidesWith &= ~(1ul << i);
        }

    }
    #endregion

    #region Can Collide With
    /// <summary>
    /// Can two collision groups can collide.
    /// </summary>
    /// <param name="group0">The first collision group</param>
    /// <param name="group1">The second collision group</param>
    /// <returns><c>true</c>, if the two collision groups can collide.</returns>
    /// <exception cref="CollisionGroupException"></exception>
    public static bool CanCollideWith(string group0, string group1)
    {
        CollisionGroup? left = GetCollisionGroup(group0, out int i),
        right = GetCollisionGroup(group1, out int j);

        if (!left.HasValue)
        {
            throw new CollisionGroupException($"{nameof(group0)} is not valid");
        }

        if (!right.HasValue)
        {
            throw new CollisionGroupException($"{nameof(group1)} is not valid");
        }

        return CanCollideWith(left.Value, right.Value, i, j);
    }

    private static bool CanCollideWith(CollisionGroup left, CollisionGroup right, int i, int j)
    {
        bool first = GUtility.IsBitSet(left.CollidesWith, j),
        second = GUtility.IsBitSet(right.CollidesWith, i);

        if (first != second)
        {
            throw new CollisionGroupException("First and Second group can collide mismatch");
        }

        return first;
    }
    #endregion

    #region Register Group
    /// <summary>
    /// Registers a collision group.
    /// </summary>
    /// <param name="name">The name of the collision group.</param>
    /// <param name="i">The outputing index</param>
    public static void RegisterCollisionGroup(string name, out int i)
    {
        CollisionGroup group = new(name);
        RegisterCollisionGroup(group, out int j);
        i = j;
    }

    /// <inheritdoc cref="RegisterCollisionGroup(string, out int)"/>
    public static void RegisterCollisionGroup(string name)
    {
        CollisionGroup group = new(name);
        RegisterCollisionGroup(group, out _);
    }

    private static void RegisterCollisionGroup(CollisionGroup group, out int i)
    {
        if (CollisionGroupCount >= MaxGroups)
        {
            throw new CollisionGroupException($"Collision Group is over {MaxGroups}");
        }

        bool isPrexisting = IsGroupRegistered(group.Name, out int _);
        if (isPrexisting)
        {
            throw new CollisionGroupException($"Collision Group {group.Name} already exists");
        }

        CollisionGroups[CollisionGroupCount] = group;
        i = CollisionGroupCount;
        CollisionGroupCount++;
    }
    #endregion

    #region Is Registeting
    /// <summary>
    /// Is this group registered.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="i">The outputing index</param>
    /// <returns><c>true</c>, if the group is registered.</returns>
    public static bool IsGroupRegistered(string name, out int i)
    {
        for (int j = 0; j < CollisionGroups.Length; j++)
        {
            CollisionGroup group = CollisionGroups[j];
            if (group.Name == name)
            {
                i = j;
                return true;
            }
        }

        i = -1;
        return false;
    }

    /// <inheritdoc cref="IsGroupRegistered(string, out int)"/>
    public static bool IsGroupRegistered(string name)
    {
        return IsGroupRegistered(name, out _);
    }
    #endregion

    static Physics()
    {
        try
        {
            RegisterCollisionGroup(DefaultCollisionGroup);
        }
        catch (Exception) { }
    }
}