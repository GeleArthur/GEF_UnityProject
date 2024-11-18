using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    private void Awake()
    {
        WaterManagerSingleton.Instance.AddWater(this);
    }

    public void OnDestroy()
    {
        WaterManagerSingleton.Instance.RemoveWater(this);
    }
}
