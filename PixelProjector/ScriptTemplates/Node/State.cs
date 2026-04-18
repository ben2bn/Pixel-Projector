// meta-description: Base template for states

namespace StateSystem;

using _BINDINGS_NAMESPACE_;
using System;

[GlobalClass]
public partial class _CLASS_ : State
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

    }

    public override bool CanTransitionIn(bool signalCall = false)
    {
        return false;
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
}