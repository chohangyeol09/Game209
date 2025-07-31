using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class H_Boss : MonoBehaviour
{
    [SerializeField] private H_EnemyDataSO BulletSO;
    private H_DangerZone _dangerZone;
    private Ku_PlayerMovement _playerScript;

    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private GameObject FireCanon;
    [SerializeField] private GameObject FirePos;
    [SerializeField] private GameObject DangerZonePrefab;
    private float _coolTime = 5;
    private float _curTime = 0;
    private GameObject _target;
    private Action[] AttackPetton;

    private void Awake()
    {
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();
        _dangerZone = DangerZonePrefab.GetComponent<H_DangerZone>();

    }

    private void Start()
    {
        AttackPetton = new Action[]
        {
            LongAttack,
            DangerZone
        };
    }

    private void Update()
    {
        _curTime += Time.deltaTime;

        if (_coolTime < _curTime)
        {
            int r = Random.Range(0, AttackPetton.Length);
            AttackPetton[r]();
            _curTime = 0;
        }

    }
    private void FixedUpdate()
    {
        Vector3 dir = _target.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }


    #region 보스 패턴
    [ContextMenu("Shoot")]
    private IEnumerator Shoot()
    {
        for (int i = 0; i < 10; i++)
        {
            FireCanon.transform.DOPunchPosition(new Vector3(0f, 0.02f, 0), 0.2f);

            Spown();

            yield return new WaitForSeconds(0.2f);
        }

    }
    private void LongAttack()
    {
        StartCoroutine(Shoot());
    }

    private void DangerZone()
    {
       GameObject danger = Instantiate(DangerZonePrefab,_target.transform);
        StartCoroutine(DangerZoneBoom(danger));

    }
    private IEnumerator DangerZoneBoom(GameObject dangerzone)
    {
        dangerzone.transform.DOScale(1.5f, 1).OnComplete(()=> dangerzone.transform.SetParent(null));

        yield return new WaitForSeconds(1.5f);

        if(_dangerZone.IsCollision)
        {
            _playerScript.nowHp -= 20; //임시
        }

        Destroy(dangerzone);
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
