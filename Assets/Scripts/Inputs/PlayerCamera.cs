using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private float _distance = 6f;
    [SerializeField] private float _sensitivity = 2f;
    [SerializeField] private float _minY = -30f;
    [SerializeField] private float _maxY = 60f;

    private float _yaw;
    private float _pitch;

    private Camera _cam;
    
    private InputAction _lookAction;

    private void Start()
    {
        _lookAction = InputSystem.actions.FindAction("Look");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _cam = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (!_cam) return;

        float mouseX = _lookAction.ReadValue<Vector2>().x * _sensitivity * 10f * Time.deltaTime;
        float mouseY = _lookAction.ReadValue<Vector2>().y * _sensitivity * 10f * Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;

        _pitch = Mathf.Clamp(_pitch, _minY, _maxY);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -_distance);

        _cam.transform.position = transform.position + offset;
        _cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}