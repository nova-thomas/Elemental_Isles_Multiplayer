using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;
using TMPro;  // Added for TextMeshPro support

public class NPM : NetworkComponent
{
    public string PName;
    public bool IsReady;
    public int ElementSelected;
    public Text PlayerNumberText;
    public Text ElementText;
    public GameObject HUDPanel;

    public int score;
    public int antennaCount;
    public int crystals;
    public int health;
    public int ammo;
    public int maxAmmo;

    // UI Elements
    public Slider HealthBar;   
    public TMP_Text AmmoText; 
    public TMP_Text CrystalText;  

    private string[] elementNames =
    {
        "The water ability can push obstacles and enemies!",
        "The fire ability can burn and melt obstacles!",
        "The earth ability can build bridges!",
        "The air ability can launch you upwards!"
    };

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

            if (ElementText != null && ElementSelected >= 0 && ElementSelected < elementNames.Length)
            {
                ElementText.text = elementNames[ElementSelected];
            }

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
                this.transform.GetChild(1).gameObject.SetActive(true);
            }

            if (HUDPanel != null)
            {
                HUDPanel.SetActive(true);
            }
        }

        if (flag == "HEALTH")
        {
            health = int.Parse(value);
            if (HealthBar != null)
            {
                HealthBar.value = health;
            }
        }

        if (flag == "AMMO")
        {
            string[] ammoValues = value.Split('/');
            if (ammoValues.Length == 2)
            {
                ammo = int.Parse(ammoValues[0]);
                maxAmmo = int.Parse(ammoValues[1]);

                if (AmmoText != null)
                {
                    AmmoText.text = $"{ammo} / {maxAmmo}";
                }
            }
        }

        if (flag == "CRYSTALS")
        {
            crystals = int.Parse(value);
            if (CrystalText != null)
            {
                CrystalText.text = crystals.ToString();
            }
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

        if (HUDPanel != null)
        {
            HUDPanel.SetActive(false);
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
            if (IsServer)
            {
                PlayerCharacter pc = FindPlayerCharacter();
                if (pc != null)
                {
                    score = pc.score;
                    antennaCount = pc.antennaCollected;
                    crystals = pc.crystals;
                    health = pc.health;
                    ammo = pc.ammo;
                    maxAmmo = pc.maxAmmo;

                    SendUpdate("HEALTH", health.ToString());
                    SendUpdate("AMMO", $"{ammo}/{maxAmmo}");
                    SendUpdate("CRYSTALS", crystals.ToString());
                }

                if (IsDirty)
                {
                    SendUpdate("READY", IsReady.ToString());
                    SendUpdate("ELEMENT", ElementSelected.ToString());
                    SendUpdate("PNAME", PName);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    private PlayerCharacter FindPlayerCharacter()
    {
        foreach (PlayerCharacter pc in FindObjectsOfType<PlayerCharacter>())
        {
            if (pc.Owner == this.Owner)  // Ensure the player character belongs to this NPM
            {
                return pc;
            }
        }
        return null; // Return null if no matching character is found
    }
}
