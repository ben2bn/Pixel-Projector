using Godot;
using System;

public static class NodeExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if the node is neither <see langword="null"/>, queued for deletion or <see cref="GodotObject.IsInstanceValid(GodotObject?)"/> is <see langword="false"/>.
    /// </summary>
    public static bool IsValidInstance(this Node node)
    {
        return GodotObject.IsInstanceValid(node) && !node.IsQueuedForDeletion();
    }

    /// <summary>
    /// Returns the <paramref name="node"/> if it is valid through <see cref="IsValidInstance(Node)"/>, otherwise returns <see langword="null"/>.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static Node NullIfInvalid(this Node node)
    {
        return node.IsValidInstance() ? node : null;
    }

    /// <summary>
    /// Only frees the <paramref name="node"/> if it is not already queued for deletion.
    /// </summary>
    public static void SafeQueueFree(this Node node)
    {
        if (!node.IsValidInstance()) return;
        node.QueueFree();
    }

    /// <summary>
    /// Returns the <see cref="Node"/> from <paramref name="nodePath"/> relative to the parent of <paramref name="node"/>.
    /// </summary>
    public static Node GetSiblingNode(this Node node, NodePath nodePath)
    {
        return node.GetParentOrNull<Node>()?.GetNodeOrNull(nodePath);
    }

    /// <summary>
    /// <para>Returns the <see cref="Node"/> of specified <paramref name="type"/> that is a sibling of <paramref name="node"/> (i.e. is a child of the same parent, but not the same <see cref="Node"/>).</para>
    /// <para>If <paramref name="matchExactType"/> is set to <see langword="true"/> (by default), then the sibling's <see cref="Type"/> must be the same as <paramref name="type"/>, otherwise if <see langword="false"/> the <see cref="Node"/> found can be of a type derived (directly or indirectly) from <paramref name="type"/>.</para>
    /// </summary>
    public static Node GetSiblingNode(this Node node, Type type, bool matchExactType = true)
    {
        Node parent = node.GetParentOrNull<Node>();
        foreach (Node sibling in parent.GetChildren())
        {
            if (sibling == node) continue;

            if ((type == sibling.GetType()) ||
                (type.IsAssignableFrom(sibling.GetType()) && !matchExactType))
            {
                return sibling;
            }
        }

        return null;
    }
}