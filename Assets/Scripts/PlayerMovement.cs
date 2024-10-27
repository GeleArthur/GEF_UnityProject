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
    
    private PlayerBodyManagement _bodyManagement;
    private Camera _playerCamera;
    
    private Rigidbody _playerRigidBody;
    private Vector2 _inputVector;
    
    private bool _isGrounded;
    
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
    
    private Vector3[] _cornerOffset =
    {
        new(1, 0, 0),
        new(-1, 0, 0),
        new(0, 1, 0),
        new(0, -1, 0),
        new(0, 0, 1),
        new(0, 0, -1),
    };

    // TODO: Dont self intersect
    private void GroundCheck()
    {
        foreach (GameObject bodyPart in _bodyManagement.BodyParts)
        {
            Debug.DrawRay(bodyPart.transform.position, Vector3.down * 1.0f, Color.red);
            if (Physics.Raycast(bodyPart.transform.position, Vector3.down, 1.0f, ~notPlayerMask, QueryTriggerInteraction.Ignore))
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
        
        if (_isGrounded && movementDirection.sqrMagnitude < 0.0001f ) 
            _playerRigidBody.useGravity = false;
        else
            _playerRigidBody.useGravity = true;
        
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
            _playerRigidBody.rotation = magic * _playerRigidBody.rotation;
        }
        else
        {
            Quaternion magic = Quaternion.FromToRotation(Vector3.forward, input.y > 0 ? Vector3.up : Vector3.down);
            _playerRigidBody.rotation = magic * _playerRigidBody.rotation;
        }
        
        // Matrix4x4 formula = Matrix4x4.Translate(_bodyManagement.CenterOfMass - transform.position) * Matrix4x4.Rotate(magic) *
        //                     Matrix4x4.Translate(transform.position - _bodyManagement.CenterOfMass);

        // transform.position += _bodyManagement.CenterOfMass;
        
        // transform.position -= _bodyManagement.CenterOfMass;
        
        // var hi = transform.;


        // float rotation = _playerCamera.transform.rotation.eulerAngles.y * Mathf.Deg2Rad + Mathf.Atan2(input.y, input.x);

        // Debug.DrawRay(_bodyManagement.CenterOfMass, yea, Color.red, 3.0f);
    }

    private void Rotation()
    {
        // Debug.DrawRay(transform.position, _rotationGoal * 3.0f, Color.red);
        // _rotationGoal.Normalize();
        // Quaternion deltaRotation = Quaternion.LookRotation(_rotationGoal) * Quaternion.Inverse(_playerRigidBody.rotation);
        // deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        //
        // _playerRigidBody.angularVelocity = (axis * angle * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }
    
}
