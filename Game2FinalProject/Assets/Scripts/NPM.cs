using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NPM : NetworkComponent
{
    public string PName;
    public bool IsReady;
    public int ElementSelected;
    public Text PlayerNumberText;  // Reference to the UI Text object

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "READY")
        {
            IsReady = bool.Parse(value);  // Correctly update IsReady when received
            if (IsServer)
            {
                SendUpdate("READY", value);  // Sync the value to all clients
            }
        }

        if (flag == "ELEMENT")
        {
            ElementSelected = int.Parse(value);
            if (IsServer)
            {
                SendUpdate("ELEMENT", value);
            }
        }

        if (flag == "PNAME")
        {
            PName = value;
            if (PlayerNumberText != null)
            {
                PlayerNumberText.text = PName;  // Update UI text
            }
        }

        if (flag == "GAMESTART")
        {
            // Hide the canvas UI when the game starts (restored from original script)
            if (this.transform.childCount > 0)
            {
                this.transform.GetChild(0).gameObject.SetActive(false);
            }

            // Spawn the players
            SpawnPlayers();
        }
    }

    public void UI_Ready(bool r)
    {
        if (IsLocalPlayer)
        {
            IsReady = r;  // Ensure local IsReady updates immediately
            SendCommand("READY", r.ToString());  // Send to server
        }
    }

    public override void NetworkedStart()
    {
        if (!IsLocalPlayer)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }

        if (IsServer)
        {
            SendUpdate("PNAME", PName);
        }
    }

    public void UI_ElementInput(int e)
    {
        if (IsLocalPlayer)
        {
            SendCommand("ELEMENT", e.ToString());
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer && IsDirty)
            {
                SendUpdate("READY", IsReady.ToString());  // Ensure READY is always updated
                SendUpdate("ELEMENT", ElementSelected.ToString());
                SendUpdate("PNAME", PName);
                IsDirty = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    void Start()
    {
    }


    // Method to spawn the players after the game starts
    void SpawnPlayers()
    {
        if (IsServer)
        {
            // Ensure that the server spawns the players based on their selected elements
            foreach (NPM npm in FindObjectsOfType<NPM>())
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
}
