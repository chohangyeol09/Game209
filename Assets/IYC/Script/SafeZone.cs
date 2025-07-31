using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// ===== 2D �������� ��Ʈ�ѷ� =====
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
    [SerializeField] private SpriteRenderer zoneFillSprite; // ���� ��������Ʈ
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

    // ���� ����
    private float currentRadius;
    private Vector2 currentCenter;
    private int currentPhase = 0;
    private bool isShrinking = false;
    private bool isWaiting = true;

    // ���� �������� ����
    private float nextRadius;
    private Vector2 nextCenter;

    // �̺�Ʈ
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
        // Zone Outline ���� (������ ��)
        if (zoneOutline == null)
        {
            GameObject outlineObj = new GameObject("Zone Outline");
            outlineObj.transform.parent = transform;
            zoneOutline = outlineObj.AddComponent<LineRenderer>();
        }

        zoneOutline.startWidth = 1f;  // �� �β���
        zoneOutline.endWidth = 1f;
        zoneOutline.loop = true;
        zoneOutline.sortingOrder = 10;

        // LineRenderer Material ����
        zoneOutline.material = new Material(Shader.Find("Sprites/Default"));
        zoneOutline.startColor = Color.red;
        zoneOutline.endColor = Color.red;

        // Next Zone Outline ����
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

        // �ʱ� �ð� ȿ�� ������Ʈ
        UpdateZoneVisuals();

        // ��ƼŬ ����
        if (edgeParticles != null)
        {
            var shape = edgeParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = currentRadius;
            shape.radiusThickness = 0.1f;
        }

        Debug.Log($"�������� �ʱ�ȭ �Ϸ� - �ʱ� �ݰ�: {currentRadius}");
    }

    Sprite CreateCircleSprite()
    {
        // ������ ���� ��������Ʈ ���� (�����δ� ���� ��� ����)
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
            // ���� �������� ��� �� ǥ��
            CalculateNextZone();
            ShowNextZone();

            // ��� �ð�
            isWaiting = true;
            isShrinking = false;
            yield return new WaitForSeconds(phaseWaitTimes[currentPhase]);

            // ��� �˸�
            StartCoroutine(PlayWarningSequence());
            yield return new WaitForSeconds(5f);

            // ��� ����
            isWaiting = false;
            isShrinking = true;

            if (audioSource != null && shrinkingSound != null)
            {
                audioSource.PlayOneShot(shrinkingSound);
            }

            // ��� �ִϸ��̼�
            yield return StartCoroutine(ShrinkToNextZone());

            // �ܰ� �Ϸ�
            currentPhase++;
            OnPhaseChanged?.Invoke(currentPhase);

            if (currentPhase >= totalPhases)
            {
                Debug.Log("���� �������뿡 �����߽��ϴ�!");
                break;
            }
        }
    }

    void CalculateNextZone()
    {
        // ���� �ݰ� ���
        float radiusReduction = (initialRadius - finalRadius) / totalPhases;
        nextRadius = currentRadius - radiusReduction;

        // ���� �߽��� ��� (���� ������)
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

            // ���� ȿ���� ���� ��Ƽ���� ����
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

                // ���� ����
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

        // ���� ���� ǥ�� �����
        if (nextZoneOutline != null)
        {
            nextZoneOutline.enabled = false;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Ease-in-out �
            t = Mathf.SmoothStep(0, 1, t);

            // �ݰ�� �߽� ����
            currentRadius = Mathf.Lerp(startRadius, nextRadius, t);
            currentCenter = Vector2.Lerp(startCenter, nextCenter, t);

            // �ð� ȿ�� ������Ʈ
            UpdateZoneVisuals();

            // �÷��̾� ������ üũ
            CheckPlayersInZone();

            yield return null;
        }

        // ���� �� ����
        currentRadius = nextRadius;
        currentCenter = nextCenter;
    }

    void UpdateZoneVisuals()
    {
        // ���� �ܰ��� �׸��� (������ ��)
        DrawCircle(zoneOutline, currentCenter, currentRadius, 64);

        // �׻� ���������� ����
        if (zoneOutline.material != null)
        {
            zoneOutline.material.color = Color.red;
        }

        // Zone Fill�� �����ϰų� �ɼ����� (������ ����)
        if (zoneFillSprite != null && zoneFillSprite.gameObject.activeSelf)
        {
            zoneFillSprite.transform.position = currentCenter;
            float scale = currentRadius * 2f;
            zoneFillSprite.transform.localScale = new Vector3(scale, scale, 1);
            zoneFillSprite.color = new Color(1, 0, 0, 0.1f); // �ణ ������ ������
        }

        // ��ƼŬ ������Ʈ
        if (edgeParticles != null)
        {
            edgeParticles.transform.position = currentCenter;
            var shape = edgeParticles.shape;
            shape.radius = currentRadius;
        }

        Debug.Log($"Zone ������Ʈ - ���� �ݰ�: {currentRadius:F1}, �߽�: {currentCenter}");
    }

    void DrawCircle(LineRenderer lineRenderer, Vector2 center, float radius, int segments)
    {
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2 * Mathf.PI;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;

            // Z���� �ణ ������ �ؼ� �ٸ� ������Ʈ���� ���� �׷�������
            lineRenderer.SetPosition(i, new Vector3(x, y, -1));
        }

        // LineRenderer ���� Ȯ��
        lineRenderer.enabled = true;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
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

    // ���� �޼����
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