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

    public float EncounterDis;
    public float attackDis;
    public float speed;

    private Animator animator;
    private CharacterController cc;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        animator = gameObject.GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if ( EncounterDis <= distance)
        {
            state = EnemyState.IDLE;
        } 
        else if (attackDis <= distance && distance < EncounterDis )
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

