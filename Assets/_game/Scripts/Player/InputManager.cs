using UnityEngine;
using Photon.Pun;

public class InputManager : MonoBehaviour
{
    private PhotonView PV;
    private PlayerManager _playerManager;
    private Controls _controls;

    private PlayerController _playerController;

    private Rigidbody _rigidbody;

    [SerializeField] private Transform cameraHolder;
    [SerializeField] private GameObject FPSCam;
    [SerializeField] private GameObject EmoteCam;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject WeaponUI;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        _playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        _playerController = GetComponentInChildren<PlayerController>();
        _playerController.PV = PV;

        // If PhotonView is owned by player allow them to use input
        if (PV.IsMine)
        {
            _controls = new Controls();
        
            // Jumping
            _controls.Player.Jump.started += _ => _playerController.jumping = true;
            _controls.Player.Jump.performed += _ => _playerController.jumping = true;
            _controls.Player.Jump.canceled += _ => _playerController.jumping = false;
        
            // Crouching
            _controls.Player.Crouch.started += _ => _playerController.StartCrouch();
            _controls.Player.Crouch.performed += _ => _playerController.isCrouching = true;
            _controls.Player.Crouch.canceled += _ => _playerController.StopCrouch();
        
            // Emote
            _controls.Player.Emote.performed += _ => _playerController.Emote();
            
            // WeaponSwap
            _controls.Player.ControllerSwap.performed += _ => XboxSwap();
            _controls.Player.Primary.performed += _ => _playerController.EquipItem(0);
            _controls.Player.Secondary.performed += _ => _playerController.EquipItem(1);
            
            // WeaponShoot
            _controls.Player.Shoot.performed += _ => Shoot();

            // Reload
            _controls.Player.Reload.performed += _ => Reload();
        }
    }

    private void Start()
    {
        // If PhotonView is not owned by player delete the cameras and rigidbody (Rigidbody isnt needed as postions, rotation and animations are synced across the server || Camera isnt needed as it can cause the wrong camera to be displayed)
        if (PV.IsMine)
        {
            _playerController.EquipItem(0);
        }
        else
        {
            FPSCam.SetActive(false);
            EmoteCam.SetActive(false);
            WeaponUI.SetActive(false);
            Destroy(cam.gameObject);
            Destroy(_rigidbody);
            Destroy(healthBar);
        }
    }

    private void Update()
    {
        MyInput();

        if (PV.IsMine)
        {
            ScrollSwap();
        }
    }

    public void MyInput()
    {
        // If PhotonView is owned by player allow them to use input
        if (PV.IsMine)
        {
            _playerController.x = _controls.Player.Movement.ReadValue<Vector2>().x;
            _playerController.y = _controls.Player.Movement.ReadValue<Vector2>().y;
            
            _playerController.mouseX = _controls.Player.Mouse.ReadValue<Vector2>().x;
            _playerController.mouseY = _controls.Player.Mouse.ReadValue<Vector2>().y;
        }
    }

    void Shoot()
    {
        _playerController.items[_playerController.itemIndex].Use();
    }

    void Reload()
    {
        _playerController.items[_playerController.itemIndex].GetComponent<Gun>()?.Reload();
    }

    void XboxSwap()
    {
        if (_playerController.itemIndex == 0)
        {
            _playerController.EquipItem(1);
        } else if (_playerController.itemIndex == 1)
        {
            _playerController.EquipItem(0);
        }
    }

    void ScrollSwap()
    {
        float z = _controls.Player.ScrollSwap.ReadValue<float>();

        if (z > 0)
        {
            // Scroll UP
            if (_playerController.itemIndex == 0)
            {
                _playerController.EquipItem(1);
            } else if (_playerController.itemIndex == 1)
            {
                _playerController.EquipItem(0);
            }
        } else if (z < 0)
        {
            // Scroll DOWN
            if (_playerController.itemIndex == 0)
            {
                _playerController.EquipItem(1);
            } else if (_playerController.itemIndex == 1)
            {
                _playerController.EquipItem(0);
            }
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
