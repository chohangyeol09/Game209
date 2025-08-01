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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerAttackSound.Play();
            Transform enemyTransform = collision.transform;

            // 나에서 적으로 가는 방향 벡터
            Vector2 pushDir = (enemyTransform.position - transform.position).normalized;

            // 최종 목표 위치 계산
            Vector3 targetPosition = enemyTransform.position + (Vector3)(pushDir * pushDistance);

            // DOTween으로 자연스럽게 이동
            enemyTransform.DOMove(targetPosition, pushDuration)
                          .SetEase(Ease.OutQuad); // 밀리듯이 점점 멈추게

            _enemyScripts = collision.gameObject.GetComponent<H_Enemy>();
            _enemyScripts.Health -= damage;
        }
    }
}
