using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class H_EnemySpowner : MonoBehaviour
{
    [SerializeField] private Transform[] SpownPosition;
    [SerializeField] private GameObject EnemyPrefabs;
    [SerializeField] private H_EnemyDataSO[] EnemyData;

    private float _timer = 0;
    private float _spownTime = 1f;

    private int SpawnCount = 10;
    private float Range = 5f;
    private void Awake()
    {
        SpownPosition = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > _spownTime)
        {
            _timer = 0;
            Spown();
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
                GameObject enemy = Instantiate(EnemyPrefabs);
                enemy.transform.position = SpownPosition[Random.Range(0, SpownPosition.Length)].position;
                H_Enemy enemyScript = enemy.GetComponent<H_Enemy>();
                enemyScript.Data = EnemyData[r];
                enemyScript.SetData();
                break;
                

        }

        /*if (Keyboard.current.spaceKey.wasPressedThisFrame) //일정 시간마다
        {
            for (int i )
        }*/

    }
}
