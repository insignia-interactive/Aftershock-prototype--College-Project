using UnityEngine;

public class FriendMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject otherContainer;

    public void OnClick()
    {
        otherContainer.SetActive(false);

        mainContainer.SetActive(!mainContainer.activeSelf);
    }
}