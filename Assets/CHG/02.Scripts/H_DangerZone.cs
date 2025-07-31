using UnityEngine;

public class H_DangerZone : MonoBehaviour
{
    public bool IsCollision = false;

    private SpriteRenderer _spriteRen;

    private void Awake()
    {
        _spriteRen = GetComponent<SpriteRenderer>();
        _spriteRen.color = Color.red;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IsCollision = true;
        _spriteRen.color = Color.red;
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        IsCollision = false;
        _spriteRen.color = Color.green;
    }
}
