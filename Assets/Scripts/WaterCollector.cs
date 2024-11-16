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
        int waterAmount = 0;
        foreach (Water water in WaterManager.Instance.GetWater)
        {
            if ((water.transform.position - transform.position).sqrMagnitude < rangeSqr)
            {
                waterAmount++;
            }
        }

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
            enableOnComplete.SetActive(true);
        }
    }

    private void UpdateText()
    {
        collectedText.text = $"{_collectedWater} / {waterNeededToCollect}";
    }
}
