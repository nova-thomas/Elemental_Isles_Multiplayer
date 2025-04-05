using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireBramble : NetworkComponent
{
    public bool burning;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "BURNING")
        {
            if (IsServer)
            {
                burning = bool.Parse(value);
                SendUpdate("BURNING", burning.ToString());

                if (burning)
                {
                    // Start coroutine
                    StartCoroutine(BurnAndShrink());
                }
            }
            if (IsClient)
            {
                burning = bool.Parse(value);
            }


            
        }
    }

    public override void NetworkedStart()
    {
        burning = false;
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsConnected)
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
        if (other.tag == "Flame")
        {
            SendCommand("BURNING", "true");
        }
    }

    private IEnumerator BurnAndShrink()
    {
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
