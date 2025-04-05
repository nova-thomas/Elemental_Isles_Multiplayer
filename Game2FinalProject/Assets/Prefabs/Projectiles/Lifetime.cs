using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : NetworkComponent
{
    public float lifetime;
    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {
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
            while (timeElapsed < lifetime)
            {
                timeElapsed += Time.deltaTime;
                yield return null;
            }


            MyCore.NetDestroyObject(this.NetId);
        }
    }
}
