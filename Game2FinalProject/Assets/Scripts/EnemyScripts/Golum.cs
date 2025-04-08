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

            //Animations

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
        //GameObject hitbox = Instantiate(hitboxPrefab, hitboxTransform);
        //Destroy(hitbox, 1);
        //myAnimator.SetBool("SWIPE", false);
        //myAnimator.SetBool("SLAM", false);
    }

    public void SwingAttack()
    {
        // Audio
        //audioSource.PlayOneShot(a_SwingAttack);

        // Animation
        //myAnimator.CrossFade("Swipe", .2f);
        //myAnimator.SetBool("SWIPE", true);

        attackSpeed = 1;
        StartCoroutine(AttackTime(attackSpeed));
    }

    public void SlamAttack()
    {
        // Audio
        //audioSource.PlayOneShot(a_SlamAttack);

        // Animation
        //myAnimator.CrossFade("Slam", .2f);
        //myAnimator.SetBool("SLAM", true);

        attackSpeed = 1.5f;
        StartCoroutine(AttackTime(attackSpeed));
    }

}
