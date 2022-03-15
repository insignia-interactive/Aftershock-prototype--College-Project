using UnityEngine;
using Photon.Pun;

public class InputManager : MonoBehaviour
{
    private PhotonView PV;
    private PlayerManager _playerManager;
    private Controls _controls;

    private PlayerMovement _playerMovement;

    private Rigidbody _rigidbody;

    [SerializeField] private Transform cameraHolder;
    [SerializeField] private GameObject FPSCam;
    [SerializeField] private GameObject EmoteCam;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        _playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        _playerMovement = GetComponentInChildren<PlayerMovement>();

        // If PhotonView is owned by player allow them to use input
        if (PV.IsMine)
        {
            _controls = new Controls();
        
            // Jumping
            _controls.Player.Jump.started += _ => _playerMovement.jumping = true;
            _controls.Player.Jump.performed += _ => _playerMovement.jumping = true;
            _controls.Player.Jump.canceled += _ => _playerMovement.jumping = false;
        
            // Crouching
            _controls.Player.Crouch.started += _ => _playerMovement.StartCrouch();
            _controls.Player.Crouch.performed += _ => _playerMovement.isCrouching = true;
            _controls.Player.Crouch.canceled += _ => _playerMovement.StopCrouch();
        
            // Emote
            _controls.Player.Emote.performed += _ => _playerMovement.Emote();
        }
    }

    private void Start()
    {
        // If PhotonView is not owned by player delete the cameras and rigidbody (Rigidbody isnt needed as postions, rotation and animations are synced across the server || Camera isnt needed as it can cause the wrong camera to be displayed)
        if (!PV.IsMine)
        {
            FPSCam.SetActive(false);
            EmoteCam.SetActive(false);
            Destroy(_rigidbody);
        }
    }

    private void Update()
    {
        MyInput();
    }

    public void MyInput()
    {
        // If PhotonView is owned by player allow them to use input
        if (PV.IsMine)
        {
            _playerMovement.x = _controls.Player.Movement.ReadValue<Vector2>().x;
            _playerMovement.y = _controls.Player.Movement.ReadValue<Vector2>().y;
            
            _playerMovement.mouseX = _controls.Player.Mouse.ReadValue<Vector2>().x;
            _playerMovement.mouseY = _controls.Player.Mouse.ReadValue<Vector2>().y;
        }
    }

    private void Respawn()
    {
        _playerManager.Respawn();
    }

    // Sets players rotation according to the spawnpoint rotation
    public void SetRotation(Transform spawnpoint)
    {
        cameraHolder.rotation = spawnpoint.rotation;
    }
    
    private void OnEnable()
    {
        if(PV.IsMine) _controls.Player.Enable();
    }

    private void OnDisable()
    {
        if(PV.IsMine) _controls.Player.Disable();
    }
}
