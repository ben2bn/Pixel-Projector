namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class HitBoxComponent : Area2D, IComponent
{
    [Signal]
    public delegate void OnHitByHurtBoxComponentEventHandler();

    [Export]
    public MixerAiComponent MixerAiComponent { get; private set; }

    public override void _Ready()
    {
        if (!MixerAiComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.MixerAiComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }

    public override void _EnterTree()
    {
        OnHitByHurtBoxComponent += OnHit;
    }

    public void OnHit()
    {
        GD.Print("Bonk");
        MixerAiComponent.TaskQueue = [];
        MixerAiComponent.GetTasks();
    }
}
