
using UnityEngine;

public class CharacterNew : MonoBehaviour
{
    [SerializeField] private Camera characterCamera;
    
    private bool spawning;
    private bool spawned;
    private Vector2 move;
    private bool isFalling;
    private bool isSprinting;

    [SerializeField] private CharacterController characterController;
    private InputController inputController;
    [SerializeField] private Animator animator;

    private float movementSpeed = 2.0f;
    private float sprintSpeed = 5.0f;
    private float currentSpeed;
    private float rotationSpeed = 0.2f;
    private float animationBlendSpeed = 0.2f;
    private float jumpSpeed = 7.0f;
    private float speedY = 0.0f;
    private float gravity = -9.8f;
    
    private float targetAnimationSpeed = 0;
    private float rotationAngle = 0f;

    private void Start()
    {
        spawning = false;
        spawned = false;

        inputController = new();
        inputController.Enable();

        RaycastHit hit;
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, LayerMask.GetMask("Default")))
        {
            animator.Play("Jump");
            isFalling = true;
        }
    }

    private void Update()
    {
        move = new Vector2();

        Fall();

        if (!spawned)
        {
            Spawn();
        }
        else
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Strike"))
            {
                if (characterController.isGrounded)
                {
                    Strike();
                }

                Jump();

                Move();

                Sprint();

                if (!isFalling)
                {
                    Die();
                }

            }

        }

        //                         move character
        Vector3 movement = new(move.x, 0f, move.y);
        Vector3 rotatedMovement = Quaternion.Euler(0f, characterCamera.transform.rotation.eulerAngles.y, 0f) * movement.normalized;
        Vector3 verticalMovement = Vector3.up * speedY;
        characterController.Move((verticalMovement + (rotatedMovement * currentSpeed)) * Time.deltaTime);

        //                       rotate character
        if (rotatedMovement.sqrMagnitude > 0)
        {
            rotationAngle = Mathf.Atan2(rotatedMovement.x, rotatedMovement.z) * Mathf.Rad2Deg;
            targetAnimationSpeed = isSprinting ? 1.0f : 0.5f;
        }
        else
        {
            targetAnimationSpeed = 0.0f;
        }
        Quaternion targetRotation = Quaternion.Euler(0f, rotationAngle, 0f);
        characterController.transform.rotation = Quaternion.Lerp(characterController.transform.rotation, targetRotation, rotationSpeed);

        //                     animate character
        animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), targetAnimationSpeed, animationBlendSpeed));
        animator.SetFloat("SpeedY", speedY / jumpSpeed);
    }

    private void Spawn()
    {
        if (characterController.isGrounded && !spawning)
        {
            animator.SetTrigger("Spawned");
            spawning = true;
        }
        if (spawning && animator.GetCurrentAnimatorStateInfo(0).IsName("Move"))
        {
            spawned = true;
        }
    }

    public void Move()
    {
        move = inputController.Control.Move.ReadValue<Vector2>();
        currentSpeed = movementSpeed;
    }

    public void Sprint()
    {
        if (inputController.Control.Sprint.ReadValue<float>() > 0)
        {
            currentSpeed = sprintSpeed;
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    public void Strike()
    {
        if (inputController.Control.Strike.triggered)
        {
            float strikeNumber = Random.Range(0.0f, 1.0f);
            animator.SetFloat("StrikeNumber", strikeNumber);
            animator.SetTrigger("Strike");
        }
    }

    public void Fall()
    {
        if (characterController.isGrounded)
        {
            speedY = -0.1f;
            return;
        }
        if (!characterController.isGrounded)
        {
            speedY += gravity * Time.deltaTime;

            if (isFalling)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, LayerMask.GetMask("Default")) && speedY < -0.1f)
                {
                    if (spawned)
                    {
                        animator.SetTrigger("Land");
                    }

                    isFalling = false;
                }
            }
        }
    }

    public void Jump()
    {
        if (inputController.Control.Jump.triggered && !isFalling)
        {
            speedY = jumpSpeed;
            animator.ResetTrigger("Land");
            animator.SetTrigger("Jump");
            isFalling = true;
        }
    }

    private void Die()
    {
        if (inputController.Control.Die.triggered)
        {
            animator.SetTrigger("Die");
            inputController.Disable();
        }
    }
}
