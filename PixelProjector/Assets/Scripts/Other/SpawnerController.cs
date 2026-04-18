using Godot;
using System;

public partial class SpawnerController : Control
{
    [Signal]
    public delegate void OnMaxCountValueChangedEventHandler(int newValue);

    [Export]
    public int MaxCountValue
    {
        get => maxCountValue;
        set
        {
            if (maxCountValue < 0) return;

            maxCountValue = value;
            EmitSignal(SignalName.OnMaxCountValueChanged, maxCountValue);
        }
    }
    private int maxCountValue = 5;

    [Export]
    public Label ValueLeftDisplay { get; private set; }

    [Export]
    public Control SpawnerIncrements { get; private set; }

    public override void _Ready()
    {
        if (!ValueLeftDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ValueLeftDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!SpawnerIncrements.IsValidInstance()) GD.PushWarning($"{PropertyName.SpawnerIncrements} at {GetPath()} should be defined explicitely in editor before running if available.");

        AssignOnUsedCountValueChangeToSignals();
        OnMaxCountValueChanged += OnUsedCountValueChange;
        OnUsedCountValueChange(MaxCountValue);
    }

    private void AssignOnUsedCountValueChangeToSignals()
    {
        foreach (Node spawnerIncrement in SpawnerIncrements.GetChildren())
        {
            if (spawnerIncrement is not CustomIncrementer) continue;

            CustomIncrementer incrementer = spawnerIncrement as CustomIncrementer;
            incrementer.OnCountValueChanged += OnUsedCountValueChange;
        }
    }

    public void OnUsedCountValueChange(Color color, int value)
    {
        OnUsedCountValueChange(value);
    }

    public void OnUsedCountValueChange(int value)
    {
        int usedTotal = GetCountValueInSpawnerIncrements();
        int leftCount = MaxCountValue - usedTotal;
        ValueLeftDisplay.Text = leftCount.ToString();
        ClampCountValueInSpawnerIncrements(leftCount);
    }

    public int GetCountValueInSpawnerIncrements()
    {
        int total = 0;
        foreach (Node spawnerIncrement in SpawnerIncrements.GetChildren())
        {
            if (spawnerIncrement is not CustomIncrementer) continue;

            total += (spawnerIncrement as CustomIncrementer).CountValue;
        }
        return total;
    }

    public void ClampCountValueInSpawnerIncrements(int leftCount)
    {
        foreach (Node spawnerIncrement in SpawnerIncrements.GetChildren())
        {
            if (spawnerIncrement is not CustomIncrementer) continue;

            CustomIncrementer incrementer = spawnerIncrement as CustomIncrementer;
            incrementer.ClampMaxCountValue = incrementer.CountValue + leftCount;
        }
    }
}
