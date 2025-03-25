using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class GameMaster : NetworkComponent
{
    public bool GameStarted = false;
    private List<NPM> players = new List<NPM>();

    public GameObject ScoreScreenPanel;  

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
                StartCoroutine(GameCycle());
            }
        }

        if (flag == "SHOWSCORES")
        {
            if (IsClient)
            {
                if (ScoreScreenPanel != null)
                {
                    ScoreScreenPanel.SetActive(true);
                }
            }
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            StartCoroutine(WaitForPlayers());
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

    private IEnumerator GameCycle()
    {
        yield return new WaitForSeconds(5f);  
        SendUpdate("SHOWSCORES", "1");
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            yield return new WaitForSeconds(.1f);
        }
    }
}
