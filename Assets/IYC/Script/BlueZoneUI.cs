using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlueZoneUI : MonoBehaviour
{
    [Header("Zone Info")]
    [SerializeField] private Text phaseText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text zoneStatusText;

    [Header("Minimap")]
    [SerializeField] private RectTransform minimapContainer;
    [SerializeField] private Image minimapBlueZone;
    [SerializeField] private Image minimapSafeZone;
    [SerializeField] private Image minimapNextZone;
    [SerializeField] private RectTransform minimapPlayer;
    [SerializeField] private float minimapScale = 1f;

    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text healthText;
    [SerializeField] private Image damageIndicator;

    [Header("References")]
    [SerializeField] private BlueZone blueZone;
    [SerializeField] private Transform player;
    [SerializeField] private Iyc_PlayerController playerController;

    void Start()
    {
        if (blueZone != null)
        {
            blueZone.OnPhaseStart += UpdatePhase;
            blueZone.OnTimerUpdate += UpdateTimer;
            blueZone.OnNextZoneRevealed += ShowNextZoneOnMinimap;
        }

        if (playerController != null)
        {
            playerController.OnHealthChanged += UpdateHealth;
            playerController.OnBlueZoneStatusChanged += UpdateBlueZoneStatus;
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
        if (phaseText != null)
            phaseText.text = $"페이즈 {phase}";
    }

    void UpdateTimer(float time)
    {
        if (timerText != null)
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
    }

    void UpdateZoneStatus()
    {
        bool inSafeZone = blueZone.IsInSafeZone(player.position);

        if (zoneStatusText != null)
        {
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
        }

        playerController.SetBlueZoneStatus(!inSafeZone);
    }

    void UpdateMinimap()
    {
        BlueZoneInfo info = blueZone.GetCurrentInfo();

        if (minimapBlueZone != null)
        {
            minimapBlueZone.rectTransform.anchoredPosition = info.blueZoneCenter * minimapScale;
            float size = info.blueZoneRadius * 2 * minimapScale;
            minimapBlueZone.rectTransform.sizeDelta = new Vector2(size, size);
        }

        if (minimapSafeZone != null)
        {
            minimapSafeZone.rectTransform.anchoredPosition = info.safeZoneCenter * minimapScale;
            float size = info.safeZoneRadius * 2 * minimapScale;
            minimapSafeZone.rectTransform.sizeDelta = new Vector2(size, size);
        }

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