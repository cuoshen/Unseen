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
        private float attackDis = 4f;

        private int stunnedCounter = 0;
        private NavMeshAgent agent;
        public bool isAttacked = false;
        private Animator animator;
        private CharacterController cc;



        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindWithTag("Player");
            agent = GetComponent<NavMeshAgent>();
            animator = gameObject.GetComponent<Animator>();
            cc = GetComponent<CharacterController>();
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

                Vector3 offset = player.transform.position - transform.position;
                float speed = 2f;
                float x = 0, z = 0;
                float angularSpeed = 3.0f;
                if (offset.z > 0)
                {
                    z = 1;
                }
                else if (offset.z < 0)
                {
                    z = -1;

                }

                if (offset.x > 0)
                {
                    x = 1;

                }
                else if (offset.x < 0)
                {
                    x = -1;
                }
                Vector3 direction = new Vector3(x, 0, z);
                //NextPosition = transform.position + new Vector3(x, 0, z);
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularSpeed);
                cc.Move(direction * speed * Time.deltaTime);
                //agent.SetDestination(player.transform.position);
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

