using MukiaEngine.NodeSystem;

namespace MukiaEngine.Physics;

/// <summary>
/// The result of a raycast being fired.
/// </summary>
public struct RaycastResult
{
    /// <summary>
    /// The collider hit.
    /// </summary>
    public required Collider Target;
    /// <summary>
    /// On where the ray hit on the collider.
    /// </summary>
    public required Vector3 Hit;

    public override readonly string ToString()
    {
        return $"Raycast Result {Target} {Hit}";
    }
}

/// <summary>
/// Contains information about a ray.
/// </summary>
/// <param name="origin"><inheritdoc cref="Origin" path="/summary"/></param>
/// <param name="dir"><inheritdoc cref="Direction" path="/summary"/></param>
public struct Ray(Vector3 origin, Vector3 dir) : ICollisionFilter
{
    /// <summary>
    /// The starting point of the ray.
    /// </summary>
    public Vector3 Origin = origin;
    /// <summary name="dir">
    /// The direction of the ray (Magnitude means range of ray).
    /// </summary>
    public Vector3 Direction = dir;

    public CollisionFilter FilterType { get; set; } = CollisionFilter.Exclude;
    public Node[] FilterList { get; set; } = [];
    public string CollisionGroup { get; set; } = Physics.DefaultCollisionGroup;

    public override readonly string ToString()
    {
        return $"Ray origin {Origin} direction {Direction}";
    }
}

public static partial class Physics
{
    /// <summary>
    /// How big are voxels for collidision.
    /// </summary>
    public const float CollisionVoxelSize = 0.2f;

    private static bool IsIn180Sight(Vector3 origin, Vector3 direction, Collider collider)
    {
        Vector3[] corners = GetCornersOfCollider(collider);

        foreach (Vector3 corn in corners)
        {
            Vector3 cornDir = (corn - origin).Unit;

            float dot = Vector3.Dot(direction.Unit, cornDir),
            angle = float.Acos(dot);

            if (angle <= float.Pi) // angle <= 180 degress
            {
                return true;
            }
        }

        return false;
    }

    private static List<RaycastResult> RaycastListUnsorted(Ray ray)
    {
        List<RaycastResult> hitTarget = [];

        if (ray.Direction == Vector3.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ray), "Raycast in zero direction");
        }

        for (int i = 0; i < Collider.Colliders.Count; i++)
        {
            Collider collider = Collider.Colliders[i];

            bool inFilter = IncludedInFilter(collider, ray),
            isIncluded = collider.Enabled || !collider.IsTrigger;
            if (!inFilter || !isIncluded)
            {
                continue;
            }

            bool inSight = IsIn180Sight(ray.Origin, ray.Direction, collider);
            if (!inSight)
            {
                continue;
            }

            CollisionShape? shape = collider.CollisionShape;
            if (shape is null)
            {
                continue;
            }

            Vector3 globalStep = Vector3.Zero;
            while (globalStep.Magnitude <= ray.Direction.Magnitude)
            {
                Vector3 point = globalStep + ray.Origin;

                bool isColliding = shape.IsPointColliding(point);
                if (isColliding)
                {
                    RaycastResult result = new()
                    {
                        Hit = point,
                        Target = collider
                    };

                    hitTarget.Add(result);
                    break;
                }

                globalStep += ray.Direction.Unit * CollisionVoxelSize;
            }
        }

        return hitTarget;
    }

    /// <summary>
    /// Gets all objects that have collided with a ray.
    /// </summary>
    /// <param name="ray">The ray object</param>
    /// <returns>The array of objects hit by the ray (from closest to farthest).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Direction is zero</exception>
    public static RaycastResult[] RaycastList(Ray ray)
    {
        var casts = RaycastListUnsorted(ray);

        casts.Sort((x, y) => (int)(y.Hit * 1000 - x.Hit * 1000).Magnitude);

        return [.. casts];
    }

    /// <summary>
    /// Gets the first object that has collided with a ray.
    /// </summary>
    /// <param name="ray">The ray object</param>
    /// <exception cref="ArgumentOutOfRangeException">Direction is zero</exception>
    /// <returns>The first object hit</returns>
    public static RaycastResult? Raycast(Ray ray)
    {
        var casts = RaycastListUnsorted(ray);

        float currentDist = float.PositiveInfinity;
        RaycastResult? current = null;

        foreach (RaycastResult raycast in casts)
        {
            float dist = (ray.Origin - raycast.Hit).Magnitude;
            if (currentDist > dist)
            {
                current = raycast;
            }
        }

        return current;
    }

    /// <inheritdoc cref="RaycastList(Ray)"/>
    /// <param name="origin"><inheritdoc cref="Ray.Origin" path="/summary"/></param>
    /// <param name="direction"><inheritdoc cref="Ray.Direction" path="/summary"/></param>
    /// <param name="filterList"><inheritdoc cref="Ray.FilterList" path="/summary"/></param>
    /// <param name="filterType"><inheritdoc cref="Ray.FilterType" path="/summary"/></param>
    public static RaycastResult[] RaycastList(Vector3 origin, Vector3 direction, IEnumerable<Node> filterList, CollisionFilter filterType = CollisionFilter.Exclude)
    {
        Ray ray = new(origin, direction)
        {
            FilterType = filterType,
            FilterList = [.. filterList]
        };

        return RaycastList(ray);
    }

    /// <inheritdoc cref="Raycast(Ray)"/>
    /// <param name="origin"><inheritdoc cref="Ray.Origin" path="/summary"/></param>
    /// <param name="direction"><inheritdoc cref="Ray.Direction" path="/summary"/></param>
    /// <param name="filterList"><inheritdoc cref="Ray.FilterList" path="/summary"/></param>
    /// <param name="filterType"><inheritdoc cref="Ray.FilterType" path="/summary"/></param>
    public static RaycastResult? Raycast(Vector3 origin, Vector3 direction, IEnumerable<Node> filterList, CollisionFilter filterType = CollisionFilter.Exclude)
    {
        Ray ray = new(origin, direction)
        {
            FilterType = filterType,
            FilterList = [.. filterList]
        };

        return Raycast(ray);
    }
}