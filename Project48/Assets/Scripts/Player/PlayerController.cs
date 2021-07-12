using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    Vector2 curMovementInput;
    Vector3 curMovement;

    [SerializeField]
    float speed;
    [SerializeField]
    float angularSpeed;
    [SerializeField]
    float gravity;

    void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        playerInput.CharacterControls.Move.started += OnMovementInput;
        playerInput.CharacterControls.Move.canceled += OnMovementInput;
        playerInput.CharacterControls.Move.performed += OnMovementInput;
    }

    void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }

    void Update()
    {
        characterController.Move(curMovement * speed * Time.deltaTime);
        Rotate();
        Gravity();
        ResolveVelocity();
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        curMovementInput = context.ReadValue<Vector2>();
        curMovement.x = curMovementInput.x;
        curMovement.z = curMovementInput.y;
    }

    void ResolveVelocity()
    {
        float horizontalVelocity = new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude;
        float verticalVelocity = -characterController.velocity.y;
        animator.SetFloat("Move Speed", horizontalVelocity);
        animator.SetFloat("Vertical Speed", verticalVelocity);
    }

    void Rotate()
    {
        Vector3 lookAt = new Vector3(curMovement.x, 0, curMovement.z);
        if (lookAt != Vector3.zero)
        {
            Quaternion curRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(lookAt);
            transform.rotation = Quaternion.Slerp(curRotation, targetRotation, angularSpeed * Time.deltaTime);
        }
    }

    void Gravity()
    {
        if (characterController.isGrounded)
            curMovement.y = -0.05f;
        else
            curMovement.y -= gravity;
    }
}
