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
}
