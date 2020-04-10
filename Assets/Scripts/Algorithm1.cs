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

    public (List<Coordinate>, int) PlanConfigurationMap_Optimization(ref int[,] userCountMap, int n_UAVs, List<Coordinate> manyCoordinatesClosestToTower)
    {
        List<Coordinate> initialConfigurationCoordinate = new List<Coordinate>();
        List<Coordinate> bestConfigurationCoordinate = new List<Coordinate>();
        int highestUserCount = 0;
        foreach (Coordinate coordinateClosestToTower in manyCoordinatesClosestToTower)
        {
            initialConfigurationCoordinate.Clear();
            initialConfigurationCoordinate.Add(coordinateClosestToTower);
            int initialUsersCount = userCountMap[coordinateClosestToTower.r, coordinateClosestToTower.c];

            (List<Coordinate> configurationCoordinate, int usersCount) = RecursivePlanConfigurationMap(ref userCountMap, ref n_UAVs, initialUsersCount, initialConfigurationCoordinate);

            if (usersCount > highestUserCount)
            {
                highestUserCount = usersCount;
                bestConfigurationCoordinate = configurationCoordinate;
            }
        }

        return (bestConfigurationCoordinate, highestUserCount);
    }

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
        List<Coordinate> neighboringCoordinatesWithConnections = ConfigurationMap.inst.GetEmptyConnectedCoordinates(currentConfigurationCoordinates, rows, columns);
        if (neighboringCoordinatesWithConnections.Count == 0)
        {
            return (currentConfigurationCoordinates, n_Users);
        }

        List<Coordinate> bestConfigurationCoordinates = new List<Coordinate>();
        int highestUserCount = 0;
        foreach(Coordinate oneNeighboringCoordinateWithConnection in neighboringCoordinatesWithConnections)
        {
            List<Coordinate> tempConfiurationCoordinates = new List<Coordinate>(currentConfigurationCoordinates);
            tempConfiurationCoordinates.Add(oneNeighboringCoordinateWithConnection);

            int newUserCount = n_Users;
            newUserCount += userCountMap[oneNeighboringCoordinateWithConnection.r, oneNeighboringCoordinateWithConnection.c];

            (List<Coordinate> returnedConfigurationCoordinates, int returnedUserCount) = RecursivePlanConfigurationMap(ref userCountMap, ref n_UAVs, newUserCount, tempConfiurationCoordinates);

            if (returnedUserCount > highestUserCount)
            {
                highestUserCount = returnedUserCount;
                bestConfigurationCoordinates = returnedConfigurationCoordinates;
            }
        }

        return (bestConfigurationCoordinates, highestUserCount);
    }

    public (List<Coordinate>, int) PlanConfigurationMap_LocalMaximum(ref int[,] userCountMap, int n_UAVs, List<Coordinate> manyCoordinatesClosestToTower)
    {
        int currentUserCount = 0;
        int rows = userCountMap.GetLength(0);
        int columns = userCountMap.GetLength(1);
        List<Coordinate> currentCoordinates = new List<Coordinate>();

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
        currentCoordinates.Add(localBestCoordinate);
        currentUserCount += localUserCount;

        List<Coordinate> neighborCoordinatesWithConnections = ConfigurationMap.inst.GetEmptyConnectedCoordinates(currentCoordinates, rows, columns);
        while (currentCoordinates.Count < n_UAVs && neighborCoordinatesWithConnections.Count > 0)
        {
            localUserCount = -1;
            foreach (Coordinate coordinate in neighborCoordinatesWithConnections)
            {
                if (userCountMap[coordinate.r, coordinate.c] > localUserCount)
                {
                    localUserCount = userCountMap[coordinate.r, coordinate.c];
                    localBestCoordinate = coordinate;
                }
            }
            currentCoordinates.Add(localBestCoordinate);
            currentUserCount += localUserCount;

            neighborCoordinatesWithConnections = ConfigurationMap.inst.GetEmptyConnectedCoordinates(currentCoordinates, rows, columns);
        }

        return (currentCoordinates, currentUserCount);
    }

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

    public void DispatchUAV()
    {
        List<UAVEntity> UAVToBeDispatchedList = new List<UAVEntity>(EntityManager.inst.uavs);
        foreach (NodeEntity node in plannedNodeEntitys)
        {
            int bestID = -1;
            float lowestCost = Mathf.Infinity;
            foreach (UAVEntity uav in UAVToBeDispatchedList)
            {
                if (node != null)
                {
                    float cost = Vector3.Distance(node.transform.position, uav.transform.position);
                    if (cost < lowestCost)
                    {
                        lowestCost = cost;
                        bestID = uav.GetID();
                    }
                }
            }
            if (bestID > -1)
            {
                ConfigurationMap.inst.SendUAV(bestID, node);
                UAVToBeDispatchedList.Remove(EntityManager.inst.uavs[bestID]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
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
