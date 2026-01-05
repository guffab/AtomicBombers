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
        InstantiateExplosion(grid, currentPos, strength);
        for (int i = 1; i <= strength; i++)
        {
            #warning stop at hard wall per direction
            //TODO: how to stop in one direction if a hard wall is hit? 
            InstantiateExplosion(grid, currentPos + (Vector2Int.up * i), strength);
            InstantiateExplosion(grid, currentPos + (Vector2Int.down * i), strength);
            InstantiateExplosion(grid, currentPos + (Vector2Int.left * i), strength);
            InstantiateExplosion(grid, currentPos + (Vector2Int.right * i), strength);
        }
        Destroy(gameObject);

        void InstantiateExplosion(GridSystem grid, Vector2Int position, int strength)
        {
            if ((uint)position.x > grid.GridDimensions.x || (uint)position.y > grid.GridDimensions.y)
                return;

            if (grid.IsOccupied(position, out var objects) && objects.Any(x => x != null && x.TryGetComponent<Explosion>(out _)))
                return;

            foreach (var item in objects)
            {
                if (item.TryGetComponent<ExplodingBomb>(out var explodingBomb))
                {
                    explodingBomb.Explode(strength, unstoppable);
                    return;
                }
            }
            
            var explosion = Instantiate(ExplosionPrefab, grid.ToWorld(position), Quaternion.identity);
            explosion.GetComponent<Explosion>().Strength = strength;

            grid.Add(explosion, position);
        }
    }
}