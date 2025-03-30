using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class FloatingObject : NetworkComponent
{
    public float rotationSpeed = 50f; 
    private float syncedYRotation;    

    public override void NetworkedStart()
    {
        if (!IsServer)
        {
            StartCoroutine(SmoothRotationUpdate());
        }
    }

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "ROT")
        {
            if (float.TryParse(value, out float yRotation))
            {
                syncedYRotation = yRotation; 
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (IsServer)
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

                SendUpdate("ROT", transform.eulerAngles.y.ToString());
            }

            yield return new WaitForSeconds(0.05f); 
        }
    }

    private IEnumerator SmoothRotationUpdate()
    {
        while (true)
        {
            if (!IsServer)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0, syncedYRotation, 0),
                    Time.deltaTime * 10f 
                );
            }
            yield return null; 
        }
    }
}
