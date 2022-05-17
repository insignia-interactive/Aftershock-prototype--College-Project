using UnityEngine;

public class FriendEnableDisable : MonoBehaviour
{
    public bool onEnable;
    [SerializeField] private GameObject FriendUI;
    
    private void OnEnable()
    {
        if (onEnable)
        {
            FriendUI.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (!onEnable)
        {
            FriendUI.SetActive(false);
        }
    }
}