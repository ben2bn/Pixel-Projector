namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class HoldPixelComponent : ColorRect, IComponent
{
    [Signal]
    public delegate void OnPixelColorChangedEventHandler();

    public Color? PixelColor
    {
        get => pixelColor;
        set
        {
            pixelColor = value;
            EmitSignal(SignalName.OnPixelColorChanged);
        }
    }
    private Color? pixelColor = null;

    public override void _Ready()
    {
        OnPixelColorChanged += OnPixelColorChange;
    }

    public void OnPixelColorChange()
    {
        if (!PixelColor.HasValue) Visible = false;
        else
        {
            SetColor(PixelColor.Value);
            Visible = true;
        }
    }
}
