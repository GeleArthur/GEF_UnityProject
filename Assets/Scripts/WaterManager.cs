using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WaterManager
{
    private static WaterManager _instance;
    public static WaterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WaterManager();
            }
            return _instance;
        }
    }
    
    private readonly List<Water> _allWater = new List<Water>();
    public IEnumerable<Water> GetWater => _allWater.AsEnumerable();

    public void AddWater(Water water)
    {
        _allWater.Add(water);
    }
    
    public void RemoveWater(Water water)
    {
        _allWater.Remove(water);
    }
    
    
    
}
