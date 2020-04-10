using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager inst;
    private void Awake()
    {
        inst = this;
        devices = new Dictionary<int, Device>();
        routers = new Dictionary<int, Router>();
    }

    public float connectionRadius = 1;
    public int totalConnectedUsers = 0;

    public float updateFrequency = 1;
    private float counter = 0.01f;


    private void Start()
    {
        connectionRadius = ConfigurationMap.inst.nodeDistance + 0.1f;
    }

    //public WISP wirelessInternetProvider;
    public Router tower;
    public Dictionary<int, Device> devices;
    public Dictionary<int, Router> routers;

    // display stats of the environment

    public void Update()
    { 
        if (counter <= 0)
        {
            counter = updateFrequency;
            ComputeTransmissionPath_AStar();
        }
        counter -= Time.deltaTime;

        totalConnectedUsers = 0;
        foreach (KeyValuePair<int, Router> router in routers)
        {
            totalConnectedUsers += router.Value.connectedDevices.Count;
        }
    }

    public void GetNearbyRouters(Router focusRouter)
    {

        if (focusRouter == null)
        {
            return;
        }

        focusRouter.connectedRouters.Clear();

        if (focusRouter.name == "Tower" || (focusRouter.entity != null && focusRouter.entity.battery != null && focusRouter.entity.battery.batteryLife > focusRouter.entity.battery.batteryReserveThreshold))
        {
            foreach (KeyValuePair<int, Router> router in routers)
            {
                if (router.Value == focusRouter)
                {
                    continue;
                }
                if ((router.Value.transform.position - focusRouter.transform.position).magnitude <= connectionRadius)
                {
                    focusRouter.connectedRouters.Add(router.Key, router.Value);
                }
            }
        }
    }

    public struct NodeEntity_Astar
    {
        public float pathCost;
        public float heuristicCost;

        public NodeEntity_Astar(float new_pathCost, float new_heuristicCost)
        {
            pathCost = new_pathCost;
            heuristicCost = new_heuristicCost;
        }
    }

    public void ComputeTransmissionPath_AStar()
    {
        /// Compute the shortest connection path between the tower and each router of UAV using A* algorithm.

        // Append each router to A* graph node dictionary.
        Dictionary<int, NodeEntity_Astar> unvisitedNodes = new Dictionary<int, NodeEntity_Astar>();
        foreach (KeyValuePair<int, Router> router in NetworkManager.inst.routers)
        {
            if (router.Value.name == "Tower")
            {
                unvisitedNodes.Add(router.Value.GetID(), new NodeEntity_Astar(0f, 0f)); // Tower is the starting point (zero cost).
            }
            else
            {
                // Reset router connection information.
                router.Value.AStarConnectionLength = Mathf.Infinity;
                router.Value.parentRouter = null;
                router.Value.numberOfHops = 0;

                unvisitedNodes.Add(router.Value.GetID(), new NodeEntity_Astar(Mathf.Infinity, Mathf.Infinity)); // Initialize all to infinite cost.
            }
        }

        // While there are unvisited nodes.
        while (unvisitedNodes.Count > 0)
        {
            // Find the lowest cost node.
            float lowestHeuristicCost = Mathf.Infinity;
            int currentID = -1;
            foreach (KeyValuePair<int, NodeEntity_Astar> entry in unvisitedNodes)
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
            NodeEntity_Astar currentNodeEntity = unvisitedNodes[currentID];
            unvisitedNodes.Remove(currentID);
            Router currentRouter = NetworkManager.inst.routers[currentID];

            // For each neighboring nodes of the visiting node.
            foreach (KeyValuePair<int, Router> consideringRouter in currentRouter.connectedRouters)
            {
                // If neighboring node is unvisited.
                if (unvisitedNodes.ContainsKey(consideringRouter.Key))
                {
                    // Compute new cost.
                    float newPathCost = currentNodeEntity.pathCost + Vector3.Distance(consideringRouter.Value.transform.position, currentRouter.transform.position) + consideringRouter.Value.connectedDevices.Count;
                    float newHeuristicCost = newPathCost + consideringRouter.Value.numberOfHops;

                    // If the new cost is smaller than the cost of the neighboring node.
                    if (newHeuristicCost < unvisitedNodes[consideringRouter.Key].heuristicCost)
                    {
                        // Update the cost.
                        NodeEntity_Astar nodeUnderConsideration = new NodeEntity_Astar(newPathCost, newHeuristicCost);
                        unvisitedNodes[consideringRouter.Key] = nodeUnderConsideration;

                        // Update connection information.
                        consideringRouter.Value.parentRouter = currentRouter;
                        consideringRouter.Value.AStarConnectionLength = newPathCost;
                        consideringRouter.Value.numberOfHops = currentRouter.numberOfHops + 1;
                    }
                }
            }
        }

    }
}
