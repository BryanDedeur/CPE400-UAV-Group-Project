using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float connectionLength = 0f;
    public int numberOfHops = 0;
    public NetworkRouter parentRouter = null;
    private int userServing = 0;

    private struct Node_Astar
    {
        public float pathCost;
        public float heuristicCost;

        public Node_Astar(float new_pathCost, float new_heuristicCost)
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
        } 
        else
        {
            userServing = 0;
        }

        cm.GetNearbyRouters(this);
        displayingConnectedRouters = connectedRouters.Values.ToList();

        foreach (KeyValuePair<int, NetworkRouter> connection in connectedRouters)
        {
            Debug.DrawLine(transform.position, connection.Value.transform.position, new Color(.5f,.5f,1f));
        }
    }

    public void ComputeTransmissionPath_AStar()
    {
        /// Compute the shortest connection path between the tower and each router of UAV using A* algorithm.

        // Append each router to A* graph node dictionary.
        Dictionary<int, Node_Astar> unvisitedNodes = new Dictionary<int, Node_Astar>();
        foreach (KeyValuePair<int, NetworkRouter> router in cm.allRouters)
        {
            if (router.Value.name == "Tower")
            {
                unvisitedNodes.Add(router.Value.GetID(), new Node_Astar(0f, 0f)); // Tower is the starting point (zero cost).
            }
            else
            {
                // Reset router connection information.
                router.Value.connectionLength = Mathf.Infinity;
                router.Value.parentRouter = null;
                router.Value.numberOfHops = 0;

                unvisitedNodes.Add(router.Value.GetID(), new Node_Astar(Mathf.Infinity, Mathf.Infinity)); // Initialize all to infinite cost.
            }
        }

        // While there are unvisited nodes.
        while(unvisitedNodes.Count > 0)
        {
            // Find the lowest cost node.
            float lowestHeuristicCost = Mathf.Infinity;
            int currentID = -1;
            foreach (KeyValuePair<int , Node_Astar> entry in unvisitedNodes)
            {
                if (entry.Value.heuristicCost < lowestHeuristicCost)
                {
                    lowestHeuristicCost = entry.Value.heuristicCost;
                    currentID = entry.Key;
                }
            }

            // If all of the nodes in unvisited node dictionary has infinite cost, 
            // that means all of the unvisited nodes do not have connection to the tower.
            if (currentID == -1)
            {
                break;
            }

            // Remove the visiting node from unvisited node dicionary.
            Node_Astar currentNode = unvisitedNodes[currentID];
            unvisitedNodes.Remove(currentID);
            NetworkRouter currentRouter = cm.allRouters[currentID];

            // For each neighboring nodes of the visiting node.
            foreach (KeyValuePair<int, NetworkRouter> consideringRouter in currentRouter.connectedRouters)
            {
                // If neighboring node is unvisited.
                if (unvisitedNodes.ContainsKey(consideringRouter.Key))
                {
                    // Compute new cost.
                    float newPathCost = currentNode.pathCost + Vector3.Distance(consideringRouter.Value.transform.position, currentRouter.transform.position) + consideringRouter.Value.userServing;
                    float newHeuristicCost = newPathCost + consideringRouter.Value.numberOfHops;

                    // If the new cost is smaller than the cost of the neighboring node.
                    if (newHeuristicCost < unvisitedNodes[consideringRouter.Key].heuristicCost)
                    {
                        // Update the cost.
                        Node_Astar nodeUnderConsideration = new Node_Astar(newPathCost, newHeuristicCost);
                        unvisitedNodes[consideringRouter.Key] = nodeUnderConsideration;
                        
                        // Update connection information.
                        consideringRouter.Value.parentRouter = currentRouter;
                        consideringRouter.Value.connectionLength = newPathCost;
                        consideringRouter.Value.numberOfHops = currentRouter.numberOfHops + 1;
                    }
                }
            }
        }
    }
}
