using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialConfiguration : MonoBehaviour
{
    private ConfigurationMap configurationMap;

    // ---------- EDIT THIS FOR INITIAL ENVIRONMENT SETUP ----------- //
    private void Setup()
    {
        // Specify the configuration map settings
        configurationMap.Setup(2f, 5, 5);

        Node nearestNodeToTower = configurationMap.PlaceTower(.5f, .1f);

        configurationMap.InsertUAV(nearestNodeToTower);

        List<Node> neighbors = configurationMap.GetNeighbors(nearestNodeToTower);

        for (int i = 0; i < neighbors.Count; i++)
        {
            configurationMap.InsertUAV(neighbors[i]);
        }

    }

    // ---------- DONT TOUCH ----------- //
    private void Start()
    {
        configurationMap = GetComponent<ConfigurationMap>();
        Setup();
    }
}
