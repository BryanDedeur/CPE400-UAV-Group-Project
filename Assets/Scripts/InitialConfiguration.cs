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
        configurationMap.Setup(6f, 5, 5);

        Node nearestNodeToTower = configurationMap.PlaceTower(.5f, .0f);

        // configurationMap.InsertUAV(nearestNodeToTower);

        List<Node> neighbors = configurationMap.GetNeighbors(nearestNodeToTower);

        /*
        foreach (Node node in configurationMap.GetAllNodes())
        {
            GameObject go = configurationMap.InsertUAV(node);
            configurationMap.MoveUAV(go, new Vector3(Random.RandomRange(0,20), 20, Random.RandomRange(0, 20)));

        }
        */

        for (int i = 0; i < 8; ++i)
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
