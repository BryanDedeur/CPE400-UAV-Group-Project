﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialConfiguration : MonoBehaviour
{
    public int numberOfUsers;
    public int numberOfUAVs;
    public int numberOfColumns;
    public int numberOfRows;
    [Range(0,1)]
    public float towerPosX;
    [Range(0, 1)]
    public float towerPosZ;

    private ConfigurationMap configurationMap;
    bool runOnce = false;

    // ---------- EDIT THIS FOR INITIAL ENVIRONMENT SETUP ----------- //
    private void Setup()
    {
        // Specify the configuration map settings
        configurationMap.Setup(6f, numberOfRows, numberOfColumns);

        Node nearestNodeToTower = configurationMap.PlaceTower(towerPosX, towerPosZ);

        List<Node> neighbors = configurationMap.GetNeighbors(nearestNodeToTower);

        for (int i = 0; i < numberOfUAVs; ++i)
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
