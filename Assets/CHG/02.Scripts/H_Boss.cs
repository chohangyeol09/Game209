using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class H_Boss : MonoBehaviour
{
    [SerializeField] private GameObject BulletPrefab;

    [SerializeField] private GameObject FirePos;


    private Stack<GameObject> _bulletpool = new Stack<GameObject>();

    private int _health;
    private int _damage;
    private bool _islive;
    private Transform _target;

    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject bullet = Instantiate(BulletPrefab);
            bullet.SetActive(false);
            _bulletpool.Push(bullet);
        }
    }

    [ContextMenu("Shoot")]
    private IEnumerator Shoot()
    {
        for (int i = 0; i < 10; i++)
        {
            FirePos.transform.DOPunchPosition(new Vector3(0f, 1f, 0), 0.5f, 10, 0.8f);
            //FirePos.transform.DOPunchPosition(new Vector3(0,1,0),0.4f);

            if (_bulletpool.Count <= 0)
                for (int j = 0; j < 5; j++)
                {
                    GameObject obj = Instantiate(BulletPrefab);
                    obj.SetActive(false);
                    _bulletpool.Push(obj);
                }


            GameObject bullet = _bulletpool.Pop();
            bullet.transform.position = FirePos.transform.position;
            bullet.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        }

    }

    [ContextMenu("Test")]
    private void Test()
    {
        StartCoroutine(Shoot());
    }
}
