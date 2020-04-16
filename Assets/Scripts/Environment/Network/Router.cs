using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Router : MonoBehaviour
{
    public UAVEntity entity;
    static public double maximumMillisecondTimerPerPriority = 250.0f;

    public Color routerToDeviceColor = Color.red;
    public Color routerToRouterColor = Color.blue;

    public float connectionRadius = 5f;
    public int maximumDeviceCapacity = 5;

    public float timeConnected;
    public Dictionary<int, Router> connectedRouters;
    public Dictionary<int, Device> connectedDevices;

    public Router parentRouter;
    public int numberOfHops;
    public int numberOfUsersServing;

    public float disconnectTimer = 5;
    private float disconnectTimeRemaining;

    public float AStarConnectionLength;

    private static int count = 0;
    private int ID = count;

    private void Awake()
    {
        count += 1;
    }

    public int GetID()
    {
        return ID;
    }

    /// <summary>
    /// Determines if one transform is within the range of another transform
    /// </summary>
    /// <param name="a"> A transform </param>
    /// <param name="b"> Another transform </param>
    /// <param name="ignoreY"> To ignore the y component of the vector 3 positions </param>
    /// <returns> Whether the transform is in range or not</returns>
    public bool IsInRange(Transform a, Transform b, bool ignoreY)
    {
        if (ignoreY)
        {
            // compared against squared magnitude for runtime efficiency and ignore the y position.
            return (a.position - new Vector3(b.position.x, a.position.y, b.position.z)).sqrMagnitude <= NetworkManager.inst.connectionRadiusSquared;
        }

        // compared against squared magnitude for runtime efficiency.
        return (a.position - b.position).sqrMagnitude <= NetworkManager.inst.connectionRadiusSquared;
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

        NetworkManager.inst.GetNearbyRouters(this);

        // Keep track of the total number of users the UAV is serving (the users are not served all the time when there are more users than UAV's capacity).
        numberOfUsersServing = 0;
        if (name != "Tower" && entity.assignedNode != null)
        {
            foreach (UserEntity user in entity.assignedNode.usersInRange)
            {
                // If there is a path to the control center and the user is within the connection radius
                if (numberOfHops > 0 && IsInRange(user.transform, transform, true))
                {
                    ++numberOfUsersServing;
                }
            }
        }

        // If the UAV is connected to the internet.
        if (numberOfHops > 0)
        {
            // Reset the disconnect timer.
            disconnectTimeRemaining = disconnectTimer;

            if (entity.battery == null)
            {
                entity.battery = GetComponent<Battery>();
            }
            // If the UAV is still running.
            if (entity.battery.running)
            {
                // Draw connection line to the nearby connected routers.
                foreach (KeyValuePair<int, Router> connection in connectedRouters)
                {
                    //Debug.DrawLine(transform.position, connection.Value.transform.position, routerToRouterColor, 0);
                    LineManager.inst.DrawLine(transform.position, connection.Value.transform.position, routerToRouterColor);
                }

                // If there are users currently being served.
                if (connectedDevices.Count > 0)
                {
                    // Look for users to remove from the list of user serving.
                    List<int> removingIDs = new List<int>();
                    foreach (KeyValuePair<int, Device> deviceKeyPair in connectedDevices)
                    {
                        if (entity.assignedNode != null)
                        {
                            // If user is served longer than the allotted time or the user is moves out of the assigned coordinate or user moves out of connection range.
                            if (deviceKeyPair.Value.TimeSinceConnected().TotalMilliseconds > maximumMillisecondTimerPerPriority * deviceKeyPair.Value.priority || !entity.assignedNode.usersInRange.Contains(deviceKeyPair.Value.entity) || !IsInRange(deviceKeyPair.Value.transform, transform, true))
                            {
                                // Mark that user to remove later.
                                removingIDs.Add(deviceKeyPair.Key);
                            }
                            else // Draw connection line to the remaining eligible users.
                            {
                                LineManager.inst.DrawLine(transform.position, deviceKeyPair.Value.transform.position, routerToDeviceColor);
                            }
                        }
                    }
                    // Remove the marked users from the user serving list.
                    foreach (int id in removingIDs)
                    {
                        connectedDevices[id].DisconnectFromRouter();
                        connectedDevices.Remove(id);
                    }
                }
                
                // If there are spaces to serve more users.
                if (connectedDevices.Count < maximumDeviceCapacity)
                {
                    // Find the user who has the highest impatient score.
                    double highestImpatientScore = 0;
                    int impatientUserIndex = -1;
                    if (entity.assignedNode != null && entity.assignedNode.usersInRange.Count > 0)
                    {
                        // For each user under the assigned coordinate.
                        for (int i = 0; i < entity.assignedNode.usersInRange.Count; ++i)
                        {
                            // User is within connection range.
                            if (IsInRange(entity.assignedNode.usersInRange[i].transform, transform, true))
                            {
                                // If that user is not currently being served.
                                if (!connectedDevices.ContainsKey(entity.assignedNode.usersInRange[i].GetID()))
                                {
                                    // Compute the impatient score = priority * duration of time disconnected from the internet.
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

                    // If new user is found.
                    if (impatientUserIndex >= 0 && IsInRange(entity.assignedNode.usersInRange[impatientUserIndex].transform, transform, true))
                    {
                        // Add that user to the connection list.
                        connectedDevices.Add(entity.assignedNode.usersInRange[impatientUserIndex].GetID(), entity.assignedNode.usersInRange[impatientUserIndex].device);
                        // Connect user to the UAV's router.
                        entity.assignedNode.usersInRange[impatientUserIndex].device.ConnectToRouter(this);
                    }
                }

            }
        }
        
        // If UAV is disconnected from the internet.
        else if (numberOfHops == 0)
        {
            if (entity != null && entity.name != "Tower")
            {
                // Disconnect user from the internet.
                foreach (KeyValuePair<int, Device> device in connectedDevices)
                {
                    device.Value.DisconnectFromRouter();
                }
                connectedDevices.Clear();

                // Wait for certain duration of time before moving closer to the tower to connect to the internet and receive instructions from the tower.
                if (entity.battery != null && disconnectTimeRemaining < 0 && entity.battery.batteryLife > entity.battery.batteryReserveThreshold)
                {
                    disconnectTimeRemaining = disconnectTimer;
                    ConfigurationMap.inst.SendUAVToTower(entity);
                }
            }
            disconnectTimeRemaining -= Time.deltaTime;

        }
    }
}


