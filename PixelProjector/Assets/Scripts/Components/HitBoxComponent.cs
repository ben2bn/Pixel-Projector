namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class HitBoxComponent : Area2D, IComponent
{
    [Signal]
    public delegate void OnHitByHurtBoxComponentEventHandler();

    public override void _EnterTree()
    {
        OnHitByHurtBoxComponent += OnHit;
    }

    public void OnHit()
    {
        GD.Print("hit");
    }
}
