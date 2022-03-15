using System.IO;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviour
{
    private PhotonView PV;
    private GameObject controller;
    private InputManager _inputManager;

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

    public void Respawn()
    {
        // Gets random spawnpoint from SpawnManager script
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        // Sets player position and rotation to the spawnpoints position and rotation
        controller.transform.position = spawnpoint.position;
        _inputManager.SetRotation(spawnpoint);
    }
}
