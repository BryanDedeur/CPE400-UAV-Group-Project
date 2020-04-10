using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStopStartDirectionalMovement : Command
{ 
    private float timeUntilNextSpeedUpdate = 0;
    private float timeUntilNextRotation;
    private float zMax;
    private float zMin;
    private float xMax;
    private float xMin;
    private bool bounded = false;

    public RandomStopStartDirectionalMovement(Entity ent) : base(ent)
    {
        entity = ent;
    }

    public RandomStopStartDirectionalMovement(Entity ent, float minX, float maxX, float minZ, float maxZ) : base(ent)
    {
        entity = ent;
        zMax = maxZ;
        zMin = minZ;
        xMax = maxX;
        xMin = minX;
        bounded = true;
    }

    public override void Init()
    {
    }

    public override void Tick()
    {
        if (timeUntilNextSpeedUpdate < 0)
        {
            timeUntilNextSpeedUpdate = Random.Range(0, 20);
            if (Random.Range(0, 2) == 0)
            {
                entity.physics.desiredSpeed = entity.physics.maxSpeed;
            }
            else
            {
                entity.physics.desiredSpeed = 0;
            }
        }
        timeUntilNextSpeedUpdate -= Time.deltaTime;

        if (timeUntilNextRotation < 0)
        {
            timeUntilNextRotation = Random.Range(0, 20);
            entity.physics.desiredHeading = Random.Range(0, 360);
        }
        timeUntilNextRotation -= Time.deltaTime;

        if (bounded)
        {
            if ((entity.transform.position.x > xMax || entity.transform.position.x < xMin || entity.transform.position.z > zMax || entity.transform.position.z < zMin))
            {
                entity.transform.position = new Vector3(Mathf.Clamp(entity.transform.position.x, xMin, xMax), entity.transform.position.y, Mathf.Clamp(entity.transform.position.z, zMin, zMax));
                entity.physics.desiredHeading = Mathf.Rad2Deg * (Mathf.Atan2(0 - entity.transform.position.x, (0 - entity.transform.position.z)));
                entity.physics.desiredSpeed = entity.physics.maxSpeed;
                timeUntilNextRotation = Random.Range(0, 20);
                timeUntilNextSpeedUpdate = Random.Range(0, 20);
            }
        }
    }

    public override bool IsDone()
    {
        return false;
    }

    public override void Stop()
    {

    }
}

