using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NETWORK_ENGINE;

public class EnemySpawner : NetworkComponent
{
    /*public GameObject lizardPrefab;
    public GameObject plaguePrefab;
    public GameObject golemPrefab;
    public GameObject dragonPrefab;*/

    public int airLizard, earthLizard, fireLizard, waterLizard;
    public int plague;
    public int golem;

    public int maxMembers;

    public enum EnemyType
    {
        airLizard,
        earthLizard,
        fireLizard,
        waterLizard,
        Plague,
        Golem
    }

    [SerializeField]
    private EnemyType enemyToSpawn;

    [SerializeField]
    private float respawnDelay = 5f;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private bool respawning = false; // Ensure coroutine isn't called multiple times

    void Start()
    {
        if (IsServer)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        if (AllEnemiesDead() && !respawning && IsServer)
        {
            respawning = true;
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        ClearSpawnedEnemies();
        SpawnEnemy();
        respawning = false; // Allow future respawns
    }

    private void SpawnEnemy()
    {
        //GameObject prefab = GetPrefabForEnemyType();
        int prefab = GetPrefabForEnemyType();
        if (prefab == 9) return;

        if (enemyToSpawn != EnemyType.Golem)
        {
            for (int i = 0; i < maxMembers; i++)
            {
                Vector3 spawnPosition = GetValidSpawnPosition(transform.position, 4f);
                //GameObject member = Instantiate(prefab, spawnPosition, transform.rotation);
                GameObject member = MyCore.NetCreateObject(prefab, Owner, spawnPosition);
                spawnedEnemies.Add(member);
                InitializeAgent(member);
            }
        }
        else
        {
            Vector3 spawnPosition = GetValidSpawnPosition(transform.position, 0f);
            //GameObject enemy = Instantiate(prefab, spawnPosition, transform.rotation);
            GameObject enemy = MyCore.NetCreateObject(prefab, Owner, spawnPosition);
            spawnedEnemies.Add(enemy);
            InitializeAgent(enemy);
        }
    }

    private void InitializeAgent(GameObject enemy)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false; // Temporarily disable the agent until initialization
            agent.Warp(enemy.transform.position);
            agent.enabled = true;
            agent.SetDestination(enemy.transform.position);
        }
    }

    private int GetPrefabForEnemyType()
    {
        switch (enemyToSpawn)
        {
            case EnemyType.airLizard: return airLizard;
            case EnemyType.earthLizard: return earthLizard;
            case EnemyType.fireLizard: return fireLizard;
            case EnemyType.waterLizard: return waterLizard;
            case EnemyType.Plague: return plague;
            case EnemyType.Golem: return golem;
            default: return 9;
        }
    }

    private bool AllEnemiesDead()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count == 0;
    }

    private void ClearSpawnedEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    private Vector3 GetValidSpawnPosition(Vector3 origin, float range)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-range, range), -3, Random.Range(-range, range));
        Vector3 tentativePosition = origin + randomOffset;

        if (NavMesh.SamplePosition(tentativePosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return origin;
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
        //throw new System.NotImplementedException();
    }

    public override void HandleMessage(string flag, string value)
    {
        //throw new System.NotImplementedException();
    }

    public override void NetworkedStart()
    {
        /*if (IsServer)
        {
            SpawnEnemy();
        }*/
    }
}