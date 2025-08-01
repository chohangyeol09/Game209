using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class H_Spowner : MonoBehaviour
{

    [SerializeField] private Transform[] SpownPosition;
    [SerializeField] private H_EnemyDataSO[] AllEnemyData;
    [SerializeField] private H_EnemyDataSO CircleData;
    [SerializeField] private Transform Target; 
    [SerializeField] float _stage1 = 30;
    [SerializeField] float _stage2 = 60;
    [SerializeField] float _stage3 = 90;
    private float _spownTime = 1f;
    


    private float _timer = 0;
    private int SpawnCount = 30;
    private float Range = 5f;
    private float _gameTime;
    private int _curStage = 1;
    private int _prevStage = 1;
    private void Awake()
    {
        SpownPosition = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
        
    }


    private void Update()
    {
        if (Ku_PlayerUpgradeManager.Instance.isUpgrade) return;

        _gameTime += Time.deltaTime;
        _timer += Time.deltaTime;

        // 현재 스테이지 판단
        if (_gameTime < _stage1)
        {
            _curStage = 1;
        }
        else if (_gameTime < _stage2)
        {
            _curStage = 2;
        }
        else if (_gameTime < _stage3)
        {
            _curStage = 3;
        }

        // 스테이지가 바뀌었을 때 한 번만 실행
        if (_curStage != _prevStage)
        {
            CreateCircleEnemy();
            _prevStage = _curStage;
            Debug.Log(_curStage);
        }

        if (_timer > _spownTime)
        {
            _timer = 0;
            Spown();
        }
    }

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
            H_Enemy script = enemy.GetComponent<H_Enemy>();
            script.Data = CircleData;
            script.SetData();
        }
    }

    private void Spown()
    {
        var spawn = AllEnemyData.Where(d => d.SpawnStartTime <= _gameTime && d.SpawnStage == _curStage).ToList();
        if (spawn.Count == 0) return;
        H_EnemyDataSO data = spawn[Random.Range(0, spawn.Count)];
        GameObject enemy = H_PoolManager.Instance.PoolPop(data);
        enemy.transform.position = SpownPosition[Random.Range(0, SpownPosition.Length)].position;
        
        H_Enemy script = enemy.GetComponent<H_Enemy>();
        script.Data = data;
        script.SetData();
        enemy.SetActive(true);
    }
}

