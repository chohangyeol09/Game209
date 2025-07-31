using UnityEngine;

public class H_Enemy : MonoBehaviour
{
    public H_EnemyDataSO Data;

    private Rigidbody2D _rb2;
    private Collider2D _collider;
    private SpriteRenderer _spriteRen;

    private GameObject _target;
    private bool _isLive;
    private float _speed;
    private float _damage;
    private Vector2 _oneDir;
    private void Awake()
    {
        _rb2 = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRen = GetComponent<SpriteRenderer>();
        _target = GameObject.FindWithTag("Player");

    }

    private void FixedUpdate()
    {
        if (!_isLive) return;

        switch (Data.Id)
        {
            case 1:
            case 2:
                Vector2 dirVec = _target.transform.position - gameObject.transform.position;
                Vector2 nextVec = dirVec.normalized * _speed * Time.fixedDeltaTime;
                _rb2.MovePosition(_rb2.position + nextVec);
                _rb2.linearVelocity = Vector2.zero;
                break;
            /*case 3:
                _rb2.linearVelocity*/

        }
    }

    public void SetData()
    {
        gameObject.name = Data.Name;
        _spriteRen.sprite = Data.Sprite;
        _speed = Data.Speed;
        _damage = Data.Damage;

        if (Data.Id == 3)
            _oneDir = _target.transform.position;
        _isLive = true;
    }
}
