using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Algorithm : MonoBehaviour
{
    public ConfigurationMap cm;
    public List<Node> emptyNodesWithConnection;

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

        if (row - 1 >= 0 && column - 1 >= 0)
            neighborList.Add(new Coordinate(row - 1, column - 1));

        if (row - 1 >= 0)
            neighborList.Add(new Coordinate(row - 1, column));

        if (column - 1 >= 0)
            neighborList.Add(new Coordinate(row, column - 1));

        if (column + 1 < n_columns)
            neighborList.Add(new Coordinate(row, column + 1));

        if (row + 1 < n_rows && column - 1 >= 0)
            neighborList.Add(new Coordinate(row + 1, column - 1));

        if (row + 1 < n_rows)
            neighborList.Add(new Coordinate(row + 1, column));

        return neighborList;
    }

    public List<Coordinate> GetEmptyConnectedCoordinates(bool[,] compressedConfigurationMap)
    {
        List<Coordinate> coordinateList = new List<Coordinate>();

        int rows = compressedConfigurationMap.GetLength(0);
        int columns = compressedConfigurationMap.GetLength(1);
        for (int c = 0; c < columns; ++c)
        {
            for (int r = 0; r < rows; ++r)
            {
                if (compressedConfigurationMap[r, c])
                {
                    coordinateList = coordinateList.Union(GetNeighboringCoordinates(r, c, rows, columns)).ToList();
                }
            }
        }

        foreach (Coordinate coordinate in coordinateList.ToList())
        {
            Node node = cm.GetNode(coordinate);
            if (node.UAV != null)
            {
                coordinateList.Remove(coordinate);
            }
        }

        return coordinateList;
    }

    // Start is called before the first frame update
    void Start()
    {
        cm = GetComponent<ConfigurationMap>();
        emptyNodesWithConnection = new List<Node>();
    }

    // Update is called once per frame
    void Update()
    {
        emptyNodesWithConnection = cm.GetNodes(GetEmptyConnectedCoordinates(cm.GetCompressedConfigurationMap()));
    }
}
