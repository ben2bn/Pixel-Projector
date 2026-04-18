namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class BonkingComponent : Node, IComponent, IHasComponentDependency
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

            if (InputComponent != null) InputComponent.OnPressBonk -= OnBonk;

            inputComponent = value;
            if (InputComponent != null) InputComponent.OnPressBonk += OnBonk;
        }
    }
    private InputComponent inputComponent;

    [Export]
    public float BonkSpeed { get; private set; } = 2.5f;

    public override void _EnterTree()
    {
        if (!AnimationPlayer.IsValidInstance()) GD.PushWarning($"{PropertyName.AnimationPlayer} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (BonkingAnimation == null) GD.PushWarning($"{PropertyName.BonkingAnimation} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!InputComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.InputComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    public void OnBonk()
    {
        AnimationPlayer.Play(BonkingAnimation, customSpeed: BonkSpeed);
    }

    public void OnAnimationFinish(StringName animationName)
    {
        AnimationPlayer.Stop();
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
	{
		if (!InputComponent.IsValidInstance())
		{
            InputComponent = (InputComponent)componentManager.GetComponentMatch(typeof(InputComponent)) ?? InputComponent;
		}
	}
}
