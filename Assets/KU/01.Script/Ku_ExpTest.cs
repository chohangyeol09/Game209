using UnityEngine;

public class Ku_ExpTest : MonoBehaviour
{
    public int Exp = 0;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Exp"))
        {
            collision.gameObject.GetComponent<Ku_PlayerUpgradeManager>().GetExp(Exp);
            H_PoolManager.Instance.ExpPush(gameObject);
        }
    }
}
