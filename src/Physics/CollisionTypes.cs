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
        Vector3Int voxelsSize = GetVoxelsPerDimension();

        bool[,,] voxels = new bool[voxelsSize.X, voxelsSize.Y, voxelsSize.Z];

        for (int x = 0; x < voxelsSize.X; x++)
        {
            for (int y = 0; y < voxelsSize.Y; y++)
            {
                for (int z = 0; z < voxelsSize.Z; z++)
                {
                    voxels[x, y, z] = true;
                }
            }
        }

        CollisonVoxels = voxels;
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
        Vector3Int voxelSize = GetVoxelsPerDimension(),
        center = voxelSize / 2;

        bool[,,] voxels = new bool[voxelSize.X, voxelSize.Y, voxelSize.Z];

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

                    voxels[x, y, z] = inX && inY && inZ;
                }
            }
        }

        CollisonVoxels = voxels;
    }
}