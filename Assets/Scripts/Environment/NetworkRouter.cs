using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRouter : MonoBehaviour
{
    public ConfigurationMap cm;
    public List<NetworkRouter> connectedRouters;
    private LineRenderer lr;

    void Start()
    {
        connectedRouters = new List<NetworkRouter>();
        cm.allRouters.Add(this);

    }

    // Update is called once per frame
    void Update()
    {

        cm.GetNearbyRouters(this);
        foreach (NetworkRouter connection in connectedRouters)
        {
            Debug.DrawLine(transform.position, connection.transform.position, Color.red);
        }
    }
}
