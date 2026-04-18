namespace ComponentSystem;

using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class InteractComponent : Area2D, IComponent
{
	[Signal]
	public delegate void OnChangeCurrentInteractableEventHandler(InteractableComponent previousInteractable, InteractableComponent newInteractable);
	[Signal]
	public delegate void OnInteractEventHandler(InteractableComponent interactableComponent);


	public InteractableComponent CurrentInteractable
	{
		get => currentInteractable;
		set
		{
			if (CurrentInteractable == value) return;
			OnChangeCurrentInteractableCalls(CurrentInteractable, value);
			currentInteractable = value;
		}
	}

	public List<InteractableComponent> PossibleInteractables { get; private set; } = new List<InteractableComponent>();

	private InteractableComponent currentInteractable = null;

	public override void _Ready()
	{
		AreaEntered += OnInteractableComponentEnter;
		AreaExited += OnInteractableComponentExit;
	}

	public override void _Process(double delta)
	{
		SortPossibleInteractions();
	}

	private void OnInteractableComponentEnter(Area2D area)
	{
		if (!area.IsValidInstance() || area is not InteractableComponent) return;

		InteractableComponent interactable = area as InteractableComponent;
		PossibleInteractables.Add(interactable);
	}

	private void OnInteractableComponentExit(Area2D area)
	{
		if (!PossibleInteractables.Contains(currentInteractable)) return;

		InteractableComponent interactable = area as InteractableComponent;
		PossibleInteractables.Remove(interactable);
	}

	private void SetCurrentInteractable()
	{
		CurrentInteractable = PossibleInteractables.Count == 0 ? null : PossibleInteractables[0];
	}

	private void OnChangeCurrentInteractableCalls(InteractableComponent previousInteractable, InteractableComponent newInteractable)
	{
		if (previousInteractable.IsValidInstance()) previousInteractable.OnRemoveCurrentIneractable(this);
		if (newInteractable.IsValidInstance()) newInteractable.OnAssignCurrentIneractable(this);
		EmitSignal(SignalName.OnChangeCurrentInteractable, previousInteractable.NullIfInvalid(), newInteractable.NullIfInvalid());
	}

	public void InteractCurrentInteractable()
	{
		if (!CurrentInteractable.IsValidInstance()) return;
		CurrentInteractable.Interact(this);
		EmitSignal(SignalName.OnInteract, CurrentInteractable);
	}

	private void SortPossibleInteractions()
	{
		PossibleInteractables.RemoveAll((interactable) => !interactable.IsValidInstance());
		PossibleInteractables.Sort((first, second) => GlobalPosition.DistanceTo(first.GlobalPosition).CompareTo(GlobalPosition.DistanceTo(second.GlobalPosition)));
		SetCurrentInteractable();
	}
}
