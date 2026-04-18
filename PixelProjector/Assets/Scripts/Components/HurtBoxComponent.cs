namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class HurBoxtComponent : Area2D, IComponent
{
    public override void _Ready()
    {
        AreaEntered += OnHitBoxComponentEnter;
    }

    public void OnHitBoxComponentEnter(Area2D enteredArea)
    {
        if (!enteredArea.IsValidInstance() || enteredArea is not HitBoxComponent) return;

        HitBoxComponent hitBoxComponent = (HitBoxComponent)enteredArea;
        //hitBoxComponent.HealthComponent.RemoveToCurrentHealth(Damage);
    }
}
