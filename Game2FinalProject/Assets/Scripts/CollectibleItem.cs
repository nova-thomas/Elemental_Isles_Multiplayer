using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class CollectibleItem : NetworkComponent
{
    public float floatSpeed = 0.5f;
    public float floatAmplitude = 0.25f;
    public float rotationSpeed = 50f;

    private Vector3 startPosition;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            StartCoroutine(SpawnCollectibles());
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        FloatAndRotate();
    }

    void FloatAndRotate()
    {
        // Floating motion using sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Rotate around Y-axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private IEnumerator SpawnCollectibles()
    {
        // Find all empty objects with specific names like "CoinSpawn"
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("CoinSpawn");

        foreach (GameObject spawn in spawnPoints)
        {
            MyCore.NetCreateObject(5 ,-1, spawn.transform.position, Quaternion.identity);
        }

        yield return null;
    }
}
