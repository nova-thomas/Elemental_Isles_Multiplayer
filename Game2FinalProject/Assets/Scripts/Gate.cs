using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : NetworkComponent
{
    public bool gateDoorOpened;
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "OPEN")
        {
            if (IsClient)
            {
                gateDoorOpened = bool.Parse(value);
            }
            if (gateDoorOpened)
            {
                this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public override void NetworkedStart()
    {
        gateDoorOpened = false;
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsDirty)
            {
                SendUpdate("OPEN", gateDoorOpened.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
