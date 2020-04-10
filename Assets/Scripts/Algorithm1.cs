using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coordinate = ConfigurationMap.Coordinate;

public class Algorithm1 : MonoBehaviour
{
    public List<NodeEntity> plannedNodeEntitys;
    public int highestPlannedUserCount = 0;
    public float updateFrequency;
    private float counter = 0;

    /// <summary>
    /// Solve optimization problem to plan optimal configuration for the UAVs.
    /// </summary>
    /// <param name="userCountMap"></param>
    /// <param name="n_UAVs"></param>
    /// <param name="manyCoordinatesClosestToTower"></param>
    /// <returns> The highest total user count and the corresponding UAV configuration. </returns>
    public (List<Coordinate>, int) PlanConfigurationMap_Optimization(ref int[,] userCountMap, int n_UAVs, List<Coordinate> manyCoordinatesClosestToTower)
    {
        List<Coordinate> initialConfigurationCoordinate = new List<Coordinate>();
        List<Coordinate> bestConfigurationCoordinate = new List<Coordinate>();

        // Find best configuration based on the highest hypothetical user count when UAVs move to desired positions.
        int highestUserCount = 0;
        // For each coordinate that is close to the tower.
        foreach (Coordinate coordinateClosestToTower in manyCoordinatesClosestToTower)
        {
            // Have the node coordinate closest to the tower as the initial coordinate in the configuration.
            initialConfigurationCoordinate.Clear();
            initialConfigurationCoordinate.Add(coordinateClosestToTower);
            // Add the user count of the initial configuration node.
            int initialUsersCount = userCountMap[coordinateClosestToTower.r, coordinateClosestToTower.c];
            
            // Recursively compute all possible configurations and pick the one that yields the highest user count.
            (List<Coordinate> configurationCoordinate, int usersCount) = RecursivePlanConfigurationMap(ref userCountMap, ref n_UAVs, initialUsersCount, initialConfigurationCoordinate);

            // Pick the configuration of the node near the tower that yields the highest total user count.
            if (usersCount > highestUserCount)
            {
                highestUserCount = usersCount;
                bestConfigurationCoordinate = configurationCoordinate;
            }
        }

        return (bestConfigurationCoordinate, highestUserCount);
    }

    /// <summary>
    /// Recursive function for the optimization problem solver algorithm. 
    /// </summary>
    /// <param name="userCountMap"></param>
    /// <param name="n_UAVs"></param>
    /// <param name="n_Users"></param>
    /// <param name="currentConfigurationCoordinates"></param>
    /// <returns> The best configuration with the given current configuration. </returns>
    private (List<Coordinate>, int) RecursivePlanConfigurationMap(ref int[,] userCountMap, ref int n_UAVs, int n_Users, List<Coordinate> currentConfigurationCoordinates)
    {
        // Base case 1: no more UAV to plot
        if (currentConfigurationCoordinates.Count >= n_UAVs)
        {
            return (currentConfigurationCoordinates, n_Users);
        }

        // Base case 2: no more empty node to place new UAV
        int rows = userCountMap.GetLength(0);
        int columns = userCountMap.GetLength(1);
        // Get the neighboring coordinates of the given configuration coordinates with potential connection to the internet.
        List<Coordinate> neighboringCoordinatesWithConnections = ConfigurationMap.inst.GetEmptyConnectedCoordinates(currentConfigurationCoordinates, rows, columns);
        if (neighboringCoordinatesWithConnections.Count == 0)
        {
            return (currentConfigurationCoordinates, n_Users);
        }

        // Find the configuration with the highest user count.
        List<Coordinate> bestConfigurationCoordinates = new List<Coordinate>();
        int highestUserCount = 0;
        // For each neighboring coordinates with potential connection to the internet with placing the UAV there.
        foreach(Coordinate oneNeighboringCoordinateWithConnection in neighboringCoordinatesWithConnections)
        {
            // Take the given configuration and add a neighboring coordinate with potential connection to the internet.
            List<Coordinate> tempConfiurationCoordinates = new List<Coordinate>(currentConfigurationCoordinates);
            tempConfiurationCoordinates.Add(oneNeighboringCoordinateWithConnection);

            // Add the user count of the neighboring coordinate with potential connection to the internet.
            int newUserCount = n_Users;
            newUserCount += userCountMap[oneNeighboringCoordinateWithConnection.r, oneNeighboringCoordinateWithConnection.c];

            // Recursively call itself with the new configuration to obtain the highest hypothetical total user count with that configuration when planning further.
            (List<Coordinate> returnedConfigurationCoordinates, int returnedUserCount) = RecursivePlanConfigurationMap(ref userCountMap, ref n_UAVs, newUserCount, tempConfiurationCoordinates);
            // Pick the returned configuration that yields the highest total user count.
            if (returnedUserCount > highestUserCount)
            {
                highestUserCount = returnedUserCount;
                bestConfigurationCoordinates = returnedConfigurationCoordinates;
            }
        }

        return (bestConfigurationCoordinates, highestUserCount);
    }

    /// <summary>
    /// Plan the UAV configuration by placing the UAV at the neighboring coordinate with potential internet connection with the highest user count (greedy).
    /// </summary>
    /// <param name="userCountMap"></param>
    /// <param name="n_UAVs"></param>
    /// <param name="manyCoordinatesClosestToTower"></param>
    /// <returns>  </returns>
    public (List<Coordinate>, int) PlanConfigurationMap_LocalMaximum(ref int[,] userCountMap, int n_UAVs, List<Coordinate> manyCoordinatesClosestToTower)
    {
        int currentUserCount = 0;
        int rows = userCountMap.GetLength(0);
        int columns = userCountMap.GetLength(1);
        List<Coordinate> currentCoordinates = new List<Coordinate>();

        // Find the initial coordinate (near tower) with the highest user count.
        int localUserCount = -1;
        Coordinate localBestCoordinate = new Coordinate(0, 0);
        foreach (Coordinate coordinate in manyCoordinatesClosestToTower)
        {
            if (userCountMap[coordinate.r, coordinate.c] > localUserCount)
            {
                localUserCount = userCountMap[coordinate.r, coordinate.c];
                localBestCoordinate = coordinate;
            }
        }
        // Pick the coordinate near the tower with the highest user count.
        currentCoordinates.Add(localBestCoordinate);
        currentUserCount += localUserCount;

        // Get a list of neighboring coordinate with potential internet conneciton.
        List<Coordinate> neighborCoordinatesWithConnections = ConfigurationMap.inst.GetEmptyConnectedCoordinates(currentCoordinates, rows, columns);
        // While there are UAVs to plot and coordinates to plot new UAVs.
        while (currentCoordinates.Count < n_UAVs && neighborCoordinatesWithConnections.Count > 0)
        {
            // For each neighboring coordinate with potential internet connection, find the coordinate with the highest user count.
            localUserCount = -1;
            foreach (Coordinate coordinate in neighborCoordinatesWithConnections)
            {
                if (userCountMap[coordinate.r, coordinate.c] > localUserCount)
                {
                    localUserCount = userCountMap[coordinate.r, coordinate.c];
                    localBestCoordinate = coordinate;
                }
            }
            // Add the neighboring coordinate with the highest user count.
            currentCoordinates.Add(localBestCoordinate);
            // Add user count of the neighboring coordinate to total user count.
            currentUserCount += localUserCount;

            // Get a new list of neighboring coordinate with potential internet conneciton.
            neighborCoordinatesWithConnections = ConfigurationMap.inst.GetEmptyConnectedCoordinates(currentCoordinates, rows, columns);
        }

        return (currentCoordinates, currentUserCount);
    }

    /// <summary>
    /// Convert a list of nodes to a list of coordinates.
    /// </summary>
    /// <param name="nodeList"></param>
    /// <returns> a list of coordinates. </returns>
    public List<Coordinate> NodeEntitysToCoordinates(List<NodeEntity> nodeList)
    {
        List<Coordinate> coordinateList = new List<Coordinate>();
        foreach(NodeEntity node in nodeList)
        {
            coordinateList.Add(new Coordinate(node.row, node.col));
        }

        return coordinateList;
    }

    // Start is called before the first frame update
    void Start()
    {
        //cm = GetComponent<ConfigurationMap>();
        plannedNodeEntitys = new List<NodeEntity>();
        highestPlannedUserCount = 0;
        counter = .5f;
    }

    /// <summary>
    /// Send the UAVs to the planned configuration using branch and bound algorith to solve the assignment problem.
    /// </summary>
    public void DispatchUAV()
    {
        List<UAVEntity> UAVToBeDispatchedList = new List<UAVEntity>(EntityManager.inst.uavs);
        // For each node in the planned configuration.
        foreach (NodeEntity node in plannedNodeEntitys)
        {
            // Find the UAV with the lowest cost to that node.
            int bestID = -1;
            float lowestCost = Mathf.Infinity;
            foreach (UAVEntity uav in UAVToBeDispatchedList)
            {
                // Dispatch UAV only when the UAV is connected to the tower.
                if (node != null && uav.router.numberOfHops > 0)
                {
                    // Cost = distance + number of user uav is serving - battery life.
                    float cost = Vector3.Distance(node.transform.position, uav.transform.position) + uav.router.numberOfUsersServing - uav.battery.batteryLife;
                    if (cost < lowestCost)
                    {
                        lowestCost = cost;
                        bestID = uav.GetID();
                    }
                }
            }
            if (bestID > -1)
            {
                // Dispatch UAV.
                ConfigurationMap.inst.SendUAV(bestID, node);
                // Remove the dispatched UAV from the list of available UAVs.
                UAVToBeDispatchedList.Remove(EntityManager.inst.uavs[bestID]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Plan and dispatch UAV every time step using local maximum greedy algorithm.
        if (counter <= 0)
        {
            counter = updateFrequency;

            int[,] userCountMap = ConfigurationMap.inst.GetUserCountMap();
            // (List<Coordinate> newConfigurationCoordinates, int users) = PlanConfigurationMap_Optimization(ref userCountMap, cm.totalUAVs, NodeEntitysToCoordinates(cm.GetManyNeasestNodeEntitysFromTower()));
            (List<Coordinate> newConfigurationCoordinates, int users) = PlanConfigurationMap_LocalMaximum(ref userCountMap, EntityManager.inst.uavs.Count, NodeEntitysToCoordinates(ConfigurationMap.inst.GetManyNeasestNodesFromTower()));
            plannedNodeEntitys = ConfigurationMap.inst.CoordinatesToNodes(newConfigurationCoordinates);
            highestPlannedUserCount = users;

            DispatchUAV();
        }
        counter -= Time.deltaTime;
    }
    
}
