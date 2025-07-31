using UnityEngine;

public class Ku_PortionAbb : MonoBehaviour
{
    [SerializeField] private int exp = 2;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Exp"))
        {
            collision.gameObject.GetComponent<Ku_PlayerUpgradeManager>().GetExp(exp);
        }
    }
}
