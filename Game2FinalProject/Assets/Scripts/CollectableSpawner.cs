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
        
    }

    public override IEnumerator SlowUpdate()
    {
        // Spawn collectable once every 3 seconds
        while (IsConnected)
        {
            SpawnCollectable();

            yield return new WaitForSeconds(3f);
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

        // Randomize spawn position within defined area
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            0f, // Keep Y consistent unless floating collectables are needed
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        Vector3 spawnPosition = this.transform.position + randomOffset;

        // Spawn the collectable object
        MyCore.NetCreateObject(Random.Range(10, 14), this.Owner, spawnPosition, Quaternion.identity);
    }
}
