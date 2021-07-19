using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectThing : Character
{
    public float visionRange;
    public float attackRange;

    GameObject player;

    new void Update()
    {
        base.Update();

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        else
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
            if (distance < attackRange)
            {
                animator.SetTrigger("Attack");
                Debug.LogAssertion("KIWI");
            }
            else if(visionRange + 1 <= distance || Physics.Linecast(transform.position, player.transform.position, out RaycastHit hit, maze_layer))
            {
                movement.x = 0;
                movement.z = 0;
            }
            else if (distance >= attackRange && distance < visionRange)
            {
                Vector3 diff = (player.transform.position - transform.position).normalized;
                movement.x = diff.x;
                movement.z = diff.z;
            }
        }
    }
}
