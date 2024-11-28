using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public GameObject projectile;
    public float attackRange;
    public float projectileSpeed;

    private Transform player;

    private float timer = 0f;
    public float attackCooldown;
    private bool canAttack;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    private void Update()
    {
        if (AttackCooldownComplete() && PlayerWithinRange())
        {
            Attack();
        }
    }

    private bool PlayerWithinRange()
    {
        Vector3 pos = transform.position;
        Vector3 playerPos = player.position;
        float dist = Mathf.Abs((pos - playerPos).magnitude);

        if (dist < attackRange)
        {
            return true;
        }

        return false;
    }

    private void Attack()
    {
        GameObject attack = Instantiate(projectile, transform.position, projectile.transform.rotation);
        attack.GetComponent<CollisionProjectile>().MoveProjectile(transform.position, player.position, projectileSpeed);
        canAttack = false;
    }

    private bool AttackCooldownComplete()
    {
        if (!canAttack)
        {
            if (timer < attackCooldown)
            {
                timer += Time.deltaTime;
            }
            else
            {
                canAttack = true;
                timer = 0f;
            }
        }

        return canAttack;

    }
}
