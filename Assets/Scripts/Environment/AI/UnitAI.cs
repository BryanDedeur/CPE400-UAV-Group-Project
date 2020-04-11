using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI : MonoBehaviour
{
    public Entity entity;
    public bool rejectInstructions;

    public bool debug;

    void Awake()
    {
        entity = GetComponent<Entity>();
        commands = new List<Command>();
    }

    public List<Command> commands;

    void FixedUpdate()
    {
        if (commands.Count > 0)
        {
            if (commands[0].IsDone())
            {
                StopAndRemoveCommand(0);
            }
            else
            {
                commands[0].Tick();

                if (debug)
                {
                }
            }
        }


    }

    /// <summary>
    /// Stop and remove a specific command
    /// </summary>
    /// <param name="index"></param>
    void StopAndRemoveCommand(int index)
    {
        if (!rejectInstructions)
        {
            commands[index].Stop();
            commands.RemoveAt(index);
        }
    }

    /// <summary>
    /// Stops all the commands and removes them from the queue.
    /// </summary>
    public void StopAndRemoveAllCommands()
    {
        if (!rejectInstructions)
        {
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                StopAndRemoveCommand(i);
            }
        }
    }

    /// <summary>
    /// Adds a command to the queue.
    /// </summary>
    public void AddCommand(Command c)
    {
        if (!rejectInstructions)
        {
            c.Init();
            commands.Add(c);
        }
    }

    /// <summary>
    /// Clears the queue and add a command to the queue.
    /// </summary>
    public void SetCommand(Command c)
    {
        if (!rejectInstructions)
        {
            StopAndRemoveAllCommands();
            commands.Clear();
            AddCommand(c);
        }
    }
}
