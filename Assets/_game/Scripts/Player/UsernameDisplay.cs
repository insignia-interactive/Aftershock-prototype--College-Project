using Photon.Pun;
using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] private PhotonView playerPV;
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        text.text = playerPV.Owner.NickName;

        if (playerPV.IsMine)
        {
            text.text = "";
        }
    }
}
