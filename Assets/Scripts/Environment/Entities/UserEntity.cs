using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEntity : Entity
{
    public bool debug;
    public NodeEntity nearestNode;

    private static int count = 0;

    private void Awake()
    {
        ID = count;
        count = count + 1;
    }


    private void Update()
    {
        if (nearestNode != null)
        { 
            if (debug)
            {
                Debug.DrawLine(transform.position, nearestNode.transform.position);
            }
        }
    }
}
