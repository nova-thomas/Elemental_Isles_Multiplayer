using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golum : Enemy
{
    /*              **Variables**              */
    [Header("Golem Specific Variables")]
    public GameObject hitboxPrefab;
    public Transform hitboxTransform;
    public float attackSpeed;

    public int damage = 5;

    /*              **Functions**              */
    public override void HandleMessage(string flag, string value)
    {
        if (IsClient)
        {
            if (flag == "STOP")
            {
                myAnimator.SetInteger("DIR", 0);
            }

            if (flag == "WALK")
            {
                myAnimator.SetInteger("DIR", 1);
            }

            if (flag == "SWIPE")
            {
                myAnimator.SetBool("SWIPE", bool.Parse(value));
                if (bool.Parse(value))
                {
                    myAnimator.CrossFade("Swipe", .2f);
                }
            }

            if (flag == "SLAM")
            {
                myAnimator.SetBool("SLAM", bool.Parse(value));
                if (bool.Parse(value))
                {
                    myAnimator.CrossFade("Slam", .2f);
                }
            }

            if (flag == "DEATH")
            {
                myAnimator.CrossFade("Death", .2f);
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
            int attack = Random.Range(0, 2);

            switch (attack)
            {
                case 0:
                    SwingAttack();
                    break;
                case 1:
                    SlamAttack();
                    break;
                default:
                    break;
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private IEnumerator AttackTime(float attackSpeed)
    {
        yield return new WaitForSeconds(attackSpeed);
        //Debug.Log("attack");
        GameObject hitbox = Instantiate(hitboxPrefab, hitboxTransform);
        Destroy(hitbox, 1);
        yield return new WaitForSeconds(.5f);
        SendUpdate("SWIPE", false.ToString());
        SendUpdate("SLAM", false.ToString());
    }

    public void SwingAttack()
    {
        // Audio
        //audioSource.PlayOneShot(a_SwingAttack);

        // Animation
        SendUpdate("SWIPE", true.ToString());

        attackSpeed = 1;
        StartCoroutine(AttackTime(attackSpeed));
    }

    public void SlamAttack()
    {
        // Audio
        //audioSource.PlayOneShot(a_SlamAttack);

        // Animation
        SendUpdate("SLAM", true.ToString());

        attackSpeed = 1.5f;
        StartCoroutine(AttackTime(attackSpeed));
    }

}
