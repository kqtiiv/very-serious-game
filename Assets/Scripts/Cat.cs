using System.Collections;
using UnityEngine;

public class Cat : MonoBehaviour
{
    public enum Lane
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }

    [System.Serializable]
    public struct CatAction
    {
        public float time; // Time when the bite should hit
        public Lane lane;
        public bool bite;
    }

    [SerializeField] private CatAction[] timeline;
    [SerializeField] private Transform[] lanePositions = new Transform[3];
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float biteDuration = 0.5f;
    [SerializeField] private float biteAnticipationPercent = 0.8f;
    [SerializeField] private AnimationClip biteClip;
    [SerializeField] private Animator animator;
    [SerializeField] private string moveTrigger = "Move";
    [SerializeField] private string biteTrigger = "Bite";
    [SerializeField] private string idleTrigger = "Idle";
    [Header("Float")]
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatFrequency = 2f;
    [Header("Attack Motion")]
    [SerializeField] private float attackRiseHeight = 0.4f;
    [SerializeField] private float attackDropHeight = -0.25f;
    [Header("Damage")]
    [SerializeField] private GameObject biteHitbox;
    [SerializeField] private float biteActiveDuration = 0.08f;

    private int currentTimelineIndex;
    private Lane currentLane = Lane.Middle;
    private bool isBusy;

    private SpriteRenderer spriteRenderer;

    private Vector3 basePosition;
    private float attackOffsetY;

    public Lane CurrentLane => currentLane;
    public bool IsBusy => isBusy;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        biteHitbox.SetActive(false);
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        MoveToLaneInstant(currentLane);
    }

    private void Update()
    {
        if (currentTimelineIndex >= timeline.Length)
            return;

        if (isBusy)
            return;

        float songTime = GetSongTime();
        CatAction action = timeline[currentTimelineIndex];

        float hitDelay = action.bite ? biteDuration * biteAnticipationPercent : 0f;
        float timeToStart = action.time - moveDuration - hitDelay;

        if (songTime >= timeToStart)
        {
            currentTimelineIndex++;
            StartCoroutine(DoAction(action));
        }
    }

    private void LateUpdate()
    {
        UpdateVisualPosition();
    }

    private float GetSongTime()
    {
        // UPDATE LATER TO GET FROM SONG
        return Time.timeSinceLevelLoad;
    }

    private void UpdateVisualPosition()
    {
        float floatY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        transform.position = basePosition + Vector3.up * (floatY + attackOffsetY);
    }

    private IEnumerator DoAction(CatAction action)
    {
        isBusy = true;

        yield return MoveToLane(action.lane);

        if (action.bite)
            yield return BiteAttack();

        isBusy = false;
    }

    private IEnumerator MoveToLane(Lane lane)
    {
        currentLane = lane;

        Vector3 start = basePosition;
        Vector3 target = lanePositions[(int)lane].position;

        if (spriteRenderer != null)
            spriteRenderer.flipX = (target - start).x > 0f;

        if (animator != null)
            animator.SetTrigger(moveTrigger);

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);

            basePosition = Vector3.Lerp(start, target, t);

            yield return null;
        }

        basePosition = target;

        if (animator != null)
            animator.SetTrigger(idleTrigger);
    }

    private IEnumerator BiteAttack()
    {
        if (animator != null)
        {
            if (biteClip != null)
                animator.speed = biteClip.length / biteDuration;

            animator.SetTrigger(biteTrigger);
        }

        float anticipationTime = biteDuration * biteAnticipationPercent;
        float recoveryTime = biteDuration - anticipationTime;

        float riseTime = anticipationTime * 0.75f;
        float dropTime = anticipationTime * 0.25f;

        yield return LerpAttackOffset(0f, attackRiseHeight, riseTime);
        yield return LerpAttackOffset(attackRiseHeight, attackDropHeight, dropTime);
        yield return LerpAttackOffset(attackDropHeight, 0f, recoveryTime);

        attackOffsetY = 0f;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetTrigger(idleTrigger);
        }
    }

    private IEnumerator LerpAttackOffset(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            attackOffsetY = to;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            attackOffsetY = Mathf.Lerp(from, to, t);

            yield return null;
        }

        attackOffsetY = to;
    }

    private void MoveToLaneInstant(Lane lane)
    {
        currentLane = lane;
        basePosition = lanePositions[(int)lane].position;
    }

    public void OnBite()
    {
        Debug.Log($"Bite hit at: {GetSongTime()} seconds, lane: {currentLane}");
        StartCoroutine(ActivateBiteHitbox());
    }

    private IEnumerator ActivateBiteHitbox()
    {
        if (biteHitbox == null)
            yield break;

        biteHitbox.SetActive(true);
        yield return new WaitForSeconds(biteActiveDuration);
        biteHitbox.SetActive(false);
    }
}