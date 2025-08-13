using OpenTK.Mathematics;

namespace MukiaEngine;

[SaveNode("engine.node3d")]
public class Node3D : Node
{
    private EVector3 _GlobalPosition;
    private EVector3 _GlobalRotation;
    private EVector3 _GlobalScale;
    private Quaternion _GlobalQuaternion;

    private Quaternion _Quaternion;
    private EVector3 _Position = EVector3.Zero;
    private EVector3 _Rotation = EVector3.Zero;
    private EVector3 _Scale = EVector3.One;

    /// <summary>
    /// The node's local 3D position.
    /// </summary>
    [Export]
    public EVector3 Position
    {
        get => _Position;
        set
        {
            _Position = value;
            UpdateTransformations();
        }
    }
    /// <summary>
    /// The node's local 3D rotation.
    /// </summary>
    /// <remarks>
    /// Uses euler angles.
    /// </remarks>
    [Export]
    public EVector3 Rotation
    {
        get => _Rotation;
        set
        {
            _Rotation = value;
            UpdateVectors();
            UpdateTransformations();
            OnNonPositionUpdate();
        }
    }
    /// <summary>
    /// The node's local 3D scale.
    /// </summary>
    [Export]
    public EVector3 Scale
    {
        get => _Scale;
        set
        {
            _Scale = value;
            UpdateTransformations();
            OnNonPositionUpdate();
        }
    }
    public Quaternion Quaternion => _Quaternion;

    /// <summary>
    /// The node's global 3D position.
    /// </summary>
    public EVector3 GlobalPosition
    {
        get => _GlobalPosition;
        set
        {
            Vector3 global = _GlobalPosition - Position;

            Position = value - global;
        }
    }
    /// <summary>
    /// The node's global 3D rotation
    /// </summary>
    /// <remarks>
    /// Uses euler angles.
    /// </remarks>
    public EVector3 GlobalRotation
    {
        get => _GlobalRotation;
        set
        {
            Vector3 global = _GlobalRotation - Rotation;

            Rotation = value - global;
        }
    }
    /// <summary>
    /// The node's global 3D scale.
    /// </summary>
    public EVector3 GlobalScale
    {
        get => _GlobalScale;
        set
        {
            Vector3 global = _GlobalScale / Scale;

            Scale = global / value;
        }
    }
    public Quaternion GlobalQuaternion => _GlobalQuaternion;

    private EVector3 _Front = EVector3.Forward;
    private EVector3 _Up = EVector3.Up;
    private EVector3 _Right = EVector3.Right;

    /// <summary>
    /// The direction of which the front face is facing.
    /// </summary>
    public EVector3 Front => _Front;
    /// <summary>
    /// The direction of which the up face is facing.
    /// </summary>
    public EVector3 Up => _Up;
    /// <summary>
    /// The direction of which the right face is facing.
    /// </summary>
    public EVector3 Right => _Right;

    protected override void OnParent(Node? futureParent)
    {
        base.OnParent(futureParent);

        UpdateTransformations();
    }

    public override void Start()
    {
        base.Start();

        UpdateTransformations();
        UpdateVectors();
    }

    private void UpdateTransformationsToChildren()
    {
        foreach (Node node in GetChildren())
        {
            if (node is not Node3D node3D)
            {
                continue;
            }
            node3D.UpdateTransformations();
        }
    }

    protected virtual void OnNonPositionUpdate() { }

    protected virtual void UpdateTransformations()
    {
        Vector3 gPosition = Position,
        gRotation = Rotation,
        gScale = Scale;
        Node3D current = this;

        _Quaternion = new(
            float.DegreesToRadians(Rotation.X),
            float.DegreesToRadians(Rotation.Y),
            float.DegreesToRadians(Rotation.Z)
        );

        while (current.Parent is not null)
        {
            if (current.Parent is not Node3D node3D)
            {
                continue;
            }

            gPosition += node3D.Position;
            gRotation += node3D.Rotation;
            gScale *= node3D.Scale;
            current = node3D;
        }

        _GlobalPosition = gPosition;
        _GlobalRotation = gRotation;
        _GlobalScale = gScale;
        _GlobalQuaternion = new(
            float.DegreesToRadians(gRotation.X),
            float.DegreesToRadians(gRotation.Y),
            float.DegreesToRadians(gRotation.Z)
        );

        UpdateTransformationsToChildren();
    }

    /// <summary>
    /// Updates the Front, Up and Right vectors.
    /// </summary>
    private void UpdateVectors()
    {
        float pitch = Rotation.Y,
        yaw = Rotation.X;

        // First, the front matrix is calculated using some basic trigonometry.
        _Front.X = float.Cos(pitch) * float.Cos(yaw);
        _Front.Y = float.Sin(pitch);
        _Front.Z = float.Cos(pitch) * float.Sin(yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
        _Front = _Front.Unit;

        // Calculate both the right and the up vector using cross product.
        // Note that we are calculating the right from the global up; this behaviour might
        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
        _Right = EVector3.Cross(_Front, EVector3.Up).Unit;
        _Up = EVector3.Cross(_Right, _Front).Unit;
    }
}