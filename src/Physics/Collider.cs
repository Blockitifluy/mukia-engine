using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using MukiaEngine.NodeSystem;

namespace MukiaEngine.Physics;

/// <summary>
/// How collisions are filtered. Used in:
/// <see cref="Collider.IsColliding(Collider, IntersectionFilter)"/> and <see cref="Collider.GetTouchingColliders(IntersectionFilter)"/>
/// </summary>
public struct IntersectionFilter : ICollisionFilter
{
    public CollisionFilter FilterType { get; set; } = CollisionFilter.Exclude;
    public Node[] FilterList { get; set; } = [];
    /// <inheritdoc cref="Collider.CollisionGroup"/>
    public string CollisionGroup { get; set; } = Physics.DefaultCollisionGroup;

    public IntersectionFilter() { }
}

/// <summary>
/// A collision shape that uses a voxel system to detect collisions.
/// </summary>
public abstract class CollisionShape
{
    // Used because the C# bool takes 4 byte
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.I1)]
    /// <summary>
    /// All of the voxels of the Collision Shape.
    /// </summary>
    public bool[,,] CollisonVoxels = new bool[0, 0, 0];

    /// <summary>
    /// The bounds that the CollisionShape bounds to.
    /// </summary>
    [JsonIgnore]
    public abstract Vector3 Bounds { get; }

    /// <summary>
    /// The global position.
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// The global rotation.
    /// </summary>
    public Vector3 Rotation;
    /// <summary>
    /// The global scale.
    /// </summary>
    public Vector3 Scale = Vector3.One;

    /// <summary>
    /// Calculates all of the voxels in <see cref="CollisionVoxels"/>.
    /// </summary>
    public abstract void CalculateCollisionVoxels();

    public delegate bool ForVoxel(Vector3Int pos, bool voxel);

    /// <summary>
    /// Loops over every voxel, firing <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to be fired every voxel.</param>
    public void ForEachVoxel(ForVoxel func)
    {
        Vector3Int voxelSize = GetVoxelsPerDimension();
        bool stop = false;

        for (int x = 0; x < voxelSize.X; x++)
        {
            for (int y = 0; y < voxelSize.Y; y++)
            {
                for (int z = 0; z < voxelSize.Z; z++)
                {
                    Vector3Int pos = new(x, y, z);

                    stop = func(pos, CollisonVoxels[x, y, z]);
                }

                if (stop)
                {
                    break;
                }
            }

            if (stop)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Gets voxels per dimension of the CollisionShape.
    /// </summary>
    /// <returns>Voxels per dimension</returns>
    public Vector3Int GetVoxelsPerDimension()
    {
        return (Vector3Int)(Bounds * Scale / Physics.CollisionVoxelSize);
    }

    /// <summary>
    /// Checks if two Collision Shapes' bounds are intersecting. 
    /// </summary>
    /// <param name="first">The first collision shape.</param>
    /// <param name="second">The second collision shape.</param>
    /// <returns>True, if two collision shapes are intersecting.</returns>
    public static bool IsCollidingInBounds(CollisionShape first, CollisionShape second)
    {
        for (int cube = 0; cube < 2; cube++)
        {
            CollisionShape shape0 = cube == 0 ? first : second,
            shape1 = cube == 0 ? second : first;

            Vector3 rotDiff = shape0.Rotation - shape1.Rotation;

            Vector3 pos0 = Vector3.RotateEuler(shape0.Position - shape1.Position, rotDiff) + shape1.Position,
            pos1 = shape1.Position,
            bounds0 = (shape0.Scale * shape0.Bounds) + pos0,
            bounds1 = (shape1.Scale * shape1.Bounds) + pos1;

            bool inside = Physics.InPointInside(pos0, pos1, bounds1) || Physics.InPointInside(bounds0, pos1, bounds1);

            if (inside)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if two Collision Shapes are inside or colliding.
    /// </summary>
    /// <remarks>
    /// More presice than <see cref="IsCollidingInBounds"/>.
    /// </remarks>
    /// <param name="first">The first collision shape.</param>
    /// <param name="second">The second collision shape.</param>
    /// <returns>True, if two collision shapes are intersection.</returns>
    public static bool IsColliding(CollisionShape first, CollisionShape second)
    {
        bool inBounds = IsCollidingInBounds(first, second);

        if (!inBounds)
        {
            return false;
        }

        Vector3 rotDiff = second.Rotation - first.Rotation;

        Vector3Int pos = (Vector3Int)((Vector3.RotateEuler(second.Position - first.Position, rotDiff) + first.Position) / Physics.CollisionVoxelSize);

        bool isColliding = false;
        first.ForEachVoxel((pos0, voxel0) =>
        {
            if (!voxel0)
            {
                return false;
            }

            Vector3Int localPos = pos - pos0;

            bool inside = Physics.InPointInside(localPos, second.GetVoxelsPerDimension());
            if (!inside)
            {
                return false;
            }

            bool other = second.CollisonVoxels[localPos.X, localPos.Y, localPos.Z];

            if (other)
            {
                isColliding = true;
                return true;
            }

            return false;
        });

        return isColliding;
    }

    /// <summary>
    /// Checks if a point is intersecting with the collider.
    /// </summary>
    /// <param name="point">A point</param>
    /// <returns>True, if a point is intersecting.</returns>
    public bool IsPointCollidingInBounds(Vector3 point)
    {
        Vector3 bounds = Scale * Bounds,
        pos = point - Position;

        return Physics.InPointInside(pos, bounds);
    }

    /// <summary>
    /// Checks if a point is inside or colliding with Collision Shapes.
    /// </summary>
    /// <param name="point">A point</param>
    /// <returns>True, if a point is inside or colliding.</returns>
    public bool IsPointColliding(Vector3 point)
    {
        if (!IsPointCollidingInBounds(point))
        {
            return false;
        }

        Vector3 posRot = point - Position;

        Vector3Int intStep = (Vector3Int)(posRot * Physics.CollisionVoxelSize);
        bool colliding = CollisonVoxels[intStep.X, intStep.Y, intStep.Z];

        return colliding;
    }
}

/// <summary>
/// Used for detecting collisions. 
/// </summary>
[SaveNode("engine.collider")]
public sealed class Collider : Node3D
{
    public static readonly List<Collider> Colliders = [];

    /// <inheritdoc cref="Engine.Physics.CollisionShape"/>
    [Export]
    public CollisionShape? CollisionShape { get; set; }
    /// <summary>
    /// If <c>true</c> then, change the Collider's position when it's parent changes.
    /// </summary>
    [Export]
    public bool ChangeShapeTransform { get; set; } = true;
    /// <summary>
    /// Set the collider as a trigger. This means that the collider doesn't collide or accept raycasts, only detect hits.
    /// </summary>
    [Export]
    public bool IsTrigger { get; set; } = false;

    private string _CollisionGroup = Physics.DefaultCollisionGroup;

    /// <summary>
    /// The collision group that the Collider is in.
    /// </summary>
    [Export]
    public string CollisionGroup
    {
        get => _CollisionGroup;
        set
        {
            bool isRegistered = Physics.IsGroupRegistered(value);

            if (!isRegistered)
            {
                throw new CollisionGroupException($"Collision Group {value} is not valid");
            }

            _CollisionGroup = value;
        }
    }

    /// <summary>
    /// Invoked when the collider hits another.
    /// </summary>
    public event EventHandler<Collider>? OnCollision;

    /// <summary>
    /// Applies the <paramref name="shape"/> to the Collider with transformations applied and voxels calculated.
    /// </summary>
    /// <param name="shape"><inheritdoc cref="CollisionShape" path="/summary"/></param>
    public void ApplyCollisionShape(CollisionShape shape)
    {
        shape.Rotation = GlobalRotation;
        shape.Scale = GlobalScale;
        shape.Position = GlobalPosition;

        CollisionShape = shape;
        shape.CalculateCollisionVoxels();
    }

    protected override void UpdateTransformations()
    {
        base.UpdateTransformations();

        if (ChangeShapeTransform && CollisionShape is not null)
        {
            CollisionShape.Rotation = GlobalRotation;
            CollisionShape.Scale = GlobalScale;
            CollisionShape.Position = GlobalPosition;
        }
    }

    protected override void OnNonPositionUpdate()
    {
        base.OnNonPositionUpdate();

        CollisionShape?.CalculateCollisionVoxels();
    }

    /// <summary>
    /// Checks if the collider isn't null and has a shape.
    /// </summary>
    /// <param name="collider">The collider</param>
    /// <param name="shape">The outputing shape.</param>
    /// <returns><c>true</c>, if the collider is valid.</returns>
    public static bool IsColliderValid([NotNullWhen(true)] Collider? collider, [NotNullWhen(true)] out CollisionShape? shape)
    {
        if (collider is null)
        {
            shape = null;
            return false;
        }

        if (collider.CollisionShape is null || !collider.Enabled)
        {
            shape = null;
            return false;
        }

        shape = collider.CollisionShape;
        return true;
    }

    public override void Awake()
    {
        base.Awake();

        CollisionShape?.CalculateCollisionVoxels();
        Colliders.Add(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Colliders.Remove(this);

    }

    /// <inheritdoc cref="IsColliding(Collider, IntersectionFilter)"/>
    public bool IsColliding(Collider other)
    {
        return IsColliding(other, DefaultIntersection);
    }

    /// <summary>
    /// Checks if a collider is colliding with <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other collider</param>
    /// <param name="filter">How the collision is filtered.</param>
    /// <returns></returns>
    public bool IsColliding(Collider other, IntersectionFilter filter)
    {
        CollisionShape? shape = CollisionShape;
        if (shape is null)
        {
            return false;
        }

        if (other == this)
        {
            return false;
        }

        if (Physics.IncludedInFilter(other, filter))
        {
            return false;
        }

        if (!IsColliderValid(other, out var otherShape))
        {
            return false;
        }

        return CollisionShape.IsColliding(shape, otherShape);
    }

    /// <inheritdoc cref="GetTouchingColliders(IntersectionFilter)"/>
    public Collider[] GetTouchingColliders()
    {
        return GetTouchingColliders(DefaultIntersection);
    }

    /// <summary>
    /// Returns colliders currently collider with this collider.
    /// </summary>
    /// <param name="filter">How the collision is filtered.</param>
    /// <returns>An array of colliders</returns>
    public Collider[] GetTouchingColliders(IntersectionFilter filter)
    {
        List<Collider> colliders = [];

        if (!IsColliderValid(this, out var _))
        {
            return [];
        }

        foreach (Collider other in Colliders)
        {
            bool isCollision = IsColliding(other, filter);

            if (isCollision)
            {
                colliders.Add(other);
            }
        }

        return [.. colliders];
    }

    private IntersectionFilter DefaultIntersection = new()
    {
        FilterList = [],
        FilterType = CollisionFilter.Exclude
    };

    public override void UpdateFixed()
    {
        base.UpdateFixed();

        var touching = GetTouchingColliders();
        foreach (var collider in touching)
        {
            OnCollision?.Invoke(this, collider);
        }
    }
}