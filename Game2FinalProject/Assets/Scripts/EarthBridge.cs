using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EarthBridge : NetworkComponent
{
    public bool built;
    public Transform child;
    public MeshCollider meshCollider;
    public Renderer renderer;

    public Material baseMaterial;
    public Material TransparentMaterial;


    public override void HandleMessage(string flag, string value)
    {
        if (flag == "BUILT")
        {
            if (IsServer)
            {
                built = bool.Parse(value);
                SendUpdate("BUILT", built.ToString());
            }
            if (IsClient)
            {
                built = bool.Parse(value);
                if (built)
                {
                    if (meshCollider != null) meshCollider.enabled = true;
                    if (renderer != null)
                    {
                        renderer.material = baseMaterial;
                    }
                }
                else
                {
                    if (meshCollider != null) meshCollider.enabled = false;
                    if (renderer != null)
                    {
                        renderer.material = TransparentMaterial;
                    }
                }
            }
        }
    }

    public override void NetworkedStart()
    {
        built = false;

        child = transform.GetChild(0);
        meshCollider = child.GetComponent<MeshCollider>();
        renderer = child.GetComponent<Renderer>();

        if (meshCollider != null) meshCollider.enabled = false;
        if (renderer != null)
        {
            renderer.material = TransparentMaterial;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsDirty)
            {
                SendUpdate("BUILT", built.ToString());
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
        if (other.tag == "MudShot")
        {
            built = true;
            if (built)
            {
                if (meshCollider != null) meshCollider.enabled = true;
                if (renderer != null)
                {
                    renderer.material = baseMaterial;
                }
            }
            else
            {
                if (meshCollider != null) meshCollider.enabled = false;
                if (renderer != null)
                {
                    renderer.material = TransparentMaterial;
                }
            }
            SendUpdate("BUILT", built.ToString());
        }
    }
}
