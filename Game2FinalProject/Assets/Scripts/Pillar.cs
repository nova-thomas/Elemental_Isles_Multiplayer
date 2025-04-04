using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Pillar : NetworkComponent
{
    public bool doorOpened;

    public Gate gate;
    public PlayerCharacter.Elements GateElement;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "ACTIVATE" && value == "true")
        {
            doorOpened = true;

            // Update visuals
            this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            this.gameObject.transform.GetChild(1).gameObject.SetActive(true);

            if (gate != null)
            {
                gate.OpenDoor();  // Calls SendCommand("OPEN", "true")
            }

            SendUpdate("ACTIVATE", doorOpened.ToString());
        }
    }

    public override void NetworkedStart()
    {
        doorOpened = false;
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if(IsDirty)
            {
                SendUpdate("ACTIVATE", doorOpened.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }


    public void ActivatePedistal()
    {
        if (!doorOpened)
        {
            SendCommand("ACTIVATE", "true");
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !doorOpened)
        {
            PlayerCharacter player = other.GetComponent<PlayerCharacter>();
            if (player.playerElement == this.GateElement)
            {
                this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerCharacter player = other.GetComponent<PlayerCharacter>();
            if (player.playerElement == this.GateElement)
            {
                this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
