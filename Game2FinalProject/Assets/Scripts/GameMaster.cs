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

    public Sprite[] ElementImages; // 0 = Water, 1 = Fire, 2 = Earth, 3 = Air
    public Image P1ElementImage, P2ElementImage, P3ElementImage, P4ElementImage;

    // Game Variables
    private List<int> PlayerScores = new List<int>();
    public int AntennaCount;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GAMESTART")
        {
            GameStarted = true;
            AntennaCount = 0;


            for (int i = 0; i < PlayerScores.Count; i++)
            {
                PlayerScores[i] = 0;
            }
            foreach (NPM npm in FindObjectsOfType<NPM>())
            {
                if (npm.transform.childCount > 0)
                {
                    npm.transform.GetChild(0).gameObject.SetActive(false);
                    npm.transform.GetChild(1).gameObject.SetActive(true);
                }
            }

            UpdateElementImages();
            InitializeScoreboard();

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

        if (flag == "ELEMENTS")
        {
            string[] elements = value.Split('|');

            if (elements.Length > 0 && P1ElementImage != null)
                P1ElementImage.sprite = ElementImages[int.Parse(elements[0])];

            if (elements.Length > 1 && P2ElementImage != null)
                P2ElementImage.sprite = ElementImages[int.Parse(elements[1])];

            if (elements.Length > 2 && P3ElementImage != null)
                P3ElementImage.sprite = ElementImages[int.Parse(elements[2])];

            if (elements.Length > 3 && P4ElementImage != null)
                P4ElementImage.sprite = ElementImages[int.Parse(elements[3])];
        }

        if (flag == "WIN")
        {
            // Handle win condition
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

    private void InitializeScoreboard()
    {
        List<NPM> sortedPlayers = new List<NPM>(FindObjectsOfType<NPM>());
        sortedPlayers.Sort((a, b) => a.Owner.CompareTo(b.Owner));

        string[] playerScores = new string[4];

        for (int i = 0; i < sortedPlayers.Count && i < 4; i++)
        {
            playerScores[i] = $"{sortedPlayers[i].PName} - 0 Points";
        }

        if (Player1Text != null) Player1Text.text = playerScores.Length > 0 ? playerScores[0] : "";
        if (Player2Text != null) Player2Text.text = playerScores.Length > 1 ? playerScores[1] : "";
        if (Player3Text != null) Player3Text.text = playerScores.Length > 2 ? playerScores[2] : "";
        if (Player4Text != null) Player4Text.text = playerScores.Length > 3 ? playerScores[3] : "";
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
        AntennaCount = 0;
        PlayerScores.Clear();

        foreach (NPM npm in players)
        {
            PlayerScores.Add(npm.score);
            AntennaCount += npm.antennaCount;
        }
    }

    public void Win()
    {
        SendUpdate("WIN", "");
    }

    private void UpdateElementImages()
    {
        List<NPM> sortedPlayers = new List<NPM>(FindObjectsOfType<NPM>());
        sortedPlayers.Sort((a, b) => a.Owner.CompareTo(b.Owner));


        if (sortedPlayers.Count > 0 && P1ElementImage != null)
            P1ElementImage.sprite = ElementImages[sortedPlayers[0].ElementSelected];
        if (sortedPlayers.Count > 1 && P2ElementImage != null)
            P2ElementImage.sprite = ElementImages[sortedPlayers[1].ElementSelected];
        if (sortedPlayers.Count > 2 && P3ElementImage != null)
            P3ElementImage.sprite = ElementImages[sortedPlayers[2].ElementSelected];
        if (sortedPlayers.Count > 3 && P4ElementImage != null)
            P4ElementImage.sprite = ElementImages[sortedPlayers[3].ElementSelected];

        List<string> elementData = new List<string>();
        foreach (var p in sortedPlayers)
        {
            elementData.Add(p.ElementSelected.ToString());
        }

        string elementMessage = string.Join("|", elementData);
        SendUpdate("ELEMENTS", elementMessage);
    }

    public float GetCurrentTimer()
    {
        return currentTimer;
    }

}
