using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(NetworkRigidBody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : NetworkComponent
{
    public enum ElementType { Water, Fire, Earth, Air };

    /*              **Variables**              */
    [Header("Enemy Settings")]
    public ElementType enemyElementType;
    //Loot
    public int coin;
    public int coinAmount;
    public int crystal;
    public float monsterDropHeight;

    [Header("Movement & Attack")]
    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float sightRange, attackRange;
    public float timeBetweenAttacks;
    public bool alreadyAttacked;

    [Header("Other Variables")]
    //Basic Variables
    public float speed;
    public float health;
    public float maxHealth;
    public float deathTime;
    public double damage;
    public bool slowed;

    //Prefab Components
    public HealthbarControl healthbar;
    public NavMeshAgent agent;
    public Rigidbody myRig;

    //Player(s)
    public GameObject[] players;
    public GameObject nearestPlayer;
    //private Player playerScript;

    public LayerMask whatIsGround, whatIsPlayer;

    public Animator myAnimator;

    // States
    public bool playerInSightRange, playerInAttackRange;
    public bool playedAmbient;
    public float timeBetweenAmbient;


    /*              **Functions**              */
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
        if (flag == "default")
        {
            
        }
        //throw new System.NotImplementedException();
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
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
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsServer)
        {
            if (IsServer && players.Length < 2)
            {
                players = GameObject.FindGameObjectsWithTag("Player");
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    //Default Functions
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        myRig = GetComponent<Rigidbody>();
        //myAnimator = GetComponent<Animator>();
    }

    void Update()
    {

    }

    public void findNearestPlayer()
    {
        float dist = 0, currentDist = 0;
        for (int i = 0; i < players.Length; i++)
        {
            currentDist = Mathf.Abs(transform.position.magnitude - players[i].transform.position.magnitude);
            if (currentDist < dist || dist == 0)
            {
                dist = currentDist;
                nearestPlayer = players[i];
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
            SendUpdate("WALK", " ");
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
        SendUpdate("WALK", " ");

        Vector3 lookAtVar = new Vector3(nearestPlayer.transform.position.x, gameObject.transform.position.y, nearestPlayer.transform.position.z);
        agent.SetDestination(lookAtVar);
        transform.LookAt(lookAtVar);
    }

    public void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void AmbientPlayed()
    {
        playedAmbient = false;
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1f, whatIsGround);
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
                    Hurt();
                    Destroy(other);
                    break;
                case "MudShot":
                    Hurt();
                    Destroy(other);
                    slowed = true;
                    StartCoroutine(Slow());
                    break;
                case "Flame":
                    Hurt();
                    Destroy(other);
                    for (int i = 0; i < 6; i++)
                    {
                        StartCoroutine(Burn());
                    }
                    break;
                case "WaterBlast":
                    Hurt();
                    Destroy(other);
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

                //MyCore.NetDestroyObject(NetId);
                StartCoroutine(Death());
            }
        }
    }

    public void Hurt()
    {
        health--;
        SendUpdate("HURT", health.ToString());
    }

    public IEnumerator Death()
    {
        SendUpdate("DEATH", " ");
        yield return new WaitForSeconds(deathTime);
        MyCore.NetDestroyObject(NetId);
    }

    public IEnumerator Slow()
    {
        yield return new WaitForSeconds(5f);
        slowed = false;
    }

    public IEnumerator Burn()
    {
        yield return new WaitForSeconds(.5f);
        Hurt();
    }

    public void Push()
    {
    
    }
}
