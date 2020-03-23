using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICommands : MonoBehaviour
{
    public Queue<Command> commands;
    public enum CommandType { MoveTo };
    private Physics physics;

    private void Awake()
    {
        commands = new Queue<Command>();
    }

    public abstract class Command
    {
        public Vector3 targetPos;
        public GameObject targetObject = null;
        public bool isFinished = false;
        public Physics physics;
        public Transform transform;

        public virtual void UpdatePhysics() { }
    }

    public class MoveTo : Command
    {
        private float timeToStop;
        private float stoppingDistance = 10;

        public override void UpdatePhysics() {
            if (physics == null)
            {
                physics = transform.GetComponent<Physics>();
            }

            physics.desiredSpeed = physics.maxSpeed;

            timeToStop = physics.speed / physics.acceleration;
            stoppingDistance = (50f * physics.acceleration * (Mathf.Pow(timeToStop, 2f)));

            float computedHeading = 0;

            if (targetObject != null)
            {
                computedHeading = Mathf.Rad2Deg * (Mathf.Atan2(targetObject.transform.position.x - transform.position.x, (targetObject.transform.position.z - transform.position.z)));
                if ((transform.position - targetObject.transform.position).magnitude <= (stoppingDistance))
                {
                    physics.desiredSpeed = physics.acceleration;
                }
                if ((transform.position - targetObject.transform.position).magnitude < 0.03f)
                {
                    physics.desiredSpeed = 0;
                    isFinished = true;
                }
            } else
            {
                // FIX THIS computedHeading = Mathf.Rad2Deg * (Mathf.Atan2(targetPos.z - transform.position.z, targetPos.x - transform.position.x));
                if ((transform.position - targetPos).magnitude < stoppingDistance)
                {
                    physics.desiredSpeed = 0;
                    isFinished = true;
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

    private void Update()
    {

        if (commands.Count > 0)
        {
            commands.Peek().UpdatePhysics();
            if (commands.Peek().isFinished)
            {
                commands.Dequeue();
                print(commands.Count);
            }
        }
    }



}
