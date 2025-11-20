using System.Text.Json.Serialization;

namespace MukiaEngine.Physics;

public sealed class CubeCollision : CollisionShape
{
    private Vector3 _Dimensions = Vector3.One;
    public Vector3 Dimensions
    {
        get => _Dimensions;
        set
        {
            _Dimensions = value;
            CalculateCollisionVoxels();
        }
    }

    [JsonIgnore]
    public override Vector3 Bounds => Dimensions;

    public override void CalculateCollisionVoxels()
    {
        base.CalculateCollisionVoxels();

        Vector3Int voxelsSize = VoxelsPerDimension;
        int volume = voxelsSize.X * voxelsSize.Y * voxelsSize.Z;

        PrepareCollisionVoxels(volume);
        for (int i = 0; i < volume; i++)
        {
            SetVoxel(i);
        }
    }
}

public sealed class SphereCollision : CollisionShape
{
    private Vector3 _Dimensions = Vector3.One;
    public Vector3 Dimensions
    {
        get => _Dimensions;
        set
        {
            _Dimensions = value;
            CalculateCollisionVoxels();
        }
    }

    [JsonIgnore]
    public override EVector3 Bounds => Dimensions;

    public override void CalculateCollisionVoxels()
    {
        base.CalculateCollisionVoxels();

        Vector3Int voxelSize = VoxelsPerDimension,
        center = voxelSize / 2;

        int i = 0;
        for (int x = 0; x < voxelSize.X; x++)
        {
            for (int y = 0; y < voxelSize.Y; y++)
            {
                for (int z = 0; z < voxelSize.Z; z++)
                {
                    Vector3Int cell = new(x, y, z);

                    float dist = (cell - center).Magnitude;

                    bool inX = dist <= center.X,
                    inY = dist <= center.Y,
                    inZ = dist <= center.Z;

                    if (inX && inY && inZ)
                    {
                        SetVoxel(i);
                    }

                    i++;
                }
            }
        }
    }
}