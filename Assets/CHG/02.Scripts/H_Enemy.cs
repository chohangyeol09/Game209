using System.Collections;
using UnityEngine;

public class H_Enemy : MonoBehaviour
{
    public H_EnemyDataSO Data;
    private Ku_PlayerMovement _playerScript;

    private Rigidbody2D _rb2;
    private SpriteRenderer _spriteRen;

    private GameObject _target;
    private bool _isLive;
    private bool _canAttack = true;
    private bool _canMove = true;
    private float _timer;

    [Header("stats")]
    public int Health;
    private float _speed;
    private int _damage;
    private GameObject _boss;
    private Vector3 _bossvec;
    private Vector2 _oneDir;

    // 새로운 변수들 (푸시 & 자폭 기능용)
    [Header("Push Enemy Settings (Id: 2)")]
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float pushCooldown = 2f;
    
    [Header("Bomber Enemy Settings (Id: 8)")]
    [SerializeField] private float explosionRange = 3f;
    [SerializeField] private int explosionDamage = 25;
    [SerializeField] private float fuseTime = 1.5f;
    [SerializeField] private float explodeDistance = 2.5f;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private GameObject explosionEffect;

    private bool _isExploding = false;
    private bool _canPush = true;
    private AudioSource _audioSource;

    private void Awake()
    {
        _rb2 = GetComponent<Rigidbody2D>();
        _spriteRen = GetComponent<SpriteRenderer>();
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();
        _boss = GameObject.FindWithTag("Boss");
        _audioSource = GetComponent<AudioSource>();
        
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _canAttack = true;
        _canMove = true;
        _canPush = true;
        _isExploding = false;
    }

    private void Update()
    {
        if (!_isLive) return;

        if (Health <= 0)
        {
            Dead();
            return;
        }

        // 기존 Id 7 로직
        if (Data.Id == 7)
        {
            _timer += Time.deltaTime;
            if (_timer > 15)
            {
                _timer = 0;
                H_PoolManager.Instance.EnemyPush(Data, gameObject);
            }
        }

        // 새로운 자폭 Enemy 로직 (Id: 8)
        if (Data.Id == 8 && !_isExploding)
        {
            CheckForExplosion();
        }
    }

    private void FixedUpdate()
    {
        if (!_isLive || !_canMove || Ku_PlayerUpgradeManager.Instance.isUpgrade)
        {
            _rb2.linearVelocity = Vector2.zero;
            return;
        }

        switch (Data.Id)
        {
            case 1: // 기본 추적 Enemy (그대로 유지)
                Vector3 dir1 = (_target.transform.position - transform.position).normalized;
                transform.position += dir1 * _speed * Time.deltaTime;
                break;

            case 2: // 푸시 Enemy (플레이어를 날림)
                Vector3 dir2 = (_target.transform.position - transform.position).normalized;
                transform.position += dir2 * _speed * Time.deltaTime;
                
                // 근접 시 푸시 공격 체크
                float distanceToPush = Vector2.Distance(transform.position, _target.transform.position);
                if (distanceToPush <= 2f && _canPush)
                {
                    PushPlayer();
                }
                break;

            case 8: // 자폭 Enemy
                if (!_isExploding)
                {
                    Vector3 dir8 = (_target.transform.position - transform.position).normalized;
                    transform.position += dir8 * _speed * Time.deltaTime;
                }
                break;

            case 3:
            case 4:
            case 5:
                _rb2.linearVelocity = _oneDir * _speed * Time.fixedDeltaTime;
                break;

            case 6:
                _rb2.linearVelocity = _bossvec * _speed * Time.deltaTime;
                break;
        }
    }

    // 푸시 Enemy 기능 (Id: 2)
    private void PushPlayer()
    {
        if (!_canPush || _target == null) return;

        _canPush = false;

        // 플레이어에게 데미지
        _playerScript.AttackPlayer(_damage);

        // 플레이어 밀어내기
        Vector2 pushDirection = (_target.transform.position - transform.position).normalized;
        
        // 플레이어 Rigidbody2D가 있다면 물리적 밀어내기
        Rigidbody2D playerRb = _target.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
        }

        Debug.Log($"푸시 Enemy가 플레이어를 밀어냈습니다! 데미지: {_damage}");

        // 쿨다운
        StartCoroutine(PushCooldownCoroutine());
    }

    private IEnumerator PushCooldownCoroutine()
    {
        yield return new WaitForSeconds(pushCooldown);
        _canPush = true;
    }

    // 자폭 Enemy 기능 (Id: 8)
    private void CheckForExplosion()
    {
        if (_target == null || _isExploding) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _target.transform.position);
        
        if (distanceToPlayer <= explodeDistance)
        {
            StartExplosion();
        }
    }

    private void StartExplosion()
    {
        if (_isExploding) return;

        _isExploding = true;
        _canMove = false; // 움직임 정지

        Debug.Log($"자폭 Enemy 폭발 시작! {fuseTime}초 후 폭발...");

        // 깜빡이는 효과
        StartCoroutine(BlinkEffect());

        // 일정 시간 후 폭발
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator BlinkEffect()
    {
        Color originalColor = _spriteRen.color;
        float blinkSpeed = 0.2f;

        while (_isExploding && _isLive)
        {
            _spriteRen.color = Color.red;
            yield return new WaitForSeconds(blinkSpeed);
            _spriteRen.color = originalColor;
            yield return new WaitForSeconds(blinkSpeed);
            blinkSpeed *= 0.9f; // 점점 빨라짐
        }
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(fuseTime);
        
        if (_isLive)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (!_isLive) return;

        Debug.Log("자폭 Enemy 폭발!");

        // 폭발 사운드
        if (_audioSource != null && explosionSound != null)
            _audioSource.PlayOneShot(explosionSound);

        // 폭발 이펙트
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 범위 내 플레이어 데미지 체크
        if (_target != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _target.transform.position);
            if (distanceToPlayer <= explosionRange)
            {
                _playerScript.AttackPlayer(explosionDamage);
                Debug.Log($"폭발 데미지! {explosionDamage} 데미지 입힘!");
            }
        }

        // 자폭으로 사망
        Dead();
    }

    public void SetData()
    {
        _target = GameObject.FindWithTag("Player");
        if(_playerScript != null)
        {
            _playerScript = _target.GetComponent<Ku_PlayerMovement>();
        }

        gameObject.name = Data.Name;
        _spriteRen.sprite = Data.Sprite;
        _speed = Data.Speed;
        _damage = Data.Damage;
        Health = Data.MaxHealth;
        _spriteRen.color = Data.color;

        if (IfBullet())
            _oneDir = (_target.transform.position - transform.position).normalized;

        if (Data.Id == 6)
        {
            _boss = GameObject.FindWithTag("Boss");
            if (_boss != null)
                _bossvec = _boss.transform.up;
        }

        // 새로운 타입들 초기화
        if (Data.Id == 2) // 푸시 Enemy
        {
            _canPush = true;
        }
        
        if (Data.Id == 8) // 자폭 Enemy
        {
            _isExploding = false;
        }

        _isLive = true;
    }

    private void Dead()
    {
        if (H_AudioManager.Instance != null)
            H_AudioManager.Instance.SfxPlay(H_AudioManager.Sfx.EnemyDead);

        if (!IfBullet())
        {
            GameObject expbead = H_PoolManager.Instance.ExpPop();
            expbead.transform.position = transform.position;
            expbead.GetComponent<Ku_ExpTest>().Exp = Data.Exp;
        }

        int heal = Ku_PlayerUpgradeManager.Instance._upgradeType[UpgradeType.Blood] * 2;
        if(_playerScript != null)
        {
            _playerScript.HealPlayer(heal);
        }

        _isLive = false;
        _isExploding = false;
        _rb2.linearVelocity = Vector2.zero;

        H_PoolManager.Instance.EnemyPush(Data, gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isLive) return;

        if (collision.CompareTag("Wall"))
        {
            H_PoolManager.Instance.EnemyPush(Data, gameObject);
        }

        if (collision.CompareTag("Player"))
        {
            // Id 2 (푸시 Enemy)는 여기서 충돌 시 푸시하지 않음 (FixedUpdate에서 처리)
            // Id 8 (자폭 Enemy)는 여기서 충돌 시 즉시 폭발
            if (Data.Id == 8 && !_isExploding)
            {
                StartExplosion();
                return;
            }

            // 기존 로직 (Id 1과 다른 타입들)
            if (_canAttack && gameObject.activeInHierarchy && Data.Id != 2)
            {
                _playerScript.AttackPlayer(_damage);
                _canAttack = false;
                Dead();
            }
        }

        if (collision.gameObject.name == "Visual" && IfBullet())
        {
            Debug.Log("CollisionVirsual");
            _canMove = true;

            Vector2 hitNormal = (transform.position - _boss.transform.position).normalized;

            if (Data.Id == 4 || Data.Id == 5)
                _oneDir = Vector2.Reflect(_oneDir.normalized, hitNormal).normalized;

            if (Data.Id == 6)
                _bossvec = Vector2.Reflect(_oneDir.normalized, hitNormal).normalized;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_isLive) return;

        // 무기와 충돌 시 아무것도 하지 않음
        if (collision.gameObject.CompareTag("Weapon")) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (IfBullet())
                Dead();

            // 자폭 Enemy는 충돌 시 바로 폭발
            if (Data.Id == 8 && !_isExploding)
            {
                StartExplosion();
                return;
            }

            // 푸시 Enemy는 별도 처리 (FixedUpdate에서)
            if (Data.Id == 2)
                return;

            if (_canAttack && gameObject.activeInHierarchy)
            {
                _playerScript.AttackPlayer(_damage);
                _canAttack = false;
                StartCoroutine(CanAttack());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody != null)
            collision.rigidbody.linearVelocity = Vector2.zero;
    }

    private IEnumerator CanAttack()
    {
        yield return new WaitForSeconds(0.5f);
        _canAttack = true;
    }

    private bool IfBullet()
    {
        return Data.Id == 4 || Data.Id == 5 || Data.Id == 6;
    }

    // 블루존에서 호출할 수 있는 메서드들
    public void InstantKill()
    {
        Health = 0;
        Dead();
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Dead();
        }
    }
}