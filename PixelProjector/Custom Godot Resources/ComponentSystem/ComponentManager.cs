namespace ComponentSystem;

using Godot;
using System;

/// <summary>
/// <para>The <see cref="ComponentManager"/> inherits <see cref="Node"/> and should be placed in the scene tree. It should be added as a script of a <see cref="Node2D"/> or <see cref="Node3D"/> depending on the components behaviour (e.g. <see cref="Area2D"/> components need to be under <see cref="Node2D"/>).</para>
/// <para>All <see cref="IComponent"/> of the <see cref="ComponentManagerOwner"/> should be added as a child of the <see cref="ComponentManager"/>.</para>
/// <para>Avoid adding non <see cref="IComponent"/> nodes to the <see cref="ComponentManager"/>.</para>
/// </summary>
[GlobalClass]
public partial class ComponentManager : Node
{
    /// <summary>
    /// Emitted when a component is added to or removed from the <see cref="ComponentManager"/>.
    /// </summary>
    [Signal]
    public delegate void OnComponentChildrenCountChangedEventHandler(ComponentManager componentManager);

    /// <summary>
    /// Emitted when a component is added to the <see cref="ComponentManager"/>.
    /// </summary>
    [Signal]
    public delegate void OnComponentChildEnteredTreeEventHandler(Node component);

    /// <summary>
    /// Emitted when a component is removed from the <see cref="ComponentManager"/>.
    /// </summary>
    [Signal]
    public delegate void OnComponentChildExitingTreeEventHandler(Node component);

    /// <summary>
    /// The owner of the <see cref="ComponentManager"/> (i.e. the node which the <see cref="ComponentManager"/> and its children components' behaviour should apply to. It is generally the parent of the <see cref="ComponentManager"/> node).
    /// </summary>
    [Export]
    public Node ComponentManagerOwner { get; private set; }

    /// <summary>
    /// <para>Attaches default signals for the behaviour of the state children signals behaviours.</para>
    /// </summary>
    public override void _EnterTree()
    {
        ChildEnteredTree += OnComponentAdded;
        ChildExitingTree += OnComponentRemoved;
    }

    /// <summary>
    /// <para>Returns the <see cref="IComponent"/> of specified <paramref name="componentType"/> from the <see cref="ComponentManager"/>'s children. Returns <see langword="null"/> if none match.</para>
    /// <para>If <paramref name="exactMatch"/> is set to <see langword="true"/>, then only the <see cref="IComponent"/> of the same type is returned, otherwise if set to <see langword="false"/>, all types directly or indirectly inheriting from <paramref name="componentType"/> are accepted.</para>
    /// </summary>
    public IComponent GetComponentMatch(Type componentType, bool exactMatch = true)
    {
        if (componentType == null) return null;

        foreach (Node child in GetChildren())
        {
            if (child is not IComponent) continue;

            if ((componentType == child.GetType()) ||
                (componentType.IsAssignableFrom(child.GetType()) && !exactMatch))
            {
                return child as IComponent;
            }
        }
        return null;
    }

    /// <summary>
    /// <para>Called when a <see cref="Node"/> enters as a <paramref name="child"/> of the <see cref="ComponentManager"/>.</para>
    /// <para>If it is a <see cref="IComponent"/> emits the signals <see cref="OnComponentChildrenCountChanged"/> and <see cref="OnComponentChildEnteredTree"/>.</para>
    /// <para>Connects the <see cref="IComponent"/> if it inherits <see cref="IHasComponentDependency"/> to the signal <see cref="OnComponentChildrenCountChanged"/>.</para>
    /// </summary>
    private void OnComponentAdded(Node child)
    {
        if (child is not IComponent) { GD.PushWarning("Avoid adding non components nodes as childs of ComponentManager."); return; }

        TryAddOnComponentChildrenCountChangedListener(child);

        EmitSignal(SignalName.OnComponentChildrenCountChanged, this);
        EmitSignal(SignalName.OnComponentChildEnteredTree, child);
    }

    /// <summary>
    /// <para>Called a <paramref name="child"/> exits from the <see cref="ComponentManager"/>.</para>
    /// <para>If it is a <see cref="IComponent"/> emits the signals <see cref="OnComponentChildrenCountChanged"/> and <see cref="OnComponentChildExitingTree"/>.</para>
    /// <para>Disconnects the <see cref="IComponent"/> if it inherits <see cref="IHasComponentDependency"/> from the signal <see cref="OnComponentChildrenCountChanged"/>.</para>
    /// </summary>
    private void OnComponentRemoved(Node child)
    {
        if (child is not IComponent) return;

        TryRemoveOnComponentChildrenCountChangedListener(child);

        EmitSignal(SignalName.OnComponentChildrenCountChanged, this);
        EmitSignal(SignalName.OnComponentChildExitingTree, child);
    }

    /// <summary>
    /// Connects the <paramref name="node"/> as a listener of the signal <see cref="OnComponentChildrenCountChanged"/> if <paramref name="node"/> inherits <see cref="IHasComponentDependency"/>.
    /// </summary>
    public bool TryAddOnComponentChildrenCountChangedListener(Node node)
    {
        if (node is not IHasComponentDependency) return false;
        OnComponentChildrenCountChanged += (node as IHasComponentDependency).TryUpdateComponentDependencies;
        return true;
    }

    /// <summary>
    /// Disconnects the <paramref name="node"/> as a listener of the signal <see cref="OnComponentChildrenCountChanged"/> if <paramref name="node"/> inherits <see cref="IHasComponentDependency"/>.
    /// </summary>
    public bool TryRemoveOnComponentChildrenCountChangedListener(Node node)
    {
        if (node is not IHasComponentDependency) return false;
        OnComponentChildrenCountChanged -= (node as IHasComponentDependency).TryUpdateComponentDependencies;
        return true;
    }
}
