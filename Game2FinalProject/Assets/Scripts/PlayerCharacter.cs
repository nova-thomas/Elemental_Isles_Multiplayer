using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class PlayerCharacter : NetworkComponent
{
    public Text PlayerName;
    public string PName = "<Default>";

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "SETUP")
        {
            PName = value;
            ApplyCustomization();
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            SendUpdate("SETUP", PName);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer && IsDirty)
            {
                SendUpdate("SETUP", PName);
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    public void ApplyCustomization()
    {
        if (PlayerName != null)
        {
            PlayerName.text = PName;
        }
    }

    void Start() { }

    void Update() { }
}
