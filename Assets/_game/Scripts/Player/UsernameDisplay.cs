using Photon.Pun;
using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] private PhotonView PV;
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        // Sets the text in the nametag to the name of the player with the nametag
        text.text = PV.Owner.NickName;

        // If the nametag is owned by the player empty the nametag text (Makes the nametag invisible for self)
        if (PV.IsMine)
        {
            text.text = "";
        }
    }
}
