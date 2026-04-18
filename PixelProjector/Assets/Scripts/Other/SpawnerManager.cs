using ComponentSystem;
using Godot;
using System;

public partial class SpawnerManager : Node
{
	[Export]
	public Control SpawnerIncrements { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!SpawnerIncrements.IsValidInstance()) GD.PushWarning($"{PropertyName.SpawnerIncrements} at {GetPath()} should be defined explicitely in editor before running if available.");

		foreach (Node spawnerIncrement in SpawnerIncrements.GetChildren())
		{
			if (spawnerIncrement is not CustomIncrementer) continue;
			CustomIncrementer incrementer = spawnerIncrement as CustomIncrementer;
			incrementer.OnCountValueChanged += OnChangeCountValueOfIncrementer;
		}
	}

	public void OnChangeCountValueOfIncrementer(Color color, int newValue)
	{
		foreach (Node spawner in GetChildren())
		{
			if (spawner is not StaticBody2D) continue;
			ComponentManager componentManager = spawner.GetNode<ComponentManager>("ComponentManager");
			if (!componentManager.IsValidInstance()) continue;
			ItemBoxComponent itemBoxComponent = componentManager.GetComponentMatch(typeof(ItemBoxComponent)) as ItemBoxComponent;
			SpawnerComponent spawnerComponent = componentManager.GetComponentMatch(typeof(SpawnerComponent)) as SpawnerComponent;
			if (!spawnerComponent.IsValidInstance() || !itemBoxComponent.IsValidInstance()) continue;
			if (!color.IsEqualApprox(itemBoxComponent.PixelColor)) continue;
			spawnerComponent.SpawnRate = newValue;
		}
	}
}
