namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class MixingComponent : Node, IComponent, IHasComponentDependency
{
	[Signal]
	public delegate void OnChangedColorInMixerEventHandler();

	[Export]
	public ItemBoxComponent ItemBoxComponent { get; set; }

    [Export]
    public ColorRect ColorDisplay { get; private set; }

    [Export]
    public Label ItemCountDisplay { get; private set; }

    public Color? FirstColor
	{
		get => firstColor;
		private set
		{
			if (value.HasValue && !value.Value.IsPrimaryRGB()) return;
            if (value != null && ItemBoxComponent.ItemCount != 0) return;
            firstColor = value;
			EmitSignal(SignalName.OnChangedColorInMixer);
		}
	}
	private Color? firstColor = null;

    public Color? SecondColor
    {
        get => secondColor;
        private set
        {
            if (value.HasValue && !value.Value.IsPrimaryRGB()) return;
            if (value != null && ItemBoxComponent.ItemCount != 0) return;
            secondColor = value;
            EmitSignal(SignalName.OnChangedColorInMixer);
        }
    }
    private Color? secondColor = null;

	public bool HasColor => firstColor.HasValue || secondColor.HasValue;

    public override void _Ready()
    {
        if (!ColorDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ColorDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!ItemCountDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ItemCountDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!ItemBoxComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.ItemBoxComponent} at {GetPath()} should be defined explicitely in editor before running if available.");

		ItemBoxComponent.MaxItemCount = 1;
		ItemBoxComponent.IsFixColor = false;
        OnChangedColorInMixer += OnChangeColorInMixer;
    }

	public void AddColorToMixer(Color color)
	{
		if (!firstColor.HasValue) FirstColor = color;
		else if (!secondColor.HasValue) SecondColor = color;
	}

	public Color? GetOneColorAndRemoveFromMixer()
	{
		Color? result = null;
		if (secondColor.HasValue) { result = secondColor; secondColor = null; }
		else if (firstColor.HasValue) { result = firstColor; firstColor = null; }
		return result;
    }

    public void OnChangeColorInMixer()
	{
		if (FirstColor.HasValue)
		{
			ColorDisplay.GetParent<Control>().Visible = true;
			ItemCountDisplay.Visible = false;
			ColorDisplay.Color = FirstColor.Value;
		}
		else if (SecondColor.HasValue)
		{
            ColorDisplay.GetParent<Control>().Visible = true;
			ItemCountDisplay.Visible = false;
            ColorDisplay.Color = SecondColor.Value;
		}
		else ColorDisplay.Visible = false;

		MixColors();
	}

	private void MixColors()
	{
		if (!FirstColor.HasValue || !SecondColor.HasValue) return;
		if (ItemBoxComponent.ItemCount != 0) return;

		Color newColor = new Color(Mathf.Clamp(FirstColor.Value.R + SecondColor.Value.R, 0, 1), Mathf.Clamp(FirstColor.Value.G + SecondColor.Value.G, 0, 1), Mathf.Clamp(FirstColor.Value.B + SecondColor.Value.B, 0, 1), Mathf.Clamp(FirstColor.Value.A + SecondColor.Value.A, 0, 1));

        FirstColor = null;
        SecondColor = null;

        ItemBoxComponent.PixelColor = newColor;
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
