namespace ComponentSystem;

using Godot;
using Microsoft.VisualBasic;
using System;

[GlobalClass]
public partial class ReturnBoxInteractableComponent : InteractableComponent, IComponent
{
	[Signal]
	public delegate void OnEnterColorEventHandler(Color color);

    [Export]
    public QuestMaker QuestMaker { get; private set; }

    public override void _Ready()
    {
        if (!QuestMaker.IsValidInstance()) GD.PushWarning($"{PropertyName.QuestMaker} at {GetPath()} should be defined explicitely in editor before running if available.");

        OnInteract += DoInteraction;
    }

    public void DoInteraction(InteractComponent interactComponent)
    {
        ComponentManager componentManager = interactComponent.GetParent<ComponentManager>();
        HoldPixelComponent holdPixelComponent = componentManager.GetComponentMatch(typeof(HoldPixelComponent)) as HoldPixelComponent;
        if (!holdPixelComponent.IsValidInstance()) return;
        if (holdPixelComponent.PixelColor == null) return;

        QuestMaker.OnReceiveColor(holdPixelComponent.PixelColor.Value);
        holdPixelComponent.PixelColor = null;
    }
}
