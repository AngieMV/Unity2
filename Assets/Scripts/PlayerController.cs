using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : Unit
{

    [SerializeField]
    private Transform _CamPivot;

    [SerializeField]
    private float _MoveSpeed = 5f;

    [SerializeField]
    private float _MoveSpeedMult = 5f;

    [SerializeField]
    private float _JumpSpeed = 12f;

    [SerializeField]
    private KeyCode _SprintKey = KeyCode.LeftShift;

    [SerializeField]
    private KeyCode _JumpKey = KeyCode.Space;

    private Transform _Cam;
    private Rigidbody _RB;
    private float _XInput;
    private float _ZInput;
    private float _SpeedMult = 1f;
    private bool _JumpPressed;

    protected override void UnitAwake()
    {
        _RB = GetComponent<Rigidbody>();
        _Cam = Camera.main.transform;
    }

    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        ApplyCameraZoom();

        if(IsAlive == false) return;

        ReadMoveInputs();
        SetAnimValues();
        ApplyCameraRotation();
        ShootLasers();
    }
    
    private void FixedUpdate()
    {
        if(IsAlive == false) return;
        
        var newVel = new Vector3(_XInput, 0f, _ZInput) * _MoveSpeed * _SpeedMult;
        newVel = transform.TransformVector(newVel);
        newVel.y = _JumpPressed ? _JumpSpeed : _RB.velocity.y;
        _RB.velocity = newVel;
        
        _JumpPressed = false;
    }

    private void ShootLasers()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var ray = new Ray(_Cam.position, _Cam.forward);
            if(Physics.Raycast(ray, out var hit))
            {
                if(CanSee(hit.point, hit.transform))
                {
                    ShootLasersFromEyes(hit.point, hit.transform);
                }
            }
        }
    }

    private void SetAnimValues()
    {
        _Anim.SetFloat("Horizontal", _XInput);
        _Anim.SetFloat("Vertical", _ZInput);
        _Anim.SetFloat("SpeedMult", _SpeedMult);
    }

    private void ApplyCameraZoom()
    {
        var newZoom = _Cam.localPosition;
        newZoom.z += Input.mouseScrollDelta.y;
        newZoom.z = Mathf.Clamp(newZoom.z, -24f, 0f);
        _Cam.localPosition = newZoom;
    }

    private void ReadMoveInputs()
    {
        _XInput = Input.GetAxis("Horizontal");
        _ZInput = Input.GetAxis("Vertical");
        // if(Input.GetKey(KeyCode.LeftShift) == true)
        // {
        //     _SpeedMult = _MoveSpeedMult;
        // }
        // else
        // {
        //     _SpeedMult = 1f;
        // }

        //Ternary operator
        _SpeedMult = Input.GetKey(_SprintKey) ? _MoveSpeedMult : 1f;

        if(Input.GetKeyDown(_JumpKey))
        {
            _JumpPressed = true;
            _Anim.SetTrigger("Jump");
        }
    }

    private void ApplyCameraRotation()
    {
        var mouseX = Input.GetAxisRaw("Mouse X");
        var mouseY = Input.GetAxisRaw("Mouse Y");
        transform.Rotate(0f, mouseX, 0f);
        _CamPivot.Rotate(-mouseY, 0f, 0f);
    }

}
