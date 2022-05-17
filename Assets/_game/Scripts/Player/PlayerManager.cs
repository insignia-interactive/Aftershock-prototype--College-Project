using System.IO;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviour
{
    private PhotonView PV;
    private GameObject controller;
    private InputManager _inputManager;
    
    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // If PhotonView owned by played call the CreateController function
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        Debug.Log("Instantiated Player Controller");
        // Gets random spawnpoint from SpawnManager script
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        // Instantiates the Player over the server with the spawnpoints position and rotation
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerHolder"), spawnpoint.position, Quaternion.Euler(0,0,0), 0, new object[] { PV.ViewID });

        _inputManager = controller.GetComponent<InputManager>();
        _inputManager.SetRotation(spawnpoint);
    }

    public const byte KillDeathEvent = 10;
    
    public void Die(Player killer, Player dead, string weaponIcon)
    {
        PhotonNetwork.RaiseEvent(KillDeathEvent, new object[] { killer, dead, weaponIcon }, raiseEventOptions, SendOptions.SendReliable);
        
        PhotonNetwork.Destroy(controller);
        CreateController();
    }

    public void Respawn()
    {
        // Gets random spawnpoint from SpawnManager script
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        // Sets player position and rotation to the spawnpoints position and rotation
        controller.transform.position = spawnpoint.position;
        _inputManager.SetRotation(spawnpoint);
    }
}