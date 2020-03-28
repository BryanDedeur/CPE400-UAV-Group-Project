using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICommands : MonoBehaviour
{
    public Queue<Command> commands;
    public enum CommandType { MoveTo };
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

            } else
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

    public class RandomHeading : Command
    {
        private float timeToStop;
        private float stoppingDistance = 10;
        float computedHeading = 0;
        float magnitude;

        public override void UpdateBryansPhysics()
        {

        }
    }


    public void AddCommand(CommandType commandType, GameObject gameObject)
    {
        Command command = new MoveTo();
        switch (commandType)
        {
            case CommandType.MoveTo:
                if (commands.Count > 0)
                {
                    if (gameObject == commands.Peek().targetObject)
                    {
                        print("Same");
                    }
                    else
                    {
                        print("different");
                    }
                }
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
