using System.Collections.Generic;
using UnityEngine;

public class H_PoolManager : MonoBehaviour
{
    public Stack<GameObject> EnemyPool = new Stack<GameObject>();
    public Stack<GameObject> ExpPool = new Stack<GameObject>();

    [SerializeField] private GameObject ExpBeadPrefabs;
    [SerializeField] private GameObject EnemyPrefabs;
    public static H_PoolManager Instance { get; private set; }

    private int _Count = 5;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        CreatePool("Enemy");
        CreatePool("Exp");
    }


    public GameObject PoolPop(string s)
    {
        switch (s)
        {
            case "Enemy":
                if (EnemyPool.Count <= 0)
                    CreatePool("Enemy");

                GameObject enemy = EnemyPool.Pop();
                enemy.SetActive(true);
                return enemy;
            case "Exp":
                if (ExpPool.Count <= 0)
                    CreatePool("Exp");

                GameObject Exp = ExpPool.Pop();
                Exp.SetActive(true);
                return Exp;
            default:
                return null;

        }
    }
    private void CreatePool(string s)
    {
        switch (s)
        {
            case "Enemy":
                for (int i = 0; i < _Count; i++)
                {
                    GameObject enemy = Instantiate(EnemyPrefabs);
                    EnemyPool.Push(enemy);
                    enemy.SetActive(false);
                }
                break;
            case "Exp":
                for (int i =0; i < _Count; i++)
                {
                    GameObject exp = Instantiate(ExpBeadPrefabs);
                    ExpPool.Push(exp);
                    exp.SetActive(false);
                }
                break;

        }



    }
}
