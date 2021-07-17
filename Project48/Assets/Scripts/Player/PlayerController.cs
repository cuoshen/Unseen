using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    Animator animator;

    Vector3 movement;

    [SerializeField]
    float speed;
    [SerializeField]
    float angularSpeed;
    [SerializeField]
    float gravity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        cc.Move(Time.deltaTime * speed * movement);
        ResolveVelocity();
        Rotate();
        Gravity();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        movement.x = movementInput.x;
        movement.z = movementInput.y;
    }

    void ResolveVelocity()
    {
        float moveSpeed = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
        animator.SetFloat("Move Speed", moveSpeed);
    }

    void Rotate()
    {
        Vector3 lookAt = new Vector3(movement.x, 0, movement.z);
        if (lookAt != Vector3.zero)
        {
            Quaternion curRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(lookAt);
            transform.rotation = Quaternion.Slerp(curRotation, targetRotation, Time.deltaTime * angularSpeed);
        }
    }

    void Gravity()
    {
        if (cc.isGrounded)
            movement.y = -0.05f;
        else
            movement.y -= gravity;
    }
}
