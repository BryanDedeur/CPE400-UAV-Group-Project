using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Router : MonoBehaviour
{
    public UAVEntity entity;
    static private double maximumMillisecondTimerPerPriority = 250.0f;

    public Color routerToDeviceColor = Color.red;
    public Color routerToRouterColor = Color.blue;

    public float connectionRadius = 5f;
    public int maximumDeviceCapacity = 2;

    public float timeConnected;
    public Dictionary<int, Router> connectedRouters;
    public Dictionary<int, Device> connectedDevices;

    public Router parentRouter;
    public int numberOfHops;
    public bool disconnected = true;

    public float disconnectTimer = 5;
    private float disconnectTimeRemaining;

    private float disconnectCheckTimeRemaining;
    public float AStarConnectionLength;

    private static int count = 0;
    private int ID = count;

    private void Awake()
    {
        count = count + 1;
    }

    public int GetID()
    {
        return ID;
    }

    private void Start()
    {
        connectedRouters = new Dictionary<int, Router>();
        connectedDevices = new Dictionary<int, Device>();

        if (entity == null)
        {
            entity = GetComponent<UAVEntity>();
        }

        if (name == "Tower")
        {
            NetworkManager.inst.tower = this;
        }

        ID = NetworkManager.inst.routers.Count;
        NetworkManager.inst.routers.Add(ID, this);
        connectionRadius = NetworkManager.inst.connectionRadius;
    }

    private void Update()
    {

        if (connectedRouters.Count > 0)
        {
            timeConnected += Time.deltaTime;
        }

        // TODO move this code into clearly defined functions
        // TODO router probably wont ever care about the entity.entity.battery values, only on and off

        NetworkManager.inst.GetNearbyRouters(this);
        //displayingConnectedRouters = connectedRouters.Values.ToList();

        if (numberOfHops > 0)
        {
            disconnectTimeRemaining = disconnectTimer;
            if (entity.battery == null)
            {
                entity.battery = GetComponent<Battery>();
            }
            if (entity.battery.running)
            {
                foreach (KeyValuePair<int, Router> connection in connectedRouters)
                {
                    Debug.DrawLine(transform.position, connection.Value.transform.position, routerToRouterColor, 0);
                }

                if (connectedDevices.Count > 0)
                {
                    List<int> removingIDs = new List<int>();
                    foreach (KeyValuePair<int, Device> deviceKeyPair in connectedDevices)
                    {
                        if (entity.assignedNode != null)
                        {
                            if (deviceKeyPair.Value.TimeSinceConnected().TotalMilliseconds > maximumMillisecondTimerPerPriority * deviceKeyPair.Value.priority || !entity.assignedNode.usersInRange.Contains(deviceKeyPair.Value.entity) || (deviceKeyPair.Value.transform.position - transform.position).magnitude > NetworkManager.inst.connectionRadius)
                            {
                                removingIDs.Add(deviceKeyPair.Key);
                            }
                            else
                            {
                                Debug.DrawLine(transform.position, deviceKeyPair.Value.transform.position, routerToDeviceColor, 0);
                            }
                        }
                    }
                    foreach (int id in removingIDs)
                    {
                        connectedDevices[id].DisconnectFromRouter();
                        connectedDevices.Remove(id);
                    }
                }
                if (connectedDevices.Count < maximumDeviceCapacity)
                {
                    double highestImpatientScore = 0;
                    int impatientUserIndex = -1;
                    if (entity.assignedNode != null && entity.assignedNode.usersInRange.Count > 0)
                    {
                        for (int i = 0; i < entity.assignedNode.usersInRange.Count; ++i)
                        {
                            if ((entity.assignedNode.usersInRange[i].transform.position - transform.position).magnitude < NetworkManager.inst.connectionRadius)
                            {
                                if (!connectedDevices.ContainsKey(entity.assignedNode.usersInRange[i].GetID()))
                                {
                                    double impatientScore = entity.assignedNode.usersInRange[i].device.ComputeImpatientScore();
                                    if (highestImpatientScore < impatientScore)
                                    {
                                        highestImpatientScore = impatientScore;
                                        impatientUserIndex = i;
                                    }
                                }
                            }
                        }
                    }

                    if (impatientUserIndex >= 0 && (entity.assignedNode.usersInRange[impatientUserIndex].transform.position - transform.position).magnitude < NetworkManager.inst.connectionRadius)
                    {
                        connectedDevices.Add(entity.assignedNode.usersInRange[impatientUserIndex].GetID(), entity.assignedNode.usersInRange[impatientUserIndex].device);
                        entity.assignedNode.usersInRange[impatientUserIndex].device.ConnectToRouter(this);
                    }
                }

            }
        } else if (numberOfHops == 0)
        {
            if (entity != null && entity.name != "Tower" && entity.battery != null && disconnectTimeRemaining < 0 && entity.battery.batteryLife > entity.battery.batteryReserveThreshold)
            {
                disconnected = true;
                disconnectTimeRemaining = disconnectTimer;
                ConfigurationMap.inst.SendUAVToTower(entity);
            }
            disconnectTimeRemaining -= Time.deltaTime;

        }
    }
}


