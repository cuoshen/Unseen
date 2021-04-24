using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Jail
{
    public enum EnemyState
    {
        attack,
        death,
        stunned,
        steady,
        move
    }

    public enum AttackEffects
    {
        stun,
        noEff
    }

    public class AI : MonoBehaviour
    {
        private GameObject player;
        public EnemyState state = EnemyState.steady;
        private float distance;
        private float EncounterDis = 10f;
        private float attackDis = 2f;

        private int stunnedCounter = 0;
        private NavMeshAgent agent; // not sure what type is that
        public bool isAttacked = false;

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindWithTag("Player");
            agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            AttackEffects attackEffect = AttackEffects.noEff;
            if (isAttacked && attackEffect == AttackEffects.stun)
            {
                state = EnemyState.stunned;
                stunnedCounter = 100;
            }
            else if (state == EnemyState.stunned)
            {
                if (stunnedCounter == 0)
                {
                    state = EnemyState.steady;
                }
                else
                {
                    stunnedCounter -= 1;
                }
                return;
            }

            distance = Vector3.Distance(player.transform.position, transform.position);
            if (EncounterDis >= distance )
            {
                state = EnemyState.steady;
            } 
            else if (distance < EncounterDis && attackDis <= distance)
            {
                state = EnemyState.move;
                agent.SetDestination(player.transform.position);
            } else {
                state = EnemyState.attack;
            }
        }
    }
}

