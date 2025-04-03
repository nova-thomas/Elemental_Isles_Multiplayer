using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
    public float bulletSpeed = 3f;
    public float flameSpeed = 1f;
    public float waterBlastSpeed = 2f;
    public float mudShotSpeed = 2f;

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
        }

        if (flag == "FIREABILITY" && IsServer && (crystals >= 1))
        {
            GameObject currentAbility = null;
            float abilitySpeed = 0f;

            switch (playerElement)
            {
                case Elements.Fire:
                    currentAbility = MyCore.NetCreateObject(24, this.Owner, bulletLoc.transform.position, bulletLoc.transform.rotation);
                    abilitySpeed = flameSpeed;
                    break;
                case Elements.Earth:
                    currentAbility = MyCore.NetCreateObject(25, this.Owner, bulletLoc.transform.position, bulletLoc.transform.rotation);
                    abilitySpeed = mudShotSpeed;
                    break;
                case Elements.Water:
                    currentAbility = MyCore.NetCreateObject(26, this.Owner, bulletLoc.transform.position, bulletLoc.transform.rotation);
                    abilitySpeed = waterBlastSpeed;
                    break;
            }

            if (currentAbility != null)
            {
                Rigidbody abilityRig = currentAbility.GetComponent<Rigidbody>();
                if (abilityRig != null)
                {
                    abilityRig.velocity = bulletLoc.transform.forward * abilitySpeed;
                    Debug.Log("Ability fired with speed: " + abilitySpeed);
                }
            }
            crystals--;
        }

        if (flag == "COINADD")
        {
            

            if (IsServer)
            {
                score += 100;
                SendUpdate("COINADD", "");
            }
            if (IsLocalPlayer)
            {
                // Update UI
            }
        }

        if (flag == "CRYSTALADD")
        {
            
            if (IsServer)
            {
                crystals++;
                score += 500;


                SendUpdate("CRYSTALADD", "");
            }
            if (IsLocalPlayer)
            {
                // Update UI

            }
        }

        if (flag == "ANTENNAADD")
        {
            
            if (IsServer)
            {
                antennaCollected++;

                score += 5000;

                SendUpdate("ANTENNAADD", "");
            }
            if (IsLocalPlayer)
            {
                // Update UI

            }
        }

        if (flag == "TRIBUTE")
        {
            if (IsServer)
            {
                crystals--;

                SendUpdate("TRIBUTE", "");
            }
            if (IsClient)
            {
                if (IsLocalPlayer)
                {
                    // Update UI

                }
                nearestPillar.ActivatePedistal();

            }
            
        }
    }

    public override void NetworkedStart()
    {
        gameObject.tag = "Player";
        antennaCollected = 0;
        if (IsServer)
        {
            SendUpdate("SETUP", PName);
        }

        if (IsLocalPlayer)
        {
            //gameObject.GetComponentInChildren<Renderer>().enabled = false;
            playerModel.SetActive(false);
            Debug.Log("invisible");
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer && IsDirty)
            {
                SendUpdate("SETUP", PName);
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
        canShoot = false;
        SendCommand("FIRE", " ");
        //StartCoroutine(Reload());
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

    public void AbilityFire(InputAction.CallbackContext fr)
    {
        canShootAbility = false;
        SendCommand("FIREABILITY", " ");
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

    public void TributeCrystal()
    {
        if (canTribute)
        {
            SendCommand("TRIBUTE", "");
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
            SendCommand("COINADD", "");
        }

        if (other.gameObject.tag == "Crystal")
        {
            CollectibleItem crystal = other.gameObject.GetComponent<CollectibleItem>();
            if (crystal.CrystalElement == playerElement)
            {
                SendCommand("CRYSTALADD", "");
            }

        }

        if (other.gameObject.tag == "AntennaPiece")
        {
            SendCommand("ANTENNAADD", "");
        }

        if (other.gameObject.tag == "Pillar")
        {
            Pillar pillar = other.gameObject.GetComponent<Pillar>();
            nearestPillar = pillar;
            if (crystals > 0 && pillar.GateElement == playerElement)
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

    public void Interact()
    {
        if (canTribute && nearestPillar != null)
        {
            TributeCrystal();
        }
    }
}
