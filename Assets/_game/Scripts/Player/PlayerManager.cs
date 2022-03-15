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
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        Debug.Log("Instantiated Player Controller");
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerHolder"), spawnpoint.position, Quaternion.Euler(0,0,0), 0, new object[] { PV.ViewID });

        _inputManager = controller.GetComponent<InputManager>();
        _inputManager.SetRotation(spawnpoint);
    }

    public void Respawn()
    {
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller.transform.position = spawnpoint.position;
        _inputManager.SetRotation(spawnpoint);
    }
}
