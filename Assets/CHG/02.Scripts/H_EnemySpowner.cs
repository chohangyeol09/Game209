using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class H_EnemySpowner : MonoBehaviour
{

    [SerializeField] private Transform[] SpownPosition;
    [SerializeField] private H_EnemyDataSO[] EnemyData1;
    [SerializeField] private H_EnemyDataSO[] EnemyData2;
    [SerializeField] private H_EnemyDataSO[] EnemyData3;
    private float _timer = 0;
    private float _spownTime = 1f;

    private int SpawnCount = 30;
    private float Range = 5f;
    private float _gameTime;
    private GameObject _enemy;
    private void Awake()
    {
        SpownPosition = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        _gameTime += Time.deltaTime;
        _timer += Time.deltaTime;


        if (_timer > _spownTime)
        {
            _timer = 0;
            Spown();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame) //일정 시간마다
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                float angle = i * Mathf.PI * 2 / SpawnCount;
                float x = Mathf.Cos(angle) * Range;
                float z = Mathf.Sin(angle) * Range;

                float angleDegrees = -angle * Mathf.Rad2Deg;
                GameObject enemy = H_PoolManager.Instance.PoolPop("Enemy");
                enemy.transform.position = new Vector3(x, z, 0);
            }
        }
    }

    private void Spown()
    {

        int r = Random.Range(0, 4);
        switch (r)
        {
            case 0:
            case 1:
            case 2:
                _enemy = H_PoolManager.Instance.PoolPop("Enemy");

                _enemy.transform.position = SpownPosition[Random.Range(0, SpownPosition.Length)].position;
                H_Enemy enemyScript = _enemy.GetComponent<H_Enemy>();
                //enemyScript.Data = EnemyData[r];
                enemyScript.SetData();
                break;
        }
    }
}

