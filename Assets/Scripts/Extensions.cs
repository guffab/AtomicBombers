using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector2Int Normalize(this Vector2Int vector)
    {
        var tmp = ((Vector2)vector).normalized;
        return new Vector2Int((int) tmp.x, (int) tmp.y);
    }

    public static bool Have<T>(this List<GameObject> objects) where T : MonoBehaviour
    {
        return objects.Have<T>(out _);
    }

    public static bool Have<T>(this List<GameObject> objects, out T tout) where T : MonoBehaviour
    {
        foreach (var item in objects)
            if (item.TryGetComponent<T>(out tout))
                return true;

        tout = default;
        return false;
    }
}