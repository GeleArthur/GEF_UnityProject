using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyManagement : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _bodyParts = new List<GameObject>();
    
    [SerializeField]
    private GameObject BodyPart;

    private void Start()
    {
        AddBodyPart(Vector3Int.zero);
        AddBodyPart(new Vector3Int(0,0,1));
    }

    public void AddBodyPart(Vector3Int position)
    {
        GameObject newBodyPart = Instantiate(BodyPart, transform, false);
        newBodyPart.transform.localPosition = position;
        
        _bodyParts.Add(newBodyPart);
    }
}
