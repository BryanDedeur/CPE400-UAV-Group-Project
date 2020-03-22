using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRouter : MonoBehaviour
{
    static private int maximumUserCapacity = 5;
    static private float userConnectionDistance = 10f;
    static private int IDcounter = 0;

    public ConfigurationMap cm;
    public Dictionary<int, NetworkRouter> connectedRouters;
    public List<NetworkRouter> displayingConnectedRouters;
    private LineRenderer lr;

    private int ID;
    public int connectionLength = 0;
    public NetworkRouter parentRouter = null;
    private int userServing = 0;

    private struct Node_Astar
    {
        public int pathCost;
        public int heuristicCost;

        public Node_Astar(int new_pathCost, int new_heuristicCost)
        {
            pathCost = new_pathCost;
            heuristicCost = new_heuristicCost;
        }
    }

    public int GetID()
    {
        return ID;
    }

    void Start()
    {
        connectedRouters = new Dictionary<int, NetworkRouter>();
        displayingConnectedRouters = new List<NetworkRouter>();
        ID = IDcounter++;
        cm.allRouters.Add(this.ID, this);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.parent != null)
        {
            userServing = gameObject.GetComponentInParent<Node>().numberUsers;
        } else
        {
            userServing = 0;
        }
        displayingConnectedRouters.Clear();
        cm.GetNearbyRouters(this);
        foreach (KeyValuePair<int, NetworkRouter> connection in connectedRouters)
        {
            displayingConnectedRouters.Add(connection.Value);
            Debug.DrawLine(transform.position, connection.Value.transform.position, Color.red);
        }
        ComputeTransmissionPath_AStar();
    }

    void ComputeTransmissionPath_AStar()
    {
        Dictionary<int, Node_Astar> unvisitedNodes = new Dictionary<int, Node_Astar>();

        foreach (KeyValuePair<int, NetworkRouter> router in cm.allRouters)
        {
            if (router.Value.name == "Tower")
            {
                unvisitedNodes.Add(router.Value.GetID(), new Node_Astar(0, 0));
            }
            else
            {
                unvisitedNodes.Add(router.Value.GetID(), new Node_Astar(int.MaxValue, int.MaxValue));
            }
        }

        while(unvisitedNodes.Count > 0)
        {
            int lowestHeuristicCost = int.MaxValue;
            int currentID = -1;
            foreach (KeyValuePair<int , Node_Astar> entry in unvisitedNodes)
            {
                if (entry.Value.heuristicCost < lowestHeuristicCost)
                {
                    lowestHeuristicCost = entry.Value.heuristicCost;
                    currentID = entry.Key;
                }
            }

            if (currentID == -1)
            {
                break;
            }

            Node_Astar currentNode = unvisitedNodes[currentID];
            unvisitedNodes.Remove(currentID);
            foreach (KeyValuePair<int, NetworkRouter> consideringRouter in cm.allRouters[currentID].connectedRouters)
            {
                if (unvisitedNodes.ContainsKey(consideringRouter.Key))
                {
                    int newPathCost = currentNode.pathCost + 1;
                    int newHeuristicCost = newPathCost + consideringRouter.Value.userServing;
                    if (newHeuristicCost < unvisitedNodes[consideringRouter.Key].heuristicCost)
                    {
                        Node_Astar nodeUnderConsideration = new Node_Astar(newPathCost, newHeuristicCost);
                        unvisitedNodes[consideringRouter.Key] = nodeUnderConsideration;
                        consideringRouter.Value.parentRouter = cm.allRouters[currentID];
                        consideringRouter.Value.connectionLength = newPathCost;
                    }
                }
            }
        }
    }
}
