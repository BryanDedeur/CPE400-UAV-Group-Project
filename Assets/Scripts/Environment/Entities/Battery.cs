﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    private Entity entity;
    public bool staticBattery = false;
    [Range(0,1)]
    public float batteryLife = 1;
    [Range(0, 1)]
    public float batteryReserveThreshold = .2f;

    public float batteryDrainRateRelativeToSpeed = 0.005f;
    public float batteryDrainRateServingUsers = 0.005f;
    public float batteryDrainRateConstant = 0.001f;

    public bool running = true;

    private void Awake()
    {
        entity = GetComponent<Entity>();
    }

    void Update()
    {
        if (!staticBattery)
        {
            // If battery is dead or UAV is turned off.
            if (batteryLife <= 0 || running == false)
            {
                // Turn off UAV (if not already turned off)
                running = false;
                batteryLife = 0;
                // Stop UAV from flying.
                entity.physics.desiredSpeed = 0;
                entity.physics.desiredAltitude = 0;
                // Prevents UAV from receiving any instruction from the tower.
                entity.ai.rejectInstructions = true;
            }
            else // Drain the battery by some constant rate.
            {
                batteryLife -= batteryDrainRateConstant * Time.deltaTime;
            }
            // If UAV is moving.
            if (entity.physics.speed > 0)
            {
                // Drain battery by the speed of movement and the drainage rate.
                batteryLife -= ((entity.physics.speed / entity.physics.maxSpeed) * batteryDrainRateRelativeToSpeed) * Time.deltaTime;
            }
            // If UAV is serving users.
            if (entity.router.connectedDevices.Count > 0)
            {
                // Drain the battery by the number of uers being served and the drainage rate.
                batteryLife -= (entity.router.connectedDevices.Count * batteryDrainRateServingUsers) * Time.deltaTime;
            }
            // If UAV is almost out of battery, decomission the UAV.
            if (batteryLife < batteryReserveThreshold)
            {
                ConfigurationMap.inst.DecomissionUAV(entity);
            }
        }
    }
}
