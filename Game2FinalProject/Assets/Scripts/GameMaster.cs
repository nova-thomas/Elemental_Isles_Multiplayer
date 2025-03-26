using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class GameMaster : NetworkComponent
{
    public bool GameStarted = false;
    private List<NPM> players = new List<NPM>();

    public GameObject TimerPanel;  // Assign this in Unity Inspector (the panel containing the timer)
    public Text GameTimerText;  // Assign the Timer Text in Unity Inspector

    public int TimerDuration = 5;  // Set default countdown time
    private int currentTime;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GAMESTART")
        {
            GameStarted = true;

            // Show the timer panel
            if (TimerPanel != null)
            {
                TimerPanel.SetActive(true);
            }

            foreach (NPM npm in FindObjectsOfType<NPM>())
            {
                if (npm.transform.childCount > 0)
                {
                    npm.transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            if (IsServer)
            {
                StartCoroutine(StartCountdown());
            }
        }

        if (flag == "UPDATETIMER")
        {
            currentTime = int.Parse(value);
            if (GameTimerText != null)
            {
                GameTimerText.text = currentTime.ToString();
            }
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            StartCoroutine(WaitForPlayers());
        }

        // Hide the timer panel at the start
        if (TimerPanel != null)
        {
            TimerPanel.SetActive(false);
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
        }
    }

    private IEnumerator StartCountdown()
    {
        currentTime = TimerDuration;
        while (currentTime > 0)
        {
            SendUpdate("UPDATETIMER", currentTime.ToString());  // Sync timer with all clients
            yield return new WaitForSeconds(1f);
            currentTime--;
        }
        SendUpdate("UPDATETIMER", "0");  // Ensure the timer reaches 0
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            yield return new WaitForSeconds(.1f);
        }
    }
}
