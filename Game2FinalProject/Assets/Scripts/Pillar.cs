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
            if (IsClient)
            {
                doorOpened = bool.Parse(value);
            }
            
            if (doorOpened == true)
            {
                if(IsServer)
                {
                    gate.gateDoorOpened = true;
                    gate.SendUpdate("OPEN", gate.gateDoorOpened.ToString());
                }

                this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
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

        string gateName = GateElement.ToString() + "Gate";
        Gate foundGate = FindFirstObjectWithNamePrefix(gateName);
        if (foundGate != null)
        {
            gate = foundGate;
        }
        else
        {
            Debug.LogWarning("Could not find gate with name: " + gateName);
        }
    }

    Gate FindFirstObjectWithNamePrefix(string prefix)
    {
        Gate[] allObjects = Gate.FindObjectsOfType<Gate>();
        foreach (Gate obj in allObjects)
        {
            if (obj.name.StartsWith(prefix))
            {
                return obj;
            }
        }
        return null;
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (gate == null)
            {
                string gateName = GateElement.ToString() + "Gate";
                Gate foundGate = FindFirstObjectWithNamePrefix(gateName);
                if (foundGate != null)
                {
                    gate = foundGate;
                }
                else
                {
                    Debug.LogWarning("Could not find gate with name: " + gateName);
                }
            }
            if (IsDirty)
            {
                SendUpdate("ACTIVATE", doorOpened.ToString());

                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
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
