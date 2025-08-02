using UnityEngine;

public class H_Clener : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
    
        if (collision.CompareTag("Enemy") || collision.name.Contains("BlueZone"))
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnEnable()
    {
        Destroy(gameObject,0.2f);
    }
}
