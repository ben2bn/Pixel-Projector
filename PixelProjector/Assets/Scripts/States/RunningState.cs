namespace StateSystem;

using Godot;
using System;
using ComponentSystem;

[GlobalClass]
public partial class RunningState : State, IHasComponentDependency
{
    [Export]
    public MovementComponent MovementComponent { get; private set; }

    public override void _EnterTree()
    {
        if (!MovementComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.MovementComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    public override void OnEnter()
    {
        MovementComponent.CanMove = true;
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Process(double delta)
    {

    }

    public override void PhysicsProcess(double delta)
    {
        //GD.Print("running");

        TryTransitionOut();
    }

    public override bool CanTransitionIn(bool signalCall = false)
    {
        if (StateOwner is not CharacterBody2D) return false;

        if ((StateOwner as CharacterBody2D).Velocity.IsEqualApprox(Vector2.Zero)) return false;

        return true;
    }

    public override bool CanTransitionOut(bool signalCall = false)
    {
        return true;
    }

    public override bool TryTransitionOut(bool signalCall = false)
    {
        if (!CanTransitionOut(signalCall)) return false;

        if (TryTransitionTo(typeof(IdleState), signalCall)) return true;

        return false;
    }

    public override void TryForceTransition()
    {
        base.TryForceTransition();
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!MovementComponent.IsValidInstance())
        {
            MovementComponent = (MovementComponent)componentManager.GetComponentMatch(typeof(MovementComponent)) ?? MovementComponent;
        }
    }
}