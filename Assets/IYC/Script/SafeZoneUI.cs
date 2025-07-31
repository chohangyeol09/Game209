using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SafeZone2DUI : MonoBehaviour
{
    [Header("Zone Info")]
    [SerializeField] private Text phaseText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Image zoneStatusIcon;
    [SerializeField] private Sprite inZoneIcon;
    [SerializeField] private Sprite outZoneIcon;

    [Header("Minimap")]
    [SerializeField] private RectTransform minimapContainer;
    [SerializeField] private RectTransform minimapZone;
    [SerializeField] private RectTransform minimapNextZone;
    [SerializeField] private RectTransform minimapPlayer;
    [SerializeField] private float minimapScale = 2f;

    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text healthText;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Gradient healthGradient;

    [Header("Warnings")]
    [SerializeField] private GameObject zoneWarning;
    [SerializeField] private Text warningText;
    [SerializeField] private CanvasGroup screenEdgeWarning; // ȭ�� �����ڸ� ���� ȿ��

    [Header("References")]
    [SerializeField] private SafeZone zoneController;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;

    private bool showingWarning = false;

    void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealth;
            playerHealth.OnZoneStatusChanged += UpdateZoneStatus;
        }

        if (zoneController != null)
        {
            zoneController.OnPhaseChanged += UpdatePhase;
        }
    }

    void Update()
    {
        if (zoneController == null || player == null) return;

        UpdateZoneInfo();
        UpdateMinimap();
        UpdateScreenEdgeWarning();
    }

    void UpdateZoneInfo()
    {
        ZoneInfo currentZone = zoneController.GetCurrentZoneInfo();

        // �Ÿ� ǥ��
        float distance = zoneController.GetDistanceFromEdge(player.position);
        if (distance > 0)
        {
            distanceText.text = $"�����������: {distance:F1}m";
            distanceText.color = Color.white;
        }
        else
        {
            distanceText.text = $"�������� ��: {Mathf.Abs(distance):F1}m";
            distanceText.color = Color.red;
        }

        // ���� ���� ������
        bool inZone = zoneController.IsPlayerInSafeZone(player.position);
        if (zoneStatusIcon != null)
        {
            zoneStatusIcon.sprite = inZone ? inZoneIcon : outZoneIcon;
        }

        if (playerHealth != null)
        {
            playerHealth.SetZoneStatus(inZone);
        }

        // Ÿ�̸� ������Ʈ
        if (timerText != null)
        {
            if (currentZone.isShrinking)
            {
                timerText.text = "���� ��� ��!";
                timerText.color = Color.red;
            }
            else if (currentZone.isWaiting)
            {
                timerText.text = "���� ��� ��� ��";
                timerText.color = Color.yellow;
            }
        }
    }

    void UpdateMinimap()
    {
        if (minimapZone == null) return;

        ZoneInfo currentZone = zoneController.GetCurrentZoneInfo();
        ZoneInfo nextZone = zoneController.GetNextZoneInfo();

        // ���� ����
        float currentSize = currentZone.radius * 2 * minimapScale;
        minimapZone.sizeDelta = new Vector2(currentSize, currentSize);
        minimapZone.anchoredPosition = WorldToMinimap(currentZone.center);

        // ���� ����
        if (minimapNextZone != null && currentZone.isWaiting)
        {
            minimapNextZone.gameObject.SetActive(true);
            float nextSize = nextZone.radius * 2 * minimapScale;
            minimapNextZone.sizeDelta = new Vector2(nextSize, nextSize);
            minimapNextZone.anchoredPosition = WorldToMinimap(nextZone.center);
        }
        else if (minimapNextZone != null)
        {
            minimapNextZone.gameObject.SetActive(false);
        }

        // �÷��̾� ��ġ
        if (minimapPlayer != null)
        {
            minimapPlayer.anchoredPosition = WorldToMinimap(player.position);
        }
    }

    Vector2 WorldToMinimap(Vector2 worldPos)
    {
        return worldPos * minimapScale;
    }

    void UpdateScreenEdgeWarning()
    {
        if (screenEdgeWarning == null) return;

        bool inZone = zoneController.IsPlayerInSafeZone(player.position);
        if (!inZone)
        {
            // ȭ�� �����ڸ� ������ ȿ��
            float alpha = Mathf.PingPong(Time.time * 2, 0.5f);
            screenEdgeWarning.alpha = alpha;
        }
        else
        {
            screenEdgeWarning.alpha = 0;
        }
    }

    void UpdatePhase(int phase)
    {
        if (phaseText != null)
        {
            phaseText.text = $"�ܰ� {phase}/{5}";
        }
    }

    void UpdateHealth(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.value = current / max;

            if (healthBarFill != null && healthGradient != null)
            {
                healthBarFill.color = healthGradient.Evaluate(current / max);
            }
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }

    void UpdateZoneStatus(bool inZone)
    {
        if (zoneWarning != null)
        {
            if (!inZone && !showingWarning)
            {
                StartCoroutine(ShowZoneWarning());
            }
        }
    }

    IEnumerator ShowZoneWarning()
    {
        showingWarning = true;
        zoneWarning.SetActive(true);
        warningText.text = "�������븦 ������ϴ�!";

        // ������ ȿ��
        for (int i = 0; i < 3; i++)
        {
            warningText.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            warningText.color = Color.white;
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(2f);
        zoneWarning.SetActive(false);
        showingWarning = false;
    }
}