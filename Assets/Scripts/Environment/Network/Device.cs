using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public Entity entity;
    public float timeConnected;
    public float timeDisconnected;
    public Router connectedRouter;

    public int priority;
    private System.DateTime lastTimeConnected;
    private System.DateTime lastTimeDisconnected;

    public int ID;

    private static int count = 0;

    private void Awake()
    {
        ID = count;
        count = count + 1;
    }

    private void Start()
    {
        priority = Random.Range(1, 4);

        if (entity == null)
        {
            entity = GetComponent<Entity>();
        }
        ID = NetworkManager.inst.devices.Count;
        NetworkManager.inst.devices.Add(ID, this);
    }

    private void Update()
    {
        if (connectedRouter != null)
        {
            timeConnected += Time.deltaTime;
        } else
        {
            timeDisconnected += Time.deltaTime;
        }
    }

    public void ConnectToRouter(Router router)
    {
        connectedRouter = router;
        lastTimeConnected = System.DateTime.Now;
    }

    public System.TimeSpan TimeSinceConnected()
    {
        return System.DateTime.Now - lastTimeConnected;
    }

    public void DisconnectFromRouter()
    {
        connectedRouter = null;
        lastTimeDisconnected = System.DateTime.Now;
    }

    public System.TimeSpan TimeSinceDisconnected()
    {
        return System.DateTime.Now - lastTimeDisconnected;
    }

    public double ComputeImpatientScore()
    {
        return priority * TimeSinceDisconnected().TotalMilliseconds;
    }
}
