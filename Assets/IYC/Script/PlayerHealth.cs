using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Blue Zone Effects")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private GameObject blueZoneDamageEffect;

    private bool isInBlueZone = false;

    public System.Action<float, float> OnHealthChanged;
    public System.Action<DamageType> OnDeath;
    public System.Action<bool> OnBlueZoneStatusChanged;

    void Start()
    {
        currentHealth = maxHealth;
        if (playerSprite == null)
            playerSprite = GetComponent<SpriteRenderer>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void InstantKill()
    {
        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("���");
        Die(DamageType.BlueZone);
    }

    public void TakeDamage(float damage, DamageType damageType)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (damageType == DamageType.BlueZone)
        {
            Debug.LogWarning("�ڱ����� ����ε� TakeDamage�� ȣ��Ǿ����ϴ�!");
        }

        if (currentHealth <= 0)
        {
            Debug.Log("���");
            Die(damageType);
        }
    }

    public void SetBlueZoneStatus(bool inBlueZone)
    {
        if (isInBlueZone != inBlueZone)
        {
            isInBlueZone = inBlueZone;
            OnBlueZoneStatusChanged?.Invoke(inBlueZone);

            if (!inBlueZone && blueZoneDamageEffect != null)
            {
                blueZoneDamageEffect.SetActive(false);
            }
        }
    }

    void Die(DamageType cause)
    {
        if (cause == DamageType.BlueZone)
        {
            Debug.Log("��� - �ڱ��忡 ���� ���!");
        }
        else
        {
            Debug.Log($"��� - ����: {cause}");
        }

        OnDeath?.Invoke(cause);
        gameObject.SetActive(false);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
}
