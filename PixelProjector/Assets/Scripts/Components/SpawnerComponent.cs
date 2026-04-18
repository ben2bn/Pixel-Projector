namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class SpawnerComponent : Node, IComponent, IHasComponentDependency
{
    [Export]
    public ItemBoxComponent ItemBoxComponent { get; private set; }

	[Export]
	public float SpawnTime
    {
        get => spawnTime;
        set
        {
            if (value < 0.1) return;

            spawnTime = value;
            if (timer.IsValidInstance()) timer.WaitTime = SpawnTime;
            else SetTimer();
        }
    }
    private float spawnTime = 1f;

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
        timer.WaitTime = SpawnTime;

        if (!timer.IsInsideTree())
        { 
            timer.Timeout += SpawnPixel;
            AddChild(timer);
        }
    }

    public void SpawnPixel()
    {
        ItemBoxComponent.AddAmount(1);
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!ItemBoxComponent.IsValidInstance())
        {
            ItemBoxComponent = (ItemBoxComponent)componentManager.GetComponentMatch(typeof(ItemBoxComponent)) ?? ItemBoxComponent;
        }
    }
}
