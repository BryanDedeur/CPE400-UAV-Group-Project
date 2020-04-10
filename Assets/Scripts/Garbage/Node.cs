using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
public class Node : MonoBehaviour
{
    public GameObject visual = null;
    public GameObject UAV = null;
    public int row = 0;
    public int col = 0;

    public Dictionary<int, User> users;

    private void Awake()
    {
        users = new Dictionary<int, User>();
    }

    /*
    void Update()
    {
        Dictionary<int, User> tempUserDict = new Dictionary<int, User>(users);
        foreach (KeyValuePair<int, User> userKeyPair in tempUserDict)
        {
            float userDistance = (userKeyPair.Value.transform.position - transform.position).magnitude;
            if (userDistance > ConfigurationMap.inst.connectionRadius / 2f)
            {
                Node bestNode = this;
                List<Node> neighbors = ConfigurationMap.inst.GetNeighbors(row, col);
                foreach (Node node in neighbors)
                {
                    try
                    {
                        float neighborDistance = (userKeyPair.Value.transform.position - node.transform.position).magnitude;
                        if (neighborDistance < userDistance)
                        {
                            userDistance = neighborDistance;
                            bestNode = node;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                if (bestNode != this)
                {
                    bestNode.users.Add(userKeyPair.Key, userKeyPair.Value);
                    userKeyPair.Value.nearestNode = bestNode;

                    users.Remove(userKeyPair.Key);
                }
            }
        }
        visual.transform.localScale = new Vector3(1, .01f, 1) * users.Count;
    }
    
}
*/