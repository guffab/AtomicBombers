using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplodingBomb : ExplosiveBase
{
    public int strength;
    public float delay;
    public Kind kind;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ExecuteAfterWait(delay));
    }

    IEnumerator ExecuteAfterWait(float duration)
    {
        yield return new WaitForSeconds(duration);
        Explode(strength, kind is Kind.Atomic);
    }

    public enum Kind
    {
        Normal,
        Atomic,
    }
}

public abstract class ExplosiveBase : MonoBehaviour
{
    public GameObject ExplosionPrefab;

    public void Explode(int strength, bool unstoppable = false)
    {
        var grid = GridSystem.Current;
        var currentPos = grid.Remove(gameObject);

        //spawn explosions unless already present
        TryInstantiateExplosion(grid, currentPos, strength, unstoppable);

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.up * i), strength, unstoppable))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.down * i), strength, unstoppable))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.left * i), strength, unstoppable))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.right * i), strength, unstoppable))
                break;

        Destroy(gameObject);
    }

    private bool TryInstantiateExplosion(GridSystem grid, Vector2Int position, int strength, bool unstoppable)
    {
        grid.IsOccupied(position, out var objects);
        bool isExploding = objects.Any(x => x.TryGetComponent<Explosion>(out _));

        if (!Has<Explosion>(objects)) //prevent doing more damage
        {
            var explosion = Instantiate(ExplosionPrefab, grid.ToWorld(position), Quaternion.identity);
            explosion.GetComponent<Explosion>().Strength = strength;
            grid.Add(explosion, position);
        }

        if (Has<Block>(objects, out var block))
            return unstoppable && block.state is Block.State.Solid or Block.State.Walkable;// or Block.State.HeavyDamaged;

        if (unstoppable)
            return true;
        
        return !Has<Player>(objects) && !Has<Consumable>(objects) && !Has<DeadPlayer>(objects);
    }

    static bool Has<T>(List<GameObject> objects) where T : MonoBehaviour => objects.Any(x => x.TryGetComponent<T>(out _));

    static bool Has<T>(List<GameObject> objects, out T tout) where T : MonoBehaviour
    {
        foreach (var item in objects)
            if (item.TryGetComponent<T>(out tout))
                return true;

        tout = default;
        return false;
    }
}