using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialConfiguration : MonoBehaviour
{
    public int numberOfUsers;

    private ConfigurationMap configurationMap;
    bool runOnce = false;

    // ---------- EDIT THIS FOR INITIAL ENVIRONMENT SETUP ----------- //
    private void Setup()
    {
        // Specify the configuration map settings
        configurationMap.Setup(6f, 11, 10);

        Node nearestNodeToTower = configurationMap.PlaceTower(.5f, .1f);

        List<Node> neighbors = configurationMap.GetNeighbors(nearestNodeToTower);

        for (int i = 0; i < 15; ++i)
        {
            GameObject go = configurationMap.InsertUAV();
        }

        for (int i = 0; i < numberOfUsers; ++i)
        {
            configurationMap.InsertUser(5);
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
