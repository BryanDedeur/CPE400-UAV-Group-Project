using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Algorithm : MonoBehaviour
{
    public ConfigurationMap cm;
    public List<Node> plannedNodes;
    public int highestPlannedUserCount = 0;

    public struct Coordinate
    {
        public int r;
        public int c;

        public Coordinate(int row, int column)
        {
            r = row;
            c = column;
        }
    }

    static public List<Coordinate> GetNeighboringCoordinates(int row, int column, int n_rows, int n_columns)
    {
        List<Coordinate> neighborList = new List<Coordinate>();

        if (row % 2 == 0) // even rows
        {
            if (row + 1 < n_rows && column - 1 >= 0) // top left
                neighborList.Add(new Coordinate(row + 1, column - 1));
            if (row + 1 < n_rows) // top
                neighborList.Add(new Coordinate(row + 1, column));
        } 
        else // odd rows
        {
            if (row + 1 < n_rows) // top left
                neighborList.Add(new Coordinate(row + 1, column));
            if (row + 1 < n_rows && column + 1 < n_columns) // top
                neighborList.Add(new Coordinate(row + 1, column + 1));
        }
        if (column - 1 >= 0) // left
            neighborList.Add(new Coordinate(row, column - 1));
        if (column + 1 < n_columns) // right
            neighborList.Add(new Coordinate(row, column + 1));
        if (row % 2 == 0) // even rows
        {
            if (row - 1 >= 0 && column - 1 >= 0) // bottom left
                neighborList.Add(new Coordinate(row - 1, column - 1));
            if (row - 1 >= 0) // bottom right
                neighborList.Add(new Coordinate(row - 1, column));
        } 
        else // odd rows
        {
            if (row - 1 >= 0) // bottom left
                neighborList.Add(new Coordinate(row - 1, column));
            if (row - 1 >= 0 && column + 1 < n_columns) // bottom right
                neighborList.Add(new Coordinate(row - 1, column + 1));
        }

        return neighborList;
    }

    static public List<Coordinate> GetNeighboringCoordinates(Coordinate coordinate, int n_rows, int n_columns)
    {
        List<Coordinate> neighborList = new List<Coordinate>();
        int row = coordinate.r;
        int column = coordinate.c;

        if (row % 2 == 0) // even rows
        {
            if (row + 1 < n_rows && column - 1 >= 0) // top left
                neighborList.Add(new Coordinate(row + 1, column - 1));
            if (row + 1 < n_rows) // top
                neighborList.Add(new Coordinate(row + 1, column));
        }
        else // odd rows
        {
            if (row + 1 < n_rows) // top left
                neighborList.Add(new Coordinate(row + 1, column));
            if (row + 1 < n_rows && column + 1 < n_columns) // top
                neighborList.Add(new Coordinate(row + 1, column + 1));
        }
        if (column - 1 >= 0) // left
            neighborList.Add(new Coordinate(row, column - 1));
        if (column + 1 < n_columns) // right
            neighborList.Add(new Coordinate(row, column + 1));
        if (row % 2 == 0) // even rows
        {
            if (row - 1 >= 0 && column - 1 >= 0) // bottom left
                neighborList.Add(new Coordinate(row - 1, column - 1));
            if (row - 1 >= 0) // bottom right
                neighborList.Add(new Coordinate(row - 1, column));
        }
        else // odd rows
        {
            if (row - 1 >= 0) // bottom left
                neighborList.Add(new Coordinate(row - 1, column));
            if (row - 1 >= 0 && column + 1 < n_columns) // bottom right
                neighborList.Add(new Coordinate(row - 1, column + 1));
        }

        return neighborList;
    }

    public List<Coordinate> GetEmptyConnectedCoordinates(List<Coordinate> configurationCoordiates, int rows, int columns)
    {
        List<Coordinate> coordinateList = new List<Coordinate>();

        foreach (Coordinate coordinate in configurationCoordiates)
        {
            coordinateList = coordinateList.Union(GetNeighboringCoordinates(coordinate.r, coordinate.c, rows, columns)).ToList();
        }

        foreach (Coordinate coordinate in coordinateList.ToList())
        {
            int index = configurationCoordiates.FindIndex(x => x.c == coordinate.c && x.r == coordinate.r);
            if (index >= 0)
            {
                coordinateList.Remove(coordinate);
            }
        }

        return coordinateList;
    }

    public (List<Coordinate>, int) PlanConfigurationMap(ref int[,] userCountMap, int n_UAVs, List<Coordinate> manyCoordinatesClosestToTower)
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
        List<Coordinate> neighboringCoordinatesWithConnections = GetEmptyConnectedCoordinates(currentConfigurationCoordinates, rows, columns);
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

    public List<Coordinate> NodesToCoordinates(List<Node> nodeList)
    {
        List<Coordinate> coordinateList = new List<Coordinate>();
        foreach(Node node in nodeList)
        {
            coordinateList.Add(new Coordinate(node.row, node.col));
        }

        return coordinateList;
    }

    // Start is called before the first frame update
    void Start()
    {
        cm = GetComponent<ConfigurationMap>();
        plannedNodes = new List<Node>();
        highestPlannedUserCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        int[,] userCountMap = cm.GetUserCountMap();
        (List<Coordinate> newConfigurationCoordinates, int users) = PlanConfigurationMap(ref userCountMap, cm.totalUAVs, NodesToCoordinates(cm.GetManyNeasestNodesFromTower()));
        plannedNodes = cm.GetNodes(newConfigurationCoordinates);
        highestPlannedUserCount = users;
    }
}
