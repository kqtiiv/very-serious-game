using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Bed Positions")]
    [SerializeField] private Transform[] bedPositions = new Transform[3];

    [Header("Movement")]
    [SerializeField] private int currentPositionIndex = 1;
    [SerializeField] private float rollDuration = 0.25f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string rollLeftTrigger = "RollLeft";
    [SerializeField] private string rollRightTrigger = "RollRight";
    [SerializeField] private AnimationClip rollAnimation;
    [Header("Sound Effects")]
    public AudioClip roll;
    private float rollAnimationLength => rollAnimation != null ? rollAnimation.length : 0f;

    private bool isRolling;
    private float rollTimer;
    private Vector3 rollStartPosition;
    private Vector3 rollTargetPosition;

    public int CurrentPositionIndex => currentPositionIndex;
    public bool IsRolling => isRolling;

    private void Start()
    {
        currentPositionIndex = Mathf.Clamp(currentPositionIndex, 0, bedPositions.Length - 1);
        transform.position = bedPositions[currentPositionIndex].position;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isRolling)
        {
            UpdateRoll();
            return;
        }
    }
    private void OnMove(InputValue val)
    {
        // if (isRolling) return;
        float x = val.Get<Vector2>().x;
        if (x > 0.5f)
            TryRoll(1);
        else if (x < -0.5f)
            TryRoll(-1);
    }

    private void TryRoll(int direction)
    {
        int targetIndex = currentPositionIndex + direction;

        if (targetIndex < 0 || targetIndex >= bedPositions.Length)
            return;

        currentPositionIndex = targetIndex;

        rollStartPosition = transform.position;
        rollTargetPosition = bedPositions[currentPositionIndex].position;
        rollTimer = 0f;
        isRolling = true;
        // AudioManager.Instance.PlaySFX(roll);
        StartRollAnimation(direction);
    }

    private void StartRollAnimation(int direction)
    {
        animator.speed = rollAnimationLength / rollDuration;

        if (direction < 0)
            animator.SetTrigger(rollLeftTrigger);
        else
            animator.SetTrigger(rollRightTrigger);
    }

    private void UpdateRoll()
    {
        rollTimer += Time.deltaTime;

        float t = rollTimer / rollDuration;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(
            rollStartPosition,
            rollTargetPosition,
            t
        );

        if (t >= 1f)
        {
            transform.position = rollTargetPosition;
            isRolling = false;
        }
    }
}