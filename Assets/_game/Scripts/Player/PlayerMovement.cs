using System;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Assignables")]
    [Tooltip("this is a reference to the MainCamera object, not the parent of it.")]
    public Transform playerCam;
    public CinemachineVirtualCamera mainCamera;
    [Tooltip("reference to orientation object, needed for moving forward and not up or something.")]
    public Transform orientation;
    [Tooltip("LayerMask for ground layer, important because otherwise the collision detection wont know what ground is")]
    public LayerMask whatIsGround;
    private Rigidbody _rigidbody;
    private Controls _controls;
    private Animator _playerAnimator;

    [Header("Rotation and look")]
    private float xRotation;
    [Tooltip("mouse/look sensitivity")]
    public float sensitivity = 50f;
    private float sensMultiplier = 0.02f;

    private float defaultFOV = 90f;
    private float zoomOutFOV = 105f;

    [Header("Movement")]
    [Tooltip("additive force amount. every physics update that forward is pressed, this force (multiplied by 1/tickrate) will be added to the player.")]
    public float moveSpeed = 4500;
    [Tooltip("maximum local velocity before input is cancelled")]
    public float maxSpeed = 20;
    [Tooltip("normal countermovement when not crouching.")]
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    [Tooltip("the maximum angle the ground can have relative to the players up direction.")]
    public float maxSlopeAngle = 35f;
    [Tooltip("forward force for when a crouch is started.")]
    public float slideForce = 400;
    [Tooltip("countermovement when sliding. this doesnt work the same way as normal countermovement.")]
    public float slideCounterMovement = 0.2f;
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    [Tooltip("this determines the jump force but is also applied when jumping off of walls, if you decrease it, you may end up being able to walljump and then get back onto the wall leading to infinite height.")]
    public float jumpForce = 550f; 
    float x, y;
    bool jumping;
    private Vector3 normalVector = Vector3.up;

    [Header("Wallrunning")]
    private float actualWallRotation;
    private float wallRotationVel;
    private Vector3 wallNormalVector;
    [Tooltip("when wallrunning, an upwards force is constantly applied to negate gravity by about half (at default), increasing this value will lead to more upwards force and decreasing will lead to less upwards force.")]
    public float wallRunGravity = 1;
    [Tooltip("when a wallrun is started, an upwards force is applied, this describes that force.")]
    public float initialForce = 20f; 
    [Tooltip("float to choose how much force is applied outwards when ending a wallrun. this should always be greater than Jump Force")]
    public float escapeForce = 600f;
    private float wallRunRotation;
    [Tooltip("how much you want to rotate the camera sideways while wallrunning")]
    public float wallRunRotateAmount = 15f;
    [Tooltip("a bool to check if the player is wallrunning because thats kinda necessary.")]
    public bool isWallRunning;
    [Tooltip("a bool to determine whether or not to actually allow wallrunning.")]
    public bool useWallrunning = true;

    [Header("Collisions")]
    [Tooltip("a bool to check if the player is on the ground.")]
    public bool isGrounded;
    [Tooltip("a bool to check if the player is currently crouching.")]
    public bool isCrouching;
    private bool isSurfing;
    private bool cancellingGrounded;
    private bool cancellingSurf;
    private bool cancellingWall;
    private bool onWall;
    private bool cancelling;
    [SerializeField] private bool isDancing;

    float timer = 0;

    public static PlayerMovement Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        _rigidbody = GetComponent<Rigidbody>();
        _playerAnimator = GetComponent<Animator>();
        _controls = new Controls();

        // Jumping
        _controls.Player.Jump.started += _ => jumping = true;
        _controls.Player.Jump.performed += _ => jumping = true;
        _controls.Player.Jump.canceled += _ => jumping = false;
        
        // Crouching
        _controls.Player.Crouch.started += _ => StartCrouch();
        _controls.Player.Crouch.performed += _ => isCrouching = true;
        _controls.Player.Crouch.canceled += _ => StopCrouch();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        readyToJump = true;
        wallNormalVector = Vector3.up;
    }


    private void FixedUpdate()
    {
        if(isDancing) return;
        
        Movement();
    }

    private void Update()
    {
        if(isDancing) return;
        
        MyInput();
        Look();
        
        Animations();
    }

    private void LateUpdate()
    {
        //call the wallrunning Function
        WallRunning();
        WallRunRotate();
    }

    private void WallRunRotate()
    {
        FindWallRunRotation();
        float num = 33f;
        actualWallRotation = Mathf.SmoothDamp(actualWallRotation, wallRunRotation, ref wallRotationVel, num * Time.deltaTime);
        playerCam.localRotation = Quaternion.Euler(playerCam.rotation.eulerAngles.x, playerCam.rotation.eulerAngles.y, actualWallRotation);
    }
    
    private void MyInput()
    {
        x = _controls.Player.Movement.ReadValue<Vector2>().x;
        y = _controls.Player.Movement.ReadValue<Vector2>().y;
    }

    private void StartCrouch()
    {
        if (_rigidbody.velocity.magnitude > 0.2f)
        {
            mainCamera.m_Lens.FieldOfView = zoomOutFOV;

            if (isGrounded)
            {
                _rigidbody.AddForce(orientation.transform.forward * slideForce);
            }
        }
        else
        {
            mainCamera.m_Lens.FieldOfView = defaultFOV;
        }
    }

    private void StopCrouch()
    {
        mainCamera.m_Lens.FieldOfView = defaultFOV;

        isCrouching = false;
    }

    private void Movement()
    {
        //Extra gravity
        _rigidbody.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (isCrouching && isGrounded && readyToJump)
        {
            _rigidbody.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!isGrounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (isGrounded && isCrouching) multiplierV = 0f;

        //Apply forces to move player
        _rigidbody.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        _rigidbody.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if ((isGrounded || isWallRunning || isSurfing) && readyToJump)
        {
            _playerAnimator.SetTrigger("Jump");
            
            Vector3 velocity = _rigidbody.velocity;
            readyToJump = false;
            _rigidbody.AddForce(Vector2.up * jumpForce * 1.5f);
            _rigidbody.AddForce(normalVector * jumpForce * 0.5f);
            if (_rigidbody.velocity.y < 0.5f)
            {
                _rigidbody.velocity = new Vector3(velocity.x, 0f, velocity.z);
            }
            else if (_rigidbody.velocity.y > 0f)
            {
                _rigidbody.velocity = new Vector3(velocity.x, velocity.y / 2f, velocity.z);
            }
            if (isWallRunning)
            {
                _rigidbody.AddForce(wallNormalVector * jumpForce * 3f);
            }
            Invoke("ResetJump", jumpCooldown);
            if (isWallRunning)
            {
                isWallRunning = false;
            }
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = _controls.Player.Mouse.ReadValue<Vector2>().x * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = _controls.Player.Mouse.ReadValue<Vector2>().y * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        float clamp = 89.5f;
        xRotation = Mathf.Clamp(xRotation, -clamp, clamp);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!isGrounded || jumping) return;

        //Slow down sliding
        if (isCrouching)
        {
            _rigidbody.AddForce(moveSpeed * Time.deltaTime * -_rigidbody.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            _rigidbody.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            _rigidbody.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(_rigidbody.velocity.x, 2) + Mathf.Pow(_rigidbody.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = _rigidbody.velocity.y;
            Vector3 n = _rigidbody.velocity.normalized * maxSpeed;
            _rigidbody.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }
    
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rigidbody.velocity.x, _rigidbody.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = _rigidbody.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }
    //a lot of math (dont touch)
    private void FindWallRunRotation()
    {

        if (!isWallRunning)
        {
            wallRunRotation = 0f;
            return;
        }
        _ = new Vector3(0f, playerCam.transform.rotation.y, 0f).normalized;
        new Vector3(0f, 0f, 1f);
        float num = 0f;
        float current = playerCam.transform.rotation.eulerAngles.y;
        if (Math.Abs(wallNormalVector.x - 1f) < 0.1f)
        {
            num = 90f;
        }
        else if (Math.Abs(wallNormalVector.x - -1f) < 0.1f)
        {
            num = 270f;
        }
        else if (Math.Abs(wallNormalVector.z - 1f) < 0.1f)
        {
            num = 0f;
        }
        else if (Math.Abs(wallNormalVector.z - -1f) < 0.1f)
        {
            num = 180f;
        }
        num = Vector3.SignedAngle(new Vector3(0f, 0f, 1f), wallNormalVector, Vector3.up);
        float num2 = Mathf.DeltaAngle(current, num);
        wallRunRotation = (0f - num2 / 90f) * wallRunRotateAmount;
        if (!useWallrunning)
        {
            return;
        }
        if ((Mathf.Abs(wallRunRotation) < 4f && y > 0f && Math.Abs(x) < 0.1f) || (Mathf.Abs(wallRunRotation) > 22f && y < 0f && Math.Abs(x) < 0.1f))
        {
            if (!cancelling)
            {
                cancelling = true;
                CancelInvoke("CancelWallrun");
                Invoke("CancelWallrun", 0.2f);
            }
        }
        else
        {
            cancelling = false;
            CancelInvoke("CancelWallrun");
        }
    }

    private bool IsFloor(Vector3 v)
    {
        return Vector3.Angle(Vector3.up, v) < maxSlopeAngle;
    }

    private bool IsSurf(Vector3 v)
    {
        float num = Vector3.Angle(Vector3.up, v);
        if (num < 89f)
        {
            return num > maxSlopeAngle;
        }
        return false;
    }

    private bool IsWall(Vector3 v)
    {
        return Math.Abs(90f - Vector3.Angle(Vector3.up, v)) < 0.05f;
    }

    private bool IsRoof(Vector3 v)
    {
        return v.y == -1f;
    }
    
    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if ((int)whatIsGround != ((int)whatIsGround | (1 << layer)))
        {
            return;
        }
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            if (IsFloor(normal))
            {
                if (isWallRunning)
                {
                    isWallRunning = false;
                }
                isGrounded = true;
                normalVector = normal;
                cancellingGrounded = false;
                CancelInvoke("StopGrounded");
            }
            if (IsWall(normal) && (layer == (int)whatIsGround || (int)whatIsGround == -1 || layer == LayerMask.NameToLayer("Ground") || layer == LayerMask.NameToLayer("ground"))) //seriously what is this
            {
                StartWallRun(normal);
                onWall = true;
                cancellingWall = false;
                CancelInvoke("StopWall");
            }
            if (IsSurf(normal))
            {
                isSurfing = true;
                cancellingSurf = false;
                CancelInvoke("StopSurf");
            }
            IsRoof(normal);
        }
        float num = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke("StopGrounded", Time.deltaTime * num);
        }
        if (!cancellingWall)
        {
            cancellingWall = true;
            Invoke("StopWall", Time.deltaTime * num);
        }
        if (!cancellingSurf)
        {
            cancellingSurf = true;
            Invoke("StopSurf", Time.deltaTime * num);
        }
    }

    private void StopGrounded()
    {
        isGrounded = false;
    }

    private void StopWall()
    {
        onWall = false;
        isWallRunning = false;
    }

    private void StopSurf()
    {
        isSurfing = false;
    }

    //wallrunning functions
    private void CancelWallrun()
    {
        //for when we want to stop wallrunning
        _rigidbody.AddForce(wallNormalVector * escapeForce);
    }

    private void StartWallRun(Vector3 normal)
    {
        //cancels all y momentum and then applies an upwards force.
        if (!isGrounded && useWallrunning)
        {
            wallNormalVector = normal;
            if (!isWallRunning)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
                _rigidbody.AddForce(Vector3.up * initialForce, ForceMode.Impulse);
            }
            isWallRunning = true;
        }
    }

    private void WallRunning()
    {
        //checks if the wallrunning bool is set to true and if it is then applies
        //a force to counter gravity enough to make it feel like wallrunning
        if (isWallRunning)
        {
            _rigidbody.AddForce(-wallNormalVector * Time.deltaTime * moveSpeed);
            _rigidbody.AddForce(Vector3.up * Time.deltaTime * _rigidbody.mass * 40f * wallRunGravity);
        }
    }

    private void Animations()
    {
        _playerAnimator.SetFloat("InputX", x);
        _playerAnimator.SetFloat("InputY", y);
        
        _playerAnimator.SetBool("isCrouching", isCrouching);
        
        _playerAnimator.SetBool("isGrounded", isGrounded);
    }
    
    private void OnEnable()
    {
        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
    }
}