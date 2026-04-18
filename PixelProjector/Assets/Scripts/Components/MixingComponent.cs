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

    public Color? FirstColor
	{
		get => firstColor;
		private set
		{
            if (ItemBoxComponent.ItemCount != 0) return;
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
            if (ItemBoxComponent.ItemCount != 0) return;
			GD.Print(value.ToString());
            secondColor = value;
            EmitSignal(SignalName.OnChangedColorInMixer);
        }
    }
    private Color? secondColor = null;

    public override void _Ready()
    {
        if (!ColorDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ColorDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!ItemBoxComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.ItemBoxComponent} at {GetPath()} should be defined explicitely in editor before running if available.");

		ItemBoxComponent.MaxItemCount = 1;
		ItemBoxComponent.IsFixColor = false;
        OnChangedColorInMixer += OnChangeColorInMixer;

		FirstColor = new Color(255, 0, 0);
		SecondColor = new Color(0, 255, 0);
    }

    public void OnChangeColorInMixer()
	{
		if (firstColor.HasValue)
		{
			ColorDisplay.SetColor(firstColor.Value);
		}
		else if (secondColor.HasValue)
		{
			ColorDisplay.SetColor(secondColor.Value);
		}

		MixColors();
	}

	private void MixColors()
	{
		if (!firstColor.HasValue || !secondColor.HasValue) return;
		if (ItemBoxComponent.ItemCount != 0) return;

        Color newColor = new Color(Mathf.Clamp(firstColor.Value.R + secondColor.Value.R, 0, 255), Mathf.Clamp(firstColor.Value.G + secondColor.Value.G, 0, 255), Mathf.Clamp(firstColor.Value.B + secondColor.Value.B, 0, 255), Mathf.Clamp(firstColor.Value.A + secondColor.Value.A, 0, 1));
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
