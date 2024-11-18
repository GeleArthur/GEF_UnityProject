using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerBodyManagement : MonoBehaviour
{
    [SerializeField] private GameObject bodyPartPrefab;
    [SerializeField] private GameObject waterPartPrefab;
    
    [SerializeField] private InputActionReference attachButton;
    [SerializeField] private InputActionReference removeButton;

    [SerializeField] private Material bodyMaterial;
    [SerializeField] private Material bodyHighlightMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material waterHighlightMaterial;
    
    [SerializeField] private AudioClip connectSound;
    [SerializeField] private AudioClip disconnectSound;
    
    [SerializeField] private CinemachineTargetGroup cameraTargetGroup;

    private AudioSource _audioPlayer;
    private Rigidbody _rigidbody;
    private LineRenderer _lineRenderer;
    private GameObject _lastTouchWaterBlock = null;
    private readonly Stack<GameObject> _bodyParts = new Stack<GameObject>();
    private Vector3 _centerOfMass;
    private Vector3 _sizeExtendHalf;
    
    public IEnumerable<GameObject> BodyParts => _bodyParts;
    public Vector3 BodySizeHalf => _sizeExtendHalf;
    
    private readonly Vector3[] normalsAroundCube =
    {
        new(1, 0, 0),
        new(-1, 0, 0),
        new(0, 1, 0),
        new(0, -1, 0),
        new(0, 0, 1),
        new(0, 0, -1),
    };

    private void Start()
    {
        // Setup
        attachButton.action.Enable();
        removeButton.action.Enable();
        _rigidbody = GetComponent<Rigidbody>();
        _audioPlayer = GetComponent<AudioSource>();
        _lineRenderer = GetComponent<LineRenderer>();
        
        // Add default body parts
        AddBodyPart(Vector3.zero);
        AddBodyPart(new Vector3(0,0,1));
    }
    
    private void Update()
    {
        // Remove a body part.
        if (removeButton.action.WasPressedThisFrame())
        {
            RemoveBodyPart();
        }
        
        DebugExtension.DebugBounds(new Bounds(transform.position, _sizeExtendHalf + Vector3.one));
        _lineRenderer.positionCount = 0; // Disable line renderer.
        
        // Check around body if we collide with water
        Collider[] hits = Physics.OverlapBox(transform.position, (_sizeExtendHalf + Vector3.one)/2, transform.rotation, LayerMask.GetMask("Water"));

        // Are we hitting a water block?
        if (hits.Length <= 0)
        {
            // Stop highlighting the water block
            if (_lastTouchWaterBlock != null)
            {
                _lastTouchWaterBlock.GetComponent<MeshRenderer>().material = waterMaterial;
                _lastTouchWaterBlock = null;
            }
            
            return; // STOP
        }
        
        // Find the closest water
        Collider closestWater = null;
        float closestDistance = float.MaxValue;
        foreach (Collider colliderTest in hits)
        {
            float distance = (hits[0].transform.position - transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestWater = colliderTest;
            }
        }
        
        // Stop/Start highlighting water block 
        if (_lastTouchWaterBlock != closestWater!.gameObject)
        {
            if (_lastTouchWaterBlock != null)
            {
                _lastTouchWaterBlock.GetComponent<MeshRenderer>().material = waterMaterial;
            }
            
            _lastTouchWaterBlock = closestWater.gameObject;
            _lastTouchWaterBlock.GetComponent<MeshRenderer>().material = waterHighlightMaterial;
        }
        
        // Get closest body part to closest water.
        GameObject closestBodyPart = null;
        closestDistance = float.MaxValue;
        foreach (GameObject part in _bodyParts)
        {
            float distance = (closestWater.transform.position - part.transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestBodyPart = part;
                closestDistance = distance;
            }
        }
        
        // Find closest face to water part
        float smallestAngle = 1;
        Vector3 newPlace = Vector3.zero;
        foreach (Vector3 normal in normalsAroundCube)
        {
            Vector3 normalRotated = closestBodyPart!.transform.rotation * normal;

            Vector3 toVector = (closestBodyPart.transform.position - closestWater.transform.position).normalized;
            
            float angle = Vector3.Dot(toVector, normalRotated);
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                newPlace = normal;
            }
        }

        // Enable line renderer between face and water.
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, closestBodyPart!.transform.position + closestBodyPart.transform.rotation * newPlace/2);
        _lineRenderer.SetPosition(1, closestWater.transform.position);
        
        Debug.DrawLine(closestBodyPart!.transform.position,  closestBodyPart.transform.position + closestBodyPart.transform.rotation * newPlace, Color.green);
        
        // If the player presses attach we attach the closest water block.
        if (attachButton.action.WasPressedThisFrame())
        {
            AddBodyPart(closestBodyPart.transform.localPosition + newPlace);
            closestWater.enabled = false;
            Destroy(closestWater!.gameObject);
        }
    }

    private void AddBodyPart(Vector3 position)
    {
        // Create a body part 
        GameObject newBodyPart = Instantiate(bodyPartPrefab, transform, false);
        newBodyPart.transform.localPosition = position;
        
        _bodyParts.Push(newBodyPart);
        cameraTargetGroup.AddMember(newBodyPart.transform, 1, 1 );
        CalculateBody();
        ColorLatestBodyPart();
        
        _audioPlayer.PlayOneShot(connectSound);
    }

    private void RemoveBodyPart()
    {
        // Remove body part and create water.
        if (_bodyParts.Count <= 1) return;
        GameObject oldBodyPart = _bodyParts.Pop();

        GameObject waterPart = Instantiate(waterPartPrefab, oldBodyPart.transform.position, quaternion.identity);
        cameraTargetGroup.RemoveMember(oldBodyPart.transform);
        
        Destroy(oldBodyPart);
        oldBodyPart.SetActive(false);
        CalculateBody();
        ColorLatestBodyPart();
        
        _audioPlayer.PlayOneShot(disconnectSound);

    }

    // Calculate a box around the player to find the min and max
    private void CalculateBody()
    {
        // EditorApplication.isPaused = true;
        Vector3 bottonBodyPart = Vector3.positiveInfinity;
        Vector3 topBodyPart = Vector3.negativeInfinity;
        
        foreach (GameObject bodyPart in _bodyParts)
        {
            bottonBodyPart = Vector3.Min(bottonBodyPart, bodyPart.transform.localPosition);
            topBodyPart = Vector3.Max(topBodyPart, bodyPart.transform.localPosition);
        }
        
        // We are not using the center of the cubes
        topBodyPart += new Vector3(0.5f, 0.5f, 0.5f);
        bottonBodyPart += new Vector3(-0.5f, -0.5f, -0.5f);

        DebugExtension.DebugPoint(transform.position + topBodyPart, Color.red, 0.2f, 5);
        DebugExtension.DebugPoint(transform.position + bottonBodyPart, Color.red, 0.2f, 5);
        
        // Calculate box sizes
        _centerOfMass = (topBodyPart + bottonBodyPart)/2;
        _sizeExtendHalf = topBodyPart - bottonBodyPart;

        // To make sure the center where we rotate around we move the center point around. 
        foreach (GameObject bodyPart in _bodyParts)
        {
            bodyPart.transform.localPosition -= _centerOfMass;
        }
        
        // Make sure we cancel out the movement so the body doesn't move
        transform.position += transform.rotation * _centerOfMass;
    }
    
    // Color the last body so the player know what is going to disconnect
    private void ColorLatestBodyPart()
    {
        foreach (GameObject bodyPart in _bodyParts)
        {
            bodyPart.GetComponentInChildren<MeshRenderer>().material = bodyMaterial;
        }

        _bodyParts.Peek().GetComponentInChildren<MeshRenderer>().material = bodyHighlightMaterial;
    }
}
