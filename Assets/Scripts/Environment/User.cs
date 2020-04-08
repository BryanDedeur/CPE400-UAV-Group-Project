using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    public NetworkRouter connectedRouter;
    public Node nearestNode;
    private int ID;
    public int priority;
    private System.DateTime lastTimeConnected;
    private System.DateTime lastTimeDisconnected;

    static private int IDcounter = 0;

    public int GetID()
    {
        return ID;
    }

    public void ConnectToRouter(NetworkRouter router)
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

    // Start is called before the first frame update
    void Awake()
    {
        ID = IDcounter++;
        connectedRouter = null;
        nearestNode = null;
    }

    private void Start()
    {
        lastTimeConnected = System.DateTime.Now;
        lastTimeDisconnected = System.DateTime.Now;
        priority = Random.Range(1, 3);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
