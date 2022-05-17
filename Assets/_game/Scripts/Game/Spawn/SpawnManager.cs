using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    private Spawnpoint[] spawnpoints;
    
    private void Awake()
    {
        Instance = this;
        // Gets a list of all Child objects with the Spawnpoint script
        spawnpoints = GetComponentsInChildren<Spawnpoint>();
    }

    public Transform GetSpawnpoint()
    {
        // Gets random Transform from the spawnpoints list
        return spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
    }
}