namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class MovementComponent : Node, IComponent, IHasComponentDependency
{
    [Export]
    public CharacterBody2D Body { get; private set; }

    [Export]
    public VelocityComponent VelocityComponent { get; private set; }

    [Export]
    public bool CanMove { get; set; } = true;

    public override void _EnterTree()
    {
        if (!Body.IsValidInstance())
        {
            GD.PushWarning($"{PropertyName.Body} at {GetPath()} should be defined explicitely in editor before running if available.");
            Node owner = GetParentOrNull<ComponentManager>()?.ComponentManagerOwner;
            if (owner == null || owner is not CharacterBody2D) return;
            Body = (CharacterBody2D)owner;
        }
        if (!VelocityComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.VelocityComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveTick(delta);
    }

    public void MoveTick(double delta)
    {
        if (!CanMove) return;
        if (VelocityComponent == null) return;

        Body.Velocity = VelocityComponent.Velocity;
        Body.MoveAndSlide();
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!VelocityComponent.IsValidInstance())
        {
            VelocityComponent = (VelocityComponent)componentManager.GetComponentMatch(typeof(VelocityComponent)) ?? VelocityComponent;
        }
    }
}