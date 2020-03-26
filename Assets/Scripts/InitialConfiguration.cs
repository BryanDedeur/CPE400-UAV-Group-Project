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
        configurationMap.Setup(6f, 4, 6);

        Node nearestNodeToTower = configurationMap.PlaceTower(.5f, .0f);

        List<Node> neighbors = configurationMap.GetNeighbors(nearestNodeToTower);

        for (int i = 0; i < 5; ++i)
        {
            GameObject go = configurationMap.InsertUAV();
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

    }
}
