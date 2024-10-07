using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _moveAction;
    
    [SerializeField]
    private Camera _playerCamera;
    private Rigidbody _playerRigidBody;

    private Vector2 _inputVector;
    
    private void Start()
    {
        _playerRigidBody = GetComponent<Rigidbody>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        _moveAction.action.Enable();
    }

    private void Update()
    {
        _inputVector = _moveAction.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 forward = _playerCamera.transform.forward.With(y:0).normalized;
        Vector3 right = _playerCamera.transform.right.With(y:0).normalized;

        Vector3 velocty = _playerRigidBody.velocity;
        velocty += (forward * _inputVector.y + right * _inputVector.x);
        velocty *= 0.9f;
        velocty.y -= 9.81f;


        _playerRigidBody.velocity = velocty;
    }
}
