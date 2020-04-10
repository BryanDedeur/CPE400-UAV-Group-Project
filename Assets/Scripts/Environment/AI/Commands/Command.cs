﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Command
{
    public Entity entity;

    public Command(Entity ent)
    {
        entity = ent;
    }

    public virtual void Init()
    {

    }

    public virtual void Tick()
    {

    }

    public virtual bool IsDone()
    {
        return false;
    }

    public virtual void Stop()
    {

    }
}
