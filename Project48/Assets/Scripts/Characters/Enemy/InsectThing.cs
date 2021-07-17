using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectThing : Character
{
    GameObject player;

    [SerializeField]
    float encounterDis;
    [SerializeField]
    float attackDis;

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
            if (distance < attackDis)
            {
                animator.SetTrigger("Attack");
            }
            else if(encounterDis <= distance || Physics.Linecast(transform.position, player.transform.position, out RaycastHit hit, maze_layer)) // not within encounter distance or blocked
            {
                movement = Vector3.zero;
            }
            else if (attackDis <= distance && distance < encounterDis)
            {
                Vector3 diff = (player.transform.position - transform.position).normalized;
                movement.x = diff.x;
                movement.z = diff.z;
            }
        }
    }
}
