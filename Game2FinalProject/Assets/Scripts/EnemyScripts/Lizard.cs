using NETWORK_ENGINE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Lizard : Enemy
{
    /*              **Variables**              */
    [Header("Lizard Specific Variables")]
    public Transform firePosition;
    public float fireBallSpeed;
    public int spitType;
    public bool isWalking;

    public int damage = 1;

    /*              **Functions**              */
    public override void HandleMessage(string flag, string value)
    {
        if (IsClient)
        {
            if (flag == "WALK" && !isWalking)
            {
                isWalking = true;
                myAnimator.SetInteger("DIR", 1);
                myAnimator.Play("run");
            }

            if (flag == "STOP")
            {
                isWalking = false;
                myAnimator.SetInteger("DIR", 0);
            }

            if (flag == "HURT")
            {
                health = int.Parse(value);
                healthbar.UpdateHealthBar();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            if (!IsGrounded())
            {
                return;
            }

            if (agent.destination == transform.position)
            {
                SendUpdate("STOP", " ");
            }

            findNearestPlayer();
            if (nearestPlayer != null)
            {
                playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
                playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

                if (!playerInSightRange && !playerInAttackRange) Patrolling();
                if (playerInSightRange && !playerInAttackRange) ChasePlayer();
                if (playerInSightRange && playerInAttackRange) AttackPlayer();
            }

            //Ambient Sounds

        }
    }

    public void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        Vector3 lookAtVar = new Vector3(nearestPlayer.transform.position.x, gameObject.transform.position.y, nearestPlayer.transform.position.z);
        transform.LookAt(lookAtVar);

        if (!alreadyAttacked)
        {
            SpitAttack();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public void SpitAttack()
    {
        GameObject fireBall = MyCore.NetCreateObject(spitType, Owner, firePosition.transform.position, firePosition.transform.rotation);
        fireBall.tag = "EnemyBullet";
        fireBall.GetComponent<Lifetime>().damage = damage;
        Rigidbody fireBallRB = fireBall.GetComponent<Rigidbody>();

        if (fireBallRB != null)
        {
            fireBallRB.velocity = firePosition.forward * fireBallSpeed;
        }
    }
}
