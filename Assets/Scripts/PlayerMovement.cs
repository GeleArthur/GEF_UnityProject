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
    [SerializeField] private float rotationTimeAmount = 1;
    
    
    private PlayerBodyManagement _bodyManagement;
    private Camera _playerCamera;
    private Rigidbody _playerRigidBody;
    
    private Vector2 _inputVector;
    private bool _isGrounded;

    private Quaternion _startRotation;
    private Quaternion _endRotation;
    private float _rotationTimer = 1;
    
    
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
        
        if (rotateAction.action.IsPressed())
        {
            _inputVector = Vector2.zero;
            if(_rotationTimer < 1) return;
            if (moveAction.action.WasPressedThisFrame() || (rotateAction.action.WasPressedThisFrame() && moveAction.action.IsPressed()))
            {
                var input = moveAction.action.ReadValue<Vector2>();

                var directions = new Vector2[]
                {
                    Vector2.up, Vector2.down, Vector2.right, Vector2.left
                };

                (float, Vector2) direction = (-10, Vector2.up);
                foreach (Vector2 vector in directions)
                {
                    float dotResult = Vector2.Dot(input, vector);
                    if (dotResult > direction.Item1)
                    {
                        direction.Item1 = dotResult;
                        direction.Item2 = vector;
                    }
                }
                
                StartRotation(direction.Item2);
            }
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
        
        foreach (GameObject bodyPart in _bodyManagement.BodyParts)
        {
            foreach (Vector3 offset in corners)
            {
                Debug.DrawRay(bodyPart.transform.position + offset, Vector3.down * 0.6f, Color.red);
                if (Physics.Raycast(bodyPart.transform.position + offset, Vector3.down, 0.6f, ~notPlayerMask, QueryTriggerInteraction.Ignore))
                {
                    _isGrounded = true;
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
        
        if (_isGrounded && movementDirection.sqrMagnitude < 0.0001f ) 
            _playerRigidBody.useGravity = false;
        else
            _playerRigidBody.useGravity = true;
        
        //TODO: Maybe add a start velocity when "ungrounded"
        if (_isGrounded)
        {
            _playerRigidBody.velocity = Vector3.Lerp(_playerRigidBody.velocity, movementDirection, friction);
        }
        else
        {
            _playerRigidBody.velocity = Vector3.Lerp(_playerRigidBody.velocity, movementDirection.With(y:_playerRigidBody.velocity.y), friction);

        }
    }

    private void StartRotation(Vector2 input)
    {
        if (input.x != 0)
        {
            Quaternion magic = Quaternion.FromToRotation(Vector3.forward, input.x > 0 ? Vector3.right : Vector3.left);
            _startRotation = _playerRigidBody.rotation;
            _endRotation = magic * _playerRigidBody.rotation;
        }
        else
        {
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            
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

        _rotationTimer = 0;
        RotationCollisionCheck();
    }

    private void RotationCollisionCheck()
    {
        foreach (GameObject part in _bodyManagement.BodyParts)
        {
            var boxAfterRot = _endRotation * part.transform.localPosition;
            DebugExtension.DebugBounds(new Bounds(transform.position + boxAfterRot, Vector3.one*0.9f), Color.white, 3);
            var yea = Physics.OverlapBox(transform.position + boxAfterRot, Vector3.one/2*0.9f, Quaternion.identity, ~notPlayerMask);
            if (yea.Length > 0)
            {
                _rotationTimer = 3;
                return;
            }
        }
    }

    private void Rotation()
    {
        if (_rotationTimer < 1)
        {
            _rotationTimer += Time.fixedDeltaTime / rotationTimeAmount;
            if (_rotationTimer > 1)
            {
                _rotationTimer = 1;
            }
            _playerRigidBody.rotation = Quaternion.Lerp(_startRotation, _endRotation, _rotationTimer);
        }
        
        
    }
    
}
