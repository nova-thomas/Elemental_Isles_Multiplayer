using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;

public class Beholder : Enemy
{
    /*              **Variables**              */
    [Header("Beholder Specific Variables")]
    public Transform BlastPosition;
    public float BulletSpeed;
    public int blastType;

    public int damage = 3;

    /*              **Functions**              */
    public override void HandleMessage(string flag, string value)
    {
        //Animations
        if (IsClient)
        {
            if (flag == "STOP")
            {
                myAnimator.SetInteger("FWD", 0);
            }

            if (flag == "WALK")
            {
                //myAnimator.CrossFade("WalkFWD", .2f);
                myAnimator.SetInteger("FWD", 1);
            }

            if (flag == "BLAST")
            {
                //myAnimator.CrossFade("Attack01", .2f);
                myAnimator.SetBool("ATTACK", true);
                StartCoroutine(AttackTime());

            }

            if (flag == "DEATH")
            {
                //myAnimator.CrossFade("Die", .2f);
                myAnimator.SetBool("DEAD", true);
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
            BlastAttack();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public void BlastAttack()
    {
        SendUpdate("BLAST", " ");
        GameObject fireBall = MyCore.NetCreateObject(blastType, Owner, BlastPosition.transform.position, BlastPosition.transform.rotation);
        fireBall.tag = "EnemyBullet";
        fireBall.GetComponent<Lifetime>().damage = damage;
        Rigidbody fireBallRB = fireBall.GetComponent<Rigidbody>();

        if (fireBallRB != null)
        {
            fireBallRB.velocity = BlastPosition.forward * BulletSpeed;
        }
    }

    public IEnumerator AttackTime()
    {
        yield return new WaitForSeconds(.5f);
        myAnimator.SetBool("ATTACK", false);
    }
}
