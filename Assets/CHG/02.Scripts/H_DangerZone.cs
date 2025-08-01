using UnityEngine;

public class H_DangerZone : MonoBehaviour
{
    public bool IsCollision = false;

    private SpriteRenderer _spriteRen;
    private Ku_PlayerMovement _playerScript;
    private void Awake()
    {
        _spriteRen = GetComponent<SpriteRenderer>();
        _spriteRen.color = Color.red;
        _playerScript = GetComponentInParent<Ku_PlayerMovement>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IsCollision = true;
            _spriteRen.color = Color.red;
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IsCollision = false;
            _spriteRen.color = Color.green;
        }
    }

    private void OnDisable()
    {
        if (IsCollision)
        {
            _playerScript.AttackPlayer(15);
            Debug.Log("Attack");
        }
    }
}
