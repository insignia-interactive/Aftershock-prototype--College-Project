using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class Killfeed : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private GameObject killfeedItemPrefab;

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
            Sprite icon = (Sprite)data[2];

            KillfeedItem item = Instantiate(killfeedItemPrefab, container).GetComponent<KillfeedItem>();
            item.Initialize(killer, dead, icon);
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
