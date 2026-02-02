using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Consumable : MonoBehaviour
{
    static readonly List<Kind> options = Enum.GetValues(typeof(Kind)).Cast<Kind>().Skip(1).ToList();

    public Kind Type;
    public Sprite[] sprites;

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (Type is Kind.Unset)
            Type = options[SharedRandom.Next(options.Count)];
    }

    void Update()
    {
        int index = (int)Type - 1 + (int)LevelManager.LightLevel * 10;
        sr.sprite = sprites[index];
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
        Unset = 0,
        Megabomb,
        Light,
        Powder,
        Bomb,
        Surprise,
        Atomicbomb,
        KeepForce,
        Pacman,
        Ghost,
        Immortal,
    }
}
