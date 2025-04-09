using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class CollectableSpawner : NetworkComponent
{
    public Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        //MyCore.NetCreateObject(12, this.Owner, this.transform.position, Quaternion.identity);
    }

    public override IEnumerator SlowUpdate()
    {
        // Spawn collectable once every 3 seconds
        while (IsConnected)
        {
            SpawnCollectable();

            yield return new WaitForSeconds(10f);
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

    public void SpawnCollectable()
    {
        if (!IsServer) return; // Only the server should spawn objects

        // Collectable ID (5-8 antenna, 9 coins, 10-13 collectables)
        // Spawn the collectable object
        MyCore.NetCreateObject(12, this.Owner, this.transform.position, Quaternion.identity);
    }
}
