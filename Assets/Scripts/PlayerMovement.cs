using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [FormerlySerializedAs("_moveAction")] [SerializeField]
    private InputActionReference moveAction;
    
    [FormerlySerializedAs("_playerCamera")] [SerializeField]
    private Camera playerCamera;

    [SerializeField] private LayerMask notPlayerMask;
    [Range(0,1)]
    [SerializeField] private float friction = 0.25f;
    [SerializeField] private float speed = 0.25f;
    [SerializeField] private float airSpeed = 0.25f;
    
    
    
    private PlayerBodyManagement _bodyManagement;
    
    private Rigidbody _playerRigidBody;
    private Vector2 _inputVector;
    
    private bool _isGrounded = false;
    
    private void Start()
    {
        _playerRigidBody = GetComponent<Rigidbody>();
        _bodyManagement = GetComponent<PlayerBodyManagement>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void Update()
    {
        _inputVector = moveAction.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Vector3 forward = playerCamera.transform.forward.With(y:0).normalized;
        Vector3 right = playerCamera.transform.right.With(y:0).normalized;

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
}
