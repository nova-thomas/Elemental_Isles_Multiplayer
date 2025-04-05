using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class AirLift : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {
        StartCoroutine(Spin());
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

    private IEnumerator Spin()
    {
        if(IsServer)
        {
            float duration = 0.5f;
            float timeElapsed = 0f;
            float totalRotation = 1080f; // 3 full spins
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f; // Move up by 2 units

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;

                // Rotate around Y-axis
                float currentRotation = Mathf.Lerp(0f, totalRotation, t);
                transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

                // Move upward
                transform.position = Vector3.Lerp(startPos, endPos, t);

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure final rotation/position
            transform.rotation = Quaternion.Euler(0f, totalRotation, 0f);
            transform.position = endPos;

            MyCore.NetDestroyObject(this.NetId);
        }
    }
}
