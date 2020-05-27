using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocationsMemory<T>
{
    private int memory_capacity;
    // Start is called before the first frame update
    private Queue<T> forgetting_queue;
    private HashSet<string> _memory; 

    public LocationsMemory(int memory_capacity)
    {
        this.memory_capacity = memory_capacity;
        this.forgetting_queue = new Queue<T>();
        this._memory = new HashSet<string>();
    }

    public void store(T location)
    {
        if(this.forgetting_queue.Count >= this.memory_capacity)
        {
            this.forget();
        }
        this.forgetting_queue.Enqueue(location);
        this._memory.Add(location.ToString());
    }

    public bool remember(T location)
    {
        return this._memory.Contains(location.ToString());
    }
    private void forget()
    {
        T previous_location = this.forgetting_queue.Dequeue();
        this._memory.Remove(previous_location.ToString());
    }
}
