using DG.Tweening;
using UnityEngine;

public class Ku_PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject playerWeapon;
    public float cooldown = 2f;

    public int nowHp;
    public int maxHp;

    private bool isOnCooldown = false;

    [SerializeField] Ku_PlayerUpgradeManager upgradeManager;

    public float speed = 5f;

    private void Update()
    {
        if(nowHp <= 0)
        {
            Debug.Log("Game Over");
        }
        if (!upgradeManager.isUpgrade)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            Vector2 moveDir = new Vector2(moveX, moveY).normalized;
            transform.position += (Vector3)(moveDir * speed * Time.deltaTime);

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dirToMouse = mouseWorldPos - transform.position;
            float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg - 90f;

            transform.DOKill();
            transform.DORotate(new Vector3(0, 0, angle), 0.1f).SetEase(Ease.OutSine);

            if (Input.GetMouseButtonDown(0) && !isOnCooldown)
            {
                Attack();
            }
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

    public void AttackPlayer(int damage)
    {
        nowHp -= damage;
    }
    public void LowHealthPlayer(int health)
    {
        nowHp -= health;
        maxHp -= health;
    }
    public void HealPlayer(int heal)
    {
        nowHp = Mathf.Min(nowHp + heal, maxHp);
    }
    public void MaxHPPlayer(int health)
    {
        nowHp += health;
        maxHp += health;
    }
}
