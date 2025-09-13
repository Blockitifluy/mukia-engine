using OpenTK.Mathematics;
using MukiaEngine.NodeSystem;

namespace MukiaEngine;

[SaveNode("engine.camera")]
public sealed class Camera : Node3D
{
    private float _Fov = 90;
    public float AspectRatio;

    /// <summary>
    /// The camera used for rendering.
    /// </summary>
    public static Camera? CurrentCamera { get; set; } = null;

    [Export]
    /// <summary>
    /// Is this camera is used for rendering
    /// </summary>
    public bool IsCurrentCamera
    {
        get
        {
            return this == CurrentCamera;
        }
        set
        {
            if (IsCurrentCamera && !value)
            {
                CurrentCamera = null;
            }
            else if (!IsCurrentCamera && value)
            {
                CurrentCamera = this;
            }
        }
    }

    [Export]
    public Color SkyColor { get; set; } = new(0.2f, 0.3f, 0.3f);

    [Export]
    /// <summary> 
    /// Field of View
    /// </summary>
    public float Fov
    {
        get => _Fov;
        set
        {
            var angle = float.Clamp(value, 1f, 90f);
            _Fov = angle;
        }
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt((GLVector3)GlobalPosition, (GLVector3)(GlobalPosition + Front), (GLVector3)Up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        var rFov = MathHelper.DegreesToRadians(_Fov);
        return Matrix4.CreatePerspectiveFieldOfView(rFov, AspectRatio, 0.01f, 100f);
    }
}

