using Godot;
using System;

public partial class CustomIncrementer : Control
{
    [Signal]
    public delegate void OnColorChangedEventHandler(Color newColor);
    [Signal]
    public delegate void OnCountValueChangedEventHandler(int newValue);

    [Export]
    public Color Color
    {
        get => color;
        set
        {
            color = value;
            EmitSignal(SignalName.OnColorChanged, Color);
        }
    }
    private Color color = new Color(0, 0, 0);

    [Export]
    public int CountValue
    {
        get => countValue;
        set
        {
            if (!IsValidValue(value)) return;

            countValue = value;
            EmitSignal(SignalName.OnCountValueChanged, countValue);
        }
    }
    private int countValue = 0;

    [Export]
    public int ClampMaxCountValue { get; set; } = int.MaxValue;

    [Export]
    public ColorRect ColorDisplay { get; private set; }
    [Export]
    public Label ValueDisplay { get; private set; }
    [Export]
    public Button Increment { get; private set; }
    [Export]
    public Button Decrement { get; private set; }

    public override void _Ready()
    {
        if (!Increment.IsValidInstance()) GD.PushWarning($"{PropertyName.Increment} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!Decrement.IsValidInstance()) GD.PushWarning($"{PropertyName.Decrement} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!ColorDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ColorDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!ValueDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ValueDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");

        Increment.Pressed += OnPressIncrement;
        Decrement.Pressed += OnPressDecrement;
        OnColorChanged += OnColorChange;
        OnCountValueChanged += OnCountValueChange;

        OnColorChange(Color);
        OnCountValueChange(CountValue);
    }

    public bool IsValidValue(int newValue)
    {
        if (newValue < 0) return false;
        if (newValue > ClampMaxCountValue) return false;
        return true;
    }

    public void OnPressIncrement()
    {
        CountValue++;
    }

    public void OnPressDecrement()
    {
        CountValue--;
    }

    public void OnColorChange(Color newColor)
    {
        ColorDisplay.SetColor(newColor);
    }

    public void OnCountValueChange(int newValue)
    {
        ValueDisplay.Text = newValue.ToString();
    }
}
