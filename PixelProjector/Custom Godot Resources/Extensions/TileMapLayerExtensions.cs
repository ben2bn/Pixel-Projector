using Godot;
using System;

public static class TileMapLayerExtensions
{
    /// <summary>
    /// Returns the map coordinates of the cell at the given <paramref name="globalPosition"/>.
    /// </summary>
    public static Vector2I WorldToMap(this TileMapLayer tileMapLayer, Vector2 globalPosition)
    {
        return tileMapLayer.LocalToMap(tileMapLayer.ToLocal(globalPosition));
    }

    /// <summary>
    /// Returns the global coordinates of the cell at the given <paramref name="mapPosition"/>.
    /// </summary>
    public static Vector2 MapToWord(this TileMapLayer tileMapLayer, Vector2I mapPosition)
    {
        return tileMapLayer.ToGlobal(tileMapLayer.MapToLocal(mapPosition));
    }
}
