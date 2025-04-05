using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireBramble : NetworkComponent
{
    public bool burning;

    private bool _isBurning = false;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "BURNING")
        {
            Debug.Log("Received Flame Message");
            if (IsServer)
            {
                burning = bool.Parse(value);
                Debug.Log("Sending Flame Update");
                SendUpdate("BURNING", burning.ToString());

                if (burning && !_isBurning)
                {
                    _isBurning = true;
                    StartCoroutine(BurnAndShrink());
                }


            }            
        }
    }

    public override void NetworkedStart()
    {
        burning = false;
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsDirty)
            {
                SendUpdate("BURNING", burning.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Flame" && IsClient)
        {
            Debug.Log("Flame hit");
            SendCommand("BURNING", "true");
        }
    }

    private IEnumerator BurnAndShrink()
    {
        Debug.Log("Burning and shrinking");
        Vector3 targetScale = Vector3.one * 0.05f;
        float shrinkSpeed = 0.5f; // You can tweak this for faster/slower burning

        while (transform.localScale.magnitude > targetScale.magnitude)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, shrinkSpeed * Time.deltaTime);
            yield return null;
        }

        if (IsServer) // Only the server should destroy the object
        {
            MyCore.NetDestroyObject(this.NetId);
        }
    }
}
