using ComponentSystem;

using Godot;
using System;

/// <summary>
/// Interface used for classes that have a <see cref="IComponent"/> dependency that should be updated on runtime when a new <see cref="IComponent"/> is added as a sibling <see cref="Node"/> of the same <see cref="ComponentManager"/>.
/// </summary>
public interface IHasComponentDependency
{
    /// <summary>
    /// <para>Checks for specified component dependencies within the class and tries to find matching component from <see cref="ComponentManager.GetComponentMatch(Type, bool)"/> to fulfill the dependency.</para>
    /// <para>This is generally called from <see cref="ComponentManager.OnComponentChildrenCountChanged"/> called when a component is added, removed or when starting the scene.</para>
    /// </summary>
    public abstract void TryUpdateComponentDependencies(ComponentManager componentManager);
    /*example:
    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!HealthComponent.IsValidInstance()) 
        {
            HealthComponent = (HealthComponent)componentManager.GetComponentMatch(typeof(HealthComponent)) ?? HealthComponent;
        }
    }
    */
}
