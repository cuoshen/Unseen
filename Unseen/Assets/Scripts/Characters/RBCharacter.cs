using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBCharacter : MonoBehaviour
{
    protected Rigidbody rb;
    protected Animator animator;

    protected Vector3 movement;

    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float angularSpeed;
    [SerializeField]
    protected float gravity;

    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    protected void FixedUpdate()
    {
        rb.MovePosition(transform.position + Time.deltaTime * speed * movement);
        ResolveVelocity();
        Rotate();
    }

    protected void ResolveVelocity()
    {
        float moveSpeed = new Vector2(movement.x, movement.z).magnitude;
        animator.SetFloat("Move Speed", moveSpeed);
    }

    protected void Rotate()
    {
        Vector3 lookAt = new Vector3(movement.x, 0, movement.z);
        if (lookAt != Vector3.zero)
        {
            Quaternion curRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(lookAt);
            transform.rotation = Quaternion.Slerp(curRotation, targetRotation, Time.deltaTime * angularSpeed);
        }
    }
}
