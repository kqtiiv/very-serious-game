using System.Collections;
using UnityEngine;

public class Cat : MonoBehaviour
{
    public enum Lane { Left = 0, Middle = 1, Right = 2 }
    public enum ActionType { Move, Bite, Sweep, SetActive }

    public struct CatAction
    {
        public float beat;
        public Lane lane;
        public ActionType type;
        public bool active;
    }

    [SerializeField] private TextAsset chartFile;
    [SerializeField] private Transform[] lanePositions = new Transform[3];
    [SerializeField] private float maxMoveDurationBeats = 0.5f;
    [SerializeField] private float maxBiteDurationBeats = 1f;
    [SerializeField] private float maxSweepDurationBeats = 1f;
    [SerializeField] private float movePortion = 0.3f;
    [SerializeField] private float biteAnticipationPercent = 0.8f;
    [SerializeField] private AnimationClip biteClip;
    [SerializeField] private Animator animator;
    [SerializeField] private string moveTrigger = "Move";
    [SerializeField] private string biteTrigger = "Bite";
    [SerializeField] private string idleTrigger = "Idle";

    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatFrequency = 2f;
    [SerializeField] private float attackDropHeight = -0.25f;
    [SerializeField] private GameObject biteHitbox;
    [SerializeField] private float biteActiveDuration = 0.08f;
    [SerializeField] private float sweepDropHeight = -1f;
    [SerializeField] private float sweepExtraDist = 1f;

    private AudioManager audioManager;
    private CatAction[] timeline;
    private int currentTimelineIndex;
    private Lane currentLane = Lane.Middle;
    private bool isBusy;
    private bool catVisible = true;
    private SpriteRenderer spriteRenderer;
    private Vector3 basePosition;
    private float attackOffsetY;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (biteHitbox != null) biteHitbox.SetActive(false);
    }

    private void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        if (animator == null) animator = GetComponent<Animator>();
        LoadChart();
        MoveToLaneInstant(currentLane);
        // Jump to current beat in timeline
        for (int i = 0; i < timeline.Length; i++)
        {
            if (timeline[i].beat > audioManager.CurrentBeat)
            {
                currentTimelineIndex = i;
                break;
            }
        }
    }

    private void Update()
    {
        if (audioManager == null || timeline == null || currentTimelineIndex >= timeline.Length || isBusy)
            return;

        CatAction action = timeline[currentTimelineIndex];

        if (action.type == ActionType.SetActive)
        {
            if (audioManager.CurrentBeat >= action.beat)
            {
                currentTimelineIndex++;
                SetCatVisible(action.active);
            }
            return;
        }

        float gapBeats = GetGapToNextActionBeats(currentTimelineIndex);
        float totalDurationBeats = GetActionDurationBeats(action, gapBeats);
        float moveDurationBeats = action.type == ActionType.Move ? totalDurationBeats : totalDurationBeats * movePortion;
        float attackDurationBeats = totalDurationBeats - moveDurationBeats;

        float hitDelayBeats = 0f;

        if (action.type == ActionType.Bite)
            hitDelayBeats = attackDurationBeats * biteAnticipationPercent;
        else if (action.type == ActionType.Sweep)
            hitDelayBeats = attackDurationBeats * 0.45f;

        float beatToStart = action.beat - moveDurationBeats - hitDelayBeats;

        if (audioManager.CurrentBeat >= beatToStart)
        {
            currentTimelineIndex++;
            StartCoroutine(DoAction(action, moveDurationBeats, attackDurationBeats));
        }
    }

    private void LateUpdate()
    {
        float floatY = catVisible ? Mathf.Sin(Time.time * floatFrequency) * floatAmplitude : 0f;
        transform.position = basePosition + Vector3.up * (floatY + attackOffsetY);
    }

    private IEnumerator DoAction(CatAction action, float moveDurationBeats, float attackDurationBeats)
    {
        if (!catVisible)
            yield break;

        isBusy = true;
        
        yield return MoveToLane(action.lane, audioManager.BeatToSeconds(moveDurationBeats), action.type == ActionType.Sweep);

        if (action.type == ActionType.Bite)
            yield return BiteAttack(audioManager.BeatToSeconds(attackDurationBeats));
        else if (action.type == ActionType.Sweep)
            yield return SweepAttack(action.lane, audioManager.BeatToSeconds(attackDurationBeats), transform.position);

        isBusy = false;
    }

    private IEnumerator MoveToLane(Lane lane, float duration, bool isSweep = false)
    {
        currentLane = lane;

        Vector3 start = basePosition;
        Vector3 target = lanePositions[(int)lane].position;
        if (isSweep)
        {
            target.x += target.x < 0 ? -sweepExtraDist : sweepExtraDist;
        }

        if (spriteRenderer != null)
            spriteRenderer.flipX = target.x > start.x;

        if (animator != null)
            animator.SetTrigger(moveTrigger);

        yield return LerpBasePosition(start, target, duration);

        if (animator != null)
            animator.SetTrigger(idleTrigger);
    }

    private IEnumerator BiteAttack(float duration)
    {
        if (duration <= 0f)
            yield break;

        if (animator != null)
        {
            if (biteClip != null)
                animator.speed = biteClip.length / duration;

            animator.SetTrigger(biteTrigger);
        }

        float anticipationTime = duration * biteAnticipationPercent;
        float recoveryTime = duration - anticipationTime;

        yield return LerpAttackOffset(0f, attackDropHeight, anticipationTime);
        yield return LerpAttackOffset(attackDropHeight, 0f, recoveryTime);

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetTrigger(idleTrigger);
        }
    }

    private IEnumerator SweepAttack(Lane startLane, float duration, Vector3 start)
    {
        if (duration <= 0f)
            yield break;

        Vector3 end = lanePositions[(int)Lane.Middle].position;
        Vector3 middle = lanePositions[(int)Lane.Middle].position;

        currentLane = startLane;
        basePosition = start;

        if (spriteRenderer != null)
            spriteRenderer.flipX = end.x > start.x;

        float dropDuration = duration * 0.20f;
        float sweepDuration = duration * 0.5f;
        float riseDuration = duration * 0.15f;

        yield return LerpAttackOffset(0f, sweepDropHeight, dropDuration);
        yield return new WaitForSeconds(duration * 0.15f);

        if (biteHitbox != null) biteHitbox.SetActive(true);

        yield return LerpBasePosition(start, end, sweepDuration);

        if (biteHitbox != null) biteHitbox.SetActive(false);

        yield return LerpBaseAndAttackOffset(end, middle, sweepDropHeight, 0f, riseDuration);

        currentLane = Lane.Middle;
    }

    private IEnumerator LerpBasePosition(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            basePosition = to;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Ease(timer / duration);
            basePosition = Vector3.Lerp(from, to, t);
            yield return null;
        }

        basePosition = to;
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
            float t = Ease(timer / duration);
            attackOffsetY = Mathf.Lerp(from, to, t);
            yield return null;
        }

        attackOffsetY = to;
    }

    private IEnumerator LerpBaseAndAttackOffset(Vector3 fromPos, Vector3 toPos, float fromY, float toY, float duration)
    {
        if (duration <= 0f)
        {
            basePosition = toPos;
            attackOffsetY = toY;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Ease(timer / duration);
            basePosition = Vector3.Lerp(fromPos, toPos, t);
            attackOffsetY = Mathf.Lerp(fromY, toY, t);
            yield return null;
        }

        basePosition = toPos;
        attackOffsetY = toY;
    }

    private float GetActionDurationBeats(CatAction action, float gapBeats)
    {
        float maxDuration = action.type switch
        {
            ActionType.Bite => maxMoveDurationBeats + maxBiteDurationBeats,
            ActionType.Sweep => maxMoveDurationBeats + maxSweepDurationBeats,
            _ => maxMoveDurationBeats
        };

        return Mathf.Max(0.01f, Mathf.Min(maxDuration, gapBeats * 0.95f));
    }

    private float GetGapToNextActionBeats(int index)
    {
        for (int i = index + 1; i < timeline.Length; i++)
        {
            if (timeline[i].type != ActionType.SetActive)
                return Mathf.Max(0.01f, timeline[i].beat - timeline[index].beat);
        }

        return maxMoveDurationBeats + Mathf.Max(maxBiteDurationBeats, maxSweepDurationBeats);
    }

    private void LoadChart()
    {
        if (chartFile == null)
        {
            timeline = new CatAction[0];
            return;
        }

        string[] lines = chartFile.text.Split('\n');
        var actions = new System.Collections.Generic.List<CatAction>();

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            string[] parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                continue;

            float beat = float.Parse(parts[0]);

            if (parts[1].ToLower() == "setactive")
            {
                actions.Add(new CatAction
                {
                    beat = beat,
                    type = ActionType.SetActive,
                    active = parts[2].ToLower() == "true"
                });
                continue;
            }

            Lane lane = parts[1].ToUpper() switch
            {
                "L" => Lane.Left,
                "M" => Lane.Middle,
                "R" => Lane.Right,
                _ => Lane.Middle
            };

            ActionType type = parts[2].ToLower() switch
            {
                "bite" => ActionType.Bite,
                "sweep" => ActionType.Sweep,
                _ => ActionType.Move
            };

            actions.Add(new CatAction
            {
                beat = beat,
                lane = lane,
                type = type,
                active = true
            });
        }

        timeline = actions.ToArray();
    }

    private void MoveToLaneInstant(Lane lane)
    {
        currentLane = lane;
        basePosition = lanePositions[(int)lane].position;
    }

    private void SetCatVisible(bool active)
    {
        catVisible = active;

        if (spriteRenderer != null)
            spriteRenderer.enabled = active;

        if (biteHitbox != null)
            biteHitbox.SetActive(false);
    }

    private float Ease(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    public void OnBite()
    {
        Debug.Log($"Bite hit at beat time: {audioManager.CurrentBeat}, lane: {currentLane}");
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