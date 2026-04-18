namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class MixerBoxInteractableComponent : InteractableComponent, IComponent, IHasComponentDependency
{
    [Export]
    public ItemBoxComponent ItemBoxComponent { get; private set; }

    [Export]
    public MixingComponent MixingComponent { get; private set; }

    public override void _Ready()
    {
        if (!ItemBoxComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.ItemBoxComponent} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!MixingComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.MixingComponent} at {GetPath()} should be defined explicitely in editor before running if available.");

        OnInteract += DoInteraction;
    }

    public void DoInteraction(InteractComponent interactComponent)
    {
        if (!interactComponent.IsValidInstance()) return;

        ComponentManager componentManager = interactComponent.GetParent<ComponentManager>();
        HoldPixelComponent holdPixelComponent = componentManager.GetComponentMatch(typeof(HoldPixelComponent)) as HoldPixelComponent;
        if (!holdPixelComponent.IsValidInstance()) return;
        if (holdPixelComponent.PixelColor.HasValue)
        {
            if (ItemBoxComponent.ItemCount == 1) return;
            if (MixingComponent.HasColor || holdPixelComponent.PixelColor.Value.IsPrimaryRGB())
            {
                MixingComponent.AddColorToMixer(holdPixelComponent.PixelColor.Value);
                holdPixelComponent.PixelColor = null;
            }
            else
            {
                ItemBoxComponent.PixelColor = holdPixelComponent.PixelColor.Value;
                ItemBoxComponent.AddAmount(1);
                holdPixelComponent.PixelColor = null;
            }
        }
        else
        {
            if (ItemBoxComponent.ItemCount == 1)
            {
                holdPixelComponent.PixelColor = ItemBoxComponent.PixelColor;
                ItemBoxComponent.AddAmount(-1);
            }
            else if (MixingComponent.HasColor)
            {
                Color? output = MixingComponent.GetOneColorAndRemoveFromMixer();
                if (!output.HasValue) return;
                holdPixelComponent.PixelColor = output.Value;
            }
        }
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!ItemBoxComponent.IsValidInstance())
        {
            ItemBoxComponent = (ItemBoxComponent)componentManager.GetComponentMatch(typeof(ItemBoxComponent)) ?? ItemBoxComponent;
        }
        if (!MixingComponent.IsValidInstance())
        {
            MixingComponent = (MixingComponent)componentManager.GetComponentMatch(typeof(MixingComponent)) ?? MixingComponent;
        }
    }
}
