using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ConfigurationMap : MonoBehaviour
{
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

    public static ConfigurationMap inst;

    public float nodeDistance = 10;
    public bool showNodes = true;
    public bool showHeatMap = false;
    public NodeEntity[,] configurationMapNodes;

    [Range(1, 200)]
    public int numberOfUsers;
    [Range(1, 100)]
    public int numberOfUAVs;
    [Range(1, 20)]
    public int columns = 10;
    [Range(1, 20)]
    public int rows = 5;
    [Range(0, 1)]
    public float towerPosX;
    [Range(0, 1)]
    public float towerPosZ;
    public float uavHeight = 10;

    private GameObject groundPrefab;
    public GameObject towerPrefab;

    private Vector3 groundOffset;
    private Vector3 nodeOffset;
    private float verticalMapSize, horizontalMapSize;

    private GameObject visualsFolder;

    private void Awake()
    {
        inst = this;

        if (groundPrefab == null)
        {
            groundPrefab = Resources.Load("Ground") as GameObject;
            groundPrefab = Instantiate(groundPrefab);
        }
        groundPrefab.transform.position = new Vector3(0, -0.01f, 0);
        groundPrefab.name = "Ground";

        horizontalMapSize = (columns + 1) * nodeDistance;
        verticalMapSize = ((Mathf.Sqrt(3) * nodeDistance) / 2) * (rows + 1);
        groundPrefab.transform.localScale = new Vector3(verticalMapSize, horizontalMapSize, 1);
        groundOffset = new Vector3(-verticalMapSize / 2f, 0, -horizontalMapSize / 2f);

        nodeOffset = new Vector3(-(verticalMapSize - (2 * (Mathf.Sqrt(3) * nodeDistance) / 2f)) / 2f, 0, -(horizontalMapSize - (2 * nodeDistance)) / 2f);

        if (towerPrefab == null)
        {
            towerPrefab = Resources.Load("Tower") as GameObject;
            towerPrefab = Instantiate(towerPrefab);
        }
        towerPrefab.name = "Tower";

        Vector3 towerPosition = groundOffset + new Vector3(towerPosX * verticalMapSize, 2 * uavHeight, towerPosZ * horizontalMapSize);
        towerPrefab.transform.localScale = new Vector3(1, towerPosition.y, 1);
        towerPrefab.transform.position = new Vector3(towerPosition.x, towerPosition.y / 2, towerPosition.z);

        Camera.main.orthographicSize = Mathf.Max(horizontalMapSize, verticalMapSize) / 2f;

        visualsFolder = new GameObject();
        visualsFolder.transform.parent = transform;
        visualsFolder.name = "VisualsFolder";
    }

    public void Start()
    {
        CreateNodes();
        CreateUsers();
        CreateUAVs();
    }

    public void Update()
    {
        foreach(NodeEntity node in EntityManager.inst.nodes)
        {
            node.ToggleHeatVisual(showHeatMap);
            node.ToggleNodeVisual(showNodes);
        }
    }

    private void CreateNodes()
    {
        configurationMapNodes = new NodeEntity[rows, columns];
        for (int c = 0; c < columns; ++c)
        {
            for (int r = 0; r < rows; ++r)
            {
                NodeEntity node;
                if ((r % 2 == 0))
                {
                    node = EntityManager.inst.CreateNode(nodeOffset + new Vector3((Mathf.Sqrt(3) * r * (nodeDistance)) / 2, uavHeight, c * (nodeDistance)));
                }
                else
                {
                    if (c + 1 < columns)
                    {
                        node = EntityManager.inst.CreateNode(nodeOffset + new Vector3((Mathf.Sqrt(3) * r * (nodeDistance)) / 2, uavHeight, c * (nodeDistance) + .5f * (nodeDistance)));
                    }
                    else
                    {
                        continue;
                    }
                }
                node.row = r;
                node.col = c;
                node.name = "Node[" + r + ", " + c + "]";
                configurationMapNodes[r, c] = node;
            }
        }
        EstablishNodeInformation();
    }

    private void EstablishNodeInformation()
    {
        for (int c = 0; c < columns; ++c)
        {
            for (int r = 0; r < rows; ++r)
            {
                if (configurationMapNodes[r, c] != null)
                {
                    configurationMapNodes[r, c].neighbors = GetNeighbors(r, c);
                }
            }
        }
    }

    private void CreateUsers()
    {
        for (int i = 0; i < numberOfUsers; ++i)
        {
            UserEntity user = EntityManager.inst.CreateUser(groundOffset + new Vector3(UnityEngine.Random.Range(0, 100) / 100f * verticalMapSize, 0, UnityEngine.Random.Range(0, 100) / 100f * horizontalMapSize), UnityEngine.Random.Range(0,360));
            RandomStopStartDirectionalMovement command = new RandomStopStartDirectionalMovement(user, groundOffset.x, verticalMapSize / 2, groundOffset.z, horizontalMapSize / 2);
            user.ai.SetCommand(command);
            user.nearestNode = GetNearestNode(user.transform.position);
            user.nearestNode.usersInRange.Add(user);
        }
    }

    private void CreateUAVs()
    {
        for (int i = 0; i < numberOfUAVs; ++i)
        {
            UAVEntity uav = EntityManager.inst.CreateUAV(new Vector3(towerPrefab.transform.position.x, 0, towerPrefab.transform.position.z));
        }
    }

    // --------------------------------------------- HELPERS ---------------------------------------------------------------- //

    public List<NodeEntity> GetNeighbors(int row, int column)
    {
        List<NodeEntity> neighborList = new List<NodeEntity>();
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
        int row = coordinate.r;
        int column = coordinate.c;

        return GetNeighboringCoordinates(row, column, n_rows, n_columns); ;
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

    public List<NodeEntity> GetNeighbors(NodeEntity node)
    {
        return GetNeighbors(node.row, node.col);
    }

    public bool[,] GetCompressedConfigurationMap(bool withConnection = true)
    {
        bool[,] compressedConfigurationMap = new bool[rows, columns];

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (configurationMapNodes[r, c] != null && configurationMapNodes[r, c].assignedUAV != null)
                {
                    if (withConnection)
                    {
                        if (configurationMapNodes[r, c].router.connectionRadius < int.MaxValue)
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

    public List<Coordinate> GetConfigurationCoordinates(bool withConnection = true)
    {
        List<Coordinate> configurationCoordinates = new List<Coordinate>();

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (configurationMapNodes[r, c] != null && configurationMapNodes[r, c].assignedUAV != null)
                {
                    if (withConnection)
                    {
                        if (configurationMapNodes[r, c].router.connectionRadius < int.MaxValue)
                        {
                            configurationCoordinates.Add(new Coordinate(r, c));
                        }
                    }
                    else
                    {
                        configurationCoordinates.Add(new Coordinate(r, c));
                    }
                }
            }
        }

        return configurationCoordinates;
    }

    public int[,] GetUserCountMap()
    {
        int[,] userCountMap = new int[rows, columns];

        for (int c = 0; c < columns; ++c)
        {
            for (int r = 0; r < rows; ++r)
            {
                if (configurationMapNodes[r, c] != null)
                {
                    userCountMap[r, c] = configurationMapNodes[r, c].usersInRange.Count;
                }
                else
                {
                    userCountMap[r, c] = 0;
                }
            }
        }

        return userCountMap;
    }

    public NodeEntity GetNode(Coordinate coordinate)
    {
        return configurationMapNodes[coordinate.r, coordinate.c];
    }

    public NodeEntity GetNode(int row, int column)
    {
        return configurationMapNodes[row, column];
    }

    public List<NodeEntity> CoordinatesToNodes(List<Coordinate> coordinates)
    {
        List<NodeEntity> nodeList = new List<NodeEntity>();

        foreach (Coordinate coordinate in coordinates)
        {
            nodeList.Add(GetNode(coordinate));
        }

        return nodeList;
    }

    public NodeEntity GetNearestNode(Vector3 position)
    {
        NodeEntity nearestNode = null;
        float nearestDistance = Mathf.Infinity;
        float testDistance;
        foreach(NodeEntity node in EntityManager.inst.nodes)
        {
            testDistance = (node.transform.position - position).sqrMagnitude;
            if (testDistance < nearestDistance)
            {
                nearestNode = node;
                nearestDistance = testDistance;
            }
        }

        return nearestNode;
    }

    public NodeEntity GetNearestNodeFromTower()
    {      
        return GetNearestNode(towerPrefab.transform.position);
    }

    public (NodeEntity, float) GetNearestNodeAndDistanceFromTower()
    {
        NodeEntity nearestNode = null;
        float nearestDistance = Mathf.Infinity;
        float testDistance;
        foreach (NodeEntity node in EntityManager.inst.nodes)
        {
            testDistance = (node.transform.position - towerPrefab.transform.position).magnitude;
            if (testDistance < nearestDistance)
            {
                nearestNode = node;
                nearestDistance = testDistance;
            }
        }

        return (nearestNode, nearestDistance);
    }

    public List<NodeEntity> GetManyNeasestNodesFromTower()
    {
        List<NodeEntity> manyNearestNodes = new List<NodeEntity>();
        (NodeEntity nearestNodeToTower, float distance) = GetNearestNodeAndDistanceFromTower();
        List<NodeEntity> neighborNodesNearTower = GetNeighbors(nearestNodeToTower);

        manyNearestNodes.Add(nearestNodeToTower);
        foreach (NodeEntity node in neighborNodesNearTower)
        {
            if ((node.transform.position - towerPrefab.transform.position).magnitude == distance)
            {
                manyNearestNodes.Add(node);
            }
        }

        return manyNearestNodes;
    }

    // ------------------------ UAV Movement ---------------------------------------- //

    public bool SendUAV(UAVEntity uav, NodeEntity toNode)
    {
        if (uav == null)
        {
            return false;
        }

        if (toNode == null)
        {
            return false;
        }

        uav.assignedNode = toNode;
        toNode.assignedUAV = uav;
        MoveTo moveToCommand = new MoveTo(uav, toNode.transform.position);
        uav.ai.SetCommand(moveToCommand);

        return true;
    }

    public bool SendUAV(NodeEntity fromNode, NodeEntity toNode)
    {
        return SendUAV(fromNode.assignedUAV, toNode);
    }

    public bool SendUAV(int ID, NodeEntity toNode)
    {
        return SendUAV(EntityManager.inst.uavs[ID], toNode);
    }

    public bool StopUAV(int ID)
    {
        UAVEntity uav = EntityManager.inst.uavs[ID];
        uav.ai.StopAndRemoveAllCommands();
        uav.physics.desiredSpeed = 0;

        return true;
    }

    // ----------------------- NEW ------------------------------ //

    public bool SendUAVToTower(UAVEntity uav)
    {
        if (uav == null)
        {
            return false;
        }

        uav.assignedNode = null;
        MoveToUntilConnected moveToCommand = new MoveToUntilConnected(uav, towerPrefab.transform.position);
        uav.ai.SetCommand(moveToCommand);

        return true;
    }

    public bool DecomissionUAV(Entity uav)
    {
        if (uav == null)
        {
            return false;
        }

        UAVEntity newUAV = uav as UAVEntity;
        newUAV.assignedNode = null;

        NetworkManager.inst.routers.Remove(newUAV.router.GetID());   

        MoveTo moveToCommand = new MoveTo(uav, new Vector3(towerPrefab.transform.position.x, 0, towerPrefab.transform.position.z));
        uav.ai.SetCommand(moveToCommand);
        uav.ai.rejectInstructions = true;

        return true;
    }

    public bool DecomissionUAV(NodeEntity fromNode)
    {
        return DecomissionUAV(fromNode.assignedUAV);
    }

}
