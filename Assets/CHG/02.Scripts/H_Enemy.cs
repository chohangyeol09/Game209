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

    private void Awake()
    {
        _rb2 = GetComponent<Rigidbody2D>();
        _spriteRen = GetComponent<SpriteRenderer>();
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();
        _boss = GameObject.FindWithTag("Boss");
    }

    private void OnEnable()
    {
        _canAttack = true;
        _canMove = true;
    }

    private void Update()
    {
        if (!_isLive) return;

        if (Health <= 0)
        {
            Dead();
            return;
        }

        if (Data.Id == 7)
        {
            _timer += Time.deltaTime;
            if (_timer > 15)
            {
                _timer = 0;
                H_PoolManager.Instance.EnemyPush(Data, gameObject);
            }
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
            case 1:
            case 2:
                Vector3 dir = (_target.transform.position - transform.position).normalized;
                transform.position += dir * _speed * Time.deltaTime;
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

    public void SetData()
    {
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();

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
        _playerScript.HealPlayer(heal);

        _isLive = false;
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
            if (_canAttack && gameObject.activeInHierarchy)
            {
                _playerScript.AttackPlayer(_damage);
                _canAttack = false;
                Dead();
            }
        }

        if (collision.gameObject.name == "Visual" && IfBullet())
        {
            Debug.Log("CollisionVirsual");
            _canMove = true;  // 이동은 계속되도록 유지

            // 반사 방향 계산
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

        if (collision.gameObject.CompareTag("Player"))
        {
            if (IfBullet())
                Dead();

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
}