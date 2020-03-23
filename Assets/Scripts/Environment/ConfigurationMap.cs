using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coordinate = Algorithm.Coordinate;

public class ConfigurationMap : MonoBehaviour
{
    public GameObject UAVPrefab;
    public GameObject userPrefab;
    public GameObject nodePrefab;
    public GameObject groundPrefab;
    public GameObject towerPrefab;

    public int totalUsers = 0;

    private float connectionRadius;
    private int rows;
    private int columns;
    private float groundHeight = 0;
    private float userHeight = 1;
    private float uavHeight = 2;
    private float horizontalMapSize = 0;
    private float verticalMapSize = 0;
    private Vector3 nodeOffset;
    private Vector3 groundOffset;


    public Node[,] configurationMapNodes;
    public Dictionary<int, NetworkRouter> allRouters;
    

    /* Places the tower and returns the nearest node to that tower, origin is at the lower right
     * @param verticalPercentage is a vertical position between 0 and 1 on the scale of the actual gridmap
     * @param horizontalPercentage is a horizontal position between 0 and 1 on the scale of the actual gridmap
     * @return the Node nearest that tower after it's placed
    */
    public Node PlaceTower(float verticalPercentage, float horizontalPercentage)
    {
        Vector3 newPosition = groundOffset + new Vector3(verticalPercentage * verticalMapSize, towerPrefab.transform.localScale.y - 1, horizontalPercentage * horizontalMapSize);
        towerPrefab.transform.position = newPosition;
        Node nearestNode = GetNeasestNodeFromTower();
        return nearestNode;
    }

    private void Initialize()
    {
        configurationMapNodes = new Node[rows, columns];
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                GameObject visual = null;
                
                if ((r % 2 == 0))
                {
                    visual = Instantiate(nodePrefab, nodeOffset + new Vector3((Mathf.Sqrt(3) * r * connectionRadius) / 2, uavHeight, c * connectionRadius), new Quaternion());
                } else
                {
                    if (c + 1 < columns)
                    {
                        visual = Instantiate(nodePrefab, nodeOffset + new Vector3((Mathf.Sqrt(3) * r * connectionRadius) / 2, uavHeight, c * connectionRadius + .5f * connectionRadius), new Quaternion());
                    }
                    else
                    {
                        continue;
                    }
                }


                Node newNode = visual.AddComponent<Node>();
                newNode.row = r;
                newNode.col = c;
                newNode.numberUsers = Random.Range(0, 10);
                totalUsers += newNode.numberUsers;
                visual.transform.parent = transform;
                visual.name = "Node";
                configurationMapNodes[r, c] = newNode;
            }
        }
    }

    public void Setup(float UAVConnectionRadiusInMeters, int MaxUAVrows, int MaxUAVcolumns)
    {
        connectionRadius = UAVConnectionRadiusInMeters;
        rows = MaxUAVrows;
        columns = MaxUAVcolumns;
        horizontalMapSize = (columns + 1) * connectionRadius;
        verticalMapSize = ((Mathf.Sqrt(3) * connectionRadius) / 2) * (rows + 1);
        groundPrefab.transform.localScale = new Vector3(verticalMapSize, horizontalMapSize, 1);

        groundOffset = new Vector3(-verticalMapSize / 2f, 0, -horizontalMapSize / 2f);
        nodeOffset = new Vector3(-(verticalMapSize - (2 * (Mathf.Sqrt(3) * connectionRadius) / 2f)) / 2f, 0, -(horizontalMapSize - (2 * connectionRadius)) / 2f);

        Initialize();
    }

    public List<Node> GetNeighbors(int row, int column)
    {
        List<Node> neighborList = new List<Node>();

        if (row % 2 == 0) // even rows
        {
            if (row + 1 < rows && column - 1 >= 0) // top left
                neighborList.Add(configurationMapNodes[row + 1, column - 1]);
            if (row + 1 < rows) // top right
                neighborList.Add(configurationMapNodes[row + 1, column]);
        }
        else // odd rows
        {
            if (row + 1 < rows) // top left
                neighborList.Add(configurationMapNodes[row + 1, column]);
            if (row + 1 < rows && column + 1 < columns) // top right
                neighborList.Add(configurationMapNodes[row + 1, column + 1]);
        }
        if (column - 1 >= 0) // left
            neighborList.Add(configurationMapNodes[row, column - 1]);
        if (column + 1 < columns) // right
            neighborList.Add(configurationMapNodes[row, column + 1]);
        if (row % 2 == 0) // even rows
        {
            if (row - 1 >= 0 && column - 1 >= 0) // bottom left
                neighborList.Add(configurationMapNodes[row - 1, column - 1]);
            if (row - 1 >= 0) // bottom right
                neighborList.Add(configurationMapNodes[row - 1, column]);
        }
        else // odd rows
        {
            if (row - 1 >= 0) // bottom left
                neighborList.Add(configurationMapNodes[row - 1, column]);
            if (row - 1 >= 0 && column + 1 < columns) // bottom right
                neighborList.Add(configurationMapNodes[row - 1, column + 1]);
        }

        return neighborList;
    }

    public List<Node> GetNeighbors(GameObject UAV)
    {
        Node node = UAV.transform.parent.GetComponent<Node>();
        return GetNeighbors(node.row, node.col);
    }

    public List<Node> GetNeighbors(Node node)
    {
        return GetNeighbors(node.row, node.col);
    }

    public bool[,] GetCompressedConfigurationMap(bool withConnection=true)
    {
        bool[,] compressedConfigurationMap = new bool[rows, columns];

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (configurationMapNodes[r, c] != null && configurationMapNodes[r, c].UAV != null)
                {
                    if (withConnection)
                    {
                        if (configurationMapNodes[r, c].UAV.GetComponent<NetworkRouter>().connectionLength < int.MaxValue)
                        {
                            compressedConfigurationMap[r, c] = true;
                        } 
                        else
                        {
                            compressedConfigurationMap[r, c] = false;
                        }
                    } 
                    else
                    {
                        compressedConfigurationMap[r, c] = true;
                    }
                } 
                else
                {
                    compressedConfigurationMap[r, c] = false;
                }
            }
        }

        return compressedConfigurationMap;
    }

    public Node GetNode(Coordinate coordinate)
    {
        return configurationMapNodes[coordinate.r, coordinate.c];
    }

    public List<Node> GetNodes(List<Coordinate> coordinates)
    {
        List<Node> nodeList = new List<Node>();

        foreach(Coordinate coordinate in coordinates)
        {
            nodeList.Add(GetNode(coordinate));
        }

        return nodeList;
    }

    public Node GetNeasestNodeFromTower()
    {
        Node nearestNode = null;
        float nearestDistance = Mathf.Infinity;
        for (int c = 0; c < rows; c++)
        {
            for (int r = 0; r < columns; r++)
            {
                float currentDistance = 0;
                if (r % 2 == 0)
                {
                    currentDistance = (configurationMapNodes[r, c].transform.position - towerPrefab.transform.position).magnitude;
                }
                else
                {
                    if (c + 1 < columns)
                    {
                        currentDistance = (configurationMapNodes[r, c].transform.position - towerPrefab.transform.position).magnitude;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (currentDistance < nearestDistance) {
                    nearestNode = configurationMapNodes[r, c];
                    nearestDistance = currentDistance;
                }
            }
        }

        return nearestNode;
    }

    public void GetNearbyRouters(NetworkRouter focusRouter)
    {
        if (focusRouter == null)
        {
            return;
        }
        focusRouter.connectedRouters.Clear();
        foreach (KeyValuePair<int, NetworkRouter> router in allRouters)
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

    public void InsertUAV(int row, int column)
    {
        GameObject UAV = Instantiate(UAVPrefab, configurationMapNodes[row, column].transform);
        UAV.name = "UAV";
        NetworkRouter nr = UAV.AddComponent<NetworkRouter>();
        nr.cm = this;

        UAV.transform.position = configurationMapNodes[row, column].transform.position;

        configurationMapNodes[row, column].UAV = UAV;
    }

    public void InsertUAV(Node node)
    {
        InsertUAV(node.row, node.col);
    }

    private void Start()
    {

    }

    private void Awake()
    {
        allRouters = new Dictionary<int, NetworkRouter>();

        if (UAVPrefab == null)
        {
            UAVPrefab = Resources.Load("UAV") as GameObject;
        }
        UAVPrefab.name = "UAV";

        if (userPrefab == null)
        {
            userPrefab = Resources.Load("User") as GameObject;
        }
        userPrefab.name = "User";


        if (nodePrefab == null)
        {
            nodePrefab = Resources.Load("Node") as GameObject;
        }

        if (groundPrefab == null)
        {
            groundPrefab = Resources.Load("Ground") as GameObject;
            groundPrefab = Instantiate(groundPrefab);
            groundPrefab.transform.position = new Vector3(0,groundHeight, 0);
        }
        groundPrefab.name = "Ground";


        if (towerPrefab == null)
        {
            towerPrefab = Resources.Load("Tower") as GameObject;
            towerPrefab = Instantiate(towerPrefab);
            NetworkRouter nr = towerPrefab.AddComponent<NetworkRouter>();
            nr.cm = this;
        }
        towerPrefab.name = "Tower";


    }
}
