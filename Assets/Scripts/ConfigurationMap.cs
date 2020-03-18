using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationMap : MonoBehaviour
{
    public GameObject node;
    public GameObject ground;

    public class Node
    {
        public GameObject UAV;
        public int numberUsers;
        public int row;
        public int col;
    }

    public float connectionDistance = 1;
    public int columns = 1;
    public int rows = 1;
    public float height;

    public Node[,] configurationMap;

    public void Initialize()
    {
        configurationMap = new Node[columns, rows];
        Vector3 startPosition = new Vector3(-((Mathf.Sqrt(3) * (columns - 1) * connectionDistance) / 2) / 2, 0, -((rows - 1) * connectionDistance)/2);
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                Node newNode = new Node();
                
                if (c % 2 == 0)
                {
                    newNode.UAV = Instantiate(node, startPosition + new Vector3((Mathf.Sqrt(3) * c * connectionDistance) / 2, height, r * connectionDistance), new Quaternion());
                }
                else
                {
                    if (r + 1 < rows)
                    {
                        newNode.UAV = Instantiate(node, startPosition + new Vector3((Mathf.Sqrt(3) * c * connectionDistance) / 2, height, r * connectionDistance + .5f * connectionDistance), new Quaternion());
                    } else
                    {
                        continue;
                    }
                }

                newNode.row = r;
                newNode.col = c;
                newNode.UAV.transform.parent = transform;
                configurationMap[c, r] = newNode;
            }
        }
    }


    /*
        Neighbor index format

        null|0  |   1
        2   |POS|   3
        null|4  |   5
    */
    public Node[] GetNeighbors(int row, int column)
    {
        Node[] neighborList = new Node[6];
        if (!(row - 1 < 0))
        {
            neighborList[0] = configurationMap[column, row - 1];
            if (column + 1 > columns)
            {
                neighborList[1] = configurationMap[column + 1, row - 1];
            }
        }
        if (column - 1 < columns)
        {
            neighborList[2] = configurationMap[column - 1, row];
        }

        if (column + 1 > columns)
        {
            neighborList[3] = configurationMap[column + 1, row];
        }

        if (!(row + 1 > rows))
        {
            neighborList[4] = configurationMap[column, row + 1];
            if (column + 1 > columns)
            {
                neighborList[5] = configurationMap[column + 1, row + 1];
            }
        }
        return neighborList;
    }

    public Node[] GetNeighbors(GameObject UAV)
    {
        UAVInfo uavinfo = UAV.GetComponent<UAVInfo>();
        
        return GetNeighbors(uavinfo.TargetNode.row, uavinfo.TargetNode.col);
    }




    public void InsertUAV(GameObject UAV, int row, int column)
    {

        UAV.transform.position = configurationMap[column, row].UAV.transform.position;
        UAV.transform.parent = configurationMap[column, row].UAV.transform;
    }

}
