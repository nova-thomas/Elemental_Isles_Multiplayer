using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Lifetime : NetworkComponent
{
    public float lifetime;

    private Rigidbody rb;
    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(LifeTimer());
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsConnected)
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

    private IEnumerator LifeTimer()
    {
        if (IsServer)
        {
            float timeElapsed = 0f;
            float mudGravity = -0.75f;
            float waterGravity = -0.25f;


            while (timeElapsed < lifetime)
            {
                if (this.tag == "MudShot")
                {
                    // Apply gravity-like effect to downward velocity (simulating falling)
                    rb.velocity += new Vector3(0, mudGravity * timeElapsed, 0); // Accelerate downward over time
                }
                if (this.tag == "WaterBlast")
                {
                    // Apply gravity-like effect to downward velocity (simulating falling)
                    rb.velocity += new Vector3(0, waterGravity * timeElapsed, 0); // Accelerate downward over time
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }


            MyCore.NetDestroyObject(this.NetId);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            MyCore.NetDestroyObject(this.NetId);
        }
    }
}
