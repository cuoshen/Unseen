using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    public enum PlayerState {IDLE, ATTACK, WALK };
    class TopDownPlayerController : MonoBehaviour
    {
        public CharacterController Character;
        public GameObject WinCon;
        [SerializeField]
        private float speed = 5.0f;
        [SerializeField]
        private float gravity = 9.81f;
        [SerializeField]
        private float angularSpeed = 15.0f;
        public PlayerState state;
        private Animator animator;

        // Start is called before the first frame update
        void Awake()
        {
            Character = gameObject.GetComponent<CharacterController>();
            animator = gameObject.GetComponent<Animator>();
            state = PlayerState.IDLE;
        }

        // Update is called once per frame
        void Update()
        {
            TryToRestoreIdle();

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0.0f, vertical);

            if (Input.GetMouseButtonDown(0) && state != PlayerState.WALK)
            {
                state = PlayerState.ATTACK;
                animator.Play("Attack");
            }
            else if (direction.magnitude >= 0.1f && state != PlayerState.ATTACK)
            {
                state = PlayerState.WALK;
                Character.Move(direction * speed * Time.deltaTime);
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularSpeed);
            }

            ResolveVelocity();

            Character.Move(new Vector3(0, -1, 0) * gravity * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.F))    // Grab wincon and end the game if possible
            {
                if (Vector3.Distance(transform.position, WinCon.transform.position) <= 3.0f)
                {
                    GameController.Instance.WinGame();
                }
            }
        }

        private void TryToRestoreIdle()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                state = PlayerState.IDLE;
            }
        }

        private void ResolveVelocity()
        {
            Vector2 horizontalVelocity = new Vector2(Character.velocity.x, Character.velocity.z);
            float verticalVelocity = -Character.velocity.y;
            animator.SetFloat("Move Speed", horizontalVelocity.magnitude);
            animator.SetFloat("Vertical Speed", verticalVelocity);
        }
    }
}
