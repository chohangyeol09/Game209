using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float mapSize = 100f; // 전체 맵 크기
    [SerializeField] private int totalPhases = 8; // PUBG는 보통 8단계
    [SerializeField] private Transform targetPosition; // 수축 목표 위치

    [Header("Phase Timings (PUBG Style)")]
    [SerializeField] private float[] phaseDelayTimes = { 30f, 20f, 20f, 15f, 15f, 10f, 10f, 5f }; // 대기 시간 (빠른 테스트용)
    [SerializeField] private float[] phaseShrinkTimes = { 20f, 15f, 15f, 10f, 10f, 8f, 8f, 5f }; // 수축 시간 (빠른 테스트용)

    [Header("Damage Settings")]
    [SerializeField] private bool instantKill = true; // 즉사 설정

    [Header("Visual Settings")]
    [SerializeField] private int circleSegments = 128; // 원의 세그먼트 수
    [SerializeField] private Color blueZoneColor = new Color(0f, 0.5f, 1f, 0.8f); // 파란색 자기장
    [SerializeField] private Color safeZoneColor = new Color(1f, 1f, 1f, 0.5f); // 흰색 안전지대
    [SerializeField] private float blueZoneThickness = 3f; // 자기장 두께

    [Header("References")]
    [SerializeField] private LayerMask playerLayer;

    // 자기장 시각 요소
    private LineRenderer blueZoneOutline;
    private LineRenderer safeZoneOutline;
    private LineRenderer nextSafeZoneOutline;

    // 현재 상태
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

    // 이벤트
    public System.Action<int> OnPhaseStart;
    public System.Action<float> OnTimerUpdate;
    public System.Action<Vector2, float> OnNextZoneRevealed;

    void Start()
    {
        // 초기화
        currentBlueZoneCenter = Vector2.zero;
        currentBlueZoneRadius = mapSize * 2f; // 맵 전체를 덮는 크기
        currentSafeZoneCenter = Vector2.zero;
        currentSafeZoneRadius = mapSize;

        // 타겟 위치가 없으면 (0,0)으로 설정
        if (targetPosition == null)
        {
            Debug.LogWarning("Target Position이 설정되지 않았습니다. (0,0)으로 수축합니다.");
        }

        SetupVisuals();
        StartCoroutine(RunBlueZone());
    }

    void SetupVisuals()
    {
        // 파란색 자기장 외곽선
        GameObject blueZoneObj = new GameObject("Blue Zone");
        blueZoneObj.transform.parent = transform;
        blueZoneOutline = blueZoneObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(blueZoneOutline, blueZoneColor, blueZoneThickness, 10);

        // 현재 안전지대 외곽선
        GameObject safeZoneObj = new GameObject("Safe Zone");
        safeZoneObj.transform.parent = transform;
        safeZoneOutline = safeZoneObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(safeZoneOutline, safeZoneColor, 1f, 8);

        // 다음 안전지대 외곽선
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
            Debug.Log($"=== 페이즈 {currentPhase + 1} 시작 ===");
            OnPhaseStart?.Invoke(currentPhase + 1);

            // 다음 안전지대 계산
            CalculateNextSafeZone();
            ShowNextSafeZone();

            // 대기 단계
            isWaitingPhase = true;
            isShrinking = false;
            phaseTimer = phaseDelayTimes[currentPhase];

            Debug.Log($"대기 시간: {phaseTimer}초");

            while (phaseTimer > 0)
            {
                phaseTimer -= Time.deltaTime;
                OnTimerUpdate?.Invoke(phaseTimer);
                yield return null;
            }

            // 자기장 수축 시작
            isWaitingPhase = false;
            isShrinking = true;
            phaseTimer = phaseShrinkTimes[currentPhase];

            Debug.Log($"자기장 수축 시작! 수축 시간: {phaseTimer}초");

            // 다음 안전지대 숨기기
            nextSafeZoneOutline.enabled = false;

            // 수축 애니메이션
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

                // 안전지대와 자기장 동시 수축
                currentSafeZoneCenter = Vector2.Lerp(startSafeCenter, nextSafeZoneCenter, t);
                currentSafeZoneRadius = Mathf.Lerp(startSafeRadius, nextSafeZoneRadius, t);

                // 자기장은 안전지대를 따라감
                currentBlueZoneCenter = currentSafeZoneCenter;
                currentBlueZoneRadius = currentSafeZoneRadius;

                UpdateVisuals();
                CheckPlayersInBlueZone();

                yield return null;
            }

            // 수축 완료
            currentSafeZoneCenter = nextSafeZoneCenter;
            currentSafeZoneRadius = nextSafeZoneRadius;
            currentBlueZoneCenter = nextSafeZoneCenter;
            currentBlueZoneRadius = nextSafeZoneRadius;

            currentPhase++;
            killedPlayers.Clear(); // 새 단계에서는 리스트 초기화

            if (currentPhase >= totalPhases)
            {
                // 최종 단계: 목표 위치로 완전히 수축
                Vector2 finalTarget = targetPosition != null ? (Vector2)targetPosition.position : Vector2.zero;
                currentSafeZoneCenter = finalTarget;
                currentBlueZoneCenter = finalTarget;
                currentSafeZoneRadius = 0;
                currentBlueZoneRadius = 0;
                UpdateVisuals();

                Debug.Log($"최종 자기장 완료! 최종 위치: {finalTarget}");
                break;
            }
        }
    }

    void CalculateNextSafeZone()
    {
        // PUBG 스타일: 각 단계마다 반경이 줄어듦
        float[] radiusMultipliers = { 0.65f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f };
        nextSafeZoneRadius = currentSafeZoneRadius * radiusMultipliers[currentPhase];

        // 목표 위치로 점진적 이동
        Vector2 targetPos = targetPosition != null ? (Vector2)targetPosition.position : Vector2.zero;

        // 각 단계마다 목표 위치로 이동하는 비율 (단계가 진행될수록 목표에 가까워짐)
        float moveRatio = (float)(currentPhase + 1) / totalPhases;
        nextSafeZoneCenter = Vector2.Lerp(currentSafeZoneCenter, targetPos, moveRatio);

        Debug.Log($"다음 안전지대 - 중심: {nextSafeZoneCenter}, 반경: {nextSafeZoneRadius}, 목표: {targetPos}");
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

    // 즉사 처리를 위한 리스트
    private List<GameObject> killedPlayers = new List<GameObject>();

    void CheckPlayersInBlueZone()
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(Vector2.zero, mapSize * 3, playerLayer);

        foreach (Collider2D player in players)
        {
            // 이미 죽은 플레이어는 건너뛰기
            if (killedPlayers.Contains(player.gameObject))
                continue;

            Vector2 playerPos = player.transform.position;
            float distanceFromCenter = Vector2.Distance(playerPos, currentBlueZoneCenter);

            // 플레이어가 자기장 밖에 있는지 확인 (안전지대 밖)
            if (distanceFromCenter > currentBlueZoneRadius)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    if (instantKill)
                    {
                        // 즉사 처리
                        health.InstantKill();
                        killedPlayers.Add(player.gameObject);
                        Debug.Log($"{player.name}이(가) 자기장에 닿아 즉사했습니다!");
                    }
                }
            }
        }
    }

    // 공개 메서드
    public bool IsInSafeZone(Vector2 position)
    {
        return Vector2.Distance(position, currentBlueZoneCenter) <= currentBlueZoneRadius;
    }

    public float GetDistanceFromBlueZone(Vector2 position)
    {
        float distance = Vector2.Distance(position, currentBlueZoneCenter);
        return currentBlueZoneRadius - distance; // 양수면 안전, 음수면 자기장 밖
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