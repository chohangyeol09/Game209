using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SafeZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float initialRadius = 50f;
    [SerializeField] private float finalRadius = 5f;
    [SerializeField] private int totalPhases = 5;

    [Header("Timing Settings")]
    [SerializeField] private float waitTimeBeforeFirstShrink = 30f;
    [SerializeField] private float[] phaseWaitTimes = { 60f, 50f, 40f, 30f, 20f };
    [SerializeField] private float[] phaseShrinkDurations = { 30f, 25f, 20f, 15f, 10f };

    [Header("Visual Effects")]
    [SerializeField] private LineRenderer zoneOutline;
    [SerializeField] private LineRenderer nextZoneOutline;
    [SerializeField] private SpriteRenderer zoneFillSprite; // 원형 스프라이트
    [SerializeField] private Color safeColor = new Color(0, 1, 0, 0.2f);
    [SerializeField] private Color dangerColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private ParticleSystem edgeParticles;

    [Header("Damage Settings")]
    [SerializeField] private float[] phaseDamagePerSecond = { 1f, 2f, 5f, 10f, 20f };
    [SerializeField] private LayerMask playerLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip shrinkingSound;

    // 현재 상태
    private float currentRadius;
    private Vector2 currentCenter;
    private int currentPhase = 0;
    private bool isShrinking = false;
    private bool isWaiting = true;

    // 다음 안전지대 정보
    private float nextRadius;
    private Vector2 nextCenter;

    // 이벤트
    public System.Action<int> OnPhaseChanged;
    public System.Action<float> OnDamageDealt;

    void Start()
    {
        currentRadius = initialRadius;
        currentCenter = transform.position;

        SetupVisuals();
        StartCoroutine(ManageZoneShrinking());
    }

    void SetupVisuals()
    {
        // Zone Outline 설정
        if (zoneOutline == null)
        {
            GameObject outlineObj = new GameObject("Zone Outline");
            outlineObj.transform.parent = transform;
            zoneOutline = outlineObj.AddComponent<LineRenderer>();
        }

        zoneOutline.startWidth = 0.5f;
        zoneOutline.endWidth = 0.5f;
        zoneOutline.loop = true;
        zoneOutline.sortingOrder = 10;

        // Next Zone Outline 설정
        if (nextZoneOutline == null)
        {
            GameObject nextOutlineObj = new GameObject("Next Zone Outline");
            nextOutlineObj.transform.parent = transform;
            nextZoneOutline = nextOutlineObj.AddComponent<LineRenderer>();
        }

        nextZoneOutline.startWidth = 0.3f;
        nextZoneOutline.endWidth = 0.3f;
        nextZoneOutline.loop = true;
        nextZoneOutline.sortingOrder = 9;
        nextZoneOutline.enabled = false;

        // Zone Fill 설정 (원형 스프라이트 필요)
        if (zoneFillSprite == null)
        {
            GameObject fillObj = new GameObject("Zone Fill");
            fillObj.transform.parent = transform;
            zoneFillSprite = fillObj.AddComponent<SpriteRenderer>();
            zoneFillSprite.sprite = CreateCircleSprite();
            zoneFillSprite.sortingOrder = -1;
        }

        // 초기 시각 효과 업데이트
        UpdateZoneVisuals();

        // 파티클 설정
        if (edgeParticles != null)
        {
            var shape = edgeParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = currentRadius;
            shape.radiusThickness = 0.1f;
        }
    }

    Sprite CreateCircleSprite()
    {
        // 간단한 원형 스프라이트 생성 (실제로는 에셋 사용 권장)
        Texture2D texture = new Texture2D(256, 256);
        Vector2 center = new Vector2(128, 128);

        for (int x = 0; x < 256; x++)
        {
            for (int y = 0; y < 256; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 128)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 128);
    }

    IEnumerator ManageZoneShrinking()
    {
        yield return new WaitForSeconds(waitTimeBeforeFirstShrink);

        while (currentPhase < totalPhases)
        {
            // 다음 안전지대 계산 및 표시
            CalculateNextZone();
            ShowNextZone();

            // 대기 시간
            isWaiting = true;
            isShrinking = false;
            yield return new WaitForSeconds(phaseWaitTimes[currentPhase]);

            // 경고 알림
            StartCoroutine(PlayWarningSequence());
            yield return new WaitForSeconds(5f);

            // 축소 시작
            isWaiting = false;
            isShrinking = true;

            if (audioSource != null && shrinkingSound != null)
            {
                audioSource.PlayOneShot(shrinkingSound);
            }

            // 축소 애니메이션
            yield return StartCoroutine(ShrinkToNextZone());

            // 단계 완료
            currentPhase++;
            OnPhaseChanged?.Invoke(currentPhase);

            if (currentPhase >= totalPhases)
            {
                Debug.Log("최종 안전지대에 도달했습니다!");
                break;
            }
        }
    }

    void CalculateNextZone()
    {
        // 다음 반경 계산
        float radiusReduction = (initialRadius - finalRadius) / totalPhases;
        nextRadius = currentRadius - radiusReduction;

        // 다음 중심점 계산 (랜덤 오프셋)
        float maxOffset = (currentRadius - nextRadius) * 0.7f;
        Vector2 randomOffset = Random.insideUnitCircle * maxOffset;
        nextCenter = currentCenter + randomOffset;
    }

    void ShowNextZone()
    {
        if (nextZoneOutline != null)
        {
            nextZoneOutline.enabled = true;
            DrawCircle(nextZoneOutline, nextCenter, nextRadius, 64);

            // 점선 효과를 위한 머티리얼 설정
            nextZoneOutline.material = new Material(Shader.Find("Sprites/Default"));
            nextZoneOutline.material.color = Color.white;
        }
    }

    IEnumerator PlayWarningSequence()
    {
        if (audioSource != null && warningSound != null)
        {
            for (int i = 0; i < 3; i++)
            {
                audioSource.PlayOneShot(warningSound);

                // 색상 변경
                if (zoneOutline != null)
                {
                    zoneOutline.material.color = dangerColor;
                    yield return new WaitForSeconds(0.5f);
                    zoneOutline.material.color = safeColor;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }

    IEnumerator ShrinkToNextZone()
    {
        float elapsedTime = 0f;
        float duration = phaseShrinkDurations[currentPhase];

        float startRadius = currentRadius;
        Vector2 startCenter = currentCenter;

        // 다음 구역 표시 숨기기
        if (nextZoneOutline != null)
        {
            nextZoneOutline.enabled = false;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Ease-in-out 곡선
            t = Mathf.SmoothStep(0, 1, t);

            // 반경과 중심 보간
            currentRadius = Mathf.Lerp(startRadius, nextRadius, t);
            currentCenter = Vector2.Lerp(startCenter, nextCenter, t);

            // 시각 효과 업데이트
            UpdateZoneVisuals();

            // 플레이어 데미지 체크
            CheckPlayersInZone();

            yield return null;
        }

        // 최종 값 설정
        currentRadius = nextRadius;
        currentCenter = nextCenter;
    }

    void UpdateZoneVisuals()
    {
        // 원형 외곽선 그리기
        DrawCircle(zoneOutline, currentCenter, currentRadius, 64);

        // 색상 업데이트
        Color currentColor = isShrinking ? dangerColor : safeColor;
        if (zoneOutline.material != null)
        {
            zoneOutline.material.color = currentColor;
        }

        // Fill 스프라이트 업데이트
        if (zoneFillSprite != null)
        {
            zoneFillSprite.transform.position = currentCenter;
            float scale = currentRadius * 2f / 2f; // 스프라이트 크기에 맞게 조정
            zoneFillSprite.transform.localScale = new Vector3(scale, scale, 1);
            zoneFillSprite.color = currentColor;
        }

        // 파티클 업데이트
        if (edgeParticles != null)
        {
            edgeParticles.transform.position = currentCenter;
            var shape = edgeParticles.shape;
            shape.radius = currentRadius;
        }
    }

    void DrawCircle(LineRenderer lineRenderer, Vector2 center, float radius, int segments)
    {
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2 * Mathf.PI;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    void CheckPlayersInZone()
    {
        Collider2D[] allPlayers = Physics2D.OverlapCircleAll(currentCenter, initialRadius * 2, playerLayer);

        foreach (Collider2D player in allPlayers)
        {
            float distance = Vector2.Distance(player.transform.position, currentCenter);

            if (distance > currentRadius)
            {
                ApplyDamageToPlayer(player.gameObject);
            }
        }
    }

    void ApplyDamageToPlayer(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            float damage = phaseDamagePerSecond[currentPhase] * Time.deltaTime;
            health.TakeDamage(damage, DamageType.Zone);
            OnDamageDealt?.Invoke(damage);
        }
    }

    // 공개 메서드들
    public bool IsPlayerInSafeZone(Vector2 playerPosition)
    {
        float distance = Vector2.Distance(playerPosition, currentCenter);
        return distance <= currentRadius;
    }

    public float GetDistanceFromEdge(Vector2 position)
    {
        float distance = Vector2.Distance(position, currentCenter);
        return currentRadius - distance;
    }

    public ZoneInfo GetCurrentZoneInfo()
    {
        return new ZoneInfo
        {
            center = currentCenter,
            radius = currentRadius,
            phase = currentPhase,
            isShrinking = isShrinking,
            isWaiting = isWaiting,
            damagePerSecond = currentPhase < phaseDamagePerSecond.Length ?
                phaseDamagePerSecond[currentPhase] : 0
        };
    }

    public ZoneInfo GetNextZoneInfo()
    {
        return new ZoneInfo
        {
            center = nextCenter,
            radius = nextRadius,
            phase = currentPhase + 1
        };
    }
}