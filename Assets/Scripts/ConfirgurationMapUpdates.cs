using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirgurationMapUpdates : MonoBehaviour
{
    public float updateFrequency;
    private float counter = 0;

    private ConfigurationMap cm;

    // Start is called before the first frame update
    void Awake()
    {
        cm = GetComponent<ConfigurationMap>();
    }

    // Update is called once per frame
    void Update()
    {
        

    }
}
