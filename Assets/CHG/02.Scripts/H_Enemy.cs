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

    // ���ο� ������ (Ǫ�� & ���� ��ɿ�)
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

        // ���� Id 7 ����
        if (Data.Id == 7)
        {
            _timer += Time.deltaTime;
            if (_timer > 15)
            {
                _timer = 0;
                H_PoolManager.Instance.EnemyPush(Data, gameObject);
            }
        }

        // ���ο� ���� Enemy ���� (Id: 8)
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
            case 1: // �⺻ ���� Enemy (�״�� ����)
                Vector3 dir1 = (_target.transform.position - transform.position).normalized;
                transform.position += dir1 * _speed * Time.deltaTime;
                break;

            case 2: // Ǫ�� Enemy (�÷��̾ ����)
                Vector3 dir2 = (_target.transform.position - transform.position).normalized;
                transform.position += dir2 * _speed * Time.deltaTime;
                
                // ���� �� Ǫ�� ���� üũ
                float distanceToPush = Vector2.Distance(transform.position, _target.transform.position);
                if (distanceToPush <= 2f && _canPush)
                {
                    PushPlayer();
                }
                break;

            case 8: // ���� Enemy
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

    // Ǫ�� Enemy ��� (Id: 2)
    private void PushPlayer()
    {
        if (!_canPush || _target == null) return;

        _canPush = false;

        // �÷��̾�� ������
        _playerScript.AttackPlayer(_damage);

        // �÷��̾� �о��
        Vector2 pushDirection = (_target.transform.position - transform.position).normalized;
        
        // �÷��̾� Rigidbody2D�� �ִٸ� ������ �о��
        Rigidbody2D playerRb = _target.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
        }

        Debug.Log($"Ǫ�� Enemy�� �÷��̾ �о�½��ϴ�! ������: {_damage}");

        // ��ٿ�
        StartCoroutine(PushCooldownCoroutine());
    }

    private IEnumerator PushCooldownCoroutine()
    {
        yield return new WaitForSeconds(pushCooldown);
        _canPush = true;
    }

    // ���� Enemy ��� (Id: 8)
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
        _canMove = false; // ������ ����

        Debug.Log($"���� Enemy ���� ����! {fuseTime}�� �� ����...");

        // �����̴� ȿ��
        StartCoroutine(BlinkEffect());

        // ���� �ð� �� ����
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
            blinkSpeed *= 0.9f; // ���� ������
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

        Debug.Log("���� Enemy ����!");

        // ���� ����
        if (_audioSource != null && explosionSound != null)
            _audioSource.PlayOneShot(explosionSound);

        // ���� ����Ʈ
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // ���� �� �÷��̾� ������ üũ
        if (_target != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _target.transform.position);
            if (distanceToPlayer <= explosionRange)
            {
                _playerScript.AttackPlayer(explosionDamage);
                Debug.Log($"���� ������! {explosionDamage} ������ ����!");
            }
        }

        // �������� ���
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

        // ���ο� Ÿ�Ե� �ʱ�ȭ
        if (Data.Id == 2) // Ǫ�� Enemy
        {
            _canPush = true;
        }
        
        if (Data.Id == 8) // ���� Enemy
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
            // Id 2 (Ǫ�� Enemy)�� ���⼭ �浹 �� Ǫ������ ���� (FixedUpdate���� ó��)
            // Id 8 (���� Enemy)�� ���⼭ �浹 �� ��� ����
            if (Data.Id == 8 && !_isExploding)
            {
                StartExplosion();
                return;
            }

            // ���� ���� (Id 1�� �ٸ� Ÿ�Ե�)
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

        // ����� �浹 �� �ƹ��͵� ���� ����
        if (collision.gameObject.CompareTag("Weapon")) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (IfBullet())
                Dead();

            // ���� Enemy�� �浹 �� �ٷ� ����
            if (Data.Id == 8 && !_isExploding)
            {
                StartExplosion();
                return;
            }

            // Ǫ�� Enemy�� ���� ó�� (FixedUpdate����)
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

    // ��������� ȣ���� �� �ִ� �޼����
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