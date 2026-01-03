using System;
using UnityEngine;

public class DeadPlayer : MonoBehaviour
{
    public int Strength;
    public int Bombs;
    public int AtomicBombs;
    public bool KeepForce;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<Player>(out var player))
        {
            player.Eat(this);
            Remove();
        }
    }
    
    public void Remove()
    {
        GridSystem.Current.Remove(gameObject);
        Destroy(gameObject);
    }

}
