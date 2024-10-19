using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference rotateAction;
    

    [SerializeField] private LayerMask notPlayerMask;
    [Range(0,1)] [SerializeField] private float friction = 0.25f;
    [SerializeField] private float speed = 0.25f;
    [SerializeField] private float airSpeed = 0.25f;
    
    private PlayerBodyManagement _bodyManagement;
    private Camera _playerCamera;
    
    private Rigidbody _playerRigidBody;
    private Vector2 _inputVector;
    
    private bool _isGrounded = false;

    private Quaternion _rotationGoal;
    
    private void Start()
    {
        _playerRigidBody = GetComponent<Rigidbody>();
        _bodyManagement = GetComponent<PlayerBodyManagement>();
        _playerCamera = Camera.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        rotateAction.action.Enable();
    }

    private void Update()
    {
        _inputVector = moveAction.action.ReadValue<Vector2>();

        if (rotateAction.action.WasPressedThisFrame())
        {
            if (_inputVector.x != 0)
            {
                StartRotation(_inputVector);
            }
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Movement();
        
        Rotation();
    }

    // TODO: Dont self intersect
    private void GroundCheck()
    {
        foreach (GameObject bodyPart in _bodyManagement.BodyParts)
        {
            Debug.DrawRay(bodyPart.transform.position, Vector3.down * 1.0f, Color.red);
            if (Physics.Raycast(bodyPart.transform.position, Vector3.down, 1.0f))
            {
                _isGrounded = true;
                return;
            }
        }
        
        _isGrounded = false;
    }

    private void Movement()
    {
        Vector3 forward = _playerCamera.transform.forward.With(y:0).normalized;
        Vector3 right = _playerCamera.transform.right.With(y:0).normalized;

        Vector3 movementDirection = (forward * _inputVector.y + right * _inputVector.x) * speed;
        
        if (movementDirection.sqrMagnitude > 0.001f && _isGrounded && _playerRigidBody.velocity.y < 0.00001f)
        {
            _playerRigidBody.useGravity = false;
        }
        else
        {
            _playerRigidBody.useGravity = true;
        }
        
        // movementDirection.y = _playerRigidBody.velocity.y;
        if (_isGrounded)
        {
            _playerRigidBody.velocity = Vector3.Lerp(_playerRigidBody.velocity, movementDirection, friction);
        }
        else
        {
            if (movementDirection.sqrMagnitude < _playerRigidBody.velocity.sqrMagnitude)
            {
                _playerRigidBody.AddForce(movementDirection.normalized * airSpeed); // DUM WAY ALERT
            }
        }
    }

    private void StartRotation(Vector2 input)
    {
        
        
    }

    private void Rotation()
    {
        // _playerRigidBody.MoveRotation(_rotationGoal);
    }
    
}
