using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;
public class EnemySkeleton : NetworkComponent
{
    /*
        public NavMeshAgent MyAgent;
        public List<Vector3> Goals;
        public Vector3 CurrentGoal;
        public Animator MyAnime;
        public float timer = 0f;
        public bool move = false;*/

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

    public Animator myAnimator;

    // States
    public bool playerInSightRange, playerInAttackRange;
    public bool playedAmbient;
    public float timeBetweenAmbient;


    /*              **Functions**              */
    //Start Functions
    /*public void Awake()
    {
        //findNearestPlayer();
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

        //StartBehavior();
    }*/

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
        if (IsClient && flag == "MOVE")
        {
            //move = bool.Parse(value);
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            Debug.Log("Network running");
            //Rando();
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
    }

    /*
    public void Rando()
    {
        if (IsServer)
        {
            GameObject[] temp = GameObject.FindGameObjectsWithTag("NavPoint");
            Goals = new List<Vector3>();
            foreach (GameObject g in temp)
            {
                Goals.Add(g.transform.position);
            }
            if (Random.Range(0,9) < 7)
            {
                int num = Random.Range(0, Goals.Count - 1);
                MyAgent.SetDestination(Goals[num]);
            }
            else
            {
                SendUpdate("MOVE", false.ToString());
                timer = 10;
            }
        }
    }*/

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = 0;
        }
        if (IsServer && (transform.position - MyAgent.destination).magnitude < 0.01f && timer == 0)
        {
            SendUpdate("MOVE", true.ToString());
            Rando();
        }
        if (IsClient)
        {
            if (move)
            {
                MyAnime.SetFloat("speedh", 1f);
            }
            else
            {
                MyAnime.SetFloat("speedh", 0f);
            }
        }*/
    }
}
