using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WaterManagerSingleton
{
    private static WaterManagerSingleton _instance;
    public static WaterManagerSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WaterManagerSingleton();
            }
            return _instance;
        }
    }
    
    private readonly List<Water> _allWater = new List<Water>();
    // Get collection of all the water
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
