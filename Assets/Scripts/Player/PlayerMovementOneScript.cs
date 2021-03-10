using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum State
{
    Idle,
    Moving,
    InAir,
    WallRunning
}
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementOneScript : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [Header("Speeds")]
    public float CurrentSpeed; //How fast the player is currently moving
    [Tooltip("AMS = Absolute minimum Speed, the speed the player will move without any debuffs or buffs. Default = 12f")]
    public float MaxBaseSpeed = 12f;
    public float MinBaseSpeed = 6f;
    public float Acceleration = 6f; //how fast the player will reach max base speed from min base speed
    public float Deceleration = 6f; //how fast the player will reach min base speed from buffed speed and how fast the player will reach idle from AMS with 0 input
    public float JumpHeight;

    [Header("INPUT")]
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 directionNormal;
    [Tooltip("The A-D input")]
    private float horizontal;
    [Tooltip("The W-S input")]
    private float vertical;

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
        if (IsGrounded)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playerVelocity.y = JumpHeight;
            }
            if (directionNormal.magnitude >= 0.01f)
            {
                state = State.Moving;
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
    public void Move()
    {

        if (IsGrounded)
        {
            //if the players current speed is less than the minimum default speed
            if (CurrentSpeed < MaxBaseSpeed)
            {
                //add speed untill max is reaches
                CurrentSpeed = CurrentSpeed + Acceleration * Time.deltaTime;
            }
            if (CurrentSpeed > MinBaseSpeed)
            {
                CurrentSpeed = CurrentSpeed - Deceleration * Time.deltaTime;
            }
            characterController.Move(moveDirection * CurrentSpeed * Time.deltaTime);
        }
    }
    public void Jump()
    {

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
            if (CurrentSpeed > MinBaseSpeed)
            {
                CurrentSpeed = CurrentSpeed * (1f - Time.deltaTime * Deceleration);
                //CurrentSpeed = CurrentSpeed - Deceleration * Time.deltaTime;
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
            moveDirection = transform.TransformDirection(new Vector3(directionNormal.x, 0f, directionNormal.z));
            if (CurrentSpeed < MaxBaseSpeed)
            {
                //add speed untill max is reaches
                //CurrentSpeed = CurrentSpeed + Acceleration * Time.deltaTime;
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
            //CurrentSpeed = CurrentSpeed - InAirDrag * Time.deltaTime;
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
