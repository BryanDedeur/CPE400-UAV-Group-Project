﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public OrientedPhysics physics;
    public UnitAI ai;
    public Router router;
    public Device device;
    public Battery battery;
    protected int ID;

    /// <summary>
    /// Gets the ID of the entity.
    /// </summary>
    /// <returns> The newly created entity. </returns>
    public int GetID()
    {
        return ID;
    }
}
