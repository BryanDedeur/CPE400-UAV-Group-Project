using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    private Entity entity;

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
        if (batteryLife <= 0 || running == false)
        {
            running = false;
            batteryLife = 0;
            entity.physics.desiredSpeed = 0;
            entity.physics.desiredAltitude = 0;
            entity.ai.rejectInstructions = true;
        } else
        {
            batteryLife -= batteryDrainRateConstant * Time.deltaTime;
        }

        if (entity.physics.speed > 0)
        {
            batteryLife -= ((entity.physics.speed / entity.physics.maxSpeed) * batteryDrainRateRelativeToSpeed) * Time.deltaTime;
        } 

        if (entity.router.connectedDevices.Count > 0)
        {
            batteryLife -= (entity.router.connectedDevices.Count * batteryDrainRateServingUsers) * Time.deltaTime;
        }

        if (batteryLife < batteryReserveThreshold)
        {
            ConfigurationMap.inst.DecomissionUAV(entity);
        }

    }

    /*
    private BryansPhysics physics;
    private NetworkRouter router;
    public bool running = true;

    public bool requestStop = false;

    // Start is called before the first frame update
    void Start()
    {
        physics = transform.GetComponent<BryansPhysics>();
        router = transform.GetComponent<NetworkRouter>();
        running = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (batteryLife <= 0 || requestStop)
        {
            physics.desiredSpeed = 0;
            physics.desiredHeading = 0;
            physics.desiredAltitude = 0;
        } 
        else
        {
            if (physics.speed > 0)
            {
                batteryLife -= (((physics.speed / physics.maxSpeed) * batteryDrainRateRelativeToSpeed) * Time.deltaTime);
            }
            batteryLife -= (((router.userServing.Count * batteryDrainRateServingUsers) * Time.deltaTime) + (batteryDrainRateConstant * Time.deltaTime));
        }
    }
    */
}
