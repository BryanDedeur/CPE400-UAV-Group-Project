using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;

public static class MyExtensions
{
    public static string AppendTimeStamp(this string fileName)
    {
        return string.Concat(
            Path.GetFileNameWithoutExtension(fileName),
            System.DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            Path.GetExtension(fileName)
            );
    }
}

public class Output : MonoBehaviour
{
    public float updateFrequency = 1;
    private float timeRemaining;

    private System.DateTime startTime;

    public static int remainingUAV;
    private string filePath = "Assets/Scripts/Output/";
    StreamWriter writeToFile;
    private ConfigurationMap map;

    public void decerementRemainingUAV()
    {
        remainingUAV -= 1;
    }

    void WriteLineToFile(string line)
    {
        if (!File.Exists(filePath))
        {
            File.CreateText(filePath).Dispose();
        }

        writeToFile = new StreamWriter(filePath, append: true);
        writeToFile.WriteLine(line);
        writeToFile.Close();
    }

    private void Start()
    {
        startTime = System.DateTime.Now;
        filePath += MyExtensions.AppendTimeStamp("Date.csv");
    }

    private string CreateStringOfData()
    {
        double timeStamp = (System.DateTime.Now - startTime).TotalSeconds;
        int totalConnectedUsers = NetworkManager.inst.totalConnectedUsers;
        int totalDisconnectedusers = EntityManager.inst.users.Count - totalConnectedUsers;
        int totalUsers = EntityManager.inst.users.Count;
        int totalActiveUAVs = EntityManager.inst.uavs.Count;
        float averageUserDisconnectTime = 0;
        float priority1UserAverageConnectionTime = 0;
        int priority1UserCount = 0;
        float priority2UserAverageConnectionTime = 0;
        int priority2UserCount = 0;
        float priority3UserAverageConnectionTime = 0;
        int priority3UserCount = 0;
        float averageUAVTravelDistance = 0;

        foreach (UserEntity user in EntityManager.inst.users)
        {
            averageUserDisconnectTime += user.device.timeDisconnected / totalUsers;

            if (user.device.priority == 1)
            {
                priority1UserAverageConnectionTime += user.device.timeConnected;
                ++priority1UserCount;
            }
            else if (user.device.priority == 2)
            {
                priority2UserAverageConnectionTime += user.device.timeConnected;
                ++priority2UserCount;
            }
            else
            {
                priority3UserAverageConnectionTime += user.device.timeConnected;
                ++priority3UserCount;
            }
        }
        priority1UserAverageConnectionTime /= priority1UserCount;
        priority2UserAverageConnectionTime /= priority2UserCount;
        priority3UserAverageConnectionTime /= priority3UserCount;

        foreach (UAVEntity uav in EntityManager.inst.uavs)
        {
            averageUAVTravelDistance += uav.physics.distanceTraveled / totalActiveUAVs;
        }

        return Mathf.Round((float) timeStamp).ToString() + ", " + totalConnectedUsers + ", " + totalDisconnectedusers + ", " + averageUserDisconnectTime + ", " + priority1UserAverageConnectionTime + ", " + priority2UserAverageConnectionTime + ", " + priority3UserAverageConnectionTime + ", " + totalActiveUAVs + ", " + averageUAVTravelDistance;
    }


    void Update()
    {
        if (timeRemaining < 0 && NetworkManager.inst.routers.Count > 0)
        {
            timeRemaining = updateFrequency;
            WriteLineToFile(CreateStringOfData());
        }
        timeRemaining -= Time.deltaTime;
    }


}
