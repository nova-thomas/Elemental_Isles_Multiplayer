using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class PlayerCharacter : NetworkComponent
{
    public enum Elements {Water, Fire, Earth, Air };

    /*              **Variables**              */
    //Name Variables
    public Text PlayerName;
    public string PName = "<Default>";

    //Prefab Components
    public Rigidbody myRig;
    public GameObject playerModel;
    public GameObject playerGun;

    //Projectile Variables
    public GameObject bulletLoc;
    public GameObject elementLoc;
    public float bulletSpeed = 20f;
    public float flameSpeed = 3f;
    public float waterBlastSpeed = 10f;
    public float mudShotSpeed = 7f;

    //Movement Variables
    public bool canJump;
    public float jumpForce = 5f;
    public float speed = 8f;
    public float lookSpeed = 12f;
    private float xRotation = 0f;
    private Vector2 moveIn;
    private Vector2 lookIn;
    public Transform playerCam;
    public bool canShoot;
    public bool canShootAbility;
    public float ROF = 3;

    // Game Variables
    public Elements playerElement;
    public int score;
    public int crystals;
    public Pillar nearestPillar;
    public bool canTribute;
    public int antennaCollected;
    public int health = 20;
    public int ammo = 12;
    public int maxAmmo = 12;

    public GameObject ScoreboardPanel; //tab 
    public bool isScoreboardLocked = false; //isnt working 

    /*              **Functions**              */
    //Network Functions
    public Vector2 Vector2FromString(string s)
    {
        string[] args = s.Trim().Trim('(').Trim(')').Split(',');
        return new Vector2(float.Parse(args[0]), float.Parse(args[1]));
    }

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "SETUP")
        {
            PName = value;
            ApplyCustomization();
        }

        if(flag == "ELEMENT")
        {
            if (Enum.TryParse(value, out Elements pe))
            {
                playerElement = pe;
            }
        }

        if (flag == "MOVE")
        {
            if (IsServer)
            {
                moveIn = Vector2FromString(value);
            }
        }

        if (flag == "ROT" && IsServer)
        {
            lookIn.x = float.Parse(value);
            transform.Rotate(Vector3.up * lookIn.x * Time.deltaTime);
        }

        if (flag == "JUMP" && IsServer)
        {
            myRig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
        }

        if (flag == "FIRE" && IsServer)
        {
            GameObject currentBullet = MyCore.NetCreateObject(23, this.Owner, bulletLoc.transform.position, bulletLoc.transform.rotation);
            Rigidbody bulletRig = currentBullet.GetComponent<Rigidbody>();

            if (bulletRig != null)
            {
                bulletRig.velocity = bulletLoc.transform.forward * bulletSpeed;
                Debug.Log("bullet vel " + bulletRig.velocity);
            }

            ammo--;
            SendUpdate("AMMO", $"{ammo}/{maxAmmo}");
        }

        if (flag == "FIREABILITY" && IsServer && (crystals >= 1))
        {
            GameObject currentAbility = null;
            float abilitySpeed = 0f;

            switch (playerElement)
            {
                case Elements.Fire:
                    currentAbility = MyCore.NetCreateObject(24, this.Owner, elementLoc.transform.position, elementLoc.transform.rotation);
                    abilitySpeed = flameSpeed;
                    break;
                case Elements.Earth:
                    currentAbility = MyCore.NetCreateObject(25, this.Owner, elementLoc.transform.position, elementLoc.transform.rotation);
                    abilitySpeed = mudShotSpeed;
                    break;
                case Elements.Water:
                    currentAbility = MyCore.NetCreateObject(26, this.Owner, elementLoc.transform.position, elementLoc.transform.rotation);
                    abilitySpeed = waterBlastSpeed;
                    break;
            }

            if (currentAbility != null)
            {
                Rigidbody abilityRig = currentAbility.GetComponent<Rigidbody>();
                if (abilityRig != null)
                {
                    abilityRig.velocity = elementLoc.transform.forward * abilitySpeed;
                    Debug.Log("Ability fired with speed: " + abilitySpeed);
                }
            }
            crystals--;
            SendUpdate("CRYSTALCOUNT", crystals.ToString());
        }


        if (flag == "TRIBUTE")
        {
            nearestPillar.ActivatePedistal();
        }

        if (flag == "SCORE")
        {
            score = int.Parse(value);
            Debug.Log("Score updated to: " + score);
        }
        if (flag == "CRYSTALCOUNT")
        {
            crystals = int.Parse(value);
            Debug.Log("Crystals updated to: " + crystals);
        }

        if (flag == "ANTENNA")
        {
            antennaCollected = int.Parse(value);
            Debug.Log("antennaCollected updated to: " + antennaCollected);
        }

        if (flag == "SETNEAREST" && IsServer)
        {
            int id = int.Parse(value);
            GameObject pillarObj = MyCore.NetObjs[id].gameObject;
            nearestPillar = pillarObj.GetComponent<Pillar>();
        }
    }

    public override void NetworkedStart()
    {
        gameObject.tag = "Player";
        antennaCollected = 0;
        score = 0;
        crystals = 0;
        if (IsServer)
        {
            SendUpdate("SETUP", PName);
            SendUpdate("ELEMENT", playerElement.ToString());
        }

        if (IsLocalPlayer)
        {
            //gameObject.GetComponentInChildren<Renderer>().enabled = false;
            playerModel.SetActive(false);
            Debug.Log("invisible");
        }

        if (IsLocalPlayer)
        {
            GameObject gameMaster = GameObject.FindGameObjectWithTag("GameMaster");
            if (gameMaster != null)
            {
                Debug.Log("Found GameMaster");

                if (gameMaster.transform.childCount > 0)
                {
                    Transform firstChild = gameMaster.transform.GetChild(0);

                    if (firstChild.childCount > 0)
                    {
                        // Scoreboard Panel
                        Transform scoreboardTransform = firstChild.GetChild(0);
                        ScoreboardPanel = scoreboardTransform.gameObject;
                        ScoreboardPanel.SetActive(false);
                    }
                }
            }
        }

    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer && IsDirty)
            {
                SendUpdate("SETUP", PName);
                SendUpdate("SCORE", score.ToString());
                SendUpdate("ANTENNA", antennaCollected.ToString());
                SendUpdate("CRYSTALCOUNT", crystals.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    //Player Name Function
    public void ApplyCustomization()
    {
        if (PlayerName != null)
        {
            PlayerName.text = PName;
        }
    }

    //Default Functions
    void Start()
    {
        myRig = GetComponent<Rigidbody>();
        LockCursor();
        canJump = true;
        bulletSpeed = 3f;
    }


    void Update()
    {
        if (IsServer)
        {
            Vector3 moveDirection = transform.forward * moveIn.y + transform.right * moveIn.x;
            transform.position += moveDirection * speed * Time.deltaTime;
        }

        if (IsLocalPlayer)
        {
            Camera.main.transform.position = playerCam.transform.position;
            LookAround();
        }
    }

    //Cursor Functions
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //Input Callbacks
    public void Move(InputAction.CallbackContext mv)
    {
        if (mv.phase == InputActionPhase.Performed)
        {
            SendCommand("MOVE", mv.ReadValue<Vector2>().ToString());
        }
        else if (mv.phase == InputActionPhase.Canceled)
        {
            SendCommand("MOVE", Vector2.zero.ToString());
        }
    }

    public void Look(InputAction.CallbackContext lk)
    {
        if (lk.phase == InputActionPhase.Performed)
        {
            lookIn = lk.ReadValue<Vector2>() * lookSpeed;
        }
    }

    public void Jump(InputAction.CallbackContext jm)
    {
        if (jm.phase == InputActionPhase.Started && canJump)
        {
            canJump = false;
            SendCommand("JUMP", "");
        }
    }

    public void Fire(InputAction.CallbackContext fr)
    {
        if (ammo > 0) 
        {
            canShoot = false;
            SendCommand("FIRE", " ");
            ammo--; 
            SendUpdate("AMMO", $"{ammo}/{maxAmmo}");  // Update the ammo UI on the server
            
        }
    }

    public IEnumerator Reload()
    {
        yield return new WaitForSeconds(ROF);
        if (IsServer)
        {
            canShoot = true;
            SendUpdate("CSH", canShoot.ToString());
        }
        else
        {
            //Reload Animation
        }
    }
    public void LookAtScoreboard(InputAction.CallbackContext ctx)
    {
        if (!IsLocalPlayer || ScoreboardPanel == null) return;

        if (isScoreboardLocked) return;  // Prevent toggling if locked

        if (ctx.started)
        {
            ScoreboardPanel.SetActive(true);
        }
        else if (ctx.canceled)
        {
            ScoreboardPanel.SetActive(false);
        }
    }

    public void AbilityFire(InputAction.CallbackContext afr)
    {
        canShootAbility = false;
        SendCommand("FIREABILITY", " ");
    }

    public void Interact(InputAction.CallbackContext ia)
    {
        if (canTribute)
        {
            crystals--;
            SendCommand("CRYSTALCOUNT", crystals.ToString());
            SendCommand("TRIBUTE", "");
        }
    }

    //Camera Control
    private void LookAround()
    {
        // Only rotate camera when the cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.Rotate(Vector3.up * lookIn.x * Time.deltaTime);
            xRotation -= lookIn.y * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            SendCommand("ROT", lookIn.x.ToString());
            Camera.main.transform.rotation = Quaternion.Euler(xRotation, transform.localRotation.eulerAngles.y, 0f);
            lookIn = Vector2.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            canJump = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Coin")
        {
            score += 100;
            SendCommand("SCORE", score.ToString());
            CollectibleItem item = other.gameObject.GetComponent<CollectibleItem>();
            item.DestroyObj();
        }

        if (other.gameObject.tag == "Crystal")
        {
            CollectibleItem crystal = other.gameObject.GetComponent<CollectibleItem>();
            if (crystal.CrystalElement == playerElement)
            {
                crystals++;
                score += 500;

                SendCommand("CRYSTALCOUNT", crystals.ToString());
                SendCommand("SCORE", score.ToString());

                CollectibleItem item = other.gameObject.GetComponent<CollectibleItem>();
                item.DestroyObj();
            }

        }

        if (other.gameObject.tag == "AntennaPiece")
        {
            antennaCollected++;
            score += 5000;
            SendUpdate("SCORE", score.ToString());
            SendUpdate("ANTENNA", antennaCollected.ToString());
            CollectibleItem item = other.gameObject.GetComponent<CollectibleItem>();
            item.DestroyObj();
        }

        if (other.gameObject.tag == "Pillar")
        {
            Pillar pillar = other.gameObject.GetComponent<Pillar>();
            SendUpdate("SETNEAREST", pillar.MyId.ToString());
            if (crystals > 0 && (pillar.GateElement == playerElement) && (pillar.doorOpened == false))
            { 
                canTribute = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Pillar")
        {
            canTribute = false;
            nearestPillar = null;
        }
    }

    
}
