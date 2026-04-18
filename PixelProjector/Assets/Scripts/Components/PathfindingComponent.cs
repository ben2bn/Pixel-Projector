namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class PathfindingComponent : Node, IComponent, IHasComponentDependency
{
	[Signal]
	public delegate void OnReachTargetPositionEventHandler();

	[Export]
	public VelocityComponent VelocityComponent { get; private set; }

	[Export]
	public CharacterBody2D Body { get; private set; }

	[Export]
	public NavigationAgent2D NavigationAgent
	{
		get => navigationAgent;
		private set
		{
			if (!value.IsValidInstance()) return;
			navigationAgent = value;
			NavigationAgent.TargetReached += OnTargetReach;
		}
	}
	private NavigationAgent2D navigationAgent;

	[Export]
	public float RotationSmoothingConstant { get; private set; } = 25f;

	[Export]
	public bool CanMove { get; set; } = true;

	[Export]
	public Node2D TargetDestination
	{
		get => targetDestination;
		set
		{
			if (!value.IsValidInstance()) return;
			targetDestination = value;
			OnChangeTargetDestination();
		}
	}
	private Node2D targetDestination;

	public override void _EnterTree()
	{
		if (!VelocityComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.VelocityComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
		if (!Body.IsValidInstance()) GD.PushWarning($"{PropertyName.Body} at {GetPath()} should be defined explicitely in editor before running if available.");
		if (!NavigationAgent.IsValidInstance()) GD.PushWarning($"{PropertyName.NavigationAgent} at {GetPath()} should be defined explicitely in editor before running if available.");
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveTick(delta);
		RotationTick(delta, NavigationAgent.TargetPosition);
	}

	public void MoveTick(double delta)
	{
		if (!CanMove) return;
		if (VelocityComponent == null) return;
		if (NavigationAgent.GetFinalPosition().IsEqualApprox(Body.GlobalPosition)) return;

		VelocityComponent.UpdateVelocityWithSmoothing(Body.GlobalPosition.DirectionTo(NavigationAgent.GetNextPathPosition()));

		Body.Velocity = VelocityComponent.Velocity;
		Body.MoveAndSlide();

		if (Body.GlobalPosition.DistanceSquaredTo(NavigationAgent.GetFinalPosition()) < 4f)
		{
			Body.GlobalPosition = NavigationAgent.GetFinalPosition();
			Body.Velocity = Vector2.Zero;
			OnTargetReach();
		}
	}

	public void RotationTick(double delta)
	{
		Body.GlobalRotation = Mathf.Lerp(Body.GlobalRotation, Body.GlobalRotation + Body.GetAngleTo(Body.GlobalPosition + VelocityComponent.Velocity), 1.0f - Mathf.Exp(-RotationSmoothingConstant * (float)delta));
	}
	public void RotationTick(double delta, Vector2 lookAt)
	{
		if (!NavigationAgent.GetFinalPosition().IsEqualApprox(Body.GlobalPosition)) return;
		Body.GlobalRotation = Mathf.Lerp(Body.GlobalRotation, Body.GlobalRotation + Body.GetAngleTo(lookAt), 1.0f - Mathf.Exp(-RotationSmoothingConstant * (float)delta));
	}

	private void OnChangeTargetDestination()
	{
		NavigationAgent.TargetPosition = TargetDestination.GlobalPosition;
	}

	public void OnTargetReach()
	{
		EmitSignal(SignalName.OnReachTargetPosition);
	}

	void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
	{
		if (!VelocityComponent.IsValidInstance())
		{
			VelocityComponent = (VelocityComponent)componentManager.GetComponentMatch(typeof(VelocityComponent)) ?? VelocityComponent;
		}
	}
}
