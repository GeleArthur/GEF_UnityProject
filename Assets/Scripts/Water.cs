using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    private void Awake()
    {
        WaterManager.Instance.AddWater(this);
    }

    public void OnDestroy()
    {
        WaterManager.Instance.RemoveWater(this);
    }
}
