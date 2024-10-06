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
        var movement = _moveAction.action.ReadValue<Vector2>();

        var forward = _playerCamera.transform.forward.With(y:0).normalized;
        var right = _playerCamera.transform.right.With(y:0).normalized;
        
        _playerRigidBody.velocity += (forward * movement.y + right * movement.x) * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        
    }
}
