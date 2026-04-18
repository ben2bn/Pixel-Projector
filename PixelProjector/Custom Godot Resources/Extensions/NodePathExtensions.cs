using Godot;
using System;

public static class NodePathExtensions
{
    public static NodePath Slice(this NodePath nodePath, int startIndex, int endIndex = int.MaxValue)
    {
        int nameCount = nodePath.GetNameCount();
        if (nameCount == 0) return null;

        startIndex = Math.Clamp(startIndex, 0, nameCount);
        endIndex = Math.Clamp(endIndex, 0, nameCount);

        string result = (startIndex == 0 && nodePath.GetName(0) == "root") ? "/" : "";
        for (int nodeIndex = startIndex; nodeIndex < endIndex; nodeIndex++)
        {
            result += (nodePath.GetName(nodeIndex) + "/");
        }

        return result.AsNodePath();
    }

    public static NodePath GetParentNodePath(this NodePath nodePath)
    {
        NodePath parentPath = nodePath.Slice(0, nodePath.GetNameCount() - 1);
        return parentPath;
    }

    public static NodePath GetRelativePath(this NodePath targetPath, NodePath reference)
    {
        if (reference == null) return null;

        NodePath pathToReferenceFromTargetPath = targetPath.Slice(0, reference.GetNameCount());
        if (pathToReferenceFromTargetPath != reference) return null;

        return targetPath.Slice(reference.GetNameCount());
    }

    public static Node GetParentOfPath(this NodePath nodePath)
    {
        NodePath parentPath = nodePath.GetParentNodePath();
        if (parentPath == null) return null;

        SceneTree tree = Engine.GetMainLoop() as SceneTree;
        Node nodeParent = tree.Root.GetNode(parentPath);
        return nodeParent;
    }

    public static NodePath AsNodePath(this string path)
    {
        return new NodePath(path);
    }
}