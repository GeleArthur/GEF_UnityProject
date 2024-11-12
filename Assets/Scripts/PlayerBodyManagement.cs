using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerBodyManagement : MonoBehaviour
{
    [SerializeField] private GameObject bodyPartPrefab;
    [SerializeField] private GameObject waterPartPrefab;
    
    [SerializeField] private InputActionReference attachButton;
    [SerializeField] private InputActionReference removeButton;
    
    [SerializeField]
    private CinemachineTargetGroup cameraTargetGroup;

    private Rigidbody _rigidbody;
    private readonly Stack<GameObject> _bodyParts = new Stack<GameObject>();
    private Vector3 _centerOfMass;
    private Vector3 _sizeExtendHalf;
    
    public Stack<GameObject> BodyParts => _bodyParts;
    public Vector3 BodySizeHalf => _sizeExtendHalf;

    private void Start()
    {
        attachButton.action.Enable();
        removeButton.action.Enable();
        _rigidbody = GetComponent<Rigidbody>();
        
        AddBodyPart(Vector3.zero);
        AddBodyPart(new Vector3(0,0,1));
    }
    
    private void Update()
    {
        if (removeButton.action.WasPressedThisFrame())
        {
            RemoveBodyPart();
        }
        
        Collider[] hits = Physics.OverlapBox(transform.position, (_sizeExtendHalf + Vector3.one)/2, transform.rotation, LayerMask.GetMask("Water"));

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
            new(1, 0, 0),
            new(-1, 0, 0),
            new(0, 1, 0),
            new(0, -1, 0),
            new(0, 0, 1),
            new(0, 0, -1),
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
        
        Debug.DrawLine(closestBodyPart!.transform.position,  closestBodyPart.transform.position + closestBodyPart.transform.rotation * newPlace, Color.green);

        if (attachButton.action.WasPressedThisFrame())
        {
            AddBodyPart(closestBodyPart.transform.localPosition + newPlace);
            clostedWater.enabled = false;
            Destroy(clostedWater!.gameObject);
        }
    }

    private void AddBodyPart(Vector3 position)
    {
        GameObject newBodyPart = Instantiate(bodyPartPrefab, transform, false);
        newBodyPart.transform.localPosition = position;
        
        _bodyParts.Push(newBodyPart);
        CalculateBody();

        cameraTargetGroup.AddMember(newBodyPart.transform, 1, 1 );
    }

    private void RemoveBodyPart()
    {
        if (_bodyParts.Count <= 1) return;
        GameObject oldBodyPart = _bodyParts.Pop();

        GameObject waterPart = Instantiate(waterPartPrefab, oldBodyPart.transform.position, quaternion.identity);
        cameraTargetGroup.RemoveMember(oldBodyPart.transform);
        
        Destroy(oldBodyPart);
        oldBodyPart.SetActive(false);
        CalculateBody();

    }

    private void CalculateBody()
    {
        Vector3 bottonBodyPart = Vector3.zero;
        Vector3 topBodyPart = Vector3.zero;
        
        foreach (GameObject bodyPart in _bodyParts)
        {
            // TODO: Can be better with vector3.max
            if (bottonBodyPart.x < bodyPart.transform.localPosition.x)
            {
                bottonBodyPart.x = bodyPart.transform.localPosition.x;
            }
            if (bottonBodyPart.y < bodyPart.transform.localPosition.y)
            {
                bottonBodyPart.y = bodyPart.transform.localPosition.y;
            }
            if (bottonBodyPart.z < bodyPart.transform.localPosition.z)
            {
                bottonBodyPart.z = bodyPart.transform.localPosition.z;
            }
            
            if (topBodyPart.x > bodyPart.transform.localPosition.x)
            {
                topBodyPart.x = bodyPart.transform.localPosition.x;
            }
            if (topBodyPart.y > bodyPart.transform.localPosition.y)
            {
                topBodyPart.y = bodyPart.transform.localPosition.y;
            }
            if (topBodyPart.z > bodyPart.transform.localPosition.z)
            {
                topBodyPart.z = bodyPart.transform.localPosition.z;
            }
        }
        
        topBodyPart += new Vector3(-0.5f, -0.5f, -0.5f);
        bottonBodyPart += new Vector3(0.5f, 0.5f, 0.5f);
        
        DebugExtension.DebugWireSphere(topBodyPart, 0.5f, 1);
        DebugExtension.DebugWireSphere(bottonBodyPart, 0.5f, 1);
        
        _centerOfMass = (topBodyPart + bottonBodyPart)/2;
        _sizeExtendHalf = bottonBodyPart - topBodyPart;

        transform.position += _centerOfMass;

        foreach (GameObject bodyPart in _bodyParts)
        {
            bodyPart.transform.localPosition -= _centerOfMass;
        }
        
        _rigidbody.centerOfMass = _centerOfMass;
    }
}
