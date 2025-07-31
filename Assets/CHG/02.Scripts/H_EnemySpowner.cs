using UnityEngine;
using Random = UnityEngine.Random;

public class H_EnemySpowner : MonoBehaviour
{
    [SerializeField] private Transform[] SpownPosition;
    [SerializeField] private GameObject EnemyPrefabs;
    [SerializeField] private H_EnemyDataSO[] EnemyData;

    private float _timer = 0;
    private float _spownTime = 1f;
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
        switch (Random.Range(0, 6))
        {
            case 0:
            case 1:
                GameObject enemy = Instantiate(EnemyPrefabs);
                enemy.transform.position = SpownPosition[Random.Range(0, SpownPosition.Length)].position;
                H_Enemy enemyScript = enemy.GetComponent<H_Enemy>();
                enemyScript.Data = EnemyData[Random.Range(0, EnemyData.Length)];
                Debug.Log(enemyScript.Data.name);
                enemyScript.SetData();
                break;
            //case 3:



        }



    }
}
