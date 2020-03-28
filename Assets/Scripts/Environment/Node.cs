using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : MonoBehaviour
{
    public GameObject visual = null;
    public GameObject UAV = null;
    public int row = 0;
    public int col = 0;

    public ConfigurationMap cm;
    public List<User> users;

    private void Awake()
    {
        users = new List<User>();
    }

    void Update()
    {
        for (int i = 0; i < users.Count; ++i)
        {
            float userDistance = (users[i].transform.position - transform.position).magnitude;
            if (userDistance > cm.connectionRadius / 2f)
            {
                Node bestNode = this;
                List<Node> neighbors = cm.GetNeighbors(row, col);
                foreach (Node node in neighbors)
                {
                    try
                    {
                        float neighborDistance = (users[i].transform.position - node.transform.position).magnitude;
                        if (neighborDistance < userDistance)
                        {
                            userDistance = neighborDistance;
                            bestNode = node;
                        }
                    } catch (Exception e)
                    {
                        
                    }

                }
                if (bestNode != this)
                {
                    bestNode.users.Add(users[i]);
                    users.Remove(users[i]);
                } else
                {
                    Debug.DrawLine(transform.position, users[i].transform.position, new Color(.5f, .5f, 0f));
                }
            } else
            {
                Debug.DrawLine(transform.position, users[i].transform.position, new Color(.5f, .5f, 0f));
            }

        }
        visual.transform.localScale = new Vector3(1, .01f, 1) * users.Count;
    }
}
