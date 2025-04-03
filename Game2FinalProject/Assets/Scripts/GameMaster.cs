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
    public Text Player1Text, Player2Text, Player3Text, Player4Text; 

    // Game Variables
    private List<int> PlayerScores = new List<int>();
    public int AntennaCount;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GAMESTART")
        {
            GameStarted = true;
            AntennaCount = 0;

            foreach (int player in PlayerScores)
            {
                PlayerScores[player] = 0;
            }

            foreach (NPM npm in FindObjectsOfType<NPM>())
            {
                if (npm.transform.childCount > 0)
                {
                    npm.transform.GetChild(0).gameObject.SetActive(false);
                    npm.transform.GetChild(1).gameObject.SetActive(true);

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
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
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
                ScorePanel.SetActive(true);
            }

            string[] scores = value.Split('|'); 
            if (Player1Text != null) Player1Text.text = scores.Length > 0 ? scores[0] : "";
            if (Player2Text != null) Player2Text.text = scores.Length > 1 ? scores[1] : "";
            if (Player3Text != null) Player3Text.text = scores.Length > 2 ? scores[2] : "";
            if (Player4Text != null) Player4Text.text = scores.Length > 3 ? scores[3] : "";
        }

        if(flag == "WIN")
        {

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
                    
                    switch (npm.ElementSelected)
                    {
                        case 0:
                            pc.playerElement = PlayerCharacter.Elements.Water;
                            break;

                        case 1:
                            pc.playerElement = PlayerCharacter.Elements.Fire;
                            break;

                        case 2:
                            pc.playerElement = PlayerCharacter.Elements.Earth;
                            break;

                        case 3:
                            pc.playerElement = PlayerCharacter.Elements.Air;
                            break;

                    }

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
        Cursor.lockState = CursorLockMode.None;
        UpdateScoreScreen();
    }

    private void SendFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTimer / 60);
        int seconds = Mathf.FloorToInt(currentTimer % 60);
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        SendUpdate("TIMER", formattedTime);
    }

    private void UpdateScoreScreen()
    {
        List<NPM> sortedPlayers = new List<NPM>(FindObjectsOfType<NPM>());
        sortedPlayers.Sort((a, b) => a.Owner.CompareTo(b.Owner));

        string[] playerScores = new string[4]; 

        for (int i = 0; i < sortedPlayers.Count && i < 4; i++)
        {
            int dummyScore = Random.Range(10, 100);
            playerScores[i] = $"{sortedPlayers[i].PName} - {dummyScore} Points";
        }

        string scoreData = string.Join("|", playerScores); 
        SendUpdate("SHOWSCORE", scoreData);
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            // Get variables from players
            CollectGameVariables();

            if (AntennaCount >= 4)
            {
                Win();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


    public void CollectGameVariables()
    {
        // Reset the total antenna count
        AntennaCount = 0;

        // Clear the existing player scores
        PlayerScores.Clear();

        // Loop through the list of players already gathered
        foreach (NPM npm in players)
        {
            // Store the score and sum up antenna counts
            PlayerScores.Add(npm.score);
            AntennaCount += npm.antennaCount;
        }
    }

    public void Win()
    {
        SendUpdate("WIN", "");
    }

}
