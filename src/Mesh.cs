using OpenTK.Graphics.OpenGL4;
using MukiaEngine.NodeSystem;

namespace MukiaEngine;

/// <summary>
/// A 3D structual model.
/// </summary>
public sealed class Mesh : Resource, ICloneable
{
    /// <summary>
    /// Used in <see cref="GetMeshPrimative"/>.
    /// </summary>
    public enum MeshPrimitive
    {
        Triangle,
        Quad,
        Cube
    }

    [Export]
    public required Vector3[] Vertices { get; set; }
    [Export]
    public required int[] Indices { get; set; }
    [Export]
    public required Vector2[] UVs { get; set; }
    [Export]
    public required PrimitiveType PrimitiveType { get; set; }

    private static readonly Mesh TriangleMesh = new()
    {
        Vertices = [
            Vector3.Zero,
            Vector3.Right,
            Vector3.Up
        ],
        Indices = [0, 1, 2],
        UVs = [
            Vector2.Zero,
            Vector2.Right,
            Vector2.Up
        ],
        PrimitiveType = PrimitiveType.Triangles
    };

    private static readonly Mesh QuadMesh = new()
    {
        Vertices = [
            new(1, 1),
            Vector3.Right,
            Vector3.Zero,
            Vector3.Up
        ],
        Indices = [0, 1, 3, 1, 2, 3],
        UVs = [
            Vector2.One,
            Vector2.Right,
            Vector2.Zero,
            Vector2.Up
        ],
        PrimitiveType = PrimitiveType.Triangles
    };

    private static readonly Mesh CubeMesh = new()
    {
        Vertices = [
            new(-1, -1, 1), //0
            new(1, -1, 1), //1
            new(-1, 1, 1), //2
            new(1, 1, 1), //3
            new(-1, -1, -1), //4
            new(1, -1, -1), //5
            new(-1, 1, -1), //6
            new(1, 1, -1) //7
        ],
        Indices = [
            2, 6, 7,
            2, 3, 7,

            0, 4, 5,
            0, 1, 5,

            0, 2, 6,
            0, 4, 6,

            1, 3, 7,
            1, 5, 7,

            0, 2, 3,
            0, 1, 3,

            4, 6, 7,
            4, 5, 7
        ],
        PrimitiveType = PrimitiveType.Triangles,
        UVs = [
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1),

            new(1, 0),
            new(0, 0),
            new(1, 1),
            new(0, 1)
        ]
    };

    /// <summary>
    /// Gets a mesh primative from <see cref="MeshPrimitive"/>.
    /// </summary>
    /// <param name="primative">The type of mesh.</param>
    /// <returns>A mesh</returns>
    /// <exception cref="NotImplementedException">The wanted mesh was implemented.</exception>
    public static Mesh GetMeshPrimitive(MeshPrimitive primative)
    {
        return primative switch
        {
            MeshPrimitive.Triangle => (Mesh)TriangleMesh.Clone(),
            MeshPrimitive.Quad => (Mesh)QuadMesh.Clone(),
            MeshPrimitive.Cube => (Mesh)CubeMesh.Clone(),
            _ => throw new NotImplementedException($"Mesh Primative {primative} is not implemented"),
        };
    }

    public object Clone()
    {
        Mesh mesh = new()
        {
            Vertices = (Vector3[])Vertices.Clone(),
            Indices = (int[])Indices.Clone(),
            UVs = (Vector2[])UVs.Clone(),
            PrimitiveType = PrimitiveType
        };

        return mesh;
    }

    /// <summary>
    /// Allows the mesh to be rendered.
    /// Contains it's verts (indexes: 0, 1, 2) and UVs (indexes: 3, 4).
    /// </summary>
    /// <returns>The mesh as a feed</returns>
    public float[] IntoFeed()
    {
        return IntoFeed(Vector3.Zero, Vector3.One);
    }

    /// <inheritdoc cref="IntoFeed()"/>
    /// <param name="offset">The offset of mesh.</param>
    /// <param name="scale">The scale of the mesh.</param>
    public float[] IntoFeed(Vector3 offset, Vector3 scale)
    {
        int feedLength = Vertices.Length * 5;
        float[] feed = new float[feedLength];
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vector3 vert = Vertices[i];
            feed[i * 5] = offset.X + vert.X * scale.X;
            feed[i * 5 + 1] = offset.Y + vert.Y * scale.Y;
            feed[i * 5 + 2] = offset.Z + vert.Z * scale.Z;

            Vector2 uv = i < UVs.Length ? UVs[i] : Vector2.Zero;
            feed[i * 5 + 3] = uv.X;
            feed[i * 5 + 4] = uv.Y;
        }
        return feed;
    }

    public Mesh() { }
}