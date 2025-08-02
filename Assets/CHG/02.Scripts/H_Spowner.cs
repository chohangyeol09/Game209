using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class H_Spowner : MonoBehaviour
{
    [SerializeField] private GameObject Cleaner;
    [SerializeField] private GameObject BlueZone;
    [SerializeField] private GameObject Boss;
    [SerializeField] private Transform[] SpownPosition;
    [SerializeField] private H_EnemyDataSO[] AllEnemyData;
    [SerializeField] private H_EnemyDataSO CircleData;
    [SerializeField] private Transform Target;
    [SerializeField] private GameObject Chast;
    [SerializeField] float _stage1 = 30;
    [SerializeField] float _stage2 = 60;
    [SerializeField] float _stage3 = 90;
    [SerializeField] float _bossStage = 120;

    public float _spownTime = 1f;
    private int lastTriggered = -1;
    private float _timer = 0;
    private float _gameTime;

    private int SpawnCount = 30;
    private float Range = 5f;
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

        // ���� �������� �Ǵ�
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
        else if (_gameTime < _bossStage)
        {
            _curStage = 4;
        }

        // ���������� �ٲ���� �� �� ���� ����
        if (_curStage != _prevStage)
        {

            if (_prevStage <3)
            CreateCircleEnemy();
            else if (_prevStage ==3)
            {
                Boss.SetActive(true);
                Cleaner.SetActive(true);
                Destroy(BlueZone);
                Debug.Log(gameObject.activeSelf);
            }
            _prevStage = _curStage;

            Debug.Log("Stage " +_prevStage);

        }

        if (_timer > _spownTime)
        {
            _timer = 0;
            Spown();
        }

        int current = (int)(_gameTime / 45f);
        if (current != lastTriggered)
        {
            lastTriggered = current;
            CreateChast();
        }



    }

    private void CreateChast()
    {
        // 1. ī�޶� ���� 0~1 ������ ����Ʈ ���� ��ǥ
        float randomX = Random.Range(0.1f, 0.9f); // ȭ���� 10%~90% ����
        float randomY = Random.Range(0.1f, 0.9f);

        // 2. ����Ʈ ��ǥ �� ���� ��ǥ
        Vector3 viewportPos = new Vector3(randomX, randomY, Camera.main.nearClipPlane);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        worldPos.z = 0; // 2D �����̶�� Z �� ����

        // 3. ������Ʈ ����
        Instantiate(Chast, worldPos, Quaternion.identity);
    }
    public void CreateCircleEnemy()
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

