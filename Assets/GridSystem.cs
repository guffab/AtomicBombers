using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem
{
    private Dictionary<Vector2Int, List<GameObject>> GridToObject = new();
    private Dictionary<GameObject, Vector2Int> ObjectToGrid = new();

    [HideInInspector] public static GridSystem Current;
    [HideInInspector] public Vector2Int GridDimensions { get; }

    public GridSystem(Vector2Int gridDimensions)
    {
        Current = this;
        GridDimensions = gridDimensions;
    }

    public void Add(GameObject g, Vector2Int v)
    {
        ObjectToGrid[g] = v;
        if (GridToObject.TryGetValue(v, out var list))
            list.Add(g);
        else
            GridToObject[v] = new List<GameObject> { g };
    }

    public bool IsOccupied(Vector2Int v, out List<GameObject> objects)
    {
        bool success = GridToObject.TryGetValue(v, out objects);
        objects ??= new();
        return success;
    }

    public Vector2Int GetPosition(GameObject g)
    {
        return ObjectToGrid[g];
    }

    public List<GameObject> GetObjectsAtSamePlace(GameObject g)
    {
        if (GridToObject.TryGetValue(ObjectToGrid[g], out var allObjects))
            return allObjects.Where(x => x != g).ToList();
        else
            return new();
    }

    public bool TryMove(Player player, Vector2Int direction)
    {
        var newPos = GetPosition(player.gameObject) + direction;
        if (CanMove(player))
        {
            Remove(player.gameObject);
            Add(player.gameObject, newPos);
            return true;
        }
        return false;

        bool CanMove(Player player)
        {
            if (!IsOccupied(newPos, out var collisions))
                return true;

            return collisions.All(x => !x.TryGetComponent<ExplodingBomb>(out _)) &&
                   collisions.All(x => !x.TryGetComponent<Block>(out var block) || block.state is Block.State.Walkable || (player.appearance is Player.Appearance.Ghost && block.state is Block.State.Solid));
        }
    }

    public Vector2Int Remove(GameObject g)
    {
        var pos = ObjectToGrid[g];
        if (GridToObject.TryGetValue(pos, out var list))
            list.Remove(g);

        ObjectToGrid.Remove(g);
        return pos;
    }

    public Vector3 ToWorld(Vector2Int v)
    {
        var screenPos = new Vector3(v.x * 28, v.y * 28, 10);
        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.y -= .12f; //not sure why all coordinates are wrong by this factor
        return worldPos;
    }
}
