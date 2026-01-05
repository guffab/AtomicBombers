using System.Collections;
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
        if (grid.IsOccupied(position, out var objects) && objects.Any(x => x != null && x.TryGetComponent<Explosion>(out _)))
            return true; //in case a horizontal explosion meets a vertical

        foreach (var item in objects)
        {
            //ignite exisiting bomb instead of placing new one for bigger booom
            if (item.TryGetComponent<ExplodingBomb>(out var explodingBomb))
            {
                explodingBomb.Explode(strength, unstoppable);
                return false;
            }

            //"heavy" walls stop explosions
            else if (item.TryGetComponent<Block>(out var block))
            {
                //TODO: stop explosions unless this is an atomic/unstoppable bomb
                SpawnExplosion(grid, position, strength);
                return block.state is Block.State.Solid or Block.State.Walkable or Block.State.HeavyDamaged;
            }
        }

        SpawnExplosion(grid, position, strength);
        return true;

        void SpawnExplosion(GridSystem grid, Vector2Int position, int strength)
        {
            var explosion = Instantiate(ExplosionPrefab, grid.ToWorld(position), Quaternion.identity);
            explosion.GetComponent<Explosion>().Strength = strength;

            grid.Add(explosion, position);
        }
    }
}