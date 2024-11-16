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
    [SerializeField] [Range(0,1)] private float friction = 0.25f;
    [SerializeField] private float speed = 0.25f;
    [SerializeField] private float rotationSpeed = 0.1f;
    
    
    private PlayerBodyManagement _bodyManagement;
    private Camera _playerCamera;
    private Rigidbody _playerRigidBody;
    
    private Vector2 _inputVector;
    private bool _isGrounded;
    private bool _lastIsGrounded = true;
    private Vector3 _lastGroundedPosition;

    private Quaternion _startRotation;
    private Quaternion _endRotation;
    private float _rotationTimer = 1;
    private float _rotationTimeAmount = 1;

    
    
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
        // Enable input
        moveAction.action.Enable();
        rotateAction.action.Enable();
    }

    private void Update()
    {
        _inputVector = moveAction.action.ReadValue<Vector2>();
        
        if (rotateAction.action.IsPressed())
        {
            _inputVector = Vector2.zero; // Disable movement if rotation button is pressed
            if(_rotationTimer < 1) return; // Are we rotating?
            if (moveAction.action.WasPressedThisFrame() || (rotateAction.action.WasPressedThisFrame() && moveAction.action.IsPressed()))
            {
                Vector2 input = moveAction.action.ReadValue<Vector2>();

                Vector2[] directions = new Vector2[]
                {
                    Vector2.up, Vector2.down, Vector2.right, Vector2.left
                };

                (float, Vector2) direction = (-10, Vector2.up);
                foreach (Vector2 vector in directions)
                {
                    float dotResult = Vector2.Dot(input, vector); // Check which straight angle is the closest
                    if (dotResult > direction.Item1)
                    {
                        direction.Item1 = dotResult;
                        direction.Item2 = vector;
                    }
                }
                
                StartRotation(direction.Item2);
            }
        }

        // If player falls of map save him.
        if (transform.position.y < -30)
        {
            transform.position = _lastGroundedPosition + new Vector3(0, 3f, 0); // little offset to help players find the ground.
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Movement();
        Rotation();
    }
    
    private void GroundCheck()
    {
        Vector3[] corners =
        {
            new(-0.5f, 0f, 0.5f),
            new(-0.5f, 0f, -0.5f),
            new(0.5f, 0f, -0.5f),
            new(0.5f, 0f, 0.5f),
        };
        
        // Check all bottom corners of bodys
        foreach (GameObject bodyPart in _bodyManagement.BodyParts)
        {
            foreach (Vector3 offset in corners)
            {
                Debug.DrawRay(bodyPart.transform.position + offset, Vector3.down * 0.6f, Color.red);
                if (Physics.Raycast(bodyPart.transform.position + offset, Vector3.down, 0.6f, ~notPlayerMask, QueryTriggerInteraction.Ignore))
                {
                    _isGrounded = true;
                    _lastGroundedPosition = transform.position;
                    return;
                }
            }

        }
        
        _isGrounded = false;
    }

    private void Movement()
    {
        Vector3 forward = _playerCamera.transform.forward.With(y:0).normalized;
        Vector3 right = _playerCamera.transform.right.With(y:0).normalized;

        Vector3 movementDirection = (forward * _inputVector.y + right * _inputVector.x) * speed;
        
        // Dont apply gravity to player if grounded. Prevents sliding off ramps
        if (_isGrounded && movementDirection.sqrMagnitude < 0.0001f ) 
            _playerRigidBody.useGravity = false;
        else
            _playerRigidBody.useGravity = true;
        
        // Apply movement
        _playerRigidBody.velocity = Vector3.Lerp(_playerRigidBody.velocity, movementDirection.With(y:_playerRigidBody.velocity.y), friction);


        // Removes the little bump when player goes up a ramp
        if (_lastIsGrounded != _isGrounded)
        {
            _lastIsGrounded = _isGrounded;
            if (!_isGrounded & _playerRigidBody.velocity.y > 0)
            {
                _playerRigidBody.velocity = _playerRigidBody.velocity.With(y: 0);
            }
        }
    }

    private void StartRotation(Vector2 input)
    {
        if (input.x != 0) // Rotate up or down pressed w or s
        {
            Quaternion rotater = Quaternion.FromToRotation(Vector3.forward, input.x > 0 ? Vector3.right : Vector3.left);
            _startRotation = _playerRigidBody.rotation;
            _endRotation = rotater * _playerRigidBody.rotation;
        }
        else // Rotate left or right.
        {
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            
            // Calculate which straight angle the camera is rotated to
            (float, Vector3) direction = (-10, Vector3.forward);
            foreach (Vector3 vector in directions)
            {
                float dotResult = Vector3.Dot(-_playerCamera.transform.forward, vector);
                if (dotResult > direction.Item1)
                {
                    direction.Item1 = dotResult;
                    direction.Item2 = vector;
                }
            }
            
            Quaternion magic = Quaternion.FromToRotation(direction.Item2, input.y > 0 ? Vector3.up : Vector3.down);
            _startRotation = _playerRigidBody.rotation;
            _endRotation = magic * _playerRigidBody.rotation;
        }

        // setup rotation variables
        _rotationTimer = 0;
        float longestSide = Mathf.Max(_bodyManagement.BodySizeHalf.x, _bodyManagement.BodySizeHalf.y, _bodyManagement.BodySizeHalf.z);
        _rotationTimeAmount = longestSide * rotationSpeed;
    }

    private void Rotation()
    {
        if (_rotationTimer < 1) // Are we rotating?
        {
            _rotationTimer += Time.fixedDeltaTime / _rotationTimeAmount;
            if (_rotationTimer > 1)
            {
                _rotationTimer = 1;
            }
            _playerRigidBody.rotation = Quaternion.Lerp(_startRotation, _endRotation, _rotationTimer);
        }
    }
    
}
