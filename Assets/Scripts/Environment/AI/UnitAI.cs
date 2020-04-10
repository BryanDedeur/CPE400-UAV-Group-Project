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

    void Update()
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

    void StopAndRemoveCommand(int index)
    {
        if (!rejectInstructions)
        {
            commands[index].Stop();
            commands.RemoveAt(index);
        }
    }

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

    public void AddCommand(Command c)
    {
        if (!rejectInstructions)
        {
            c.Init();
            commands.Add(c);
        }
    }

    public void SetCommand(Command c)
    {
        if (!rejectInstructions)
        {
            StopAndRemoveAllCommands();
            commands.Clear();
            AddCommand(c);
        }
    }

    public void DecorateAll()
    {
        Command prior = null;
        foreach (Command c in commands)
        {
            Decorate(prior, c);
            prior = c;
        }
    }
    public void Decorate(Command prior, Command current)
    {

    }

}
