using MukiaEngine.NodeSystem;

namespace MukiaEngine;

/// <summary>
/// Uses a mesh and textures to be rendered.
/// </summary>
[SaveNode("engine.mesh-container")]
public sealed class MeshContainer : Node3D
{
    public static readonly List<MeshContainer> MeshContainers = [];

    public Color Color = new(1, 1, 1);
    public float TextureMix = 0.2f;

    [Export]
    public Mesh? Mesh { get; set; }

    private string _Texture0 = "";
    [Export]
    public string Texture0
    {
        get => _Texture0;
        set
        {
            _Texture0 = value;
            _Textures[0] = value;
        }
    }

    private string _Texture1 = "";
    [Export]
    public string Texture1
    {
        get => _Texture1;
        set
        {
            _Texture1 = value;
            _Textures[1] = value;
        }
    }

    private readonly string[] _Textures = ["", ""];
    public string[] Textures => _Textures;

    public override void Awake()
    {
        base.Awake();

        MeshContainers.Add(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        MeshContainers.Remove(this);
    }
}

