using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beholder : Enemy
{
    /*              **Variables**              */
    [Header("Beholder Specific Variables")]
    public Transform BlastPosition;
    public float BulletSpeed;
    public int blastType;

    /*              **Functions**              */
    public override void HandleMessage(string flag, string value)
    {
        //Animations
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
            BlastAttack();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public void BlastAttack()
    {
        GameObject fireBall = MyCore.NetCreateObject(blastType, Owner, BlastPosition.transform.position, BlastPosition.transform.rotation);
        fireBall.tag = "EnemyBullet";
        Rigidbody fireBallRB = fireBall.GetComponent<Rigidbody>();

        if (fireBallRB != null)
        {
            fireBallRB.velocity = BlastPosition.forward * BulletSpeed;
        }
    }
}
