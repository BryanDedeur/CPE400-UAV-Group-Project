﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTo : Command
{
    public Entity entity;
    protected Vector3 targetPosition;
    protected float doneDistanceSquared = .04f;
    protected float targetSpeed = 0;
    protected Vector3 differenceVector;

    protected float computedHeading;
    protected float timeToStop;
    protected float stoppingDistance;
    protected float magnitude;

    public MoveTo(Entity ent, Vector3 position) : base(ent)
    {
        entity = ent;
        targetPosition = position;
        targetSpeed = entity.physics.maxSpeed;
    }

    public MoveTo(Entity ent, Vector3 position, float targetVelocity) : base(ent)
    {
        entity = ent;
        targetPosition = position;
        targetSpeed = targetVelocity;
    }

    public override void Init()
    {

    }

    public override void Tick()
    {
        timeToStop = (entity.physics.speed) / (entity.physics.acceleration);
        stoppingDistance = entity.physics.speed * timeToStop + .5f * entity.physics.acceleration * Mathf.Pow(timeToStop, 2f);

        magnitude = (entity.transform.position - targetPosition).magnitude;

        computedHeading = Mathf.Rad2Deg * (Mathf.Atan2(targetPosition.x - entity.transform.position.x, (targetPosition.z - entity.transform.position.z)));

        entity.physics.desiredSpeed = entity.physics.maxSpeed;
        entity.physics.desiredAltitude = targetPosition.y;


        if (computedHeading < 0)
        {
            computedHeading += 360;
        }
        else if (computedHeading > 360)
        {
            computedHeading = computedHeading - 360;
        }

        entity.physics.desiredHeading = computedHeading;

        magnitude = (entity.transform.position - targetPosition).magnitude;
        computedHeading = Mathf.Rad2Deg * (Mathf.Atan2(targetPosition.x - entity.transform.position.x, (targetPosition.z - entity.transform.position.z)));
        if (magnitude <= (stoppingDistance))
        {
            entity.physics.desiredSpeed = 0;
            if (magnitude < .1f)
            {
                entity.physics.desiredSpeed = 0;
            }
        }
        else
        {
            entity.physics.desiredSpeed = entity.physics.maxSpeed;
        }
        entity.physics.desiredAltitude = targetPosition.y;

    }

    public override bool IsDone()
    {
        differenceVector = (entity.transform.position - targetPosition);
        return (differenceVector.sqrMagnitude < doneDistanceSquared);
    }

    public override void Stop()
    {
        entity.physics.desiredSpeed = 0;
    }

}
