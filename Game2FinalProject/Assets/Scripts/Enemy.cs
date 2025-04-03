using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NetworkRigidBody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {
        throw new System.NotImplementedException();
    }

    public override void NetworkedStart()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator SlowUpdate()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
