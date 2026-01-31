using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Consumable : MonoBehaviour
{
    static readonly System.Random random = new();
    static readonly List<Kind> options = Enum.GetValues(typeof(Kind)).Cast<Kind>().Skip(1).ToList();
    public Kind Type;

    void Start()
    {
        if (Type is Kind.Unset)
            Type = options[random.Next(options.Count)];
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<Player>(out var player))
        {
            player.Consume(Type);
            Remove(true);
        }
    }

    public void Remove(bool consumed = false)
    {
        if (Type is Kind.Light)
            LevelManager.ChangeLight(consumed);
        
        Grid.Current.Remove(gameObject);
        Destroy(gameObject);
    }

    public enum Kind
    {
        Unset,
        Light,
        KeepForce,
        Powder,
        Bomb,
        Atomicbomb,
        Megabomb,
        Pacman,
        Immortal,
        Ghost,
        Surprise,
    }
}
