// meta-description: Base template for components

namespace ComponentSystem;

using _BINDINGS_NAMESPACE_;
using System;

[GlobalClass]
public partial class _CLASS_ : Node, IComponent, IHasComponentDependency
{
    void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
    {
        if (!A.IsValidInstance())
        {
            A = (Type)componentManager.GetComponentMatch(typeof(Type)) ?? A;
        }
    }
}