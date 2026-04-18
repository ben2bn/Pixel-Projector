namespace StateSystem;

using Godot;
using System;

/// <summary>
/// Interface used for classes that have a <see cref="State"/> dependency that should be updated on runtime when a new <see cref="State"/> is added as a sibling <see cref="Node"/> of the same <see cref="StateMachine"/>.
/// </summary>
public interface IHasStateDependency
{
    /// <summary>
    /// <para>Checks for specified state dependencies within the class and tries to find matching states from <see cref="StateMachine.GetStateMatch(Type, bool)"/> to fulfill the dependency.</para>
    /// <para>This is generally called from <see cref="StateMachine.OnStateChildrenCountChanged"/> called when a state is added, removed or when starting the scene.</para>
    /// </summary>
    public abstract void TryUpdateStateDependencies(StateMachine stateMachine);
    /*example:
    void IHasStateDependency.TryUpdateStateDependencies(StateMachine stateMachine)
    {
        if (!IdleState.IsValidInstance()) 
        {
            IdleState = stateMachine.GetStateMatch(typeof(IdleState)) ?? IdleState;
        }
    }
    */
}
