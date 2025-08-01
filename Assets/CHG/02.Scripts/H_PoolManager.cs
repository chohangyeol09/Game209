using System.Collections.Generic;
using UnityEngine;

public class H_PoolManager : MonoBehaviour
{
    public static H_PoolManager Instance { get; private set; }
    public int PoolSize = 5;
    public GameObject ExpPrefab;
    public Dictionary<string, Stack<GameObject>> Enemypools = new Dictionary<string, Stack<GameObject>>();
    public Stack<GameObject> expPool = new Stack<GameObject>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }


    public GameObject PoolPop(H_EnemyDataSO data)
    {
        if (data == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(data.Name))
        {
            return null;
        }

        string key = data.Name;

        if (!Enemypools.ContainsKey(key)) //없으면 보충
        {
            Enemypools[key] = new Stack<GameObject>();
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject obj = Instantiate(data.EnemyPrefab);
                obj.SetActive(false);
                Enemypools[key].Push(obj);
            }
        }

        GameObject enemy = Enemypools[key].Count > 0 ? Enemypools[key].Pop() : Instantiate(data.EnemyPrefab);
        enemy.SetActive(true);
        return enemy;
    }

    public void EnemyPush(H_EnemyDataSO data, GameObject obj)
    {
        obj.SetActive(false);
        Enemypools[data.Name].Push(obj);
    }

    public GameObject ExpPop()
    {
        if (expPool.Count <= 0)
        {
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject exp = Instantiate(ExpPrefab);
                exp.SetActive(false);
                expPool.Push(exp);
            }
        }

        GameObject expbead = expPool.Pop();
        expbead.SetActive(true);
        return expbead;
    }

    public void ExpPush(GameObject exp)
    {
        exp.SetActive(false);
        expPool.Push(exp);
    }
}
