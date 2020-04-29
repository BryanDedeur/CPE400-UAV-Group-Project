using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ConfigurationMap : MonoBehaviour
{
    // Coordinate container for quick access to graph components
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
    public void ToggleVertices(bool state)
    {
        showNodes = state;
    }
    public bool showHeatMap = false;
    public void ToggleHeatMap(bool state)
    {
        showHeatMap = state;
    }
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

        // Create the ground and establish the size and position.
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

        // Create the tower prefab and establish the tower positioning information.
        if (towerPrefab == null)
        {
            towerPrefab = Resources.Load("Tower") as GameObject;
            towerPrefab = Instantiate(towerPrefab);
        }
        towerPrefab.name = "Tower";

        Vector3 towerPosition = groundOffset + new Vector3(towerPosX * verticalMapSize, 2 * uavHeight, towerPosZ * horizontalMapSize);
        towerPrefab.transform.localScale = new Vector3(1, towerPosition.y, 1);
        towerPrefab.transform.position = new Vector3(towerPosition.x, towerPosition.y / 2, towerPosition.z);

        // Adjust the main camera to fit the size of the map.
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
        foreach (NodeEntity node in EntityManager.inst.nodes)
        {
            node.ToggleHeatVisual(showHeatMap);
            node.ToggleNodeVisual(showNodes);
        }
    }

    /// <summary>
    /// Creates the nodes within the configuration map.
    /// </summary>
    private void CreateNodes()
    {
        // Create a new multi dimensional array to store the node information.
        configurationMapNodes = new NodeEntity[rows, columns];
        for (int c = 0; c < columns; ++c)
        {
            for (int r = 0; r < rows; ++r)
            {
                // If even row, create a node offset by the node distance.
                NodeEntity node;
                if ((r % 2 == 0))
                {
                    node = EntityManager.inst.CreateNode(nodeOffset + new Vector3((Mathf.Sqrt(3) * r * (nodeDistance)) / 2, uavHeight, c * (nodeDistance)));
                }
                else
                {
                    // If odd row, and not the last column place nodes offset by the node distance.
                    if (c + 1 < columns)
                    {
                        node = EntityManager.inst.CreateNode(nodeOffset + new Vector3((Mathf.Sqrt(3) * r * (nodeDistance)) / 2, uavHeight, c * (nodeDistance) + .5f * (nodeDistance)));
                    }
                    else
                    {
                        continue;
                    }
                }

                // Create node and add it to the map if the prior cases are valid.
                node.row = r;
                node.col = c;
                node.name = "Node[" + r + ", " + c + "]";
                configurationMapNodes[r, c] = node;
            }
        }
        EstablishNodeInformation();
    }

    /// <summary>
    /// Establishes the neighbor information for each node and makes is accessible from within the node entity.
    /// </summary>
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

    /// <summary>
    /// Creates the users and randomly places them within the area of the map and gives the user AI command to start movement.
    /// </summary>
    private void CreateUsers()
    {
        for (int i = 0; i < numberOfUsers; ++i)
        {
            // Create a user and establish user information.
            UserEntity user = EntityManager.inst.CreateUser(groundOffset + new Vector3(UnityEngine.Random.Range(0, 100) / 100f * verticalMapSize, 0, UnityEngine.Random.Range(0, 100) / 100f * horizontalMapSize), UnityEngine.Random.Range(0, 360));
            RandomStopStartDirectionalMovement command = new RandomStopStartDirectionalMovement(user, groundOffset.x, verticalMapSize / 2, groundOffset.z, horizontalMapSize / 2);
            user.ai.SetCommand(command);
            user.nearestNode = GetNearestNode(user.transform.position);
            user.nearestNode.usersInRange.Add(user);
        }
    }

    /// <summary>
    /// Creats the UAV's and places them at the location of the tower.
    /// </summary>
    private void CreateUAVs()
    {
        for (int i = 0; i < numberOfUAVs; ++i)
        {
            // Create a UAV through the entity manager.
            UAVEntity uav = EntityManager.inst.CreateUAV(towerPrefab.transform.position);
        }
    }

    // ---------------------------------------------------- UAV MOVEMENT ---------------------------------------------------------------- //

    /// <summary>
    /// Sends the UAV to a node on the configuration map using AI.
    /// </summary>
    /// <param name="uav"></param>
    /// <param name="toNode"></param>
    /// <returns> Whether or not the node was sucessfully placed on the map. </returns>
    public bool SendUAV(UAVEntity uav, NodeEntity toNode)
    {
        // If uav is not valid
        if (uav == null)
        {
            return false;
        }

        // If destination is not valid
        if (toNode == null)
        {
            return false;
        }

        // Add AI command to the AI of the entity
        uav.assignedNode = toNode;
        toNode.assignedUAV = uav;
        MoveTo moveToCommand = new MoveTo(uav, toNode.transform.position);
        uav.ai.SetCommand(moveToCommand);

        return true;
    }

    /// <summary>
    /// Sends the UAV that exists under a node to a node on the configuration map using AI.
    /// </summary>
    /// <param name="fromNode"></param>
    /// <param name="toNode"></param>
    /// <returns> Whether or not the node was sucessfully placed on the map. </returns>
    public bool SendUAV(NodeEntity fromNode, NodeEntity toNode)
    {
        return SendUAV(fromNode.assignedUAV, toNode);
    }

    /// <summary>
    /// Sends the UAV of ID to a node on the configuration map using AI.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="toNode"></param>
    /// <returns> Whether or not the node was sucessfully placed on the map. </returns>
    public bool SendUAV(int ID, NodeEntity toNode)
    {
        return SendUAV(EntityManager.inst.uavs[ID], toNode);
    }

    /// <summary>
    /// Sends the UAV belonging to the router to a node on the configuration map using AI.
    /// </summary>
    /// <param name="router"></param>
    /// <param name="toNode"></param>
    /// <returns> Whether or not the node was sucessfully placed on the map. </returns>
    public bool SendUAV(Router router, NodeEntity toNode)
    {
        return SendUAV(router.entity, toNode);
    }

    /// <summary>
    /// Stops the UAV of ID.
    /// </summary>
    /// <param name="ID"></param>
    /// <returns> Whether or not the uav was able to be stopped. </returns>
    public bool StopUAV(int ID)
    {
        // Stop the UAV through it's AI component.
        UAVEntity uav = EntityManager.inst.uavs[ID];
        uav.ai.StopAndRemoveAllCommands();
        uav.physics.desiredSpeed = 0;

        return true;
    }

    /// <summary>
    /// Sends the UAV to the tower until it's connected to the network.
    /// </summary>
    /// <param name="uav"></param>
    /// <returns> Whether or not the uav was able to be sent to the tower. </returns>
    public bool SendUAVToTower(UAVEntity uav)
    {
        // If uav is valid. 
        if (uav == null)
        {
            return false;
        }

        // Add AI command through the uav ai component.
        uav.assignedNode = null;
        MoveToUntilConnected moveToCommand = new MoveToUntilConnected(uav, towerPrefab.transform.position);
        uav.ai.SetCommand(moveToCommand);

        return true;
    }

    /// <summary>
    /// Decomissions the uav from the network.
    /// </summary>
    /// <param name="uav"></param>
    /// <returns> Whether or not the uav was able to be decomissioned. </returns>
    public bool DecomissionUAV(Entity uav)
    {
        // Make sure UAV is valid
        if (uav == null)
        {
            return false;
        }

        // If UAV is valid update information about the previous node it was assigned to.
        UAVEntity newUAV = uav as UAVEntity;
        newUAV.assignedNode = null;

        NetworkManager.inst.routers.Remove(newUAV.router.GetID());

        // Add AI command to the UAV ai component.
        MoveTo moveToCommand = new MoveTo(uav, new Vector3(towerPrefab.transform.position.x, 0, towerPrefab.transform.position.z));
        uav.ai.SetCommand(moveToCommand);
        uav.ai.rejectInstructions = true;

        return true;
    }

    /// <summary>
    /// Decomissions the uav from the network.
    /// </summary>
    /// <param name="fromNode"></param>
    /// <returns> Whether or not the uav was able to be decomissioned. </returns>
    public bool DecomissionUAV(NodeEntity fromNode)
    {
        return DecomissionUAV(fromNode.assignedUAV);
    }

    // ---------------------------------------------------- HELPERS ---------------------------------------------------------------- //

    /// <summary>
    /// Gets the neighbor nodes given a row column parameter.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns> A list of neighbors nodes relative to the row and column. </returns>
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

    /// <summary>
    /// Gets the neighbor nodes given a row column parameter and number of rows and columns.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="n_rows"></param>
    /// <param name="n_columns"></param>
    /// <returns> A list of neighbors coordinates. </returns>
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

    /// <summary>
    /// Gets the neighbor nodes given a row column parameter and number of rows and columns.
    /// </summary>
    /// <param name="coordinate"></param>
    /// <param name="n_rows"></param>
    /// <param name="n_columns"></param>
    /// <returns> A list of neighbors coordinates. </returns>
    static public List<Coordinate> GetNeighboringCoordinates(Coordinate coordinate, int n_rows, int n_columns)
    {
        int row = coordinate.r;
        int column = coordinate.c;

        return GetNeighboringCoordinates(row, column, n_rows, n_columns); ;
    }

    /// <summary>
    /// Gets the neighbors as a list of entitys.
    /// </summary>
    /// <param name="node"></param>
    /// <returns> a List of neighbor node entities. </returns>
    public List<NodeEntity> GetNeighbors(NodeEntity node)
    {
        return GetNeighbors(node.row, node.col);
    }

    /// <summary>
    /// Gets the empty connected coordinates.
    /// </summary>
    /// <param name="configurationCoordiates"></param>
    /// <param name="rows"></param>
    /// <param name="cols"></param>
    /// <returns> A list of empty neighbors coordinates. </returns>
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



    /// <summary>
    /// Gets a compressed version of the configuration map.
    /// </summary>
    /// <param name="withConnection"></param>
    /// <returns> A two dimensional occupancy map. </returns>
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

    /// <summary>
    /// Gets a list of cordinates based on the configuration.
    /// </summary>
    /// <param name="withConnection"></param>
    /// <returns> List of coordinates in the configuration. </returns>
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

    /// <summary>
    /// Gets user count map.
    /// </summary>
    /// <returns> A two dimensional occupancy map with intiger values representing users. </returns>
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

    /// <summary>
    /// Converts a coordinate to a node entity.
    /// </summary>
    /// <param name="coordinate"></param>
    /// <returns> A node entity. </returns>
    public NodeEntity GetNode(Coordinate coordinate)
    {
        return configurationMapNodes[coordinate.r, coordinate.c];
    }

    /// <summary>
    /// Gets the node given a row and column
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns> Returns the node entity </returns>
    public NodeEntity GetNode(int row, int column)
    {
        return configurationMapNodes[row, column];
    }

    /// <summary>
    /// Converts a list of coordinates to a list of nodes.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns> A list of nodes. </returns>
    public List<NodeEntity> CoordinatesToNodes(List<Coordinate> coordinates)
    {
        List<NodeEntity> nodeList = new List<NodeEntity>();

        foreach (Coordinate coordinate in coordinates)
        {
            nodeList.Add(GetNode(coordinate));
        }

        return nodeList;
    }

    /// <summary>
    /// Gets the nearest node to a 3d position.
    /// </summary>
    /// <param name="userCountMap"></param>
    /// <returns> The nearest node. </returns>
    public NodeEntity GetNearestNode(Vector3 position)
    {
        NodeEntity nearestNode = null;
        float nearestDistance = Mathf.Infinity;
        float testDistance;
        foreach (NodeEntity node in EntityManager.inst.nodes)
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

    /// <summary>
    /// Gets the nearest node from the tower.
    /// </summary>
    /// <returns> The closest node to the tower </returns>
    public NodeEntity GetNearestNodeFromTower()
    {
        return GetNearestNode(towerPrefab.transform.position);
    }

    /// <summary>
    /// Gets the nearest node and distance from the tower.
    /// </summary>
    /// <returns> A tuple containing the node and the distance of that node. </returns>
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

    /// <summary>
    /// Gets all the nodes near the tower within the connection distace.
    /// </summary>
    /// <returns> All the nodes near the tower. </returns>
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
}
