namespace StateSystem;

using ComponentSystem;
using Godot;
using System;

[GlobalClass]
public partial class BonkingState : State, IHasComponentDependency
{
    [Export]
    public AnimationPlayer AnimationPlayer
    {
        get => animationPlayer;
        private set
        {
            if (AnimationPlayer == value) return;

            if (AnimationPlayer != null) AnimationPlayer.AnimationFinished -= OnAnimationFinish;

            animationPlayer = value;
            if (AnimationPlayer != null) AnimationPlayer.AnimationFinished += OnAnimationFinish;
        }
    }
	private AnimationPlayer animationPlayer;

    [Export]
	public Animation BonkingAnimation { get; private set; }

	[Export]
	public InputComponent InputComponent
	{
        get => inputComponent;
        private set
        {
            if (InputComponent == value) return;

            if (InputComponent != null) InputComponent.OnPressBonk -= TryForceTransition;

            inputComponent = value;
            if (InputComponent != null) InputComponent.OnPressBonk += TryForceTransition;
        }
    }
    private InputComponent inputComponent;


    public override void _EnterTree()
    {
        if (!AnimationPlayer.IsValidInstance()) GD.PushWarning($"{PropertyName.AnimationPlayer} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (BonkingAnimation == null) GD.PushWarning($"{PropertyName.BonkingAnimation} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!InputComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.InputComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    public override void OnEnter()
	{
		AnimationPlayer.Play(BonkingAnimation, customSpeed:2.5f);
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
		if (!AnimationPlayer.IsAnimationActive()) TryTransitionOut();
	}

	public override bool CanTransitionIn(bool signalCall = false)
	{
		return true;
	}

	public override bool CanTransitionOut(bool signalCall = false)
	{
		if (AnimationPlayer.IsAnimationActive()) return false;
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

	public void OnAnimationFinish(StringName animationName)
	{
		AnimationPlayer.Stop();
		TryTransitionOut();
	}

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!InputComponent.IsValidInstance())
        {
            InputComponent = (InputComponent)componentManager.GetComponentMatch(typeof(InputComponent)) ?? InputComponent;
        }
    }
}
