using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        // Subscribes sceneLoaded event to the OnSceneLoaded function
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        // Unsubscribes sceneLoaded event from the OnSceneLoaded function
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 2) // We're in the game scene
        {
            // Instantiates PlayerManager
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }
}