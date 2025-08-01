using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlueZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float mapSize = 100f; // 전체 맵 크기
    [SerializeField] private int totalPhases = 8; // 변경 가능
    [SerializeField] private Transform targetPosition; // 수축 목표 위치

    [Header("Phase Timings (PUBG Style)")]
    [SerializeField] private float[] phaseDelayTimes = { 0f, 1f, 1f, 1, 1f, 1f, 1f, 1f }; // 대기 시간
    [SerializeField] private float[] phaseShrinkTimes = { 20f, 15f, 15f, 10f, 10f, 8f, 8f, 5f }; // 수축 시간

    [Header("Damage Settings")]
    [SerializeField] private bool instantKill = true; // 즉사 설정

    [Header("Visual Settings")]
    [SerializeField] private int circleSegments = 360;
    [SerializeField] private Color blueZoneColor = new Color(0f, 0.5f, 1f, 0.8f);
    [SerializeField] private Color safeZoneColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float blueZoneThickness = 3f;

    [Header("Blue Zone Fill Settings")]
    [SerializeField] private bool fillBlueZone = true;
    [SerializeField] private Color blueFillColor = new Color(0f, 0.3f, 0.8f, 0.5f);

    [Header("References")]
    [SerializeField] private LayerMask playerLayer;

    // 자기장 시각 요소
    private LineRenderer blueZoneOutline;
    private LineRenderer safeZoneOutline;
    private LineRenderer nextSafeZoneOutline;
    private GameObject blueZoneFill;
    private MeshFilter blueFillMeshFilter;
    private MeshRenderer blueFillMeshRenderer;

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

    // 즉사 처리를 위한 리스트
    private List<GameObject> killedPlayers = new List<GameObject>();

    // 이벤트
    public System.Action<int> OnPhaseStart;
    public System.Action<float> OnTimerUpdate;
    public System.Action<Vector2, float> OnNextZoneRevealed;

    void Start()
    {
        // 초기화
        currentBlueZoneCenter = Vector2.zero;
        currentBlueZoneRadius = mapSize * 2f;
        currentSafeZoneCenter = Vector2.zero;
        currentSafeZoneRadius = mapSize;

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

        // 자기장 채우기 설정
        if (fillBlueZone)
        {
            SetupBlueZoneFill();
        }

        UpdateVisuals();
    }

    void SetupBlueZoneFill()
    {
        blueZoneFill = new GameObject("Blue Zone Fill");
        blueZoneFill.transform.parent = transform;
        blueZoneFill.transform.position = new Vector3(0, 0, 0.1f);

        blueFillMeshFilter = blueZoneFill.AddComponent<MeshFilter>();
        blueFillMeshRenderer = blueZoneFill.AddComponent<MeshRenderer>();

        Material fillMaterial = new Material(Shader.Find("Sprites/Default"));
        fillMaterial.color = blueFillColor;
        blueFillMeshRenderer.material = fillMaterial;
        blueFillMeshRenderer.sortingOrder = 5;
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

                currentSafeZoneCenter = Vector2.Lerp(startSafeCenter, nextSafeZoneCenter, t);
                currentSafeZoneRadius = Mathf.Lerp(startSafeRadius, nextSafeZoneRadius, t);

                currentBlueZoneCenter = currentSafeZoneCenter;
                currentBlueZoneRadius = currentSafeZoneRadius;

                UpdateVisuals();
                CheckPlayersInBlueZone();

                yield return null;
            }

            currentSafeZoneCenter = nextSafeZoneCenter;
            currentSafeZoneRadius = nextSafeZoneRadius;
            currentBlueZoneCenter = nextSafeZoneCenter;
            currentBlueZoneRadius = nextSafeZoneRadius;

            currentPhase++;
            killedPlayers.Clear();

            if (currentPhase >= totalPhases)
            {
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
        float[] radiusMultipliers = { 0.65f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f };
        nextSafeZoneRadius = currentSafeZoneRadius * radiusMultipliers[currentPhase];

        Vector2 targetPos = targetPosition != null ? (Vector2)targetPosition.position : Vector2.zero;

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

        if (fillBlueZone && blueFillMeshFilter != null)
        {
            UpdateBlueZoneFillMesh();
        }
    }

    void UpdateBlueZoneFillMesh()
    {
        Mesh mesh = CreateDonutMesh(currentBlueZoneCenter, currentBlueZoneRadius,
                                   currentSafeZoneCenter, currentSafeZoneRadius);
        blueFillMeshFilter.mesh = mesh;
    }

    Mesh CreateDonutMesh(Vector2 outerCenter, float outerRadius, Vector2 innerCenter, float innerRadius)
    {
        Mesh mesh = new Mesh();

        int segments = circleSegments;
        int vertexCount = (segments + 1) * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(
                outerCenter.x + cos * outerRadius,
                outerCenter.y + sin * outerRadius,
                0
            );

            vertices[i * 2 + 1] = new Vector3(
                innerCenter.x + cos * innerRadius,
                innerCenter.y + sin * innerRadius,
                0
            );

            uvs[i * 2] = new Vector2((float)i / segments, 1);
            uvs[i * 2 + 1] = new Vector2((float)i / segments, 0);
        }

        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 6;
            int vertIndex = i * 2;

            triangles[baseIndex] = vertIndex;
            triangles[baseIndex + 1] = vertIndex + 1;
            triangles[baseIndex + 2] = vertIndex + 2;

            triangles[baseIndex + 3] = vertIndex + 1;
            triangles[baseIndex + 4] = vertIndex + 3;
            triangles[baseIndex + 5] = vertIndex + 2;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
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

    void CheckPlayersInBlueZone()
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(Vector2.zero, mapSize * 3, playerLayer);

        foreach (Collider2D player in players)
        {
            if (killedPlayers.Contains(player.gameObject))
                continue;

            Vector2 playerPos = player.transform.position;
            float distanceFromCenter = Vector2.Distance(playerPos, currentBlueZoneCenter);

            if (distanceFromCenter > currentBlueZoneRadius)
            {
                Iyc_PlayerController playerController = player.GetComponent<Iyc_PlayerController>();
                if (playerController != null)
                {
                    if (instantKill)
                    {
                        playerController.InstantKill();
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
        return currentBlueZoneRadius - distance;
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