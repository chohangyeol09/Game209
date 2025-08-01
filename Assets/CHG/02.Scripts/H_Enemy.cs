using System.Collections;
using System.Threading;
using UnityEngine;

public class H_Enemy : MonoBehaviour
{
    public H_EnemyDataSO Data;
    private Ku_PlayerMovement _playerScript;

    private Rigidbody2D _rb2;
    private SpriteRenderer _spriteRen;
    private Color _color;

    private GameObject _target;
    private bool _isLive;
    private bool _canAttack = true;
    private bool _canMove = true;
    private float _tiemr;

    [Header("stets")]
    public int Health;
    private float _speed;
    private int _damage;
    private GameObject _boss;
    private Vector3 _bossvec;

    private Vector2 _oneDir;
    private Vector2 dirVec;
    private void Awake()
    {
        _rb2 = GetComponent<Rigidbody2D>();
        _spriteRen = GetComponent<SpriteRenderer>();
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();
    }

    private void Update()
    {
        if (Health <= 0)
            Dead();

        if (!_isLive) return;

        if (Data.Id == 7)
        {
            _tiemr += Time.deltaTime;

            if (_tiemr > 15)
            {
                _tiemr = 0;
                H_PoolManager.Instance.EnemyPush(Data, gameObject);
            }
        }

    }

    private void FixedUpdate()
    {
        if (Ku_PlayerUpgradeManager.Instance.isUpgrade) return;

        if (!_isLive) return;

        if (!_canMove) return;

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
        _color = _spriteRen.color;
        _damage = Data.Damage;
        Health = Data.MaxHealth;
        _color = Data.color;
        _spriteRen.color = Data.color;

        if (Data.Id == 3 || Data.Id == 4 || Data.Id == 5)
            _oneDir = (_target.transform.position - transform.position).normalized;

        if (Data.Id == 6)
        {
            _boss = GameObject.FindWithTag("Boss");
            _bossvec = _boss.transform.up;
        }

        if (Data.Id == 7)
        {
            Debug.Log("spown");
        }

        _isLive = true;
    }

    [ContextMenu("dead")]
    private void Dead()
    {
        H_AudioManager.Instance.SfxPlay(H_AudioManager.Sfx.EnemyDead);
        if (Data.Id != 4 && Data.Id != 5 && Data.Id != 6)
        {
            GameObject expbead = H_PoolManager.Instance.ExpPop();
            expbead.transform.position = transform.position;
            expbead.GetComponent<Ku_ExpTest>().Exp = Data.Exp;
        }

        _isLive = false;
        _rb2.linearVelocity = Vector3.zero;

        H_PoolManager.Instance.EnemyPush(Data, gameObject);
    }

    //수정
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);

        if (collision.gameObject.CompareTag("Wall"))
        {
            H_PoolManager.Instance.EnemyPush(Data, gameObject);
        }
    }
    //사항
    private void OnTriggerStay2D(Collider2D collision) //이거 트리거 수정한거
    {
        if (!_isLive) return;


        if (collision.gameObject.CompareTag("Player"))
        {
            if (Data.Id == 4 || Data.Id == 5 || Data.Id == 6)
            {
                Dead();
            }

            if (!_canAttack) return;

            _playerScript.AttackPlayer(_damage);
            _canAttack = false;
            StartCoroutine(CanAttack());
        }

        if (collision.gameObject.layer == 6)
        {
            if (Data.Id == 4 || Data.Id == 5 || Data.Id == 6)
            {
                _canMove = false;
            }
        }
    }

    private IEnumerator CanAttack()
    {
        yield return new WaitForSeconds(0.5f);
        _canAttack = true;
    }
}
