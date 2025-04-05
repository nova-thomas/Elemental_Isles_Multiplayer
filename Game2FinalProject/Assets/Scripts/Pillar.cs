using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar : NetworkComponent
{
    public bool doorOpened;

    public Gate gate;
    public PlayerCharacter.Elements GateElement;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "ACTIVATE")
        {
            if (value == "true")
            {
                Debug.Log("Activate Handle Message");
                if (IsServer)
                {
                    doorOpened = true;
                    SendUpdate("ACTIVATE", doorOpened.ToString());
                }
                if (IsClient)
                {
                    doorOpened = true;
                }
                

                // Update visuals
                this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                this.gameObject.transform.GetChild(1).gameObject.SetActive(true);

                if (gate != null)
                {
                    gate.OpenDoor();  // Calls SendCommand("OPEN", "true")
                }

                
            }
            else
            {
                doorOpened = false;
            }
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
        Debug.Log("Activating Pedistal");
        if (!doorOpened)
        {
            Debug.Log("Door isn't open");
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
