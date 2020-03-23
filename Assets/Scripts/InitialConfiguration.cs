using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialConfiguration : MonoBehaviour
{
    private ConfigurationMap configurationMap;
    bool runOnce = false;

    // ---------- EDIT THIS FOR INITIAL ENVIRONMENT SETUP ----------- //
    private void Setup()
    {
        // Specify the configuration map settings
        configurationMap.Setup(6f, 7, 10);

        Node nearestNodeToTower = configurationMap.PlaceTower(.5f, .0f);

        configurationMap.InsertUAV(nearestNodeToTower);

        List<Node> neighbors = configurationMap.GetNeighbors(nearestNodeToTower);

        //for (int i = 0; i < neighbors.Count; i++)
        //{
        //    configurationMap.InsertUAV(neighbors[i]);
        //}

        foreach (Node node in configurationMap.GetAllNodes())
        {
            configurationMap.InsertUAV(node);

        }


    }

    // ---------- DONT TOUCH ----------- //
    private void Start()
    {
        configurationMap = GetComponent<ConfigurationMap>();
        Setup();
    }

    
    // MOVING UAV SAMPLE
    private void Update()
    {
        if (!runOnce)
        {
            runOnce = configurationMap.MoveUAV(configurationMap.GetNode(2, 0), configurationMap.GetNode(4, 4));
        }
    }
}
