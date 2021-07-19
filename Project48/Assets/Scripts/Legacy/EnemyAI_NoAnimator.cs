using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legacy
{
    public enum EnemyState { IDLE, ATTACK, MOVE, STUNNED }

    public class EnemyAI_NoAnimator : MonoBehaviour
    {
        private GameObject player;
        public EnemyState state = EnemyState.IDLE;

        private float distance;
        public float encounterDis;
        public float attackDis;
        public float speed;

        private Animator animator;
        private CharacterController cc;

        // Start is called before the first frame update
        void Start()
        {
            animator = gameObject.GetComponent<Animator>();
            cc = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
            }
            else
            {
                distance = Vector3.Distance(player.transform.position, transform.position);
                LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
                if (encounterDis <= distance || Physics.Linecast(transform.position, player.transform.position, out RaycastHit hit, maze_layer)) // not within encounter distance or blocked
                {
                    state = EnemyState.IDLE;
                }
                else if (attackDis <= distance && distance < encounterDis)
                {
                    state = EnemyState.MOVE;
                    Vector3 diff = player.transform.position - transform.position;
                    diff = diff.normalized;
                    Vector3 direction = new Vector3(diff.x, 0, diff.z);
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = targetRotation;
                    cc.Move(speed * Time.deltaTime * direction);
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
                }
            }
        }
    }
}

