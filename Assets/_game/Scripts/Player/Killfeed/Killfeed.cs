using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class Killfeed : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private GameObject killfeedItemPrefab;

    [SerializeField] private Sprite[] weaponIcons;

    public const byte KillDeathEvent = 10;

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == KillDeathEvent)
        {
            object[] data = (object[])photonEvent.CustomData;

            Player killer = (Player)data[0];
            Debug.Log("Killer: " + killer);
            Player dead = (Player)data[1];
            Debug.Log("Dead: " + dead);

            Sprite weaponIcon = weaponIcons[0];

            foreach (Sprite icon in weaponIcons)
            {
                if (icon.name == (string)data[2])
                {
                    weaponIcon = icon;
                }
            }

            KillfeedItem item = Instantiate(killfeedItemPrefab, container).GetComponent<KillfeedItem>();
            item.Initialize(killer, dead, weaponIcon);
            Destroy(item, 5f);
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
}
