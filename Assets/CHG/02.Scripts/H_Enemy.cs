using System.Collections;
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

    [Header("stets")]
    private int _health;
    private int _maxHealth;
    private float _speed;
    private int _damage;
    private Vector2 _oneDir;
    private Vector2 dirVec;
    private void Awake()
    {
        _rb2 = GetComponent<Rigidbody2D>();
        _spriteRen = GetComponent<SpriteRenderer>();
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();
        SetData();
    }

    private void FixedUpdate()
    {
        if (!_isLive) return;

        switch (Data.Id)
        {
            case 1:
            case 2:
                dirVec = _target.transform.position - gameObject.transform.position;
                _rb2.linearVelocity = dirVec * _speed * Time.fixedDeltaTime;
                break;
            case 3:
            case 4:
                _rb2.linearVelocity = _oneDir * _speed * Time.fixedDeltaTime;
                break;
            

        }
    }

    public void SetData()
    {
        if (Data == null)
        {
            Debug.LogError("Enemy Data is NULL");
            return;
        }
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();


        gameObject.name = Data.Name;
        _spriteRen.sprite = Data.Sprite;
        _speed = Data.Speed;
        _color = _spriteRen.color;
        _maxHealth = Data.MaxHealth;
        _health = _maxHealth;
        _damage = Data.Damage;
        _color = Data.color;
        _spriteRen.color = Data.color;
        if (Data.Id == 3)
            _oneDir = _target.transform.position - transform.position;


        _isLive = true;
    }

    [ContextMenu("dead")]
    private void Dead()
    {
        H_AudioManager.Instance.SfxPlay(H_AudioManager.Sfx.EnemyDead);
        GameObject expbead = H_PoolManager.Instance.ExpPop();
        expbead.transform.position = transform.position;
        expbead.GetComponent<H_Expbead>().Exp = Data.Exp;

        _isLive = false;
        _rb2.linearVelocity = Vector3.zero;

        H_PoolManager.Instance.EnemyPush(Data, gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isLive) return;
        
        if(Data.Id == 4)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _playerScript.nowHp -= _damage;
            }
        }

        if(collision.gameObject.CompareTag("Player"))
        {
            _playerScript.nowHp -= (int)Time.fixedDeltaTime * _damage;
        }
    }
}
