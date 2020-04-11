using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Command
{
    public Entity entity;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Command(Entity ent)
    {
        entity = ent;
    }

    /// <summary>
    /// Initialized information for the AI.
    /// </summary>
    public virtual void Init()
    {

    }

    /// <summary>
    /// Called once per frame to update ai information.
    /// </summary>
    public virtual void Tick()
    {

    }

    /// <summary>
    /// A check called every frame to signify the command is finished.
    /// </summary>
    /// <returns> If the command is finished. </returns>
    public virtual bool IsDone()
    {
        return false;
    }

    /// <summary>
    /// Wraps up actions before destruction.
    /// </summary>
    public virtual void Stop()
    {

    }
}
