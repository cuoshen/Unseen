using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    class TopDownPlayerController : MonoBehaviour
    {
        public CharacterController Character;
        public GameObject WinCon;
        [SerializeField]
        private float speed = 6.0f;
        [SerializeField]
        private float gravity = 9.81f;
        [SerializeField]
        private float dashDistance = 5.0f;

        private Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            Character = gameObject.GetComponent<CharacterController>();
            animator = gameObject.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0.0f, vertical);

            if (direction.magnitude >= 0.1f)
            {
                Character.Move(direction * speed * Time.deltaTime);
            }

            Character.Move(new Vector3(0, -1, 0) * gravity * Time.deltaTime);

            animator.SetFloat("Speed", Character.velocity.magnitude);

            if (Input.GetKeyDown(KeyCode.F))    // Grab wincon and end the game if possible
            {
                if (Vector3.Distance(transform.position, WinCon.transform.position) <= 3.0f)
                {
                    Debug.Log("You win the game");
                }
            }
        }
    }
}
