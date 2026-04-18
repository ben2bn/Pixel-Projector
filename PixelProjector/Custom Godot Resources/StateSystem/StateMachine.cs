namespace StateSystem;

using ComponentSystem;
using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// <para>The <see cref="StateMachine"/> inherits from <see cref="Node"/> and should be placed in the scene tree. It controls the transitions between states.</para>
/// <para>The <see cref="StateOwner"/> and <see cref="DefaultState"/> should be assigned on ready through the inspector tab.</para>
/// <para>States can be added to the state machine through the scene tree as children of the <see cref="StateMachine"/> node.</para>
/// <para>Avoid adding non <see cref="State"/> nodes to the <see cref="StateMachine"/>.</para>
/// <para>States can be added on runtime by calling <see cref="AddState(State)"/> or simply calling <see cref="Node.AddChild(Godot.Node, bool, Godot.Node.InternalMode)"/> (both ways will add the <see cref="State"/> to the tree).</para>
/// <para>If a <see cref="State"/> is added or removed, the signals <see cref="OnStateChildrenCountChanged"/>, <see cref="OnStateChildEnteredTree"/> and <see cref="OnStateChildExitingTree"/> are emitted. This also updates the listeners, notably for <see cref="IHasStateDependency"/>.</para>
/// </summary>
/// <remarks>
/// <para>Remark: only one <see cref="State"/> per type can added to <see cref="States"/></para>
/// </remarks>
[GlobalClass]
public partial class StateMachine : Node, IHasStateDependency
{
    /// <summary>
    /// Emitted when a state is added to or removed from the <see cref="StateMachine"/>.
    /// </summary>
    [Signal]
    public delegate void OnStateChildrenCountChangedEventHandler(StateMachine stateMachine);

    /// <summary>
    /// Emitted when a state is added to the <see cref="StateMachine"/>.
    /// </summary>
    [Signal]
    public delegate void OnStateChildEnteredTreeEventHandler(Node state);

    /// <summary>
    /// Emitted when a state is removed from the <see cref="StateMachine"/>.
    /// </summary>
    [Signal]
    public delegate void OnStateChildExitingTreeEventHandler(Node state);

    /// <summary>
    /// The <see cref="State"/> at which the <see cref="StateMachine"/> will start at and return to when <see cref="CurrentState"/> looses its reference.
    /// </summary>
    [Export]
    public State DefaultState { get; private set; }

    /// <summary>
    /// The owner of the <see cref="StateMachine"/> (i.e. the node which the <see cref="StateMachine"/> behaviour should be applied to and generally a parent of the <see cref="StateMachine"/> node).
    /// </summary>
    [Export]
    public Node2D StateOwner { get; private set; }

    /// <summary>
    /// <para>Reference to the <see cref="ComponentManager"/> for the same <see cref="Node"/> parent.</para>
    /// <para>Used for setting <see cref="IHasComponentDependency"/> for states in the <see cref="StateMachine"/>.</para>
    /// </summary>
    [Export]
    public ComponentManager ComponentManager
    {
        get => componentManager;
        private set
        {
            if (componentManager == value) return;

            componentManager = value;

            if (ComponentManager != null)
            {
                foreach (State state in States.Values)
                {
                    if (state is not IHasComponentDependency) continue;
                    (state as IHasComponentDependency).TryUpdateComponentDependencies(ComponentManager);
                }
            }
        }
    }
    private ComponentManager componentManager;

    /// <summary>
    /// The <see cref="State"/> currently applying logic (which <see cref="State.Process(double)"/> and <see cref="State.PhysicsProcess(double)"/> methods are called)
    /// </summary>
    public State CurrentState { get; private set; }

    /// <summary>
    /// The collection of all possible state transitions of this <see cref="StateMachine"/>. The keys are the names of the <see cref="State"/> types.
    /// </summary>
    /// <remarks>
    /// Remark: no duplicate type of <see cref="State"/> can be entered into <see cref="States"/>.
    /// </remarks>
    public Dictionary<string, State> States { get; private set; } = new Dictionary<string, State>();

    /// <summary>
    /// <para>Sets the signals <see cref="OnStateChildEnteredTree"/>, <see cref="OnStateChildExitingTree"/> and <see cref="OnStateChildrenCountChanged"/> to their functionalities.</para>
    /// <para>Also adds the <see cref="StateMachine"/> as a listener, since it depends on <see cref="DefaultState"/> and <see cref="CurrentState"/>.</para>
    /// </summary>
    /// <remarks>Also checks for missing dependencies and send warnings if invalid.</remarks>
    public override void _EnterTree()
    {
        TryAddStateChildrenCountChangedListener(this);
        ChildEnteredTree += OnStateAdded;
        ChildExitingTree += OnStateRemoved;

        if (!ComponentManager.IsValidInstance())
        {
            GD.PushWarning($"{PropertyName.ComponentManager} at {GetPath()} should be defined explicitely in editor before running if available.");
            TryUpdateComponentManagerDependency();
        }
        if (!DefaultState.IsValidInstance()) GD.PushWarning($"{PropertyName.DefaultState} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    /// <summary>
    /// <para>Sets the <see cref="CurrentState"/> to be the <see cref="DefaultState"/>, consequently starting the <see cref="StateMachine"/>.</para>
    /// <para>Sends an error message if the <see cref="StateOwner"/> is undefined.</para>
    /// </summary>
    public override void _Ready()
    {
        if (!StateOwner.IsValidInstance()) GD.PushError($"{PropertyName.ComponentManager} at {GetPath()} should be defined.");

        ReadyDefaultState();
    }

    /// <summary>
    /// Sets the <see cref="DefaultState"/> to be <paramref name="state"/> if found in <see cref="States"/>.
    /// </summary>
    /// <remarks>
    /// Remark: the <paramref name="state"/> instance is ignored and only checks for its <see cref="Type"/> in <see cref="States"/>.
    /// </remarks>
    public void SetDefaultState(State state)
    {
        SetDefaultState(state.GetType());
    }

    /// <summary>
    /// Sets the <see cref="DefaultState"/> to be of <paramref name="stateType"/> if found in <see cref="States"/>.
    /// </summary>
    public void SetDefaultState(Type stateType)
    {
        string stateKey = stateType.Name;
        if (!States.ContainsKey(stateKey)) return;

        DefaultState = States[stateKey];
    }

    /// <summary>
    /// <para>Returns the <see cref="State"/> of specified <paramref name="stateType"/> inside <see cref="States"/>. Returns <see langword="null"/> if none match.</para>
    /// <para>If <paramref name="exactMatch"/> is set to <see langword="true"/>, then only the <see cref="State"/> of the same type is returned, otherwise if set to <see langword="false"/>, all types directly or indirectly inheriting from <paramref name="stateType"/> are accepted.</para>
    /// </summary>
    public State GetStateMatch(Type stateType, bool exactMatch = true)
    {
        if (stateType == null) return null;

        foreach (State state in States.Values)
        {
            if ((stateType == state.GetType()) ||
                (stateType.IsAssignableFrom(state.GetType()) && !exactMatch))
            {
                return state;
            }
        }
        return null;
    }

    /// <summary>
    /// <para>Called when a <see cref="Node"/> enters as a <paramref name="child"/> of the <see cref="StateMachine"/>.</para>
    /// <para>If it is a <see cref="State"/> attempts to add it to the <see cref="StateMachine"/> system.</para>
    /// <para>Emits the signals <see cref="OnStateChildrenCountChanged"/> and <see cref="OnStateChildEnteredTree"/> if it is a <see cref="State"/>.</para>
    /// </summary>
    private void OnStateAdded(Node child)
    {
        if (child is not State) { GD.PushWarning("Avoid adding non states nodes as childs of StateMachine."); return; }

        AddState(child as State);

        EmitSignal(SignalName.OnStateChildrenCountChanged, this);
        EmitSignal(SignalName.OnStateChildEnteredTree, child);
    }

    /// <summary>
    /// <para>Called a <paramref name="child"/> exits from the <see cref="StateMachine"/>.</para>
    /// <para>If it is a <see cref="State"/> attempts to remove it from the <see cref="StateMachine"/> system.</para>
    /// <para>Emits the signals <see cref="OnStateChildrenCountChanged"/> and <see cref="OnStateChildExitingTree"/> if it is a <see cref="State"/>.</para>
    /// </summary>
    private void OnStateRemoved(Node child)
    {
        if (child is not State) return;

        RemoveState(child as State, false);

        EmitSignal(SignalName.OnStateChildrenCountChanged, this);
        EmitSignal(SignalName.OnStateChildExitingTree, child);
    }

    /// <summary>
    /// <para>Adds the <paramref name="state"/> to <see cref="States"/>, connects <see cref="TransitionState(State)"/> and <see cref="OnTryForceTransition(State)"/> to the signals <see cref="State.OnStateTransition"/> and <see cref="State.OnTryForceTransition"/> respectively and assigns the dependencies <see cref="State.StateOwner"/> and <see cref="State.StateMachine"/>.</para>
    /// <para>If the <paramref name="state"/> inherits <see cref="IHasComponentDependency"/> and <see cref="ComponentManager"/> is defined, then it will connect its <see cref="IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager)"/> to the signal <see cref="ComponentManager.OnComponentChildrenCountChanged"/>.</para>
    /// </summary>
    /// <remarks>
    /// Remark: if <see cref="States"/> already contains a <see cref="State"/> of the same type as <paramref name="state"/>, it will not be added and will be <see cref="NodeExtensions.SafeQueueFree(Node)"/>.
    /// </remarks>
    public void AddState(State state)
    {
        if (!state.IsValidInstance()) return;
        if (States.ContainsValue(state)) return;

        string stateKey = state.GetType().Name;
        if (States.ContainsKey(stateKey)) { state.SafeQueueFree(); return; }

        States.Add(stateKey, state);

        state.OnStateTransition += TransitionState;
        state.OnTryForceTransition += OnTryForceTransition;

        if (ComponentManager.IsValidInstance()) ComponentManager.TryAddOnComponentChildrenCountChangedListener(state);

        state.SetDependencies(this, StateOwner);
        if (!state.IsInsideTree()) AddChild(state);
    }

    /// <summary>
    /// <para>Removes the <paramref name="state"/> from <see cref="States"/> if found. The signals are also disconnected.</para>
    /// <para>If the <paramref name="state"/> inherits <see cref="IHasComponentDependency"/> and <see cref="ComponentManager"/> is defined, then it will disconnect its <see cref="IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager)"/> from the signal <see cref="ComponentManager.OnComponentChildrenCountChanged"/>.</para>
    /// <para>If <paramref name="queueFreeState"/> is set to <see langword="true"/>, the <paramref name="state"/> is automaticlly queued to be free.</para>
    /// </summary>
    /// <remarks>
    /// <para>Remarks:</para>
    /// <para>If the <paramref name="state"/> to remove is the <see cref="DefaultState"/>, then <see cref="DefaultState"/> is assigned the reference to the first <see cref="State"/> entry found in <see cref="States"/> that is not <paramref name="state"/>.</para>
    /// <para>If the <paramref name="state"/> to remove is the <see cref="CurrentState"/>, then <see cref="CurrentState"/> is transitionned back to the <see cref="DefaultState"/>.</para>
    /// </remarks>
    public void RemoveState(State state, bool queueFreeState = true)
    {
        if (state == null) return;
        if (!States.ContainsValue(state)) return;

        States.Remove(state.GetType().Name);

        if (state == DefaultState) DetermineNewDefaultState();
        if (state == CurrentState) OnRemoveCurrentState();

        if (ComponentManager.IsValidInstance()) ComponentManager.TryRemoveOnComponentChildrenCountChangedListener(state);

        state.OnStateTransition -= TransitionState;
        state.OnTryForceTransition -= OnTryForceTransition;

        state.SetDependencies(null, null);
        if (queueFreeState) state.SafeQueueFree();
    }

    /// <summary>
    /// <para>Removes the <paramref name="stateType"/> from <see cref="States"/> if found. The signals are also disconnected.</para>
    /// <para>If the found <see cref="State"/> of <paramref name="stateType"/> inherits <see cref="IHasComponentDependency"/> and <see cref="ComponentManager"/> is defined, then it will disconnect its <see cref="IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager)"/> from the signal <see cref="ComponentManager.OnComponentChildrenCountChanged"/>.</para>
    /// <para>If <paramref name="queueFreeState"/> is set to <see langword="true"/>, the found <see cref="State"/> of <paramref name="stateType"/> is automaticlly queued to be free.</para>
    /// </summary>
    /// <remarks>
    /// <para>Remarks:</para>
    /// <para>If the <paramref name="state"/> to remove is the <see cref="DefaultState"/>, then <see cref="DefaultState"/> is assigned the reference to the first <see cref="State"/> entry found in <see cref="States"/> that is not <paramref name="state"/>.</para>
    /// <para>If the <paramref name="state"/> to remove is the <see cref="CurrentState"/>, then <see cref="CurrentState"/> is transitionned back to the <see cref="DefaultState"/>.</para>
    /// </remarks>
    public void RemoveState(Type stateType, bool queueFreeState = true)
    {
        State state = GetStateMatch(stateType);
        if (state == null) return;

        RemoveState(state, queueFreeState);
    }

    /// <summary>
    /// Forces a transition from the <see cref="CurrentState"/> to the <see cref="DefaultState"/>. Called when the <see cref="CurrentState"/> is removed from <see cref="States"/> (from <see cref="RemoveState(State)"/>).
    /// </summary>
    /// <remarks>
    /// Remark: if the <see cref="CurrentState"/> is the <see cref="DefaultState"/>, then the first <see cref="State"/> value found in <see cref="States"/> that is not <see cref="DefaultState"/> is set to be the new <see cref="DefaultState"/> before changing to it.
    /// </remarks>
    private void OnRemoveCurrentState()
    {
        if (CurrentState == DefaultState || !DefaultState.IsValidInstance()) DetermineNewDefaultState();
        TransitionState(DefaultState, true);
    }

    /// <summary>
    /// Assigns the first <see cref="State"/> value found in <see cref="States"/> that is not already the current <see cref="DefaultState"/>.
    /// </summary>
    private void DetermineNewDefaultState()
    {
        foreach (State state in States.Values)
        {
            if (state == DefaultState) continue;

            DefaultState = state;
            break;
        }
    }

    /// <summary>
    /// Calls <see cref="DefaultState"/>'s <see cref="State.OnEnter"/> and assigns <see cref="DefaultState"/> to <see cref="CurrentState"/>.
    /// </summary>
    /// <remarks>
    /// Remark: if the <see cref="DefaultState"/> is not assigned, then the first <see cref="State"/> value found in <see cref="States"/> is set to be the new <see cref="DefaultState"/>.
    /// </remarks>
    private void ReadyDefaultState()
    {
        if (!DefaultState.IsValidInstance()) DetermineNewDefaultState();

        DefaultState.OnEnter();
        CurrentState = DefaultState;
    }

    /// <summary>
    /// <para>Transitions from <see cref="CurrentState"/> to <paramref name="stateTo"/>. Calls <see cref="CurrentState"/>'s <see cref="State.OnExit"/> and <paramref name="stateTo"/>'s <see cref="State.OnEnter"/> before assigning <see cref="CurrentState"/> to be <paramref name="stateTo"/>.</para>
    /// <para>If <paramref name="force"/> is set to <see langword="true"/>, it avoids all checks.</para>
    /// </summary>
    /// <remarks>
    /// Remark: it is recommended to use <see cref="TransitionState(string)"/> instead to check that <paramref name="stateTo"/> is in <see cref="States"/>.
    /// </remarks>
    private void TransitionState(State stateTo, bool force = false)
    {
        if (!force && !stateTo.IsValidInstance()) return;

        CurrentState?.OnExit();
        stateTo?.OnEnter();
        CurrentState = stateTo;
    }

    /// <summary>
    /// <para>Transitions from <see cref="CurrentState"/> to the <see cref="State"/> value with key <paramref name="stateTo"/> in <see cref="States"/>. Calls <see cref="CurrentState"/>'s <see cref="State.OnExit"/> and <paramref name="stateTo"/>'s <see cref="State.OnEnter"/> before assigning <see cref="CurrentState"/> to be the reference of <paramref name="stateTo"/>.</para>
    /// </summary>
    public void TransitionState(string stateTo)
    {
        if (!States.ContainsKey(stateTo)) return;

        TransitionState(States[stateTo]);
    }

    /// <summary>
    /// Calls the <see cref="CurrentState"/>'s <see cref="State.Process(double)"/> every process frame.
    /// </summary>
    public override void _Process(double delta)
    {
        if (!CurrentState.IsValidInstance()) return;

        CurrentState.Process(delta);
    }

    /// <summary>
    /// Calls the <see cref="CurrentState"/>'s <see cref="State.PhysicsProcess(double)(double)"/> every physics process frame.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        if (!CurrentState.IsValidInstance()) return;

        CurrentState.PhysicsProcess(delta);
    }

    /// <summary>
    /// <para>Attemps to force a transition from <see cref="CurrentState"/> to <paramref name="stateTo"/> checking their <see cref="State.CanTransitionOut(bool)"/> and <see cref="State.CanTransitionIn(bool)"/> with signalCall = <see langword="true"/> respectively.</para>
    /// <para>Called only through the signal <see cref="State.OnTryForceTransition"/> assigned in <see cref="AddState(State)"/>. Also, <paramref name="stateTo"/> is the reference of the signal emitter.</para>
    /// </summary>
    private void OnTryForceTransition(State stateTo)
    {
        if (!stateTo.IsValidInstance() || stateTo == CurrentState) return;

        if (!CurrentState.CanTransitionOut(true)) return;
        if (!stateTo.CanTransitionIn(true)) return;

        TransitionState(stateTo.GetType().Name);
    }

    /// <summary>
    /// Connects the <paramref name="node"/> as a listener of the signal <see cref="OnStateChildrenCountChanged"/> if <paramref name="node"/> inherits <see cref="IHasStateDependency"/>.
    /// </summary>
    public void TryAddStateChildrenCountChangedListener(Node node)
    {
        if (node is not IHasStateDependency) return;
        OnStateChildrenCountChanged += (node as IHasStateDependency).TryUpdateStateDependencies;
    }

    /// <summary>
    /// Disconnects the <paramref name="node"/> as a listener of the signal <see cref="OnStateChildrenCountChanged"/> if <paramref name="node"/> inherits <see cref="IHasStateDependency"/>.
    /// </summary>
    public void TryRemoveOnStateChildrenCountChangedListener(Node node)
    {
        if (node is not IHasStateDependency) return;
        OnStateChildrenCountChanged -= (node as IHasStateDependency).TryUpdateStateDependencies;
    }

    /// <summary>
    /// If <see cref="ComponentManager"/> is undefined, tries to find <see cref="ComponentManager"/> among the siblings of the <see cref="StateMachine"/>'s parent <see cref="Node"/>.
    /// </summary>
    public void TryUpdateComponentManagerDependency()
    {
        if (!ComponentManager.IsValidInstance())
        {
            ComponentManager = (ComponentManager)this.GetSiblingNode(typeof(ComponentManager)) ?? ComponentManager;
        }
    }

    void IHasStateDependency.TryUpdateStateDependencies(StateMachine stateMachine)
    {
        if (!DefaultState.IsValidInstance())
        {
            DefaultState = stateMachine.GetStateMatch(typeof(State), false) ?? DefaultState;
        }
        if (!CurrentState.IsValidInstance())
        {
            if (DefaultState.IsValidInstance())
            {
                CurrentState = DefaultState;
                TransitionState(CurrentState, true);
            }
        }
    }
}