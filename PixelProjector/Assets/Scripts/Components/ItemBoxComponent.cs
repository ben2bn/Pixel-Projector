namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class ItemBoxComponent : Node, IComponent
{
	[Signal]
	public delegate void OnItemCountChangedEventHandler(int itemCount);
	[Signal]
	public delegate void OnPixelColorChangedEventHandler(Color newColor);

	[Export]
	public int MaxItemCount { get; set; } = int.MaxValue;

	[Export]
	public int ItemCount
	{
		get => itemCount;
		private set
		{
			if (value < 0) return;
			
			itemCount = value;
			EmitSignal(SignalName.OnItemCountChanged, ItemCount);
		}
	}
	private int itemCount = 0;

	[Export]
	public Color PixelColor
	{
		get => pixelColor;
		set
		{
			pixelColor = value;
			EmitSignal(SignalName.OnPixelColorChanged, PixelColor);
		}
	}
	private Color pixelColor = new Color(0, 0, 0);

	[Export]
	public Label ItemCountDisplay { get; private set; }

	[Export]
	public ColorRect ColorDisplay { get; private set; }

	[Export]
	public bool IsFixColor { get; set; } = false;

    public override void _Ready()
    {
        if (!ItemCountDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ItemCountDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!ColorDisplay.IsValidInstance()) GD.PushWarning($"{PropertyName.ColorDisplay} at {GetPath()} should be defined explicitely in editor before running if available.");

        OnItemCountChanged += OnItemCountChange;
		OnPixelColorChanged += OnPixelColorChange;

		OnItemCountChange(ItemCount);
		OnPixelColorChange(PixelColor);
    }

    public bool CanAddAmount(int amount)
	{
		return ItemCount + amount >= 0 && ItemCount + amount <= MaxItemCount;
	}

	public void AddAmount(int amount)
	{
		if (!CanAddAmount(amount)) return;

		ItemCount += amount;
	}

	public void OnItemCountChange(int itemCount)
	{
		if (itemCount <= 0)
		{
			ItemCountDisplay.GetParent<Control>().Visible = false;
			return;
		}

		ItemCountDisplay.GetParent<Control>().Visible = true;
		ColorDisplay.Visible = true;
		ItemCountDisplay.Visible = true;
		ItemCountDisplay.Text = itemCount.ToString();
	}

	public void OnPixelColorChange(Color newColor)
	{
		ColorDisplay.SetColor(newColor);
	}
}
