using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    public Sprite[] sprites;
    public State state;
    public int style;

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        int styleOffset = 6 * (Math.Min(Math.Abs(style), 10) - 1);
        int lightOffset = (int)LevelManager.LightLevel * (sprites.Length / 3);
        int index = (int)state + styleOffset + lightOffset;

        sr.sprite = sprites[index];
    }

    public void Demolish()
    {
        if (state is State.Indestructible)
            return;

        if (state is State.SlightlyDamaged)
            state = State.MediumDamaged;

        else if (state is State.MediumDamaged)
            state = State.HeavyDamaged;

        else if (state is State.HeavyDamaged)
        {
            Grid.Current.Remove(gameObject);
            Destroy(gameObject);
        }

        else
        {
            var gridPos = Grid.Current.Remove(gameObject);
            LevelManager.PlaceNewElement(gridPos);
            Destroy(gameObject);
        }
    }

    public enum State
    {
        Indestructible,
        Solid,
        Walkable,
        SlightlyDamaged,
        MediumDamaged,
        HeavyDamaged,
    }
}
