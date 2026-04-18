namespace ComponentSystem;

using Godot;
using System;

[GlobalClass]
public partial class ItemBoxInteractableComponent : InteractableComponent, IComponent, IHasComponentDependency
{
    [Export]
    public ItemBoxComponent ItemBoxComponent { get; private set; }

    public override void _Ready()
    {
        if (!ItemBoxComponent.IsValidInstance()) GD.PushWarning($"{PropertyName.ItemBoxComponent} at {GetPath()} should be defined explicitely in editor before running if available.");

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
            if (!holdPixelComponent.PixelColor.Value.IsEqualApprox(ItemBoxComponent.PixelColor)) return;

            holdPixelComponent.PixelColor = null;
            ItemBoxComponent.AddAmount(1);
        }
        else
        {
            if (ItemBoxComponent.ItemCount < 1) return;
            holdPixelComponent.PixelColor = ItemBoxComponent.PixelColor;
            ItemBoxComponent.AddAmount(-1);
        }
    }

    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!ItemBoxComponent.IsValidInstance())
        {
            ItemBoxComponent = (ItemBoxComponent)componentManager.GetComponentMatch(typeof(ItemBoxComponent)) ?? ItemBoxComponent;
        }
    }
}
