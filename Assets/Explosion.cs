using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Explosion : MonoBehaviour
{
    public int Strength;
    public bool Unstoppable;

    Grid Grid => Grid.Current;

    bool hasKilledPlayer = false;

    void Start()
    {
        LevelManager.Register(this);

        foreach (var element in Grid.GetObjectsAtSamePlace(gameObject).ToList())
        {
            if (element != null && element.TryGetComponent<ExplosiveBase>(out var mine))
                mine.Explode(Strength, Unstoppable);

                #warning small delay?
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<Player>(out var player))
        {
            player.Kill();
            hasKilledPlayer = true;
        }
    }

    //referenced by animation
    public void Hide()
    {
        GetComponent<Collider2D>().enabled = false;
        foreach (var element in Grid.GetObjectsAtSamePlace(gameObject).ToList())
        {
            if (element == null || element.TryGetComponent<Explosion>(out _))
                continue;

            if (element.TryGetComponent<Block>(out var block))
                block.Demolish();

            else if (element.TryGetComponent<Consumable>(out var consumable))
                consumable.Remove();

            else if (element.TryGetComponent<DeadPlayer>(out var deadPlayer) && !hasKilledPlayer)
                deadPlayer.Remove();
        }

        GetComponent<Renderer>().enabled = false;
        Grid.Remove(gameObject);
        LevelManager.Unregister(this);
    }

    //referenced by animation
    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
