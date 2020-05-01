﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NodeEntity : Entity
{
    public int row = 0;
    public int col = 0;
    public UAVEntity assignedUAV;
    public List<NodeEntity> neighbors;
    public List<UserEntity> usersInRange;
    public bool renderUsersInRange;
    public Color lineColor;

    private GameObject nodeVisual;
    private GameObject heatVisual;

    private static int count = 0;

    private void Awake()
    {
        usersInRange = new List<UserEntity>();

        ID = count;
        count = count + 1;
        nodeVisual = transform.Find("NodeVisual").gameObject;
        heatVisual = transform.Find("HeatVisual").gameObject;
    }

    bool hold = true;
    private void Update()
    {
        EvaluateNearbyUsers();

        if (renderUsersInRange)
        {
            foreach (UserEntity user in usersInRange)
            {
                LineManager.inst.DrawLine(transform.position, user.transform.position, lineColor);
            }
        }

        if (heatVisual.activeSelf)
        {
            heatVisual.transform.localScale = usersInRange.Count * new Vector3(1, 0.01f, 1);
        }
    }

    /// <summary>
    /// Evaluate the user information in range of neighbor nodes.
    /// </summary>
    private void EvaluateNearbyUsers()
    {
        List<UserEntity> tempUsers = new List<UserEntity>(usersInRange);
        foreach (UserEntity user in tempUsers)
        {
            float userDistanceSqr = (user.transform.position - transform.position).sqrMagnitude;
            if (userDistanceSqr > Mathf.Pow(NetworkManager.inst.connectionRadius, 2f) / 2f)
            {
                NodeEntity bestNode = this;

                foreach (NodeEntity node in neighbors)
                {
                    try
                    {
                        float neighborDistanceSqr = (node.transform.position - user.transform.position).sqrMagnitude;
                        if (neighborDistanceSqr < userDistanceSqr)
                        {
                            userDistanceSqr = neighborDistanceSqr;
                            bestNode = node;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                if (bestNode != this)
                {
                    bestNode.usersInRange.Add(user);
                    user.nearestNode = bestNode;
                    usersInRange.Remove(user);
                }
            }
        }
    }

    /// <summary>
    /// Toggles the visuals of the nodes.
    /// </summary>
    public void ToggleNodeVisual(bool isRendering)
    {
        nodeVisual.SetActive(isRendering);
    }

    /// <summary>
    /// Toggles the number of user visuals of the node.
    /// </summary>
    public void ToggleHeatVisual(bool isRendering)
    {
        heatVisual.SetActive(isRendering);
    }
}
