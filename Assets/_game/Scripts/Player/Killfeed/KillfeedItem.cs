using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class KillfeedItem : MonoBehaviour
{
    public TMP_Text killer;
    public TMP_Text killed;
    public Image icon;

    public void Initialize(Player killerData, Player deadData, Sprite weaponIcon)
    {
        killer.text = deadData.NickName;
        killed.text = killerData.NickName;
        icon.overrideSprite = weaponIcon;
    }
}
