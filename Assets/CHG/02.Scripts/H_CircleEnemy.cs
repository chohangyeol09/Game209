using UnityEngine;

public class H_CircleEnemy : MonoBehaviour
{
    [SerializeField] private H_EnemyDataSO CircleData;
    [SerializeField] private Transform Target;

    private int SpawnCount = 30;
    private float Range = 5f;

    private int _health, _exp;
    private void CreateCircleEnemy()
    {
        for (int i = 0; i < SpawnCount; i++)
        {
            float angle = i * Mathf.PI * 2 / SpawnCount;
            float x = Mathf.Cos(angle) * Range;
            float y = Mathf.Sin(angle) * Range;

            float angleDegrees = -angle * Mathf.Rad2Deg;
            GameObject enemy = H_PoolManager.Instance.PoolPop(CircleData);
            enemy.transform.position = new Vector3(x + Target.transform.position.x, y + Target.transform.position.y, 0);
        }
    }
}
