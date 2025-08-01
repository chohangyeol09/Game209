using DG.Tweening;
using UnityEngine;

public class Iyc_PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    [SerializeField] private Ku_PlayerUpgradeManager upgradeManager;

    [Header("Combat Settings")]
    [SerializeField] private GameObject playerWeapon;
    public float cooldown = 2f;
    private bool isOnCooldown = false;

    [Header("Health Settings")]
    public int maxHp = 100;
    public int nowHp;

    [Header("Blue Zone Effects")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private GameObject blueZoneDamageEffect;
    private bool isInBlueZone = false;

    [Header("Audio")]
    private AudioSource playerDieAudio;

    // 이벤트
    public System.Action<float, float> OnHealthChanged;
    public System.Action<DamageType> OnDeath;
    public System.Action<bool> OnBlueZoneStatusChanged;

    private bool isDead = false;

    private void Awake()
    {
        playerDieAudio = GetComponent<AudioSource>();
        if (playerSprite == null)
            playerSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        nowHp = maxHp;
        OnHealthChanged?.Invoke(nowHp, maxHp);
    }

    private void Update()
    {
        if (isDead) return;

        if (nowHp <= 0)
        {
            Die(DamageType.Other);
            return;
        }

        if (!upgradeManager.isUpgrade)
        {
            HandleMovement();
            HandleRotation();
            HandleAttack();
        }
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(moveX, moveY).normalized;
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dirToMouse = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg - 90f;

        transform.DOKill();
        transform.DORotate(new Vector3(0, 0, angle), 0.1f).SetEase(Ease.OutSine);
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isOnCooldown)
        {
            Attack();
        }
    }

    private void Attack()
    {
        isOnCooldown = true;
        playerWeapon.SetActive(true);
        Invoke(nameof(ResetCooldown), cooldown);
    }

    private void ResetCooldown()
    {
        isOnCooldown = false;
    }

    // ===== 체력 관련 메서드 =====

    public void AttackPlayer(int damage)
    {
        TakeDamage(damage, DamageType.Enemy);
    }

    public void TakeDamage(float damage, DamageType damageType)
    {
        if (isDead) return;

        nowHp = Mathf.Max(0, nowHp - (int)damage);
        OnHealthChanged?.Invoke(nowHp, maxHp);

        if (damageType == DamageType.BlueZone)
        {
            Debug.LogWarning("자기장은 즉사인데 TakeDamage가 호출되었습니다!");
        }

        if (nowHp <= 0)
        {
            Die(damageType);
        }
    }

    public void InstantKill()
    {
        if (isDead) return;

        nowHp = 0;
        OnHealthChanged?.Invoke(nowHp, maxHp);
        Debug.Log("자기장에 의한 즉사!");
        Die(DamageType.BlueZone);
    }

    public void LowHealthPlayer(int health)
    {
        nowHp -= health;
        maxHp -= health;

        if (nowHp > maxHp)
            nowHp = maxHp;

        OnHealthChanged?.Invoke(nowHp, maxHp);
    }

    public void HealPlayer(int heal)
    {
        Heal(heal);
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        nowHp = Mathf.Min(nowHp + (int)amount, maxHp);
        OnHealthChanged?.Invoke(nowHp, maxHp);
    }

    public void MaxHPPlayer(int health)
    {
        maxHp += health;
        nowHp += health;
        OnHealthChanged?.Invoke(nowHp, maxHp);
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

    private void Die(DamageType cause)
    {
        if (isDead) return;

        isDead = true;

        if (cause == DamageType.BlueZone)
        {
            Debug.Log("사망 - 자기장에 의한 즉사!");
        }
        else
        {
            Debug.Log($"사망 - 원인: {cause}");
        }

        if (playerDieAudio != null)
            playerDieAudio.Play();

        OnDeath?.Invoke(cause);

        Invoke(nameof(DisablePlayer), 0.5f);
    }

    private void DisablePlayer()
    {
        gameObject.SetActive(false);
    }

    // 유틸리티 메서드
    public float GetHealthPercentage() => (float)nowHp / maxHp;
    public int GetCurrentHealth() => nowHp;
    public int GetMaxHealth() => maxHp;
    public bool IsAlive() => !isDead && nowHp > 0;
}
