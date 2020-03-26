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
        
        if (counter < 0)
        {
            counter = updateFrequency;
            // Do everything after a time interval
            for (int c = 0; c < cm.configurationMapNodes.GetLength(1); c++)
            {
                for (int r = 0; r < cm.configurationMapNodes.GetLength(0); r++)
                {
                    if (cm.configurationMapNodes[r, c] != null)
                    {
                        cm.configurationMapNodes[r, c].numberUsers = Random.Range(0, 10);
                        cm.configurationMapNodes[r, c].visual.transform.localScale = new Vector3(1, .01f, 1) * cm.configurationMapNodes[r, c].numberUsers * .2f;
                    }
                }
            }


        }
        counter -= Time.deltaTime;
    }
}
