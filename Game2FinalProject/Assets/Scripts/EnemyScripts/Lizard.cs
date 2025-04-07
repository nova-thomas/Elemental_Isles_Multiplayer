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

    /*              **Functions**              */
    public override void HandleMessage(string flag, string value)
    {
        //throw new System.NotImplementedException();
    }

    /*public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            Debug.Log("Net working");
            if (IsServer && IsDirty)
            {
                

                IsDirty = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }*/
    
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
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (!playerInSightRange && !playerInAttackRange) Patrolling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInSightRange && playerInAttackRange) AttackPlayer();

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
            //SpitAttack();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public void SpitAttack()
    {
        //GameObject fireBall = Instantiate(fireBallPrefab, firePosition.position, firePosition.rotation);
        GameObject fireBall = MyCore.NetCreateObject(spitType, Owner, firePosition.transform.position);
        Rigidbody fireBallRB = fireBall.GetComponent<Rigidbody>();

        if (fireBallRB != null)
        {
            fireBallRB.velocity = -firePosition.right * fireBallSpeed;
        }

        // Set the damage value on the FireProjectile component
        /*FireProjectile fireProjectile = fireBall.GetComponent<FireProjectile>();
        if (fireProjectile != null)
        {
            fireProjectile.damage = damage;
        }*/
    }
}
