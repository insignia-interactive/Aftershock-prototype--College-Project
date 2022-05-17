using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] private GameObject graphics;

    private void Awake()
    {
        graphics.SetActive(false);
    }
}