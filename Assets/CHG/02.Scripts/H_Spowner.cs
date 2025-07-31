using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class H_Spowner : MonoBehaviour
{

    [SerializeField] private Transform[] SpownPosition;
    [SerializeField] private H_EnemyDataSO[] AllEnemyData;

    private float _timer = 0;
    private float _spownTime = 1f;
    private int SpawnCount = 30;
    private float Range = 5f;
    private float _gameTime;
    private int curStage = 1;
    private void Awake()
    {
        SpownPosition = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        _gameTime += Time.deltaTime;
        _timer += Time.deltaTime;

        if (_gameTime < 20)
            curStage = 1;
        else if (_gameTime < 40)
            curStage = 2;
        else if (_gameTime < 60)
            curStage = 3;


        if (_timer > _spownTime)
        {
            _timer = 0;
            Spown();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame) //일정 시간마다로 바꾸기
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                float angle = i * Mathf.PI * 2 / SpawnCount;
                float x = Mathf.Cos(angle) * Range;
                float z = Mathf.Sin(angle) * Range;

                float angleDegrees = -angle * Mathf.Rad2Deg;
                //GameObject enemy = H_PoolManager.Instance.PoolPop("Enemy"); //소환 수정
                //enemy.transform.position = new Vector3(x, z, 0);
            }
        }
    }

    private void Spown()
    {
        var spawn = AllEnemyData.Where(d => d.SpawnStartTime <= _gameTime && d.SpawnStage == curStage).ToList();
        if (spawn.Count == 0) return;

        H_EnemyDataSO data = spawn[Random.Range(0, spawn.Count)];
        GameObject enemy = H_PoolManager.Instance.PoolPop(data);
        enemy.transform.position = SpownPosition[Random.Range(0, SpownPosition.Length)].position;

        H_Enemy script = enemy.GetComponent<H_Enemy>();
        script.Data = data;
        script.SetData();
    }
}

