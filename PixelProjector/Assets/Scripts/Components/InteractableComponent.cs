namespace ComponentSystem;

using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class InteractableComponent : Area2D, IComponent
{
    [Signal]
    public delegate void OnInteractEventHandler(InteractComponent interactComponent);
    [Signal]
    public delegate void OnBecomeCurrentInteractableEventHandler(InteractableComponent interactableComponent);
    [Signal]
    public delegate void OnLeaveCurrentInteractableEventHandler(InteractableComponent interactableComponent);

    public List<InteractComponent> CurrentInteractComponents { get; private set; } = new List<InteractComponent>();

    public virtual void Interact(InteractComponent interactComponent)
    {
        GD.Print("interacted");
        EmitSignal(SignalName.OnInteract, interactComponent);
    }

    public virtual void OnAssignCurrentIneractable(InteractComponent interactComponent)
    {
        if (!interactComponent.IsValidInstance()) return;
        CurrentInteractComponents.Add(interactComponent);
        EmitSignal(SignalName.OnBecomeCurrentInteractable, interactComponent);
    }

    public virtual void OnRemoveCurrentIneractable(InteractComponent interactComponent)
    {
        if (!interactComponent.IsValidInstance() || !CurrentInteractComponents.Contains(interactComponent)) return;
        CurrentInteractComponents.Remove(interactComponent);
        EmitSignal(SignalName.OnLeaveCurrentInteractable, interactComponent);
    }
}
