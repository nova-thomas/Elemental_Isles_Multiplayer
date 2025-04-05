using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class WaterBolder : NetworkComponent
{
    private Rigidbody rb; // Reference to the Rigidbody component
    public float knockbackForce = 10f; // Adjust this value to control knockback strength
    public Transform lastHitPos;

    public Vector3 Vector3FromString(string s)
    {
        string[] args = s.Trim().Trim('(').Trim(')').Split(',');
        return new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
    }

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "HIT")
        {
            if(IsServer)
            {
                Vector3 knockbackDirection = Vector3FromString(value);
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    public override void NetworkedStart()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsDirty)
            {
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
        if (other.tag == "WaterBlast" && IsClient)
        {
            lastHitPos = other.transform;
            Vector3 knockbackDirection = (transform.position - lastHitPos.position).normalized;

            SendCommand("HIT", knockbackDirection.ToString());
        }
    }
}
