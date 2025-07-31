using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueZoneUI : MonoBehaviour
{
    [Header("Zone Info")]
    [SerializeField] private TextMeshPro phaseText;
    [SerializeField] private TextMeshPro timerText;
    [SerializeField] private TextMeshPro zoneStatusText;

    [Header("Minimap")]
    [SerializeField] private RectTransform minimapContainer;
    [SerializeField] private Image minimapBlueZone;
    [SerializeField] private Image minimapSafeZone;
    [SerializeField] private Image minimapNextZone;
    [SerializeField] private RectTransform minimapPlayer;
    [SerializeField] private float minimapScale = 1f;

    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshPro healthText;
    [SerializeField] private Image damageIndicator; // 빨간색 화면 효과

    [Header("References")]
    [SerializeField] private BlueZone blueZone;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;

    void Start()
    {
        if (blueZone != null)
        {
            blueZone.OnPhaseStart += UpdatePhase;
            blueZone.OnTimerUpdate += UpdateTimer;
            blueZone.OnNextZoneRevealed += ShowNextZoneOnMinimap;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealth;
            playerHealth.OnBlueZoneStatusChanged += UpdateBlueZoneStatus;
        }
    }

    void Update()
    {
        if (blueZone == null || player == null) return;

        UpdateMinimap();
        UpdateZoneStatus();
    }

    void UpdatePhase(int phase)
    {
        phaseText.text = $"페이즈 {phase}";
    }

    void UpdateTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";

        BlueZoneInfo info = blueZone.GetCurrentInfo();
        if (info.isWaiting)
        {
            timerText.color = Color.white;
        }
        else if (info.isShrinking)
        {
            timerText.color = Color.red;
        }
    }

    void UpdateZoneStatus()
    {
        bool inSafeZone = blueZone.IsInSafeZone(player.position);

        if (inSafeZone)
        {
            zoneStatusText.text = "안전지대 내";
            zoneStatusText.color = Color.white;
        }
        else
        {
            float distance = blueZone.GetDistanceFromBlueZone(player.position);
            zoneStatusText.text = $"자기장까지: {Mathf.Abs(distance):F1}m";
            zoneStatusText.color = Color.red;
        }

        playerHealth.SetBlueZoneStatus(!inSafeZone);
    }

    void UpdateMinimap()
    {
        BlueZoneInfo info = blueZone.GetCurrentInfo();

        // 자기장 업데이트
        if (minimapBlueZone != null)
        {
            minimapBlueZone.rectTransform.anchoredPosition = info.blueZoneCenter * minimapScale;
            float size = info.blueZoneRadius * 2 * minimapScale;
            minimapBlueZone.rectTransform.sizeDelta = new Vector2(size, size);
        }

        // 현재 안전지대
        if (minimapSafeZone != null)
        {
            minimapSafeZone.rectTransform.anchoredPosition = info.safeZoneCenter * minimapScale;
            float size = info.safeZoneRadius * 2 * minimapScale;
            minimapSafeZone.rectTransform.sizeDelta = new Vector2(size, size);
        }

        // 플레이어 위치
        if (minimapPlayer != null)
        {
            minimapPlayer.anchoredPosition = (Vector2)player.position * minimapScale;
        }
    }

    void ShowNextZoneOnMinimap(Vector2 center, float radius)
    {
        if (minimapNextZone != null)
        {
            minimapNextZone.gameObject.SetActive(true);
            minimapNextZone.rectTransform.anchoredPosition = center * minimapScale;
            float size = radius * 2 * minimapScale;
            minimapNextZone.rectTransform.sizeDelta = new Vector2(size, size);
        }
    }

    void UpdateHealth(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.value = current / max;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    void UpdateBlueZoneStatus(bool inBlueZone)
    {
        if (damageIndicator != null)
        {
            if (inBlueZone)
            {
                // 자기장 안에 있을 때 강한 빨간색 경고
                damageIndicator.color = new Color(1, 0, 0, 0.5f);
                Debug.LogWarning("경고! 자기장에 닿으면 즉사합니다!");
            }
            else
            {
                damageIndicator.color = new Color(1, 0, 0, 0);
            }
        }
    }

}