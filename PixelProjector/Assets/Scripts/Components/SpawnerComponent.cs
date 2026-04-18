namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class SpawnerComponent : Node, IComponent, IHasComponentDependency
{
    [Export]
    public ItemBoxComponent ItemBoxComponent { get; private set; }

    [Export]
    public int SpawnRate { get; set; } = 1;


	private Timer timer;

    public override void _EnterTree()
    {
        if (!ItemBoxComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.ItemBoxComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
    }


    public override void _Ready()
    {
        SetTimer();
    }

    public void SetTimer()
    {
        if (timer == null)
        {
            timer = new Timer();
        }

        timer.Autostart = true;
        timer.OneShot = false;
        timer.WaitTime = 5f;

        if (!timer.IsInsideTree())
        { 
            timer.Timeout += SpawnPixel;
            AddChild(timer);
        }
    }

    public void SpawnPixel()
    {
        ItemBoxComponent.AddAmount(SpawnRate);
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!ItemBoxComponent.IsValidInstance())
        {
            ItemBoxComponent = (ItemBoxComponent)componentManager.GetComponentMatch(typeof(ItemBoxComponent)) ?? ItemBoxComponent;
        }
    }
}
