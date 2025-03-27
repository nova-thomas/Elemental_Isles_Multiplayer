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
    public Text PlayerNumberText;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "READY")
        {
            IsReady = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("READY", value);
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
                PlayerNumberText.text = PName;
            }
        }

        if (flag == "GAMESTART")
        {
            if (this.transform.childCount > 0)
            {
                this.transform.GetChild(0).gameObject.SetActive(false);
            }

            SpawnPlayers();
        }
    }

    public void UI_Ready(bool r)
    {
        if (IsLocalPlayer)
        {
            IsReady = r;
            SendCommand("READY", r.ToString());
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
            PName = "Player " + (Owner + 1); 
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
                SendUpdate("READY", IsReady.ToString());
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

    void SpawnPlayers()
    {
        if (IsServer)
        {
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
