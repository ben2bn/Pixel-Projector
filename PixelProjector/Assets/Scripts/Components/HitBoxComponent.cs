namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class HitBoxComponent : Area2D, IComponent
{
    public override void _EnterTree()
    {
        //if (!HealthComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.HealthComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }
}
