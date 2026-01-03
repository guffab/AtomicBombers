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
        SetStyle();
    }

    private void SetStyle()
    {
        int index = (int)state + 6 * (Math.Min(Math.Abs(style), 10) - 1);
        sr.sprite = sprites[index];
    }

    public void Demolish()
    {
        if (state is State.Indestructible)
            return;

        if (state is State.SlightlyDamaged)
        {
            state = State.MediumDamaged;
            SetStyle();
        }

        else if (state is State.MediumDamaged)
        {
            state = State.HeavyDamaged;
            SetStyle();
        }

        else if (state is State.HeavyDamaged)
        {
            GridSystem.Current.Remove(gameObject);
            Destroy(gameObject);
        }

        else
        {
            var gridPos = GridSystem.Current.Remove(gameObject);
            LevelManager.Instance.PlaceNewElement(gridPos);
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
