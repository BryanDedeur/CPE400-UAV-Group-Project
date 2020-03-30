using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BryansPhysics : MonoBehaviour
{
    public float tolerance = .2f;
    public float heading = 0; // could be direction vector
    public float desiredHeading = 0;
    public float turnRate = 30;
    public float acceleration = .01f;
    public float speed = 0;
    public float desiredSpeed = 0;
    public float desiredAltitude = 0;
    public float climbRate = 0;
    private float minSpeed = 0;
    public float maxSpeed = 1;
    public float distanceTraveled = 0;
    private Vector3 displacement = new Vector3();
    private Vector3 calculatedPosition = new Vector3();

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

        if (Mathf.Abs(desiredHeading - heading) >= tolerance * 4f)
        {
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
        } else
        {
            speed = desiredSpeed;
        }

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        distanceTraveled += speed * Time.deltaTime;

        displacement = new Vector3(Mathf.Sin(heading * Mathf.Deg2Rad), 0, Mathf.Cos(heading * Mathf.Deg2Rad)) * speed * Time.deltaTime;


        // -------- compute altitude --------- //
        if (Mathf.Abs(transform.position.y - desiredAltitude) >= tolerance)
        {
            if (transform.position.y < desiredAltitude)
            {
                transform.position += displacement + new Vector3(0, climbRate * Time.deltaTime, 0);
            }
            else if (transform.position.y > desiredAltitude)
            {
                transform.position += displacement - new Vector3(0, climbRate * Time.deltaTime, 0);
            }
        } else
        {
            transform.position += displacement;
        }


        // -------- update position ---------- //
        transform.eulerAngles = new Vector3(0, heading, 0);
    }
}