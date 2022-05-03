using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    private Controls _controls;
    [SerializeField] private GameObject ScoreboardObject;

    [SerializeField] private Transform container;
    [SerializeField] private GameObject scoreboardItemPrefab;

    private Dictionary<Player, ScoreboardItem> scoreboardItems = new Dictionary<Player, ScoreboardItem>();

    private void Awake()
    {
        _controls = new Controls();
        _controls.Player.Scoreboard.started += _ => EnableScoreboard();
        _controls.Player.Scoreboard.canceled += _ => DisableScoreboard();
    }
    
    private void Start()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddScoreboardItem(player);
        }
    }

    private void EnableScoreboard()
    {
        ScoreboardObject.SetActive(true);
    }

    private void DisableScoreboard()
    {
        ScoreboardObject.SetActive(false);
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddScoreboardItem(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        RemoveScoreboardItem(player);
    }

    void AddScoreboardItem(Player player)
    {
        ScoreboardItem item = Instantiate(scoreboardItemPrefab, container).GetComponent<ScoreboardItem>();
        item.Initialize(player);
        scoreboardItems[player] = item;
    }

    void RemoveScoreboardItem(Player player)
    {
        Destroy(scoreboardItems[player].gameObject);
        scoreboardItems.Remove(player);
    }
    
    public const byte KillDeathEvent = 10;

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == KillDeathEvent)
        {
            object[] data = (object[])photonEvent.CustomData;

            Player killer = (Player) data[0];
            Debug.Log("Killer: " + killer);
            Player dead = (Player) data[1];
            Debug.Log("Dead: " + dead);
            scoreboardItems[killer].gameObject.GetComponent<ScoreboardItem>().AddKill();
            scoreboardItems[dead].gameObject.GetComponent<ScoreboardItem>().AddDeath();
        }
    }
    
    private void OnEnable()
    {
        _controls.Player.Scoreboard.Enable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        _controls.Player.Scoreboard.Disable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
}
