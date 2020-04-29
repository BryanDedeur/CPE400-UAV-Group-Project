using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager inst;
    private void Awake()
    {
        inst = this;
    }
    public void DecomissionAI(bool state)
    {
        if (state)
        {
            foreach (UAVEntity uav in EntityManager.inst.uavs)
            {
                ConfigurationMap.inst.DecomissionUAV(uav);
            }
        }
    }
}