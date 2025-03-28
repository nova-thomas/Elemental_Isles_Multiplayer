using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class GameMaster : NetworkComponent
{
    public bool GameStarted = false;
    private List<NPM> players = new List<NPM>();

    public float TimerDuration = 5f;
    private float currentTimer;

    public GameObject TimerPanel;
    public Text TimerText;

    public GameObject ScorePanel;  

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GAMESTART")
        {
            GameStarted = true;

            foreach (NPM npm in FindObjectsOfType<NPM>())
            {
                if (npm.transform.childCount > 0)
                {
                    npm.transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            if (IsServer)
            {
                currentTimer = TimerDuration;
                StartCoroutine(StartTimer());
            }

            SendUpdate("SHOWTIMER", "1");
        }

        if (flag == "SHOWTIMER")
        {
            if (TimerPanel != null)
            {
                TimerPanel.SetActive(value == "1");
            }
        }

        if (flag == "TIMER")
        {
            if (TimerText != null)
            {
                TimerText.text = value;
            }
        }

        if (flag == "SHOWSCORE")  
        {
            if (ScorePanel != null)
            {
                ScorePanel.SetActive(value == "1");
            }
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            StartCoroutine(WaitForPlayers());
            if (TimerPanel != null)
            {
                TimerPanel.SetActive(false);
            }
            if (ScorePanel != null)  
            {
                ScorePanel.SetActive(false);
            }
        }
    }

    private IEnumerator WaitForPlayers()
    {
        while (true)
        {
            players.Clear();
            players.AddRange(FindObjectsOfType<NPM>());

            if (players.Count > 1 && players.TrueForAll(p => p.IsReady))
            {
                SendUpdate("GAMESTART", "1");
                SpawnPlayers();
                break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnPlayers()
    {
        if (IsServer)
        {
            foreach (NPM npm in players)
            {
                Transform spawnPoint = GameObject.Find("P" + (npm.Owner + 1) + "Start").transform;
                GameObject newPlayer = MyCore.NetCreateObject(npm.ElementSelected, npm.Owner, spawnPoint.position, Quaternion.identity);
                PlayerCharacter pc = newPlayer.GetComponent<PlayerCharacter>();
                if (pc != null)
                {
                    pc.PName = npm.PName;
                    pc.ApplyCustomization();
                }
            }

            currentTimer = TimerDuration;
            StartCoroutine(StartTimer());
        }
    }

    private IEnumerator StartTimer()
    {
        SendUpdate("SHOWTIMER", "1");
        SendFormattedTime();

        while (currentTimer > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTimer--;

            SendFormattedTime();
        }

        SendUpdate("SHOWTIMER", "0");
        SendUpdate("SHOWSCORE", "1"); 
    }

    private void SendFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTimer / 60);
        int seconds = Mathf.FloorToInt(currentTimer % 60);
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        SendUpdate("TIMER", formattedTime);
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            yield return new WaitForSeconds(.1f);
        }
    }
}
