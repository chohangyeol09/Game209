using UnityEngine;

public class H_Expbead : MonoBehaviour
{
    public int Exp;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Exp"))
        {
            collision.gameObject.GetComponent<Ku_PlayerUpgradeManager>().GetExp(Exp);
            H_PoolManager.Instance.ExpPush(gameObject);
        }
    }
}
