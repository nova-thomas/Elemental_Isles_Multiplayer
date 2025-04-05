using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NetworkRigidBody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : NetworkComponent
{
    public enum ElementType { Water, Fire, Earth, Air };

    /*              **Variables**              */
    //Basic Variables
    public float speed;
    public float health;
    public float maxHealth;
    public double damage;
    public bool slowed;

    //Prefab Components
    public NavMeshAgent agent;
    public Rigidbody myRig;

    //Player(s)
    public GameObject[] players;
    public GameObject nearestPlayer;
    //private Player playerScript;

    public LayerMask whatIsGround, whatIsPlayer;

    //public HealthbarControl healthbar;

    //Loot
    public int coin;
    public int coinAmount;
    public int crystal;
    public float monsterDropHeight;

    public Animator myAnimator;

    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    public bool alreadyAttacked;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public bool playedAmbient;
    public float timeBetweenAmbient;


    /*              **Functions**              */
    //Start Functions
    public void Awake()
    {
        findNearestPlayer();
        //playerScript = player.GetComponent<Player>();
        agent = GetComponent<NavMeshAgent>();
        myRig = GetComponent<Rigidbody>();
        maxHealth = health;

        if (agent != null)
        {
            agent.enabled = false; // Disable agent temporarily
            Invoke(nameof(EnableAgent), 0.2f); // Enable after a short delay
        }

        if (myRig != null)
        {
            myRig.isKinematic = true; // Temporarily disable physics
            Invoke(nameof(EnablePhysics), 0.2f); // Enable after stabilization
        }

        StartBehavior();
    }

    private void EnablePhysics()
    {
        if (myRig != null)
        {
            myRig.isKinematic = false;
        }
    }

    private void EnableAgent()
    {
        if (agent != null)
        {
            agent.enabled = true;
            agent.ResetPath(); // Clear any residual paths
        }
    }

    //Net Component Functions
    public override void HandleMessage(string flag, string value)
    {
        //throw new System.NotImplementedException();
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            players = GameObject.FindGameObjectsWithTag("PlayerCharacter");
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
    }

    //Default Functions
    void Start()
    {
        //myAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    public void findNearestPlayer()
    {
        float dist = 0, currentDist = 0;
        foreach (var player in players)
        {
            currentDist = transform.position.magnitude - player.transform.position.magnitude;
            if (currentDist < dist)
            {
                dist = currentDist;
                nearestPlayer = player;
            }
        }
    }

    private void SpawnItem(int item)
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);

        Vector3 itemSpawnPos = new Vector3(
            transform.position.x + randomX,
            transform.position.y + monsterDropHeight,
            transform.position.z + randomZ
        );

        GameObject spawnedItem = MyCore.NetCreateObject(item, Owner, itemSpawnPos);
        Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }

    //Behavior Functions
    public void StartBehavior()
    {
        Invoke(nameof(Patrolling), 0.2f); // Start patrolling after a short delay
    }

    public void Patrolling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    public void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        Vector3 tentativePoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (NavMesh.SamplePosition(tentativePoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            walkPoint = hit.position; // Valid walk point
            walkPointSet = true;
        }
    }

    public void ChasePlayer()
    {
        agent.SetDestination(nearestPlayer.transform.position);
    }

    public void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void AmbientPlayed()
    {
        playedAmbient = false;
    }

    //Interaction
    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            string tag = other.tag;

            switch (tag)
            {
                case "Bullet":
                    health--;
                    break;
                case "MudShot":
                    health--;
                    slowed = true;
                    StartCoroutine(Slow());
                    break;
                case "Flame":
                    health--;
                    for (int i = 0; i < 6; i++)
                    {
                        StartCoroutine(Burn());
                    }
                    break;
                case "WaterBlast":
                    health--;
                    Push();
                    break;
            }

            if (health <= 0)
            {
                for (int i = 0; i < coinAmount; i++) // Drop coin(s)
                {
                    SpawnItem(coin);
                }

                float randomValue = Random.Range(0f, 100f);

                if (randomValue <= 30f) // 30% chance for crystal
                {
                    SpawnItem(crystal);
                }

                MyCore.NetDestroyObject(NetId);
            }
        }
    }

    public IEnumerator Slow()
    {
        yield return new WaitForSeconds(5f);
        slowed = false;
    }

    public IEnumerator Burn()
    {
        yield return new WaitForSeconds(.5f);
        health--;
    }

    public void Push()
    {
    
    }
}
