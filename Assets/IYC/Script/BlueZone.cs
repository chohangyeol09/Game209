using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlueZone : MonoBehaviour
{
    [Header("Zone Mode")]
    [SerializeField] private bool useHexagonMode = false; // false = 원형(PUBG), true = 6각형

    [Header("General Settings")]
    [SerializeField] private float mapSize = 100f;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private bool instantKill = true;
    [SerializeField] private LayerMask playerLayer;

    [Header("Circle Mode Settings 안쓰는거 아는데 버리기 너무 아까웠음")]
    [SerializeField] private int totalPhases = 8;
    [SerializeField] private float[] phaseDelayTimes = { 30f, 20f, 20f, 15f, 15f, 10f, 10f, 5f };
    [SerializeField] private float[] phaseShrinkTimes = { 20f, 15f, 15f, 10f, 10f, 8f, 8f, 5f };

    [Header("Hexagon Mode Settings")]
    [SerializeField] private float hexagonShrinkSpeed = 5f;
    [SerializeField] private float hexagonInterval = 10f;
    [SerializeField] private Color redSegmentColor = Color.red;
    [SerializeField] private ParticleSystem destroyParticles;
    [SerializeField] private AudioClip destroySound;
    [SerializeField] private AudioClip redSegmentSound; // 빨간색 통로 생성 사운드

    [Header("Visual Settings")]
    [SerializeField] private int circleSegments = 128;
    [SerializeField] private Color blueZoneColor = new Color(0f, 0.5f, 1f, 0.8f);
    [SerializeField] private Color safeZoneColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float zoneThickness = 3f;
    [SerializeField] private bool fillZone = true;
    [SerializeField] private Color fillColor = new Color(0f, 0.3f, 0.8f, 0.5f);

    // 공통 변수
    private AudioSource audioSource;
    private List<GameObject> killedPlayers = new List<GameObject>();

    // 원형 모드 변수
    private LineRenderer blueZoneOutline;
    private LineRenderer safeZoneOutline;
    private LineRenderer nextSafeZoneOutline;
    private GameObject blueZoneFill;
    private MeshFilter blueFillMeshFilter;
    private MeshRenderer blueFillMeshRenderer;
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

    // 6각형 모드 변수
    private LineRenderer[] hexagonSegments;
    private int redSegmentIndex = -1;
    private bool hasEnemies = false;
    private bool zoneDestroyed = false;
    private Vector2[] hexagonVertices;
    private Vector2 currentCenter;
    private float currentRadius;

    // 이벤트
    public System.Action<int> OnPhaseStart;
    public System.Action<float> OnTimerUpdate;
    public System.Action<Vector2, float> OnNextZoneRevealed;
    public System.Action OnZoneDestroyed;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (targetPosition == null)
        {
            Debug.LogWarning("Target Position이 설정되지 않았습니다. (0,0)으로 수축합니다.");
        }

        if (useHexagonMode)
        {
            StartHexagonMode();
        }
        else
        {
            StartCircleMode();
        }
    }

    //원형모드
    void StartCircleMode()
    {
        currentBlueZoneCenter = Vector2.zero;
        currentBlueZoneRadius = mapSize * 2f;
        currentSafeZoneCenter = Vector2.zero;
        currentSafeZoneRadius = mapSize;

        SetupCircleVisuals();
        StartCoroutine(RunCircleBlueZone());
    }

    void SetupCircleVisuals()
    {
        GameObject blueZoneObj = new GameObject("Blue Zone");
        blueZoneObj.transform.parent = transform;
        blueZoneOutline = blueZoneObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(blueZoneOutline, blueZoneColor, zoneThickness, 10);

        GameObject safeZoneObj = new GameObject("Safe Zone");
        safeZoneObj.transform.parent = transform;
        safeZoneOutline = safeZoneObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(safeZoneOutline, safeZoneColor, 1f, 8);

        GameObject nextSafeObj = new GameObject("Next Safe Zone");
        nextSafeObj.transform.parent = transform;
        nextSafeZoneOutline = nextSafeObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(nextSafeZoneOutline, safeZoneColor * 0.5f, 0.5f, 7);
        nextSafeZoneOutline.enabled = false;

        if (fillZone)
        {
            SetupCircleFill();
        }

        UpdateCircleVisuals();
    }

    void SetupCircleFill()
    {
        blueZoneFill = new GameObject("Blue Zone Fill");
        blueZoneFill.transform.parent = transform;
        blueZoneFill.transform.position = new Vector3(0, 0, 0.1f);

        blueFillMeshFilter = blueZoneFill.AddComponent<MeshFilter>();
        blueFillMeshRenderer = blueZoneFill.AddComponent<MeshRenderer>();

        Material fillMaterial = new Material(Shader.Find("Sprites/Default"));
        fillMaterial.color = fillColor;
        blueFillMeshRenderer.material = fillMaterial;
        blueFillMeshRenderer.sortingOrder = 5;
    }

    IEnumerator RunCircleBlueZone()
    {
        while (currentPhase < totalPhases)
        {
            Debug.Log($"=== 페이즈 {currentPhase + 1} 시작 ===");
            OnPhaseStart?.Invoke(currentPhase + 1);

            CalculateNextCircleZone();
            ShowNextCircleZone();

            isWaitingPhase = true;
            isShrinking = false;
            phaseTimer = phaseDelayTimes[currentPhase];

            while (phaseTimer > 0)
            {
                phaseTimer -= Time.deltaTime;
                OnTimerUpdate?.Invoke(phaseTimer);
                yield return null;
            }

            isWaitingPhase = false;
            isShrinking = true;
            phaseTimer = phaseShrinkTimes[currentPhase];

            nextSafeZoneOutline.enabled = false;

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

                UpdateCircleVisuals();
                CheckPlayersInCircleZone();

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
                UpdateCircleVisuals();
                break;
            }
        }
    }

    void CalculateNextCircleZone()
    {
        float[] radiusMultipliers = { 0.65f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f };
        nextSafeZoneRadius = currentSafeZoneRadius * radiusMultipliers[currentPhase];

        Vector2 targetPos = targetPosition != null ? (Vector2)targetPosition.position : Vector2.zero;
        float moveRatio = (float)(currentPhase + 1) / totalPhases;
        nextSafeZoneCenter = Vector2.Lerp(currentSafeZoneCenter, targetPos, moveRatio);
    }

    void ShowNextCircleZone()
    {
        nextSafeZoneOutline.enabled = true;
        DrawCircle(nextSafeZoneOutline, nextSafeZoneCenter, nextSafeZoneRadius);
        OnNextZoneRevealed?.Invoke(nextSafeZoneCenter, nextSafeZoneRadius);
    }

    void UpdateCircleVisuals()
    {
        DrawCircle(blueZoneOutline, currentBlueZoneCenter, currentBlueZoneRadius);
        DrawCircle(safeZoneOutline, currentSafeZoneCenter, currentSafeZoneRadius);

        if (fillZone && blueFillMeshFilter != null)
        {
            UpdateCircleFillMesh();
        }
    }

    void UpdateCircleFillMesh()
    {
        Mesh mesh = CreateDonutMesh(currentBlueZoneCenter, currentBlueZoneRadius,
                                   currentSafeZoneCenter, currentSafeZoneRadius);
        blueFillMeshFilter.mesh = mesh;
    }

    void CheckPlayersInCircleZone()
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
                if (playerController != null && instantKill)
                {
                    playerController.InstantKill();
                    killedPlayers.Add(player.gameObject);
                    Debug.Log($"{player.name}이(가) 자기장에 닿아 즉사했습니다!");
                }
            }
        }
    }

    //6각형 모드
    void StartHexagonMode()
    {
        currentCenter = transform.position;
        currentRadius = mapSize;

        SetupHexagonVisuals();
        StartCoroutine(RunHexagonZone());
    }

    void SetupHexagonVisuals()
    {
        hexagonSegments = new LineRenderer[6];

        for (int i = 0; i < 6; i++)
        {
            GameObject segmentObj = new GameObject($"Hexagon Segment {i}");
            segmentObj.transform.parent = transform;

            LineRenderer lr = segmentObj.AddComponent<LineRenderer>();
            lr.startWidth = zoneThickness;
            lr.endWidth = zoneThickness;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder = 10;
            lr.positionCount = 2;

            hexagonSegments[i] = lr;
        }

        if (fillZone)
        {
            SetupHexagonFill();
        }

        UpdateHexagon();
    }

    void SetupHexagonFill()
    {
        blueZoneFill = new GameObject("Hexagon Fill");
        blueZoneFill.transform.parent = transform;
        blueZoneFill.transform.position = new Vector3(0, 0, 0.1f);

        blueFillMeshFilter = blueZoneFill.AddComponent<MeshFilter>();
        blueFillMeshRenderer = blueZoneFill.AddComponent<MeshRenderer>();

        Material fillMaterial = new Material(Shader.Find("Sprites/Default"));
        fillMaterial.color = fillColor;
        blueFillMeshRenderer.material = fillMaterial;
        blueFillMeshRenderer.sortingOrder = 5;
    }

    IEnumerator RunHexagonZone()
    {
        while (true)
        {
            // 새 자기장 생성
            zoneDestroyed = false;
            currentRadius = mapSize;
            currentCenter = transform.position;
            killedPlayers.Clear();

            CheckForEnemies();
            SelectRedSegment();
            ShowZone();
            //UpdateHexagon();

            Debug.Log($"새 자기장 생성! 다음 자기장까지 {hexagonInterval}초");

            // 수축 시작 (별도 코루틴)
            Coroutine shrinkCoroutine = StartCoroutine(ShrinkHexagon());

            // hexagonInterval만큼 대기
            yield return new WaitForSeconds(hexagonInterval);

            // 시간이 되면 현재 자기장 강제 종료
            //StopCoroutine(shrinkCoroutine);

            // 파괴 효과 (선택사항)
            //if (currentRadius > 0 && !zoneDestroyed)
            //{
            //    Debug.Log("시간 초과로 자기장 강제 제거");
            //    yield return StartCoroutine(ForceDestroyEffect());
            //}

            HideZone();
        }
    }

    IEnumerator ForceDestroyEffect()
    {
        // 강제 제거 시 효과
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            for (int i = 0; i < 6; i++)
            {
                Color color = hexagonSegments[i].startColor;
                color.a = alpha;
                hexagonSegments[i].startColor = color;
                hexagonSegments[i].endColor = color;
            }

            yield return null;
        }
    }

    void CheckForEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        hasEnemies = enemies.Length > 0;
        Debug.Log($"적 발견: {enemies.Length}명");
    }

    void SelectRedSegment()
    {
        // 초기 빨간색 통로 설정 (수축 중에는 CheckAndUpdateRedSegment에서 처리)
        if (!hasEnemies)
        {
            redSegmentIndex = Random.Range(0, 6);
            Debug.Log($"빨간색 통로: 선분 {redSegmentIndex}");
        }
        else
        {
            redSegmentIndex = -1;
            Debug.Log("적이 있어서 빨간색 통로 없음");
        }

        UpdateSegmentColors();
    }

    void UpdateSegmentColors()
    {
        for (int i = 0; i < 6; i++)
        {
            if (i == redSegmentIndex)
            {
                hexagonSegments[i].startColor = redSegmentColor;
                hexagonSegments[i].endColor = redSegmentColor;

                // 빨간색 통로 생성 효과 (선택사항)
                StartCoroutine(FlashSegment(i));
            }
            else
            {
                hexagonSegments[i].startColor = blueZoneColor;
                hexagonSegments[i].endColor = blueZoneColor;
            }
        }
    }

    IEnumerator FlashSegment(int segmentIndex)
    {
        // 빨간색 통로가 생길 때 깜빡임 효과
        LineRenderer segment = hexagonSegments[segmentIndex];
        float flashDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.PingPong(elapsed * 10f, 1f);

            Color color = redSegmentColor;
            color.a = alpha;
            segment.startColor = color;
            segment.endColor = color;

            yield return null;
        }

        // 최종 색상으로 설정
        segment.startColor = redSegmentColor;
        segment.endColor = redSegmentColor;
    }

    IEnumerator ShrinkHexagon()
    {
        Vector2 targetPos = targetPosition != null ? (Vector2)targetPosition.position : Vector2.zero;
        float targetRadius = 0f;

        while (currentRadius > targetRadius && !zoneDestroyed)
        {
            currentRadius = Mathf.MoveTowards(currentRadius, targetRadius, hexagonShrinkSpeed * Time.deltaTime);
            currentCenter = Vector2.MoveTowards(currentCenter, targetPos, hexagonShrinkSpeed * Time.deltaTime * 0.5f);

            // 수축 중에도 적 체크 및 빨간색 통로 업데이트
            CheckAndUpdateRedSegment();

            UpdateHexagon();
            CheckPlayersInHexagon();

            yield return null;
        }

        if (zoneDestroyed)
        {
            yield return StartCoroutine(DestroyZoneEffect());
        }
    }

    void CheckAndUpdateRedSegment()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        bool currentHasEnemies = enemies.Length > 0;

        // 적 상태가 변경되었을 때만 업데이트
        if (currentHasEnemies != hasEnemies)
        {
            hasEnemies = currentHasEnemies;

            if (!hasEnemies && redSegmentIndex == -1)
            {
                // 적이 사라졌고 빨간색 통로가 없다면 생성
                redSegmentIndex = Random.Range(0, 6);
                Debug.Log($"적이 사라짐! 빨간색 통로 생성: 선분 {redSegmentIndex}");
                UpdateSegmentColors();

                // 빨간색 통로 생성 사운드 재생
                if (audioSource != null && redSegmentSound != null)
                {
                    audioSource.PlayOneShot(redSegmentSound);
                }
            }
            else if (hasEnemies && redSegmentIndex >= 0)
            {
                // 적이 나타났고 빨간색 통로가 있다면 제거
                redSegmentIndex = -1;
                Debug.Log("적이 나타남! 빨간색 통로 제거");
                UpdateSegmentColors();
            }
        }
    }

    void UpdateHexagon()
    {
        hexagonVertices = CalculateHexagonVertices(currentCenter, currentRadius);

        for (int i = 0; i < 6; i++)
        {
            int nextIndex = (i + 1) % 6;
            hexagonSegments[i].SetPosition(0, hexagonVertices[i]);
            hexagonSegments[i].SetPosition(1, hexagonVertices[nextIndex]);
        }

        if (fillZone && blueFillMeshFilter != null)
        {
            UpdateHexagonFillMesh();
        }
    }

    Vector2[] CalculateHexagonVertices(Vector2 center, float radius)
    {
        Vector2[] vertices = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float angle = (60f * i - 30f) * Mathf.Deg2Rad;
            vertices[i] = center + new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
        }

        return vertices;
    }

    void UpdateHexagonFillMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[7];
        vertices[0] = currentCenter;

        for (int i = 0; i < 6; i++)
        {
            vertices[i + 1] = hexagonVertices[i];
        }

        int[] triangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = (i % 6) + 1 + 1;

            if (i == 5) triangles[triIndex + 2] = 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        blueFillMeshFilter.mesh = mesh;
    }

    void CheckPlayersInHexagon()
    {
        if (zoneDestroyed) return;

        Collider2D[] players = Physics2D.OverlapCircleAll(currentCenter, mapSize * 2, playerLayer);

        foreach (Collider2D player in players)
        {
            if (killedPlayers.Contains(player.gameObject))
                continue;

            Vector2 playerPos = player.transform.position;

            if (!IsPointInHexagon(playerPos))
            {
                if (redSegmentIndex >= 0 && IsPassingThroughRedSegment(playerPos))
                {
                    Debug.Log($"{player.name}이(가) 빨간색 통로로 통과! 자기장 파괴!");
                    zoneDestroyed = true;
                    OnZoneDestroyed?.Invoke();
                    return;
                }

                Iyc_PlayerController playerController = player.GetComponent<Iyc_PlayerController>();
                if (playerController != null && instantKill)
                {
                    playerController.InstantKill();
                    killedPlayers.Add(player.gameObject);
                    Debug.Log($"{player.name}이(가) 자기장에 닿아 즉사했습니다!");
                }
            }
        }
    }

    bool IsPointInHexagon(Vector2 point)
    {
        int crossings = 0;

        for (int i = 0; i < 6; i++)
        {
            Vector2 v1 = hexagonVertices[i];
            Vector2 v2 = hexagonVertices[(i + 1) % 6];

            if (((v1.y <= point.y) && (v2.y > point.y)) ||
                ((v1.y > point.y) && (v2.y <= point.y)))
            {
                float vt = (point.y - v1.y) / (v2.y - v1.y);
                if (point.x < v1.x + vt * (v2.x - v1.x))
                {
                    crossings++;
                }
            }
        }

        return (crossings % 2) == 1;
    }

    bool IsPassingThroughRedSegment(Vector2 playerPos)
    {
        if (redSegmentIndex < 0) return false;

        Vector2 segStart = hexagonVertices[redSegmentIndex];
        Vector2 segEnd = hexagonVertices[(redSegmentIndex + 1) % 6];

        float corridorWidth = 5f;
        float distToSegment = DistancePointToLineSegment(playerPos, segStart, segEnd);

        return distToSegment < corridorWidth;
    }

    float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float len = line.magnitude;
        line.Normalize();

        Vector2 v = point - lineStart;
        float d = Vector2.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);

        Vector2 closest = lineStart + line * d;
        return Vector2.Distance(point, closest);
    }

    IEnumerator DestroyZoneEffect()
    {
        if (audioSource != null && destroySound != null)
        {
            audioSource.PlayOneShot(destroySound);
        }

        if (destroyParticles != null)
        {
            ParticleSystem particles = Instantiate(destroyParticles, currentCenter, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, 5f);
        }

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float alpha = Mathf.Lerp(1f, 0f, t);

            for (int i = 0; i < 6; i++)
            {
                Color color = hexagonSegments[i].startColor;
                color.a = alpha;
                hexagonSegments[i].startColor = color;
                hexagonSegments[i].endColor = color;
            }

            if (blueFillMeshRenderer != null)
            {
                Color fillCol = blueFillMeshRenderer.material.color;
                fillCol.a = fillColor.a * alpha;
                blueFillMeshRenderer.material.color = fillCol;
            }

            currentRadius += Time.deltaTime * 10f;
            UpdateHexagon();

            yield return null;
        }

        HideZone();
    }

    void ResetHexagon()
    {
        currentRadius = mapSize;
        currentCenter = transform.position;
        killedPlayers.Clear();
        zoneDestroyed = false;

        ShowZone();
        UpdateHexagon();

        Debug.Log("새로운 자기장 생성!");
    }

    void HideZone()
    {
        if (useHexagonMode)
        {
            for (int i = 0; i < 6; i++)
            {
                hexagonSegments[i].enabled = false;
            }
        }
        else
        {
            if (blueZoneOutline != null) blueZoneOutline.enabled = false;
            if (safeZoneOutline != null) safeZoneOutline.enabled = false;
        }

        if (blueFillMeshRenderer != null)
        {
            blueFillMeshRenderer.enabled = false;
        }
    }

    void ShowZone()
    {
        if (useHexagonMode)
        {
            for (int i = 0; i < 6; i++)
            {
                hexagonSegments[i].enabled = true;
                Color color = (i == redSegmentIndex) ? redSegmentColor : blueZoneColor;
                hexagonSegments[i].startColor = color;
                hexagonSegments[i].endColor = color;
            }
        }
        else
        {
            if (blueZoneOutline != null) blueZoneOutline.enabled = true;
            if (safeZoneOutline != null) safeZoneOutline.enabled = true;
        }

        if (blueFillMeshRenderer != null)
        {
            blueFillMeshRenderer.enabled = true;
            blueFillMeshRenderer.material.color = fillColor;
        }
    }

    // 공통 유틸리티 메서드
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

    // 공개 메서드
    public bool IsInSafeZone(Vector2 position)
    {
        if (useHexagonMode)
        {
            return IsPointInHexagon(position);
        }
        else
        {
            return Vector2.Distance(position, currentBlueZoneCenter) <= currentBlueZoneRadius;
        }
    }

    public bool IsHexagonMode()
    {
        return useHexagonMode;
    }

    public float GetDistanceFromBlueZone(Vector2 position)
    {
        if (useHexagonMode)
        {
            // 6각형의 경우 간단히 중심으로부터의 거리로 계산
            return currentRadius - Vector2.Distance(position, currentCenter);
        }
        else
        {
            float distance = Vector2.Distance(position, currentBlueZoneCenter);
            return currentBlueZoneRadius - distance;
        }
    }

    public BlueZoneInfo GetCurrentInfo()
    {
        if (useHexagonMode)
        {
            return new BlueZoneInfo
            {
                phase = 1,
                isWaiting = false,
                isShrinking = true,
                timer = 0,
                blueZoneCenter = currentCenter,
                blueZoneRadius = currentRadius,
                safeZoneCenter = currentCenter,
                safeZoneRadius = currentRadius,
                damagePerSecond = instantKill ? 9999f : 0
            };
        }
        else
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
}