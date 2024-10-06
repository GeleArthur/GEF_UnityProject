using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerBodyManagement : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _bodyParts = new List<GameObject>();
    
    [FormerlySerializedAs("BodyPartPrefab")] [SerializeField]
    private GameObject bodyPartPrefab;
    
    [SerializeField]
    private InputActionReference _attachButton;
    
    [SerializeField]
    private CinemachineTargetGroup _cameraTargetGroup;

    private void Start()
    {
        _attachButton.action.Enable();
        
        AddBodyPart(Vector3.zero);
        AddBodyPart(new Vector3(0,0,1));
    }

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 3, LayerMask.GetMask("Water"));

        if (hits.Length <= 0) return;
        
        Collider clostedWater = null;
        float closestDistance = float.MaxValue;
        foreach (Collider colliderTest in hits)
        {
            float distance = (hits[0].transform.position - transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                clostedWater = colliderTest;
            }
        }
        
        GameObject closestBodyPart = null;
        closestDistance = float.MaxValue;
        
        foreach (GameObject part in _bodyParts)
        {
            float distance = (clostedWater!.transform.position - part.transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestBodyPart = part;
                closestDistance = distance;
            }
        }

        Vector3[] around =
        {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
        };


        float smallestAngle = 1;
        Vector3 newPlace = Vector3.zero;
        foreach (Vector3 normal in around)
        {
            Vector3 normalRotated = closestBodyPart.transform.rotation * normal;

            Vector3 toVector = (closestBodyPart.transform.position - clostedWater.transform.position).normalized;

            float angle = Vector3.Dot(toVector, normalRotated);
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                newPlace = normal;
            }
        }
        
        Debug.DrawLine(closestBodyPart.transform.position,  closestBodyPart.transform.position + closestBodyPart.transform.rotation * newPlace, Color.green);

        if (_attachButton.action.WasPressedThisFrame())
        {
            AddBodyPart(closestBodyPart.transform.localPosition + newPlace);
        }

    }

    private void AddBodyPart(Vector3 position)
    {
        GameObject newBodyPart = Instantiate(bodyPartPrefab, transform, false);
        newBodyPart.transform.localPosition = position;
        
        _bodyParts.Add(newBodyPart);
    }
}
