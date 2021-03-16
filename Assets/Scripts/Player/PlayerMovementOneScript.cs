using System;
using System.Collections;
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

    //Note that all speeds are in meters per second
    [Header("Speeds")]
    [Tooltip("This is the speed the player moves at as far as the input is concerened || Default = 6f")]
    public float CurrentSpeed = 6f; 
    [Tooltip("This is the max speed the player can go to, they can only reach it by moving forward and sprinting || Default = 7f")]
    public float MaxBaseSpeed = 7f;
    [Tooltip("This is the VERY default speed, it is the speed the player moves at in all directions without sprinting || Default = 5f")]
    public float MidBaseSpeed = 5f;
    [Tooltip("This is the speed the player starts moving at and decelerates to. NOTE: If the currentSpeed somehow falls bellow this value the player can't move, I have built in saftey nets just in case || Default = 2f")]
    public float MinBaseSpeed = 2f;
    [Tooltip("Pretty self explanitory rly, this is how fast the player gets from one speed to another || Default = 4")]
    public float Acceleration = 4f;
    [Tooltip("The exact Opposite of Acceleration || Default = 5f")]
    public float Deceleration = 5f; 
    [Tooltip("How high (in meteres) the player jumps, they are a ninja so they can jump 11 meters high, shut up || Default = 11f")]
    public float JumpHeight = 11f;
    [Tooltip("Deceleration for Moving in the air, This is optional, the script does not need it but it might make the game better later || Default = 0f")]
    public float InAirDeceleration = 0f; 
    [Tooltip("This is the amount of buffer room the player can move once they are in the air, it is added to the MaxAirVelocity || Default = 0.3f")]
    public float AirControllBuffer = 0.3f;
    [Tooltip("This is multiplied to the InAirMovement to allow the player to be cool || Default = 5f")]
    public float InAirControl;

    [Header("INPUT")]
    [Tooltip("This is what the character controller looks to for the move direction, the move direction looks to the Direction normal for the direction, which it then transforms the direction from local space to world space, the moveDirection gets the input from InAirMovement while its in the air but still keeps the movement from the ground so the velocity carries over")]
    private Vector3 moveDirection = Vector3.zero;
    [Tooltip("This vector3 gets input from the x and z axis and then normalizes it so the player can't diagonal run to go faster, the InAirMovement gets the input from here aswell as the MoveDirection vetor3")]
    private Vector3 directionNormal;
    [Tooltip("When the player enters the air, the player still moves based on moveDirection but it doesnt get updated every frame like in moving state, when the code detects input it goes to this vector3 which is multiplied by InAirControll and then added or subtracted from the moveDirection Vector3 every second, this has the fluid feeling of velocity alteration. Based off of titanfall 2 and various 2d platformers")]
    private Vector3 InAirMove = Vector3.zero;
    [Tooltip("The InAirMove gets added to move direction every second, this is the (soft) limit to how much velocity the player can build up in the air")]
    private Vector3 MaxAirVelocity;
    [Tooltip("The A-D input")]
    private float horizontal;
    [Tooltip("The W-S input")]
    private float vertical;
    [Tooltip("Do I even need to explain this one?")]
    private bool IsSprinting;

    [Header("Checks and physics")]
    [Tooltip("Unity's in-built character controller has a 'check if grounded' function but it rarely works for me so i have the habit of making a custom check my self, this transform is just the location of the detection")]
    public Transform GroundCheck;
    [Tooltip("Is the ground under the player with in the specified parameteres?")]
    public bool IsGrounded;
    [Tooltip("Hmm yes the floor is made out of floor")]
    public LayerMask GroundMask;
    public LayerMask WallMask;
    [Tooltip("This is the radius of the sphere cast that detects the ground, this has to be equal or greater than the character controllers radius || def = 0.4f")]
    public float groundDistance = 0.4f;
    [Tooltip("There is a thing, known as gravity... do you believe it? IRL gravity is about 9.7 meteres a second, this is 25 meteres a second || def = 25f")]
    public float Gravity = 25f;
    [Tooltip("Issac newton can shut the fuck up")]
    public float WallGravity = 10f;
    [Tooltip("This one is purley for gravity, since im not a pleb who uses a rigidbody movement, I like to keep gravity and move direction seperate ")]
    private Vector3 _playerVelocity = Vector3.zero;

    [Header("Debugging")]
    [Tooltip("How fast the player is moving at ANY given time in meteres a second")]
    public float TotalVelocity;
    [Tooltip("just gotta time the TotalVelocity by 3.6f and its in Kilometers an hour")]
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
        //INPUT and CHECKS
        //========================================================================================================================================================
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        directionNormal = new Vector3(horizontal, 0f, vertical).normalized;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            IsSprinting = !IsSprinting;
        }
        if (IsGrounded)
        {
            MaxAirVelocity = transform.TransformDirection(new Vector3(Mathf.Abs(moveDirection.x) + AirControllBuffer, 0f, Mathf.Abs(moveDirection.z) + AirControllBuffer));
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
        //======================================================================================================================================================
        
        //GRAVITY PHYSICS
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
            else
            {
                CurrentSpeed = MinBaseSpeed;
                moveDirection = Vector3.zero;
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
            InAirMove = transform.TransformDirection(new Vector3(directionNormal.x * InAirControl, 0f, directionNormal.z * InAirControl));
            if (directionNormal.magnitude != 0f)
            {
                if (Mathf.Abs(moveDirection.magnitude) <= Mathf.Abs(MaxAirVelocity.magnitude))
                {
                    moveDirection += InAirMove * Time.deltaTime;
                }
                else if (Mathf.Abs(moveDirection.magnitude) < 1.32f)
                {
                    moveDirection -= InAirMove * Time.deltaTime;
                }
                else
                {
                    moveDirection = moveDirection / 2f;
                }
            }
            CurrentSpeed = CurrentSpeed * (1f - Time.deltaTime * InAirDeceleration);
            characterController.Move(moveDirection * CurrentSpeed * Time.deltaTime);  
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
