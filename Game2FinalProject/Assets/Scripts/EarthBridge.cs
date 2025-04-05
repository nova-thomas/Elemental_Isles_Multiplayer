using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EarthBridge : NetworkComponent
{
    public bool built;

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
            }

            Transform child = transform.GetChild(0);
            MeshCollider meshCollider = child.GetComponent<MeshCollider>();
            Renderer renderer = child.GetComponent<Renderer>();


            if (built)
            {
                if (meshCollider != null) meshCollider.enabled = true;
                if (renderer != null)
                {
                    SetMaterialToOpaque(renderer.material);
                }
            }
            else
            {
                if (meshCollider != null) meshCollider.enabled = false;
                if (renderer != null)
                {
                    SetMaterialToTransparent(renderer.material);
                }
            }
        }
    }

    public override void NetworkedStart()
    {
        built = false;
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsConnected)
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
            SendCommand("BUILT", "true");
        }
    }

    void SetMaterialToTransparent(Material mat)
    {
        Color color = mat.color;
        color.a = 0.2f;
        mat.color = color;

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    void SetMaterialToOpaque(Material mat)
    {
        Color color = mat.color;
        color.a = 1f;
        mat.color = color;

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.EnableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }
}
