using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physics : MonoBehaviour
{
    public float heading = 0; // could be direction vector
    public float desiredHeading = 0;
    public float turnRate = 30;
    public float acceleration = .01f;
    public float speed = 0;
    public float desiredSpeed = 0;
    private float minSpeed = 0;
    public float maxSpeed = 1;
    private Vector3 velocity = new Vector3();

    // Update is called once per frame
    void FixedUpdate()
    {


        // -------- compute heading --------- //
        if (desiredHeading > 360)
        {
            desiredHeading = desiredHeading - 360;
        }
        else if (desiredHeading < 0)
        {
            desiredHeading = 360 + desiredHeading;
        }



        if (desiredHeading > heading)
        {
            if (desiredHeading - heading > 180)
            {
                heading -= turnRate * Time.deltaTime;
            }
            else
            {
                heading += turnRate * Time.deltaTime;
            }
        }
        else if (desiredHeading < heading)
        {
            if (desiredHeading - heading < -180)
            {
                heading += turnRate * Time.deltaTime;
            }
            else
            {
                heading -= turnRate * Time.deltaTime;
            }
        }

        if (heading > 360)
        {
            heading = heading - 360;
        }
        else if (heading < 0)
        {
            heading = heading + 360;
        }

        // -------- compute speed ------------ //
        desiredSpeed = Mathf.Clamp(desiredSpeed, minSpeed, maxSpeed);

        if (speed < desiredSpeed)
        {
            speed += acceleration * Time.deltaTime;
        }
        else if (speed > desiredSpeed)
        {
            speed -= acceleration * Time.deltaTime;
        }

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

        velocity = new Vector3(Mathf.Sin(heading * Mathf.Deg2Rad), 0, Mathf.Cos(heading * Mathf.Deg2Rad)) * speed;

        // -------- update position ---------- //
        transform.position += velocity;
    }
}