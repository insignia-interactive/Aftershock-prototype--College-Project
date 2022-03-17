using System;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Assignables")]
    [Tooltip("this is a reference to the MainCamera object, not the parent of it.")]
    public Transform playerCam;
    public CinemachineVirtualCamera mainCamera;
    public CinemachineFreeLook emoteCamera;
    [Tooltip("reference to orientation object, needed for moving forward and not up or something.")]
    public Transform orientation;
    [Tooltip("LayerMask for ground layer, important because otherwise the collision detection wont know what ground is")]
    public LayerMask whatIsGround;
    private Rigidbody _rigidbody;
    private Animator _playerAnimator;
    public PhotonView PV;

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
    public float x, y;
    public float mouseX, mouseY;
    public bool jumping;
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
    [SerializeField] private bool isJumping;

    float timer = 0;

    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

    public static PlayerMovement Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        _rigidbody = GetComponent<Rigidbody>();
        _playerAnimator = GetComponent<Animator>();
        
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
        // If player is dancing the return (Stops movement while player is dancing)
        if(isDancing) return;
        
        // Calls the movement function
        Movement();
    }

    private void Update()
    {
        // If player is dancing the return (Stops movement while player is dancing)
        if(isDancing) return;
        
        // Calls the Look function
        Look();
        
        // Calls the Animations function
        Animations();
    }

    private void LateUpdate()
    {
        // Call the wallrunning Functions
        WallRunning();
        WallRunRotate();
    }

    private void WallRunRotate()
    {
        // Angles the camera depending on the wall run
        FindWallRunRotation();
        float num = 33f;
        actualWallRotation = Mathf.SmoothDamp(actualWallRotation, wallRunRotation, ref wallRotationVel, num * Time.deltaTime);
        playerCam.localRotation = Quaternion.Euler(playerCam.rotation.eulerAngles.x, playerCam.rotation.eulerAngles.y, actualWallRotation);
    }

    public void StartCrouch()
    {
        // If function is called and player is dancing call the CancelEmote function
        if(isDancing) CancelEmote();
        
        // Checks if rigidbody magnitude is bigger than 0.2 and adds FOV zoom and if grounded also adds slide force to the player
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
            // If rigidbody has no magnitude dont zoom FOV
            mainCamera.m_Lens.FieldOfView = defaultFOV;
        }
    }

    public void StopCrouch()
    {
        // Resets camera FOV
        
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
        // If function is called and player is dancing call the CancelEmote function
        if(isDancing) CancelEmote();
        
        // If function is called and isGrounded or isWallRunning or isSurfing and is ready to jump the make player jump
        if ((isGrounded || isWallRunning || isSurfing) && readyToJump)
        {
            // Adds force to the player depending on speed or wallrunning
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
            // Invokes the reset jump function with a cooldown
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
        mouseX = mouseX * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        mouseY = mouseY * sensitivity * Time.fixedDeltaTime * sensMultiplier;

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
        // If player isnt grounded or is jumping then return
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
        // Gets the players look angle and move angle
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rigidbody.velocity.x, _rigidbody.velocity.z) * Mathf.Rad2Deg;

        // Calculate the smallest distance between look angle and move angle
        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;
        
        // Gets players magnitude
        float magnitude = _rigidbody.velocity.magnitude;
        // Calculates yMag multiplying magnitude by the smallest distance between look angle and move angle multiplied by the degrees-to-radians conversion
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        // Calculates xMag multiplying magnitude by 90 - the smallest distance between look angle and move angle multiplied by the degrees-to-radians conversion
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        // returns the xMag and yMag as a vector2
        return new Vector2(xMag, yMag);
    }

    private void FindWallRunRotation()
    {
        // If player isnt wall running set wallRunRotation to 0
        if (!isWallRunning)
        {
            wallRunRotation = 0f;
            return;
        }
        
        float num = 0f;
        // Gets playercam current rotation on the y axis
        float current = playerCam.transform.rotation.eulerAngles.y;
        
        // if wallNormalVector.x -1 is smaller than 0.1f then num is 90
        if (Math.Abs(wallNormalVector.x - 1f) < 0.1f)
        {
            num = 90f;
        }
        // if wallNormalVector.x -1 is smaller than 0.1f then num is 270
        else if (Math.Abs(wallNormalVector.x - -1f) < 0.1f)
        {
            num = 270f;
        }
        // if wallNormalVector.x -1 is smaller than 0.1f then num is 0
        else if (Math.Abs(wallNormalVector.z - 1f) < 0.1f)
        {
            num = 0f;
        }
        // if wallNormalVector.x -1 is smaller than 0.1f then num is 180
        else if (Math.Abs(wallNormalVector.z - -1f) < 0.1f)
        {
            num = 180f;
        }
        num = Vector3.SignedAngle(new Vector3(0f, 0f, 1f), wallNormalVector, Vector3.up);
        // Gets the shortest distance between current and num and saves it as num2
        float num2 = Mathf.DeltaAngle(current, num);
        // The wall run rotation is calculated by dividing num2 by 90 and subtracting that by 0 then multiplying the answer by wallRunRotateAmount
        wallRunRotation = (0f - num2 / 90f) * wallRunRotateAmount;
        // if not useWallRunning then return
        if (!useWallrunning)
        {
            return;
        }
        // Cancel wall run
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

    // Detects if player is walking on the floor
    private bool IsFloor(Vector3 v)
    {
        return Vector3.Angle(Vector3.up, v) < maxSlopeAngle;
    }

    // Detects if player is surfing
    private bool IsSurf(Vector3 v)
    {
        float num = Vector3.Angle(Vector3.up, v);
        // if angle is smaller than 89 degrees then return bool depending on if num is greater than maxSlopeAngle
        if (num < 89f)
        {
            return num > maxSlopeAngle;
        }
        return false;
    }

    // Detects if player is walking on the wall
    private bool IsWall(Vector3 v)
    {
        return Math.Abs(90f - Vector3.Angle(Vector3.up, v)) < 0.05f;
    }

    // Detects if player touches the roof
    private bool IsRoof(Vector3 v)
    {
        return v.y == -1f;
    }
    
    // ground check
    private void OnCollisionStay(Collision other)
    {
        // Gets the layer from the collision object 
        int layer = other.gameObject.layer;
        if ((int)whatIsGround != ((int)whatIsGround | (1 << layer)))
        {
            return;
        }
        // Loops through all collision objects
        for (int i = 0; i < other.contactCount; i++)
        {
            // Gets the contacts normal
            Vector3 normal = other.contacts[i].normal;
            // Calls the IsFloor function which returns a bool
            if (IsFloor(normal))
            {
                // Player is grounded
                if (isWallRunning)
                {
                    isWallRunning = false;
                }
                isGrounded = true;
                isJumping = false;
                normalVector = normal;
                cancellingGrounded = false;
                CancelInvoke("StopGrounded");
            }
            // Detects if player is on the wall
            if (IsWall(normal) && (layer == (int)whatIsGround || (int)whatIsGround == -1 || layer == LayerMask.NameToLayer("Ground") || layer == LayerMask.NameToLayer("ground")))
            {
                // Player is wall running
                StartWallRun(normal);
                onWall = true;
                cancellingWall = false;
                CancelInvoke("StopWall");
            }
            // Calls the IsSurf function which returns a bool
            if (IsSurf(normal))
            {
                // Player is surfing
                isSurfing = true;
                cancellingSurf = false;
                CancelInvoke("StopSurf");
            }
            // Calls IsRoof function
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

    // Called when player is no longer grounded
    private void StopGrounded()
    {
        isGrounded = false;
        isJumping = true;
    }

    // Called when player is no longer wall running
    private void StopWall()
    {
        onWall = false;
        isWallRunning = false;
    }

    // Called when player is no longer surfing
    private void StopSurf()
    {
        isSurfing = false;
    }

    //wallrunning functions
    private void CancelWallrun()
    {
        // For when we want to stop wallrunning
        _rigidbody.AddForce(wallNormalVector * escapeForce);
    }

    private void StartWallRun(Vector3 normal)
    {
        // cancels all y momentum and then applies an upwards force
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
        // Checks if the wallrunning bool is set to true and if it is then applies
        // A force to counter gravity enough to make it feel like wallrunning
        if (isWallRunning)
        {
            _rigidbody.AddForce(-wallNormalVector * Time.deltaTime * moveSpeed);
            _rigidbody.AddForce(Vector3.up * Time.deltaTime * _rigidbody.mass * 40f * wallRunGravity);
        }
    }

    public void Emote()
    {
        if (!isDancing)
        {
            isDancing = true;
            // Swaps camera to the 3rd person camera
            emoteCamera.Priority = 2;
            _playerAnimator.ResetTrigger("CancelEmote");

            if (PlayerPrefs.HasKey("Emote"))
            {
                string emote = PlayerPrefs.GetString("Emote");
                switch (emote)
                {
                    case "Pushup":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "Loser":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "Threatening":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "ChickenDance":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "DancingMaraschinoStep":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "DancingTwerk":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "GangnamStyle":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "MacarenaDance":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "RobotHipHopDance":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "Shuffling":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "SillyDancing":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                    case "TwistDance":
                        PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, emote }, raiseEventOptions, SendOptions.SendReliable);
                        break;
                }
            }
            else
            {
                PlayerPrefs.SetString("Emote", "Loser");
                PhotonNetwork.RaiseEvent(EmoteAnimate, new object[] { PV.ViewID, "Loser" }, raiseEventOptions, SendOptions.SendReliable);
            }
        } else
        {
            PhotonNetwork.RaiseEvent(CancelEmoteAnimate, new object[] { PV.ViewID }, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    // Called when animation is cancelled
    private void CancelEmote()
    {
        isDancing = false;
        // Swaps camera back to the FPS camera
        emoteCamera.Priority = 0;
        _playerAnimator.SetTrigger("CancelEmote");
    }
    
    // Plays animations
    private void Animations()
    {
        _playerAnimator.SetFloat("InputX", x);
        _playerAnimator.SetFloat("InputY", y);
        
        _playerAnimator.SetBool("isCrouching", isCrouching);
        
        _playerAnimator.SetBool("isGrounded", isGrounded);

        _playerAnimator.SetBool("isJump", isJumping);

        _playerAnimator.SetBool("isWallrunning", onWall);
    }

    public const byte EmoteAnimate = 1;
    public const byte CancelEmoteAnimate = 2;

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == EmoteAnimate)
        {
            object[] data = (object[])photonEvent.CustomData;
            int targetPV = (int)data[0];

            if (targetPV == PV.ViewID)
            {
                Debug.Log((string)data[1]);
                _playerAnimator.SetTrigger((string)data[1]);
            }
        } else if (eventCode == CancelEmoteAnimate)
        {
            object[] data = (object[])photonEvent.CustomData;
            int targetPV = (int)data[0];

            if (targetPV == PV.ViewID)
            {
                CancelEmote();
            }
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
}