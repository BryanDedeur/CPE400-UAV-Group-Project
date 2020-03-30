using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    public float batteryLife = 1;
    public float batteryDrainRateRelativeToSpeed;
    public float batteryDrainRateServingUsers;
    public float batteryDrainRateConstant;

    private BryansPhysics physics;
    private NetworkRouter router;
    private bool running;

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
        if (batteryLife <= 0)
        {
            if (running)
            {
                router.cm.allRouters.Remove(router.GetID());
                router.connectedRouters.Clear();
                router.transform.parent.GetComponent<Node>().UAV = null;
                router.transform.parent = router.transform.parent.transform.parent;
                running = false;
            }
            physics.desiredSpeed = 0;
            physics.desiredHeading = 0;
            physics.desiredVerticalHeading = 0;
        } 
        else
        {
            if (physics.speed > 0)
            {
                batteryLife -= physics.speed / physics.maxSpeed * batteryDrainRateRelativeToSpeed * Time.deltaTime;
            }
            batteryLife -= router.numberOfUsers * batteryDrainRateServingUsers * Time.deltaTime + batteryDrainRateConstant * Time.deltaTime;
        }
    }
}
