using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class H_Boss : MonoBehaviour
{
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private H_EnemyDataSO BulletSO;
    [SerializeField] private GameObject FirePos;

    private float _coolTime = 2;
    private float _curTime = 0;

    private Action[] AttackPetton;

    private void Start()
    {
        AttackPetton = new Action[]
        {
            LongAttack
        };
    }

    private void Update()
    {
        _curTime += Time.deltaTime;

        if (_coolTime < _curTime)
        {
            int r = Random.Range(0, AttackPetton.Length);
            AttackPetton[r]();
        }
    }


    #region 보스 패턴
    [ContextMenu("Shoot")]
    private IEnumerator Shoot()
    {
        for (int i = 0; i < 10; i++)
        {
            FirePos.transform.DOPunchPosition(new Vector3(0f, 0.2f, 0), 0.3f, 1, 0.8f);

            Spown();

            yield return new WaitForSeconds(0.2f);
        }
        _curTime = 0;

    }

    private void LongAttack()
    {
        StartCoroutine(Shoot());
    }
    #endregion

    private void Spown()
    {

        GameObject enemy = H_PoolManager.Instance.PoolPop(BulletSO);
        

        enemy.transform.position = FirePos.transform.position;

        H_Enemy script = enemy.GetComponent<H_Enemy>();
        
        script.Data = BulletSO;
        script.SetData();
    }
}
