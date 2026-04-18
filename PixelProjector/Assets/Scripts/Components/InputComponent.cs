namespace ComponentSystem;

using Godot;
using SceneLoadingSystem;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class InputComponent : Node, IComponent, IHasComponentDependency
{
    [Signal]
    public delegate void OnPressBonkEventHandler();
    [Signal]
    public delegate void OnChangeMoveDirectionEventHandler(Vector2 moveDirection);
    [Signal]
    public delegate void OnDebugEventHandler();

    [Export]
    public VelocityComponent VelocityComponent
    {
        get => velocityComponent;
        private set
        {
            if (VelocityComponent == value) return;

            if (VelocityComponent != null) OnChangeMoveDirection -= VelocityComponent.UpdateVelocityWithSmoothing;

            velocityComponent = value;
            if (VelocityComponent != null) OnChangeMoveDirection += VelocityComponent.UpdateVelocityWithSmoothing;
        }
    }
    private VelocityComponent velocityComponent;

    public Vector2 MoveDirection { get; private set; }

    [Export]
    public Node Debug { get; set; }

    public override void _EnterTree()
    {
        if (!VelocityComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.VelocityComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    public override void _Process(double delta)
    {
        CheckBonkInput();

        if (Input.IsActionJustPressed("Debug"))
        {
            if (Debug.IsValidInstance()) (Debug as InteractComponent).InteractCurrentInteractable();
            EmitSignal(SignalName.OnDebug);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateMoveDirection();
    }

    private void UpdateMoveDirection()
    {
        if (VelocityComponent == null) return;

        Vector2 newMoveDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
        if (!VelocityComponent.Velocity.IsEqualApprox(newMoveDirection))
        {
            MoveDirection = newMoveDirection;
            EmitSignal(SignalName.OnChangeMoveDirection, MoveDirection);
        }
    }

    private void CheckBonkInput()
    {
        if (Input.IsActionPressed("Bonk"))
        {
            EmitSignal(SignalName.OnPressBonk);
        }
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!VelocityComponent.IsValidInstance())
        {
            VelocityComponent = (VelocityComponent)componentManager.GetComponentMatch(typeof(VelocityComponent)) ?? VelocityComponent;
        }
    }
}