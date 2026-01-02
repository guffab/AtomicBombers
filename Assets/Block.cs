using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    public State CurrentState;

    public void Demolish()
    {
        if (CurrentState is State.Persistent)
            return;

        if (CurrentState is State.SlightlyDamaged)
            CurrentState = State.MediumDamaged;

        else if (CurrentState is State.MediumDamaged)
            CurrentState = State.HeavyDamaged;

        else if (CurrentState is State.HeavyDamaged)
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public enum State
    {
        Persistent,
        SlightlyDamaged,
        MediumDamaged,
        HeavyDamaged,
        Solid,
        Walkable,
    }
}
