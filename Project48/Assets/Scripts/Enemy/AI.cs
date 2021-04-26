using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Jail
{
    public enum EnemyState
    {
        ATTACK,
        STUNNED,
        IDLE,
        MOVE
    }



    public class AI : MonoBehaviour
    {
        private GameObject player;
        public EnemyState state = EnemyState.IDLE;
        private float distance;
        private float EncounterDis = 10f;
        private float attackDis = 2f;

        private int stunnedCounter = 0;
        private NavMeshAgent agent;
        public bool isAttacked = false;
        private Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindWithTag("Player");
            agent = GetComponent<NavMeshAgent>();
            animator = gameObject.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

            distance = Vector3.Distance(player.transform.position, transform.position);
            isAttacked = player.GetComponent<TopDownPlayerController>().state == PlayerState.ATTACK && distance <= 3;
            //Debug.Log(distance);
            if (isAttacked)
            {

                state = EnemyState.STUNNED;
                stunnedCounter = 100;

            }
            else if (state == EnemyState.STUNNED)
            {
                if (stunnedCounter == 0)
                {
                    state = EnemyState.IDLE;
                }
                else
                {
                    stunnedCounter -= 1;
                }
                return;
            }

            
            if ( EncounterDis <= distance)
            {
                state = EnemyState.IDLE;
            } 
            else if (attackDis <= distance && distance < EncounterDis )
            {
                state = EnemyState.MOVE;
                agent.SetDestination(player.transform.position);
            } 
            else 
            {
                state = EnemyState.ATTACK;
            }


            switch (state)
            {
                case EnemyState.IDLE:
                    animator.Play("Idle");
                    break;
                case EnemyState.ATTACK:
                    animator.Play("Attack");
                    break;
                case EnemyState.MOVE:
                    animator.Play("Run");
                    break;
                case EnemyState.STUNNED:
                    animator.Play("Stunned");
                    break;
            }

        }
    }
}

