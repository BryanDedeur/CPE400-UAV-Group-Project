using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICommands : MonoBehaviour
{
    public Queue<Command> commands;
    public enum CommandType { MoveTo, BoundedRandomStopStartDirectionalMovement };
    private BryansPhysics physics;

    private void Awake()
    {
        commands = new Queue<Command>();
    }

    public abstract class Command
    {
        public Vector3 targetPos;
        public GameObject targetObject = null;
        public bool isFinished = false;
        public BryansPhysics physics;
        public Transform transform;

        public virtual void UpdateBryansPhysics() { }
    }

    public class MoveTo : Command
    {
        private float timeToStop;
        private float stoppingDistance = 10;
        float computedHeading = 0;
        float magnitude;

        public override void UpdateBryansPhysics() {
            if (physics == null)
            {
                physics = transform.GetComponent<BryansPhysics>();
            }

            timeToStop = physics.speed / (physics.acceleration);
            //stoppingDistance = (25f * physics.acceleration * (Mathf.Pow(timeToStop, 2f)));
            stoppingDistance = Mathf.Pow(physics.speed, 2) / (2 * physics.acceleration * Time.deltaTime);
            //stoppingDistance = timeToStop;

            if (targetObject != null)
            {
                magnitude = (transform.position - targetObject.transform.position).magnitude;
                computedHeading = Mathf.Rad2Deg * (Mathf.Atan2(targetObject.transform.position.x - transform.position.x, (targetObject.transform.position.z - transform.position.z)));
                if (magnitude <= (stoppingDistance))
                {
                    physics.desiredSpeed = physics.acceleration;
                    if (magnitude < .5f)
                    {
                        physics.desiredSpeed = 0;
                        isFinished = true;
                    }
                } else
                {
                    physics.desiredSpeed = physics.maxSpeed;
                }
                physics.desiredVerticalHeading = 45f + Mathf.Rad2Deg * (Mathf.Atan2(transform.position.y, targetObject.transform.position.y));

            }
            else
            {
                magnitude = (transform.position - targetPos).magnitude;

                computedHeading = Mathf.Rad2Deg * (Mathf.Atan2(targetPos.x - transform.position.x, (targetPos.z - transform.position.z)));
                if (magnitude <= (stoppingDistance))
                {
                    physics.desiredSpeed = physics.acceleration;
                    if (magnitude < 0.3f)
                    {
                        physics.desiredSpeed = 0;
                        isFinished = true;
                    }
                }
                else
                {
                    physics.desiredSpeed = physics.maxSpeed;
                }
                physics.desiredVerticalHeading = 45f + Mathf.Rad2Deg * (Mathf.Atan2(targetPos.y, transform.position.y));


            }

            if (computedHeading < 0)
            {
                computedHeading += 360;
            } else if (computedHeading > 360)
            {
                computedHeading = computedHeading - 360;
            }


            physics.desiredHeading = computedHeading;

        }
    }

    public class BoundedRandomStopStartDirectionalMovement : Command
    {
        private float timeUntilNextSpeedUpdate = 0;
        private float timeUntilNextRotation;
        public float zMax;
        public float zMin;
        public float xMax;
        public float xMin;

        public override void UpdateBryansPhysics()
        {
            if (physics == null)
            {
                physics = transform.GetComponent<BryansPhysics>();
            }

            if (timeUntilNextSpeedUpdate < 0)
            {
                timeUntilNextSpeedUpdate = Random.Range(0, 20);
                if (Random.Range(0,2) == 0)
                {
                    physics.desiredSpeed = physics.maxSpeed;
                } else
                {
                    physics.desiredSpeed = 0;
                }
            }
            timeUntilNextSpeedUpdate -= Time.deltaTime;

            if (timeUntilNextRotation < 0)
            {
                timeUntilNextRotation = Random.Range(0, 20);
                physics.desiredHeading = Random.Range(0, 360);
            }
            timeUntilNextRotation -= Time.deltaTime;

            if ((transform.position.x > xMax || transform.position.x < xMin || transform.position.z > zMax || transform.position.z < zMin))
            {
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, xMin, xMax), transform.position.y, Mathf.Clamp(transform.position.z, zMin, zMax));
                physics.desiredHeading = Mathf.Rad2Deg * (Mathf.Atan2(0 - transform.position.x, (0 - transform.position.z)));
                physics.desiredSpeed = physics.maxSpeed;
                timeUntilNextRotation = Random.Range(0, 20);
                timeUntilNextSpeedUpdate = Random.Range(0, 20);
            }
        }
    }


    public void AddCommand(CommandType commandType, GameObject gameObject)
    {
        Command command = new MoveTo();
        switch (commandType)
        {
            case CommandType.MoveTo:
                command = new MoveTo();
                command.targetObject = gameObject;
                break;
            default:
                break;
        }

        command.transform = transform;
        command.physics = physics;
        commands.Enqueue(command);
    }

    public void AddCommand(CommandType commandType, Vector3 position)
    {
        Command command = new MoveTo();
        switch (commandType)
        {
            case CommandType.MoveTo:
                command = new MoveTo();
                command.targetPos = position;
                break;
            default:
                break;
        }

        command.transform = transform;
        command.physics = physics;
        commands.Enqueue(command);
    }

    public void AddCommand(CommandType commandType, float minX, float maxX, float minZ, float maxZ)
    {
        Command command = new BoundedRandomStopStartDirectionalMovement();
        switch (commandType)
        {
            case CommandType.BoundedRandomStopStartDirectionalMovement:
                BoundedRandomStopStartDirectionalMovement subClassCommand = new BoundedRandomStopStartDirectionalMovement();
                subClassCommand.xMin = minX;
                subClassCommand.xMax = maxX;
                subClassCommand.zMin = minZ;
                subClassCommand.zMax = maxZ;
                command = subClassCommand;
                break;
            default:
                break;
        }

        command.transform = transform;
        command.physics = physics;
        commands.Enqueue(command);
    }

    public void CancelAllCommands()
    {
        commands.Clear();
    }

    private void Update()
    {

        if (commands.Count > 0)
        {
            commands.Peek().UpdateBryansPhysics();
            if (commands.Peek().isFinished)
            {
                commands.Dequeue();
            }
        }
    }



}
