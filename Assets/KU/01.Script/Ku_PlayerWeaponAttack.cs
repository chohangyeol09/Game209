using DG.Tweening; // 꼭 필요!
using UnityEngine;

public class Ku_PlayerWeaponAttack : MonoBehaviour
{
    private H_Enemy _enemyScripts;

    private AudioSource playerAttackSound;
    public int damage = 5;
    public float pushDistance = 1.5f;
    [SerializeField] private float pushDuration = 0.2f; // 밀리는 시간

    private void Awake()
    {
        playerAttackSound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerAttackSound.Play();
            Transform enemyTransform = collision.transform;

            Vector2 pushDir = (enemyTransform.position - transform.position).normalized;

            Vector3 targetPosition = enemyTransform.position + (Vector3)(pushDir * pushDistance);

            enemyTransform.DOMove(targetPosition, pushDuration)
                          .SetEase(Ease.OutQuad);

            _enemyScripts = collision.gameObject.GetComponent<H_Enemy>();
            _enemyScripts.Health -= damage;
        }
    }
}
