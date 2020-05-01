using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using UnityEngine.UI;

//A class which is used to add the current date and time to the output file so it outputs to a different file each time the simulation runs.
public static class MyExtensions
{
    public static string AppendTimeStamp(this string fileName)
    {
        return string.Concat(
            Path.GetFileNameWithoutExtension(fileName),
            System.DateTime.Now.ToString("yyyyMMddHHmm"),
            Path.GetExtension(fileName)
            );
    }
}

public class Output : MonoBehaviour
{
    //Time counter used to output the date to a file after every second.
    public float updateFrequency = 1f;
    private float timeRemaining = 1f;

    //Time delay to output to parameters to ensure all of the parameters are initialized.
    private float parameterTimeRemaining = 2f;
    
    //Variable to ensure that the header for the parameter output file is not written more than once.
    private bool headerWritten = false;

    //Variable to ensure that the header for the parameter output file is not written more than once.
    private bool parameterHeaderWritten = false;

    //Variable to ensure that the parameter output file is not written more than once.
    private bool parameterWritten = false;

    //Variable to ensure that output file is not updated once all of the UAV's have died.
    private bool stopWriting = false;

    //The UI to write text to.
    public Text UIText;

    //Function to generate the header for the parameter output file.
    private string generateHeader()
    {
        return "Time Stamp (Sec),Total Connected Users,Total Disconnected Users,Average User Disconnection Time,Priority 1 User Average Connection Time,Priority 2 User Average Connection Time,Priority 3 User Average Connection Time,Active UAVs,Total UAVs Travel Distance";
    }

    //Function to generate the header for the output file.
    private string generateParameterHeader()
    {
        return "Total UAVs,Total Users,Node to Node Distance,Number of Rows,Number of Columns,UAV Height from Ground,Connection Radius,A-Star Update Frequency,Local Maximum Update Frequency,Maximum Timer Per Priority,Maximum Device Capacity";
    }

    //Variable used to get the current timestamp.
    private System.DateTime startTime;

    //Variables defining the file paths for the output files.
    private string filePath = "Assets/Scripts/Output/";
    private string parameterFilePath = "Assets/Scripts/Output/Parameters.csv";

    //Variable which are used to open the output file and write to it.
    StreamWriter writeToFile, parameterWriteToFile;

    //Function used to open, write and close the output file.
    void WriteOutputToFile(string line)
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

    //Function used to open, write and close the parameter output file.
    void WriteParameterToFile()
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
        writeToFile.WriteLine(CreateParameterStringOfData());
        writeToFile.Close();
    }

    //Function used to generate the data which is written to the data output file every second.
    //Param delimiter is the delimiter seperating the data.
    private string CreateStringOfData(string delimiter)
    {
        double timeStamp = (System.DateTime.Now - startTime).TotalSeconds;
        int totalConnectedUsers = NetworkManager.inst.totalConnectedUsers;
        int totalDisconnectedusers = EntityManager.inst.users.Count - totalConnectedUsers;
        int totalUsers = EntityManager.inst.users.Count;
        int totalActiveUAVs = NetworkManager.inst.routers.Count - 1;

        int totalUAVs = EntityManager.inst.uavs.Count;
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
            averageUAVTravelDistance += uav.physics.distanceTraveled / totalUAVs;
        }
        if (totalActiveUAVs == 0)
        {
            stopWriting = true;
        }
        return timeStamp.ToString() + delimiter + totalConnectedUsers + delimiter + totalDisconnectedusers + delimiter + averageUserDisconnectTime + delimiter + priority1UserAverageConnectionTime + delimiter + priority2UserAverageConnectionTime + delimiter + priority3UserAverageConnectionTime + delimiter + totalActiveUAVs + delimiter + averageUAVTravelDistance;
    }

    //Function used to generate the data which is written to the data output file every second.
    //Param delimiter is the delimiter seperating the data.
    private string CreateStringOfDataWithLabels(string delimiter)
    {
        double timeStamp = (System.DateTime.Now - startTime).TotalSeconds;
        int totalConnectedUsers = NetworkManager.inst.totalConnectedUsers;
        int totalDisconnectedusers = EntityManager.inst.users.Count - totalConnectedUsers;
        int totalUsers = EntityManager.inst.users.Count;
        int totalActiveUAVs = NetworkManager.inst.routers.Count - 1;

        int totalUAVs = EntityManager.inst.uavs.Count;
        float averageUserDisconnectTime = 0;
        float priority1UserAverageConnectionTime = 0;
        int priority1UserCount = 0;
        float priority2UserAverageConnectionTime = 0;
        int priority2UserCount = 0;
        float priority3UserAverageConnectionTime = 0;
        int priority3UserCount = 0;
        float averageUAVTravelDistance = 0;
        float averageUAVBatteryLife = 0;
        float averageUAVBatteryReserveThreshold = 0;

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
            averageUAVTravelDistance += uav.physics.distanceTraveled / totalUAVs;
            if (uav.battery != null)
            {
                averageUAVBatteryLife += uav.battery.batteryLife / totalUAVs;
                averageUAVBatteryReserveThreshold += uav.battery.batteryReserveThreshold / totalUAVs;
            }
        }
        if (totalActiveUAVs == 0)
        {
            stopWriting = true;
        }
        return "Simulation Time(s): " + ((int) timeStamp) + delimiter + "Connected Devices: " + totalConnectedUsers + delimiter + "Disconnected Devices: " + totalDisconnectedusers + delimiter + "Average Device Disconnect Time(s): " + Mathf.Round(averageUserDisconnectTime * 100f)/100f + delimiter + "Average Priority 1 Connection Time(s): " + Mathf.Round(priority1UserAverageConnectionTime*100f)/100f + delimiter + "Average Priority 2 Connection Time(s): " + Mathf.Round(priority2UserAverageConnectionTime*100f)/100f + delimiter + "Average Priority 3 Connection Time(s): " + Mathf.Round(priority3UserAverageConnectionTime*100f)/100f + delimiter + "Active UAVs: " + totalActiveUAVs + delimiter + "Average UAV Travel Distance(m): " + Mathf.Round(averageUAVTravelDistance*100f)/100f + delimiter + "Average UAV Battery Percentage: " + Mathf.Round(averageUAVBatteryLife * 100f) / 100f + delimiter + "Average UAV Battery Reserve Threshold: " + Mathf.Round(averageUAVBatteryReserveThreshold * 100f) / 100f;
    }

    //Function used to generate the data which is written parameter output to the file.
    private string CreateParameterStringOfData()
    {
        return EntityManager.inst.uavs.Count + "," + EntityManager.inst.users.Count + "," + ConfigurationMap.inst.nodeDistance + "," + ConfigurationMap.inst.rows + "," + ConfigurationMap.inst.columns + "," + ConfigurationMap.inst.uavHeight + "," + NetworkManager.inst.connectionRadius + "," + NetworkManager.inst.updateFrequency + "," + Algorithm1.inst.updateFrequency + "," + Router.maximumMillisecondTimerPerPriority + "," + Router.inst.maximumDeviceCapacity;
    }

    private void Start()
    {
        startTime = System.DateTime.Now;
        filePath += MyExtensions.AppendTimeStamp("Data.csv");
    }

    //This function is called once per frame
    void Update()
    {
        if (timeRemaining < 0 && NetworkManager.inst.routers.Count > 0 && !stopWriting)
        {
            timeRemaining = updateFrequency;
            WriteOutputToFile(CreateStringOfData(","));
        }
        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0 && !parameterWritten)
        {
            WriteParameterToFile();
            parameterWritten = true;
        }
        parameterTimeRemaining -= Time.deltaTime;
        UIText.text = CreateStringOfDataWithLabels("\n");
    }
}
