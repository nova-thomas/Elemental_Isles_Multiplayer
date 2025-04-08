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
            if (value == "true" && !doorOpened)
            {
                Debug.Log("Door Opening");
                if (IsServer)
                {
                    doorOpened = true;
                    SendUpdate("OPEN", doorOpened.ToString());
                }

                if (IsClient)
                {
                    doorOpened = bool.Parse(value);
                }
                this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                Debug.Log("Door Opened");
            }
            else
            {
                doorOpened = bool.Parse(value);
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
                SendUpdate("OPEN", doorOpened.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OpenDoor()
    {
        Debug.Log("Open Door Command");
        if (IsClient && !doorOpened)
        {
            SendCommand("OPEN", "true");
        }
    }
}
