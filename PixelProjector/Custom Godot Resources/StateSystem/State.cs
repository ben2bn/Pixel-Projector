namespace StateSystem;

using Godot;
using System;

/// <summary>
/// <para>The base class for states to be entered into the <see cref="StateMachine.States"/>.</para>
/// <para>The State can be added as a child of a <see cref="StateSystem.StateMachine"/> node in the scene tree.</para>
/// <para>The State can also be initialized on runtime and added to the <see cref="StateMachine.States"/> by calling <see cref="StateMachine.AddState(State)"/> (this will not automatically add it to the scene tree however).</para>
/// <para>New states should override all abstract and virtual methods.</para>
/// </summary>
/// <remarks>
/// Remark: Only a single <see cref="State"/> per type can be entered into <see cref="StateMachine.States"/>.
/// </remarks>
[GlobalClass]
public abstract partial class State : Node
{
    /// <summary>
    /// Emitted when the state is transitionned into (i.e. becomes <see cref="StateMachine.CurrentState"/>).
    /// </summary>
    [Signal]
    public delegate void OnEnterStateEventHandler();

    /// <summary>
    /// Emitted when the state is transitionned out (i.e. is no longer <see cref="StateMachine.CurrentState"/>).
    /// </summary>
    [Signal]
    public delegate void OnExitStateEventHandler();

    /// <summary>
    /// Emitted when this state attempts to transition to the state of name <paramref name="stateTo"/>.
    /// </summary>
    [Signal]
    public delegate void OnStateTransitionEventHandler(string stateTo);

    /// <summary>
    /// Emitted from <see cref="TryForceTransition"/>.
    /// </summary>
    [Signal]
    public delegate void OnTryForceTransitionEventHandler(State stateTo);

    /// <summary>
    /// Dependency injection referencing the parent <see cref="StateMachine"/>.
    /// </summary>
    public StateMachine StateMachine { get; private set; }

    /// <summary>
    /// Dependency injection to the <see cref="StateMachine"/>'s owner to which the state behaviour is applied to.
    /// </summary>
    public Node2D StateOwner { get; private set; }

    /// <summary>
    /// Injects the dependancies <see cref="StateMachine"/> and <see cref="StateOwner"/> (called from <see cref="StateMachine.AddState(State)"/>).
    /// </summary>
    public void SetDependencies(StateMachine stateMachine, Node2D owner)
    {
        StateMachine = stateMachine;
        StateOwner = owner;
    }

    /// <summary>
    /// Called when the state becomes the <see cref="StateMachine.CurrentState"/>.
    /// </summary>
    public virtual void OnEnter()
    {
        EmitSignal(SignalName.OnEnterState);
    }

    /// <summary>
    /// Called when the state is swapped for another (i.e. no longer the <see cref="StateMachine.CurrentState"/>).
    /// </summary>
    public virtual void OnExit()
    {
        EmitSignal(SignalName.OnExitState);
    }

    /// <summary>
    /// Called every process frame by the <see cref="StateMachine"/> if this state is the <see cref="StateMachine.CurrentState"/>.
    /// </summary>
    public virtual void Process(double delta) { }

    /// <summary>
    /// Called every physics process frame <see cref="StateMachine"/> if this state is the <see cref="StateMachine.CurrentState"/>.
    /// </summary>
    public virtual void PhysicsProcess(double delta) { }

    /// <summary>
    /// Evaluate the neccessary conditions for the state to be transitionned into and returns <see langword="true"/> if possible.
    /// </summary>
    public abstract bool CanTransitionIn(bool signalCall = false);

    /// <summary>
    /// Evaluate the neccessary conditions for the state to be transitionned out and returns <see langword="true"/> if possible.
    /// </summary>
    public abstract bool CanTransitionOut(bool signalCall = false);

    /// <summary>
    /// Returns whether this state can transitionned to the state of type name <paramref name="stateTo"/> assuming the current state can be transitionned out of.
    /// </summary>
    public bool CanTransitionTo(string stateTo, bool signalCall = false)
    {
        if (!StateMachine.States.ContainsKey(stateTo)) return false;

        return StateMachine.States[stateTo].CanTransitionIn(signalCall);
    }

    /// <summary>
    /// Tries to transition from this state to the state of type name <paramref name="stateTo"/> by sending a <see cref="Signal"/> and assuming the current state can be transitionned out of.
    /// </summary>
    /// <returns>the success of the transition</returns>
    protected bool TryTransitionTo(string stateTo, bool signalCall = false)
    {
        if (CanTransitionTo(stateTo, signalCall))
        {
            EmitSignal(SignalName.OnStateTransition, stateTo);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tries to transition from this state to <paramref name="stateTo"/> by sending a <see cref="Signal"/> and assuming the current state can be transitionned out of.
    /// </summary>
    /// <returns>the success of the transition</returns>
    protected bool TryTransitionTo(Type stateTo, bool signalCall = false)
    {
        return TryTransitionTo(stateTo.Name, signalCall);
    }

    /// <summary>
    /// <para>Tries to transition from this state to other states referenced and returns a <see cref="bool"/> of the success.</para>
    /// <example>
    /// Default implementation:
    /// <code>
    /// public override bool TryTransitionOut(bool signalCall = false)
    /// {
    ///     if (!CanTransitionOut(signalCall)) return false;
    ///
    ///     if (TryTransitionTo(typeof(StateType), signalCall)) return true;
    ///
    ///     return false;
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// The <see cref="TryTransitionTo(Type, bool)"/> can be overrided with another condition intead of the default.
    /// </remarks>
    public abstract bool TryTransitionOut(bool signalCall = false);

    /// <summary>
    /// Emits the signal <see cref="OnTryForceTransition"/> which tries to transition from <see cref="StateMachine.CurrentState"/> to this state with <c>signalCall = <see langword="true"/></c>.
    /// </summary>
    public virtual void TryForceTransition()
    {
        EmitSignal(SignalName.OnTryForceTransition, this);
    }
}