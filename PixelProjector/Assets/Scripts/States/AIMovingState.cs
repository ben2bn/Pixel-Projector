namespace ComponentSystem;

using Godot;
using StateSystem;
using System;

[GlobalClass]
public partial class AIMovingState : State, IHasComponentDependency
{
    [Export]
    public PathfindingComponent PathfindingComponent { get; private set; }

    public override void _EnterTree()
    {
        if (!PathfindingComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.PathfindingComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    public override void OnEnter()
    {
        PathfindingComponent.CanMove = true;
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
		if (!PathfindingComponent.IsValidInstance())
		{
            PathfindingComponent = (PathfindingComponent)componentManager.GetComponentMatch(typeof(PathfindingComponent)) ?? PathfindingComponent;
		}
	}
}
