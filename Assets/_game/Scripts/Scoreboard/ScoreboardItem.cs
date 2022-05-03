using UnityEngine;
using TMPro;
using Photon.Realtime;

public class ScoreboardItem : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text killText;
    public TMP_Text deathText;

    private int kills = 0;
    private int deaths = 0;

    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;
    }

    public void AddKill()
    {
        kills++;
        killText.text = kills.ToString();
    }
    
    public void AddDeath()
    {
        deaths++;
        deathText.text = deaths.ToString();
    }
}
