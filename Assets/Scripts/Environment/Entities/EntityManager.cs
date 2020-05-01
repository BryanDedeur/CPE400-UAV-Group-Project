using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager inst;
    private void Awake()
    {
        inst = this;
    }

    private GameObject nodeFolder;
    private GameObject nodePrefab;
    public List<NodeEntity> nodes;

    private GameObject uavFolder;
    private GameObject uavPrefab;
    public List<UAVEntity> uavs;

    private GameObject userFolder;
    private GameObject userPrefab;
    public List<UserEntity> users;
    public void ToggleUsers(bool state)
    {
        foreach (UserEntity user in users)
        {
            user.transform.gameObject.GetComponentInChildren<MeshRenderer>().enabled = state;
        }
    }
    public void ToggleUAVs(bool state)
    {
        foreach (UAVEntity uav in uavs)
        {
            uav.transform.gameObject.GetComponent<MeshRenderer>().enabled = state;
        }
    }
    public void ToggleNodeUserConnections(bool state)
    {
        foreach (NodeEntity node in nodes)
        {
            node.renderUsersInRange = state;
        }
    }
    public void ToggleBatteryDrainage(bool state)
    {
        foreach (UAVEntity uav in uavs)
        {
            uav.battery.staticBattery = state;
        }
    }


    private void Start()
    {
        if (nodeFolder == null)
        {
            nodeFolder = new GameObject();
            nodeFolder.name = "NodeFolder";
        }

        if (uavFolder == null)
        {
            uavFolder = new GameObject();
            uavFolder.name = "UAVFolder";
        }

        if (userFolder == null)
        {
            userFolder = new GameObject();
            userFolder.name = "UserFolder";
        }
    }

    /// <summary>
    /// Creates a entity of type node at a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns> The newly created entity. </returns>
    public NodeEntity CreateNode(Vector3 atPos)
    {
        if (nodePrefab == null)
        {
            nodePrefab = Resources.Load("Node") as GameObject;
        }

        GameObject gameObj = Instantiate(nodePrefab, atPos, new Quaternion());
        gameObj.transform.parent = nodeFolder.transform;
        NodeEntity node = gameObj.GetComponent<NodeEntity>();
        node.transform.name = "Node" + nodes.Count;
        nodes.Add(node);

        return node;
    }

    /// <summary>
    /// Creates a entity of type uav at a given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns> The newly created entity. </returns>
    public UAVEntity CreateUAV(Vector3 atPos)
    {
        if (uavPrefab == null)
        {
            uavPrefab = Resources.Load("UAV") as GameObject;
        }

        GameObject gameObj = Instantiate(uavPrefab, atPos, new Quaternion());
        gameObj.transform.parent = uavFolder.transform;
        UAVEntity uav = gameObj.GetComponent<UAVEntity>();
        uav.ai = gameObj.GetComponent<UnitAI>();
        uav.physics = gameObj.GetComponent<OrientedPhysics>();
        uav.router = gameObj.GetComponent<Router>();

        uav.transform.name = "UAV" + uavs.Count;
        uavs.Add(uav);

        return uav;
    }

    /// <summary>
    /// Creates a entity of type user at a given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns> The newly created entity. </returns>
    public UserEntity CreateUser(Vector3 atPos, float rotation)
    {
        if (userPrefab == null)
        {
            userPrefab = Resources.Load("User") as GameObject;
        }

        // TODO fix the setting of rotation

        GameObject gameObj = Instantiate(userPrefab, atPos, new Quaternion());
        gameObj.transform.eulerAngles = new Vector3(0, rotation, 0);
        gameObj.transform.parent = userFolder.transform;
        UserEntity user = gameObj.GetComponent<UserEntity>();
        user.ai = gameObj.GetComponent<UnitAI>();
        user.physics = gameObj.GetComponent<OrientedPhysics>();
        user.device = gameObj.GetComponent<Device>();

        user.transform.name = "User" + users.Count;
        users.Add(user);

        return user;
    }



}
