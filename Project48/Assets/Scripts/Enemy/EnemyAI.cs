using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Legacy;


public enum EnemyState
{
    ATTACK,
    STUNNED,
    IDLE,
    MOVE
}

public class EnemyAI : MonoBehaviour
{
    private GameObject player;
    public EnemyState state = EnemyState.IDLE;
    private float distance;
    private float EncounterDis = 2f;
    private float attackDis = 0.4f;
    float speed = 1f;

    private int stunnedCounter = 0;
    public bool isAttacked = false;
    private Animator animator;
    private CharacterController cc;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        animator = gameObject.GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    void CheckGround()
    {
        if (!rb.isKinematic && transform.position.y <= 0)
        {
            rb.isKinematic = true;
        }
        else if (transform.position.y > 0.1)
        {
            rb.isKinematic = false;
        } 

    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        if (rb.isKinematic)
        {
            distance = Vector3.Distance(player.transform.position, transform.position);
            RaycastHit hit;
            LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
            if (EncounterDis <= distance || Physics.Linecast(transform.position, player.transform.position, out hit, maze_layer)) // not within encounter distance or blocked
            {
                state = EnemyState.IDLE;
            }
            else if (attackDis <= distance && distance < EncounterDis)
            {
                state = EnemyState.MOVE;
                Vector3 diff = player.transform.position - transform.position;
                diff = diff.normalized;
                Vector3 direction = new Vector3(diff.x, 0, diff.z);
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = targetRotation;
                cc.Move(direction * speed * Time.deltaTime);
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

