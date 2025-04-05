using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;
public class EnemySkeleton : NetworkComponent
{

    public NavMeshAgent MyAgent;
    public List<Vector3> Goals;
    public Vector3 CurrentGoal;
    public Animator MyAnime;
    public float timer = 0f;
    public bool move = false;
    public override void HandleMessage(string flag, string value)
    {
        if (IsClient && flag == "MOVE")
        {
            move = bool.Parse(value);
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            Rando();
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
    }

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
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
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
        }
    }
}
