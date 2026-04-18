namespace ComponentSystem;

using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class VelocityComponent : Node, IComponent
{
    public Vector2 Velocity { get; private set; }

    [Export(PropertyHint.Range, "0, 1000, 0.1, or_greater")]
    public float Speed { get; private set; } = 275f;

    [Export(PropertyHint.Range, "0, 100, 0.1, or_greater")]
    public float SmoothingConstant { get; private set; } = 25;

    public void UpdateVelocity(Vector2 moveDirection)
    {
        Velocity = moveDirection * Speed;
    }

    public void UpdateVelocityWithSmoothing(Vector2 moveDirection)
    {
        Vector2 targetVelocity = moveDirection * Speed;
        Velocity = Velocity.Lerp(targetVelocity, 1.0f - Mathf.Exp(-SmoothingConstant * (float)GetPhysicsProcessDeltaTime()));
    }
}