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



    private bool headerWritten = false;
    private string header;

    private bool parameterHeaderWritten = false;
    private string parameterHeader;

    private string generateHeader()
    {
        return header = "Time Stamp (Sec)" + "," + "Total Connected Users" + "," + "Total Disconnected Users" + "," + "Average User Disconnection Time" + "," + "Priority 1 User Average Connection Time" + "," + "Priority 2 User Average Connection Time" + "," + "Priority 3 User Average Connection Time" + "," + "Active UAVs" + "," + "Total UAVs Travel Distance";
    }

    private string generateParameterHeader()
    {
        return parameterHeader = "HI";
    }


    private System.DateTime startTime;

    private string filePath = "Assets/Scripts/Output/";
    private string parameterFilePath = "Assets/Scripts/Output/Parameters.csv";
    StreamWriter writeToFile, parameterWriteToFile;

    void WriteLineToFile(string line)
    {
        if (!File.Exists(filePath))
        {
            File.CreateText(filePath).Dispose();
        }

        writeToFile = new StreamWriter(filePath, append: true);
        if (!headerWritten)
        {
            writeToFile.WriteLine(generateHeader());
            headerWritten = true;
        }
        writeToFile.WriteLine(line);
        writeToFile.Close();
    }

    void ParameterWriteLineToFile()
    {
        if (!File.Exists(parameterFilePath))
        {
            File.CreateText(parameterFilePath).Dispose();
        }

        writeToFile = new StreamWriter(parameterFilePath);
        if (!parameterHeaderWritten)
        {
            writeToFile.WriteLine(generateParameterHeader());
            parameterHeaderWritten = true;
        }
        writeToFile.WriteLine(generateParameterHeader());
        writeToFile.WriteLine(CreateParameterStringOfData());
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
        int totalActiveUAVs = NetworkManager.inst.routers.Count - 1;
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

        return timeStamp.ToString() + ", " + totalConnectedUsers + ", " + totalDisconnectedusers + ", " + averageUserDisconnectTime + ", " + priority1UserAverageConnectionTime + ", " + priority2UserAverageConnectionTime + ", " + priority3UserAverageConnectionTime + ", " + totalActiveUAVs + ", " + averageUAVTravelDistance;
    }

    private string CreateParameterStringOfData()
    {
        return "HI";
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
