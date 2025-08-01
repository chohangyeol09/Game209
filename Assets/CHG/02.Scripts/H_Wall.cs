using UnityEngine;

public class H_Wall : MonoBehaviour
{
    private H_Enemy _enemyScript;
    private H_EnemyDataSO _data;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            _enemyScript = collision.gameObject.GetComponent<H_Enemy>();
            _data = _enemyScript.Data;

            H_PoolManager.Instance.EnemyPush(_data, collision.gameObject);
        }
    }
}
