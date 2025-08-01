using UnityEngine;

public class H_Chast : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collision");
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log(1);
            int r = Random.Range(0, 3);

            switch (r)
            {
                case 0:
                    for (int i = 0; i < 10; i++)
                    {
                        GameObject Exp = H_PoolManager.Instance.ExpPop();
                        Exp.transform.position = transform.position;
                        Debug.Log(Exp.name);
                        Exp.GetComponent<Ku_ExpTest>().Exp = 4;
                    }
                    break;
                case 1:
                    GameObject.FindWithTag("Player").GetComponent<Ku_PlayerMovement>().HealPlayer(25);
                    break;
                case 2:
                    GameObject spowner = GameObject.Find("Spowner");
                    spowner.GetComponent<H_Spowner>().CreateCircleEnemy();
                    break;
            }
            Destroy(gameObject);
        }
    }
}
