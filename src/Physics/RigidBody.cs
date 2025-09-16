using MukiaEngine.NodeSystem;

namespace MukiaEngine.Physics;

/// <summary>
/// A physics collision body
/// </summary>
[SaveNode("engine.rigid-body")]
public sealed class RigidBody : Node3D
{
    /// <summary>
    /// The weight of the object in kg.
    /// </summary>
    [Export]
    public float Mass { get; set; } = 1.0f;
    /// <summary>
    /// The gravity of the RigidBody.
    /// </summary>
    [Export]
    public float Gravity { get; set; } = 9.8f;
    /// <summary>
    /// The air resistance of the RigidBody.
    /// </summary>
    /// <remarks>
    /// Multiples the <see cref="Velocity"/>, between 0 to 1.
    /// </remarks>
    [Export]
    public float AirResistance { get; set; } = 0.95f;

    /// <summary>
    /// The current velocity of the RigidBody.
    /// </summary>
    public Vector3 Velocity;

    /// <summary>
    /// The collider the physics body uses.
    /// </summary>
    [Export]
    public Collider? Collider { get; set; }

    /// <summary>
    /// Applies the force to the body. 
    /// </summary>
    /// <param name="force">The force</param>
    public void ApplyForce(Vector3 force)
    {
        Velocity += force;
    }

    public override void UpdateFixed()
    {
        base.UpdateFixed();

        Vector3 fall = -Vector3.Up * (Mass * Gravity),
        final = fall * (float)Tree.FixedUpdateSeconds;

        Velocity += final;
        Velocity *= AirResistance;

        Ray ray = new(GlobalPosition, Velocity)
        {
            FilterList = [this],
            FilterType = CollisionFilter.Exclude
        };

        RaycastResult? raycast = Physics.Raycast(ray);
        if (raycast.HasValue)
        {
            GlobalPosition = raycast.Value.Hit;
            return;
        }

        GlobalPosition += Velocity;
    }
}