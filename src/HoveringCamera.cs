using System.Diagnostics.CodeAnalysis;
using MukiaEngine;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MukiaEngine.Physics;

namespace MukiaEngine;

/// <summary>
/// A hovering camera, used to preview a scene.
/// </summary>
/// <remarks>
/// Doesn't desend from <see cref="MukiaEngine.Camera"/>.
/// </remarks>
[SaveNode("engine.hovering-camera")]
public class HoveringCamera : Node3D
{
    [AllowNull]
    public Camera Camera { get; set; }

    [Export]
    public float CameraSpeed { get; set; } = 1.5f;
    [Export]
    public float Sensitivity { get; set; } = 0.005f;

    private bool FirstMove = true;
    private Vector2 LastPos;

    private void MoveCamera(float delta)
    {
        float fDelta = delta,
        time = fDelta * 4;

        Vector3 xMovement = Right * Input.InputAxis(Keys.A, Keys.D)
        * CameraSpeed * time;
        Vector3 yMovement = Up * Input.InputAxis(Keys.LeftShift, Keys.Space)
        * CameraSpeed * time;
        Vector3 zMovement = Front * Input.InputAxis(Keys.S, Keys.W)
        * CameraSpeed * time;

        Position += xMovement + yMovement + zMovement;

        // Get the mouse state
        var mouse = Input.MouseState;

        if (FirstMove) // This bool variable is initially set to true.
        {
            LastPos = new Vector2(mouse.X, mouse.Y);
            FirstMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            var deltaX = mouse.X - LastPos.X;
            var deltaY = mouse.Y - LastPos.Y;
            LastPos = new Vector2(mouse.X, mouse.Y);

            Rotation += new Vector3(deltaX * Sensitivity, deltaY * Sensitivity, 0);
        }
    }

    public override void Update(double delta)
    {
        base.Update(delta);

        MoveCamera((float)delta);

        if (Input.MouseState.IsButtonPressed(MouseButton.Button1))
        {
            ShootRay();
        }
    }

    private void ShootRay()
    {
        Ray ray = new(GlobalPosition, Front * 20)
        {
            FilterList = [this]
        };

        RaycastResult? result = Physics.Physics.Raycast(ray);

        if (result.HasValue)
        {
            Console.WriteLine(result.Value);
            Position = result.Value.Hit;
        }
    }

    public override void Awake()
    {
        base.Awake();

        Camera = New<Camera>(this, "PlayerCamera");
        Camera.IsCurrentCamera = true;
        Camera.Archivable = false;
    }
}