using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : NetworkComponent
{
    public bool doorOpened;
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "OPEN")
        {
            if (value == "true")
            {
                doorOpened = true;
                SendUpdate("OPEN", doorOpened.ToString());
            }
            if (doorOpened)
            {
                this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
            
        }
    }

    public override void NetworkedStart()
    {
        doorOpened = false;
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsDirty)
            {
                SendUpdate("OPEN", "");
                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OpenDoor()
    {
        SendCommand("OPEN", "true");
    }
}
