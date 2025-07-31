using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Zone Damage Effects")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color damageColor = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private GameObject zoneDamageEffect; // 2D 파티클 효과
    [SerializeField] private AudioClip zoneDamageSound;

    private AudioSource audioSource;
    private bool isInZone = true;
    private Coroutine damageFlashCoroutine;

    public System.Action<float, float> OnHealthChanged;
    public System.Action<DamageType> OnDeath;
    public System.Action<bool> OnZoneStatusChanged;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if (playerSprite == null)
            playerSprite = GetComponent<SpriteRenderer>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage, DamageType damageType)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (damageType == DamageType.Zone)
        {
            // 구역 데미지 효과
            if (zoneDamageEffect != null && !zoneDamageEffect.activeSelf)
            {
                zoneDamageEffect.SetActive(true);
            }

            if (audioSource != null && zoneDamageSound != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(zoneDamageSound);
            }

            if (isInZone)
            {
                isInZone = false;
                OnZoneStatusChanged?.Invoke(false);
            }

            // 데미지 플래시 효과
            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die(damageType);
        }
    }

    IEnumerator DamageFlash()
    {
        if (playerSprite != null)
        {
            playerSprite.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            playerSprite.color = normalColor;
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die(DamageType cause)
    {
        OnDeath?.Invoke(cause);
        gameObject.SetActive(false);
    }

    public void SetZoneStatus(bool inZone)
    {
        if (isInZone != inZone)
        {
            isInZone = inZone;
            OnZoneStatusChanged?.Invoke(inZone);

            if (inZone && zoneDamageEffect != null)
            {
                zoneDamageEffect.SetActive(false);
            }
        }
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
}