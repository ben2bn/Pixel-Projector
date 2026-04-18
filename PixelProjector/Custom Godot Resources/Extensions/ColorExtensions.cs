namespace ComponentSystem;

using Godot;
using System;

public static class ColorExtensions
{
    public static bool IsPrimaryRGB(this Color color)
    {
        int fullCount = 0;
        if (color.R == 1) fullCount++;
        if (color.G == 1) fullCount++;
        if (color.B == 1) fullCount++;

        return fullCount == 1;
    } 
}
