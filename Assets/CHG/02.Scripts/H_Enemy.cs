using UnityEngine;

public class H_Enemy : MonoBehaviour
{
    public H_EnemyDataSO Data;
    [SerializeField] private AudioSource[] AudioSources;

    private Rigidbody2D _rb2;
    private Collider2D _collider;
    private SpriteRenderer _spriteRen;
    


    private GameObject _target;
    private bool _isLive;
    private float _speed;
    private float _damage;
    private Vector2 _oneDir;
    private Vector2 dirVec;
    private void Awake()
    {
        _rb2 = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRen = GetComponent<SpriteRenderer>();
        _target = GameObject.FindWithTag("Player");
        AudioSources = GetComponents<AudioSource>();
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
                _rb2.linearVelocity = _oneDir * _speed * Time.fixedDeltaTime;
                break;
            //case 4:
            

        }
    }

    public void SetData()
    {
        gameObject.name = Data.Name;
        _spriteRen.sprite = Data.Sprite;
        _speed = Data.Speed;
        _damage = Data.Damage;

        if (Data.Id == 3)
            _oneDir = _target.transform.position - transform.position;


        _isLive = true;
    }

    [ContextMenu("dead")]
    private void Dead()
    {
        GameObject expbead = H_PoolManager.Instance.PoolPop("Exp");
        expbead.transform.position = transform.position;
        H_Expbead exp = expbead.GetComponent<H_Expbead>();
        exp.Exp = Data.Exp;
        AudioSources[Random.Range(0, AudioSources.Length)].Play();
            

        //gameObject.SetActive(false);
        H_PoolManager.Instance.EnemyPool.Push(gameObject);
    }
}
