using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaterCollector : MonoBehaviour
{
    [SerializeField] private TextMeshPro collectedText;
    [SerializeField] private int waterNeededToCollect;
    [SerializeField] private float rangeSqr;
    [SerializeField] private GameObject enableOnComplete;
    private int _collectedWater = 0;

    private void Update()
    {
        // How much water is there around.
        int waterAmount = 0;
        foreach (Water water in WaterManagerSingleton.Instance.GetWater)
        {
            if ((water.transform.position - transform.position).sqrMagnitude < rangeSqr)
            {
                waterAmount++;
            }
        }

        // Update if amount of water is changed
        if (waterAmount != _collectedWater)
        {
            _collectedWater = waterAmount;
            UpdateText();
            CheckIfCollected();
        }
    }

    private void CheckIfCollected()
    {
        if (_collectedWater >= waterNeededToCollect)
        {
            if(enableOnComplete == null) return;
            enableOnComplete.SetActive(true);
        }
    }

    private void UpdateText()
    {
        collectedText.text = $"{_collectedWater} / {waterNeededToCollect}";
    }
}
