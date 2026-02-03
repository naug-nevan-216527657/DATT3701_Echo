using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MovementController : MonoBehaviour
{

    public PlayerInput playerInput;
    public CharacterController characterController;
    public Animator animator;
    public AudioSource footstepSound;

    private bool isWaitingForFootstep = false;
    public float footstepInterval = 0.5f;

    private int isWalkingHash;
    private int isCrouchingHash;

    private Vector2 inputVector;
    private Vector3 moveDirection;
    // private Vector3 crouchMoveDirection;
    private bool isMoving;
    private bool CrouchPressed = false;

    public float moveSpeed = 1f;
    public float crouchMoveSpeed = 0.5f;
    public float rotationSpeed = 1f;

    void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isCrouchingHash = Animator.StringToHash("isCrouching");

        playerInput.Player.Move.started += OnMovementInput;
        playerInput.Player.Move.performed += OnMovementInput;
        playerInput.Player.Move.canceled += OnMovementInput;

        playerInput.Player.CrouchToggle.performed += OnCrouchToggleInput;
        playerInput.Player.Crouch.started += OnCrouchHoldInput;
        playerInput.Player.Crouch.canceled += OnCrouchHoldInput;
    }
     
    private void OnMovementInput(InputAction.CallbackContext ctx)
    {
        inputVector = ctx.ReadValue<Vector2>();
        moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        isMoving = inputVector.x != 0 || inputVector.y != 0;
    }

    private void OnCrouchToggleInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            CrouchPressed = !CrouchPressed;
        }
    }

    private void OnCrouchHoldInput(InputAction.CallbackContext ctx)
    {
        CrouchPressed = ctx.ReadValueAsButton();
    }

    void OnEnable()
    {
        playerInput.Player.Enable();
    }

    void OnDisable()
    {
        playerInput.Player.Disable();
    }

    private void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isCrouching = animator.GetBool(isCrouchingHash);

        if (isMoving && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMoving && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if (CrouchPressed && !isCrouching)
        {
            animator.SetBool(isCrouchingHash, true);
        }
        else if (!CrouchPressed && isCrouching)
        {
            animator.SetBool(isCrouchingHash, false);
        }
    }

    private void HandleRotation()
    {
        Vector3 positionToLook = new Vector3(moveDirection.x, 0, moveDirection.z);
        Quaternion currentRotation = transform.rotation;

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator FootstepSounds()
    {
        while (isMoving && !CrouchPressed)
        {
            footstepSound.Play();
            isWaitingForFootstep = true;
            yield return new WaitForSeconds(footstepInterval); // Adjust delay as needed
            isWaitingForFootstep = false;
        }
            
    }
    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleAnimation();

        if (isMoving && !isWaitingForFootstep && !CrouchPressed)
        {
            StartCoroutine(FootstepSounds());
            
        } else if (!isMoving || CrouchPressed) 
        {
            isWaitingForFootstep = false;
            StopCoroutine(FootstepSounds());
        }
        
        
        if (CrouchPressed)
        {
            characterController.Move(moveDirection * Time.deltaTime * crouchMoveSpeed);
        } else
        {
            characterController.Move(moveDirection * Time.deltaTime * moveSpeed);
        }
        
        
        
        
    }
}
