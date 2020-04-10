using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAVEntity : Entity
{
    public NodeEntity assignedNode;

    private static int count = 0;

    private void Awake()
    {
        ID = count;
        count = count + 1;
    }
}

