using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float mapSize = 100f; // ��ü �� ũ��
    [SerializeField] private int totalPhases = 8; // PUBG�� ���� 8�ܰ�
    [SerializeField] private Transform targetPosition; // ���� ��ǥ ��ġ

    [Header("Phase Timings (PUBG Style)")]
    [SerializeField] private float[] phaseDelayTimes = { 30f, 20f, 20f, 15f, 15f, 10f, 10f, 5f }; // ��� �ð� (���� �׽�Ʈ��)
    [SerializeField] private float[] phaseShrinkTimes = { 20f, 15f, 15f, 10f, 10f, 8f, 8f, 5f }; // ���� �ð� (���� �׽�Ʈ��)

    [Header("Damage Settings")]
    [SerializeField] private bool instantKill = true; // ��� ����

    [Header("Visual Settings")]
    [SerializeField] private int circleSegments = 128; // ���� ���׸�Ʈ ��
    [SerializeField] private Color blueZoneColor = new Color(0f, 0.5f, 1f, 0.8f); // �Ķ��� �ڱ���
    [SerializeField] private Color safeZoneColor = new Color(1f, 1f, 1f, 0.5f); // ��� ��������
    [SerializeField] private float blueZoneThickness = 3f; // �ڱ��� �β�

    [Header("References")]
    [SerializeField] private LayerMask playerLayer;

    // �ڱ��� �ð� ���
    private LineRenderer blueZoneOutline;
    private LineRenderer safeZoneOutline;
    private LineRenderer nextSafeZoneOutline;

    // ���� ����
    private Vector2 currentBlueZoneCenter;
    private float currentBlueZoneRadius;
    private Vector2 currentSafeZoneCenter;
    private float currentSafeZoneRadius;
    private Vector2 nextSafeZoneCenter;
    private float nextSafeZoneRadius;

    private int currentPhase = 0;
    private bool isWaitingPhase = true;
    private bool isShrinking = false;
    private float phaseTimer = 0f;

    // �̺�Ʈ
    public System.Action<int> OnPhaseStart;
    public System.Action<float> OnTimerUpdate;
    public System.Action<Vector2, float> OnNextZoneRevealed;

    void Start()
    {
        // �ʱ�ȭ
        currentBlueZoneCenter = Vector2.zero;
        currentBlueZoneRadius = mapSize * 2f; // �� ��ü�� ���� ũ��
        currentSafeZoneCenter = Vector2.zero;
        currentSafeZoneRadius = mapSize;

        // Ÿ�� ��ġ�� ������ (0,0)���� ����
        if (targetPosition == null)
        {
            Debug.LogWarning("Target Position�� �������� �ʾҽ��ϴ�. (0,0)���� �����մϴ�.");
        }

        SetupVisuals();
        StartCoroutine(RunBlueZone());
    }

    void SetupVisuals()
    {
        // �Ķ��� �ڱ��� �ܰ���
        GameObject blueZoneObj = new GameObject("Blue Zone");
        blueZoneObj.transform.parent = transform;
        blueZoneOutline = blueZoneObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(blueZoneOutline, blueZoneColor, blueZoneThickness, 10);

        // ���� �������� �ܰ���
        GameObject safeZoneObj = new GameObject("Safe Zone");
        safeZoneObj.transform.parent = transform;
        safeZoneOutline = safeZoneObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(safeZoneOutline, safeZoneColor, 1f, 8);

        // ���� �������� �ܰ���
        GameObject nextSafeObj = new GameObject("Next Safe Zone");
        nextSafeObj.transform.parent = transform;
        nextSafeZoneOutline = nextSafeObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(nextSafeZoneOutline, safeZoneColor * 0.5f, 0.5f, 7);
        nextSafeZoneOutline.enabled = false;

        UpdateVisuals();
    }

    void ConfigureLineRenderer(LineRenderer lr, Color color, float width, int order)
    {
        lr.startWidth = width;
        lr.endWidth = width;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = order;
        lr.positionCount = circleSegments + 1;
    }

    IEnumerator RunBlueZone()
    {
        while (currentPhase < totalPhases)
        {
            Debug.Log($"=== ������ {currentPhase + 1} ���� ===");
            OnPhaseStart?.Invoke(currentPhase + 1);

            // ���� �������� ���
            CalculateNextSafeZone();
            ShowNextSafeZone();

            // ��� �ܰ�
            isWaitingPhase = true;
            isShrinking = false;
            phaseTimer = phaseDelayTimes[currentPhase];

            Debug.Log($"��� �ð�: {phaseTimer}��");

            while (phaseTimer > 0)
            {
                phaseTimer -= Time.deltaTime;
                OnTimerUpdate?.Invoke(phaseTimer);
                yield return null;
            }

            // �ڱ��� ���� ����
            isWaitingPhase = false;
            isShrinking = true;
            phaseTimer = phaseShrinkTimes[currentPhase];

            Debug.Log($"�ڱ��� ���� ����! ���� �ð�: {phaseTimer}��");

            // ���� �������� �����
            nextSafeZoneOutline.enabled = false;

            // ���� �ִϸ��̼�
            float shrinkDuration = phaseShrinkTimes[currentPhase];
            float elapsedTime = 0f;

            Vector2 startSafeCenter = currentSafeZoneCenter;
            float startSafeRadius = currentSafeZoneRadius;

            while (elapsedTime < shrinkDuration)
            {
                elapsedTime += Time.deltaTime;
                phaseTimer = shrinkDuration - elapsedTime;
                OnTimerUpdate?.Invoke(phaseTimer);

                float t = elapsedTime / shrinkDuration;

                // ��������� �ڱ��� ���� ����
                currentSafeZoneCenter = Vector2.Lerp(startSafeCenter, nextSafeZoneCenter, t);
                currentSafeZoneRadius = Mathf.Lerp(startSafeRadius, nextSafeZoneRadius, t);

                // �ڱ����� �������븦 ����
                currentBlueZoneCenter = currentSafeZoneCenter;
                currentBlueZoneRadius = currentSafeZoneRadius;

                UpdateVisuals();
                CheckPlayersInBlueZone();

                yield return null;
            }

            // ���� �Ϸ�
            currentSafeZoneCenter = nextSafeZoneCenter;
            currentSafeZoneRadius = nextSafeZoneRadius;
            currentBlueZoneCenter = nextSafeZoneCenter;
            currentBlueZoneRadius = nextSafeZoneRadius;

            currentPhase++;
            killedPlayers.Clear(); // �� �ܰ迡���� ����Ʈ �ʱ�ȭ

            if (currentPhase >= totalPhases)
            {
                // ���� �ܰ�: ��ǥ ��ġ�� ������ ����
                Vector2 finalTarget = targetPosition != null ? (Vector2)targetPosition.position : Vector2.zero;
                currentSafeZoneCenter = finalTarget;
                currentBlueZoneCenter = finalTarget;
                currentSafeZoneRadius = 0;
                currentBlueZoneRadius = 0;
                UpdateVisuals();

                Debug.Log($"���� �ڱ��� �Ϸ�! ���� ��ġ: {finalTarget}");
                break;
            }
        }
    }

    void CalculateNextSafeZone()
    {
        // PUBG ��Ÿ��: �� �ܰ踶�� �ݰ��� �پ��
        float[] radiusMultipliers = { 0.65f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f };
        nextSafeZoneRadius = currentSafeZoneRadius * radiusMultipliers[currentPhase];

        // ��ǥ ��ġ�� ������ �̵�
        Vector2 targetPos = targetPosition != null ? (Vector2)targetPosition.position : Vector2.zero;

        // �� �ܰ踶�� ��ǥ ��ġ�� �̵��ϴ� ���� (�ܰ谡 ����ɼ��� ��ǥ�� �������)
        float moveRatio = (float)(currentPhase + 1) / totalPhases;
        nextSafeZoneCenter = Vector2.Lerp(currentSafeZoneCenter, targetPos, moveRatio);

        Debug.Log($"���� �������� - �߽�: {nextSafeZoneCenter}, �ݰ�: {nextSafeZoneRadius}, ��ǥ: {targetPos}");
    }

    void ShowNextSafeZone()
    {
        nextSafeZoneOutline.enabled = true;
        DrawCircle(nextSafeZoneOutline, nextSafeZoneCenter, nextSafeZoneRadius);
        OnNextZoneRevealed?.Invoke(nextSafeZoneCenter, nextSafeZoneRadius);
    }

    void UpdateVisuals()
    {
        DrawCircle(blueZoneOutline, currentBlueZoneCenter, currentBlueZoneRadius);
        DrawCircle(safeZoneOutline, currentSafeZoneCenter, currentSafeZoneRadius);
    }

    void DrawCircle(LineRenderer lr, Vector2 center, float radius)
    {
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = (float)i / circleSegments * 2f * Mathf.PI;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    // ��� ó���� ���� ����Ʈ
    private List<GameObject> killedPlayers = new List<GameObject>();

    void CheckPlayersInBlueZone()
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(Vector2.zero, mapSize * 3, playerLayer);

        foreach (Collider2D player in players)
        {
            // �̹� ���� �÷��̾�� �ǳʶٱ�
            if (killedPlayers.Contains(player.gameObject))
                continue;

            Vector2 playerPos = player.transform.position;
            float distanceFromCenter = Vector2.Distance(playerPos, currentBlueZoneCenter);

            // �÷��̾ �ڱ��� �ۿ� �ִ��� Ȯ�� (�������� ��)
            if (distanceFromCenter > currentBlueZoneRadius)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    if (instantKill)
                    {
                        // ��� ó��
                        health.InstantKill();
                        killedPlayers.Add(player.gameObject);
                        Debug.Log($"{player.name}��(��) �ڱ��忡 ��� ����߽��ϴ�!");
                    }
                }
            }
        }
    }

    // ���� �޼���
    public bool IsInSafeZone(Vector2 position)
    {
        return Vector2.Distance(position, currentBlueZoneCenter) <= currentBlueZoneRadius;
    }

    public float GetDistanceFromBlueZone(Vector2 position)
    {
        float distance = Vector2.Distance(position, currentBlueZoneCenter);
        return currentBlueZoneRadius - distance; // ����� ����, ������ �ڱ��� ��
    }

    public BlueZoneInfo GetCurrentInfo()
    {
        return new BlueZoneInfo
        {
            phase = currentPhase + 1,
            isWaiting = isWaitingPhase,
            isShrinking = isShrinking,
            timer = phaseTimer,
            blueZoneCenter = currentBlueZoneCenter,
            blueZoneRadius = currentBlueZoneRadius,
            safeZoneCenter = currentSafeZoneCenter,
            safeZoneRadius = currentSafeZoneRadius,
            damagePerSecond = instantKill ? 9999f : 0
        };
    }
}