using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    class TopDownPlayerController : MonoBehaviour
    {
        public CharacterController characterController;
        [SerializeField]
        private float speed = 6.0f;
        [SerializeField]
        private float gravity = 9.81f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0.0f, vertical);

            if (direction.magnitude >= 0.1f)
            {
                characterController.Move(direction * speed * Time.deltaTime);
            }
        }
    }
}
