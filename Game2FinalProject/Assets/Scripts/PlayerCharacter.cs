using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
public class PlayerCharacter : NetworkComponent
{
    public Text PlayerName;
    public Material[] MColor;
    public int ColorSelected = -1;
    public string PName = "<Default>";
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "SETUP")
        {
            string[] data = value.Split(',');
            ColorSelected = int.Parse(data[0]);
            PName = data[1];
            ApplyCustomization();
        }

        if (flag == "GAMESTART")
        {
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            SendUpdate("SETUP", ColorSelected + "," + PName);
        }
    }

    public override IEnumerator SlowUpdate()
    {
      while(IsConnected)
        {

            if(IsServer)
            {
                if(IsDirty)
                {
                    SendUpdate("SETUP", ColorSelected + "," + PName);
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    public void ApplyCustomization()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && ColorSelected >= 0 && ColorSelected < MColor.Length)
        {
            sr.color = MColor[ColorSelected].color; 
        }
        if (PlayerName != null)
        {
            PlayerName.text = PName;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
