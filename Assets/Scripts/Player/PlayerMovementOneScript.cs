﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum State
{
    Idle,
    Moving,
    Sprinting,
    InAir,
    WallRunning
}
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementOneScript : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [Header("Speeds")]
    public float CurrentSpeed = 6f; //How fast the player is currently moving
    [Tooltip("AMS = Absolute minimum Speed, the speed the player will move without any debuffs or buffs. Default = 12f")]
    public float MaxBaseSpeed = 16f;
    public float MidBaseSpeed;
    public float MinBaseSpeed = 3f;
    public float Acceleration = 4f; //how fast the player will reach max base speed from min base speed
    public float Deceleration = 5f; //how fast the player will reach min base speed from buffed speed and how fast the player will reach idle from AMS with 0 input
    public float JumpHeight;
    public float InAirAcceleration; //this is for input movement
    public float InAirDeceleration; //this too

    [Header("INPUT")]
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 directionNormal;
    [Tooltip("The A-D input")]
    private float horizontal;
    [Tooltip("The W-S input")]
    private float vertical;
    public bool IsSprinting;

    [Header("Checks and physics")]
    public Transform GroundCheck;
    public bool IsGrounded;
    public LayerMask GroundMask;
    public LayerMask WallMask;
    public float groundDistance = 0.4f;
    public float Gravity = 30.0f;
    private Vector3 _playerVelocity = Vector3.zero;

    [Header("Multiplyiers")]
    public float InAirMulti;
    public float SlideMulti;
    [Header("Drag")]
    public float InAirDrag;
    [Header("Debugging")]
    public float TotalVelocity;
    public float KPH;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        IsGrounded = false;
        state = State.Idle; //make sure this is run AFTER the variables being set
        NextState();
    }
    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        directionNormal = new Vector3(horizontal, 0f, vertical).normalized;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            IsSprinting = !IsSprinting;
        }
       
        if (IsGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playerVelocity.y = JumpHeight;
            }
            if (directionNormal.magnitude >= 0.01f)
            {
                if (directionNormal.z > 0.5f && IsSprinting)
                {
                    state = State.Sprinting;
                }
                else
                {
                    state = State.Moving;
                }
               
                
            }
            else
            {
                state = State.Idle;
            }

        }
        else
        {
            state = State.InAir;
        }
        //GRAVITY
        //=============================================================================
        if (IsGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -3f;
        }
        _playerVelocity.y -= Gravity * Time.deltaTime;
        characterController.Move(_playerVelocity * Time.deltaTime);
        //============================================================================
        
        TotalVelocity = characterController.velocity.magnitude;
    }
    private void FixedUpdate()
    {
        IsGrounded = Physics.CheckSphere(GroundCheck.position, groundDistance, GroundMask);
    }
    #region States
    State state;
    void NextState()
    {
        string methodName = state.ToString() + "State";
        System.Reflection.MethodInfo info = GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        StartCoroutine((IEnumerator)info.Invoke(this, null));
    }
    private IEnumerator IdleState()
    {
        while (state == State.Idle)
        {
            IsSprinting = false;
            if (CurrentSpeed > MinBaseSpeed)
            {
                CurrentSpeed = CurrentSpeed * (1f - Time.deltaTime * Deceleration);
                characterController.Move(moveDirection * CurrentSpeed * Time.deltaTime);
            }
            TotalVelocity = characterController.velocity.magnitude;
            yield return null;
        }
        NextState();
    }
    private IEnumerator MovingState()
    {
        while (state == State.Moving)
        {
            IsSprinting = false;
            moveDirection = transform.TransformDirection(new Vector3(directionNormal.x, 0f, directionNormal.z));
            if (CurrentSpeed < MidBaseSpeed)
            {
                //add speed untill max is reaches
                CurrentSpeed = CurrentSpeed * (1f + Time.deltaTime * Acceleration);
            }
            else if (CurrentSpeed > MidBaseSpeed)
            {
                CurrentSpeed = CurrentSpeed * (1f - Time.deltaTime * Deceleration);
            }
            characterController.Move(moveDirection * CurrentSpeed * Time.deltaTime);
            TotalVelocity = characterController.velocity.magnitude;
            yield return null;
        }
        NextState();
    }
    private IEnumerator SprintingState()
    {
        while (state == State.Sprinting)
        {
            moveDirection = transform.TransformDirection(new Vector3(directionNormal.x, 0f, directionNormal.z));
            if (CurrentSpeed < MaxBaseSpeed)
            {
                CurrentSpeed = CurrentSpeed * (1f + Time.deltaTime * Acceleration);
            }
            characterController.Move(moveDirection * CurrentSpeed * Time.deltaTime);
            TotalVelocity = characterController.velocity.magnitude;
            yield return null;
        }
        NextState();
    }
    private IEnumerator InAirState()
    {
        while (state == State.InAir)
        {

            CurrentSpeed = CurrentSpeed * (1f - Time.deltaTime * InAirDrag);
            characterController.Move(moveDirection * CurrentSpeed * InAirMulti * Time.deltaTime);
            TotalVelocity = characterController.velocity.magnitude;
            yield return null;
        }
        NextState();
    }
    private IEnumerator WallRunningState()
    {
        while (state == State.WallRunning)
        {
            yield return null;
        }
        NextState();
    }
    #endregion
}
