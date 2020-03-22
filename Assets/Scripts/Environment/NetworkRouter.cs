using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkRouter : MonoBehaviour
{
    static private int maximumUserCapacity = 5;
    static private float userConnectionDistance = 10f;
    static private int IDcounter = 0;

    public ConfigurationMap cm;
    public Dictionary<int, NetworkRouter> connectedRouters;
    public List<NetworkRouter> displayingConnectedRouters;
    private LineRenderer lr;

    private int ID;
    private int connectionLength = 0;
    private NetworkRouter parentRouter = null;
    private int userServing = 0;

    private struct Node_Astar
    {
        public int pathCost;
        public int heuristicCost;

        public Node_Astar(int new_pathCost, int new_heuristicCost)
        {
            pathCost = new_pathCost;
            heuristicCost = new_heuristicCost;
        }
    }

    public int GetID()
    {
        return ID;
    }

    void Start()
    {
        connectedRouters = new Dictionary<int, NetworkRouter>();
        displayingConnectedRouters = new List<NetworkRouter>();
        ID = IDcounter++;
        cm.allRouters.Add(this.ID, this);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.parent != null)
        {
            userServing = gameObject.GetComponentInParent<Node>().numberUsers;
        } else
        {
            userServing = 0;
        }

        displayingConnectedRouters.Clear();
        cm.GetNearbyRouters(this);
        foreach (KeyValuePair<int, NetworkRouter> connection in connectedRouters)
        {
            displayingConnectedRouters.Add(connection.Value);
            Debug.DrawLine(transform.position, connection.Value.transform.position, Color.red);
        }
    }

    void ComputeTransmissionPath_AStar(ref NetworkRouter tower)
    {
        Dictionary<int, Node_Astar> visitedNodes = new Dictionary<int, Node_Astar>();
        Dictionary<int, Node_Astar> unvisitedNodes = new Dictionary<int, Node_Astar>();

        foreach (KeyValuePair<int, NetworkRouter> router in cm.allRouters)
        {
            if (router.Value.name == "Tower")
            {
                unvisitedNodes.Add(router.Value.GetID(), new Node_Astar(0, 0));
            }
            else
            {
                unvisitedNodes.Add(router.Value.GetID(), new Node_Astar(int.MaxValue, int.MaxValue));
            }
        }

        // ref NetworkRouter currentRouter = ref tower;
        while(unvisitedNodes.Count > 0)
        {
            //Stop when all nodes are visited or the smallest cost node is infinite.
            int lowestHeuristicCost = int.MaxValue;
            int currentID = 0;
            foreach (KeyValuePair<int , Node_Astar> entry in unvisitedNodes)
            {
                if (entry.Value.heuristicCost < lowestHeuristicCost)
                {
                    lowestHeuristicCost = entry.Value.heuristicCost;
                    currentID = entry.Key;
                }
            }


        }
    }
}
