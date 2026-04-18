namespace StateSystem;

using Godot;
using System;

[GlobalClass]
public partial class IdleState : State
{
    public override void OnEnter()
    {
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
        //GD.Print("idle");
        TryTransitionOut();
    }

    public override bool CanTransitionIn(bool signalCall = false)
    {
        if (StateOwner is not CharacterBody2D) return false;

        Vector2 velocity = (StateOwner as CharacterBody2D).Velocity;
        if (!velocity.IsEqualApprox(Vector2.Zero)) return false;

        return true;
    }

    public override bool CanTransitionOut(bool signalCall = false)
    {
        return true;
    }

    public override bool TryTransitionOut(bool signalCall = false)
    {
        if (!CanTransitionOut(signalCall)) return false;

        if (TryTransitionTo(typeof(RunningState), signalCall)) return true;

        return false;
    }

    public override void TryForceTransition()
    {
        base.TryForceTransition();
    }
}