using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float anger = 0f;
    [SerializeField] private float maxAnger = 10f;
    public Animator animatorExe;

    private Rigidbody2D rb;
    private Animator animator;
    

    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
       
    }

    // Рух
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Атака (кірка)
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animatorExe.SetTrigger("Pickaxe");
        }
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        UpdateAnimator();
        HandleFlip();
    }

    private void UpdateAnimator()
    {
        float speed = moveInput.magnitude;
        animator.SetFloat("Speed", speed);

        float emotion = anger >= maxAnger ? 1f : 0f;
        animator.SetFloat("Emotion", emotion);
    }

    private void HandleFlip()
    {
        if (moveInput.x > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (moveInput.x < -0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    public void AddAnger(float amount)
    {
        anger = Mathf.Clamp(anger + amount, 0f, maxAnger);
    }

    public void CalmDown(float amount)
    {
        anger = Mathf.Clamp(anger - amount, 0f, maxAnger);
    }
    // Викликається через Animation Event


}
