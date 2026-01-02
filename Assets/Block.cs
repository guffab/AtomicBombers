using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    public State CurrentState;

    public void Demolish()
    {
        
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
        DurableFull,
        DurableHalf,
        DurableLittle,
        Solid,
        Walkable,
    }
}
