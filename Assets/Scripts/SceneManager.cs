using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public GameObject UAVPrefab;
    public GameObject UserPrefab;

    public GameObject configurationMap;
    private ConfigurationMap cm;

    private void Awake()
    {
        cm = configurationMap.GetComponent<ConfigurationMap>();
        cm.Initialize();

        GameObject newUAV = Instantiate(UAVPrefab);
        cm.InsertUAV(newUAV, 4, 6);
        newUAV = Instantiate(UAVPrefab);
        cm.InsertUAV(newUAV, 17, 11);

    }
}
