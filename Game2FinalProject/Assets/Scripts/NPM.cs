using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class NPM : NetworkComponent
{
    public string PName;
    public bool IsReady;
    public int ElementSelected;

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

        if (flag == "GAMESTART")
        {
            if (this.transform.childCount > 0)
            {
                this.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void UI_Ready(bool r)
    {
        if (IsLocalPlayer)
        {
            SendCommand("READY", r.ToString());
        }
    }

    public override void NetworkedStart()
    {
        if (!IsLocalPlayer)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
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
                SendUpdate("ELEMENT", ElementSelected.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    void Start() { }

    void Update() { }
}
