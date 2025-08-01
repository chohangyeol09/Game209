using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class H_Boss : MonoBehaviour
{
    [SerializeField] private H_EnemyDataSO BulletSO;
    [SerializeField] private H_EnemyDataSO StraightBull;
    [SerializeField] private H_EnemyDataSO CannonBall;
    private H_DangerZone _dangerZone;
    private Ku_PlayerMovement _playerScript;

    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private GameObject FireCanon;
    [SerializeField] private GameObject FirePos;
    [SerializeField] private GameObject DangerZonePrefab;

    private int Health = 400;
    private float _coolTime = 5;
    private float _curTime = 0;
    private SpriteRenderer _spriteRen;
    private GameObject _target;
    private Action[] AttackPetton;

    private bool _isSpin = false;
    private void Awake()
    {
        _target = GameObject.FindWithTag("Player");
        _playerScript = _target.GetComponent<Ku_PlayerMovement>();
        _dangerZone = DangerZonePrefab.GetComponent<H_DangerZone>();
        _spriteRen = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        AttackPetton = new Action[]
        {
            /*LongAttack,
            DangerZone,
            Cannon,*/
            Spin
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
        if (_isSpin) return;
        Vector3 dir = _target.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }


    #region 보스 패턴
   
    private IEnumerator Shoot(string s)
    {
        float wait = 0.2f;
        float loop = 10;
        if (s == "Spin")
        {
            wait = 0.1f;
            loop = 20;
        }

        for (int i = 0; i < loop; i++)
        {
            FireCanon.transform.DOPunchPosition(new Vector3(0f, 0.02f, 0), 0.1f);

            if (s == "Long")
            {
                Spown(BulletSO);
            }
            else if (s == "Spin")
            {
                Spown(StraightBull);
            }



            yield return new WaitForSeconds(wait);
        }

    }
    private void LongAttack()
    {
        StartCoroutine(Shoot("Long"));
    }

    private void DangerZone()
    {
        GameObject danger = Instantiate(DangerZonePrefab, _target.transform);
        StartCoroutine(DangerZoneBoom(danger));

    }
    private IEnumerator DangerZoneBoom(GameObject dangerzone)
    {
        dangerzone.transform.DOScale(1.5f, 1).OnComplete(() => dangerzone.transform.SetParent(null));

        yield return new WaitForSeconds(1.5f);

        if (_dangerZone.IsCollision)
        {
            _playerScript.nowHp -= 20; //임시
        }

        Destroy(dangerzone);
    }

    private void Cannon()
    {
        //시간이 지날수록 붉어지다가 발사
        _spriteRen.DOColor(Color.red, 2.5f).OnComplete(() =>
        {
            Spown(CannonBall);
            _spriteRen.DOColor(Color.white, 0.5f);
        });
    }

    private void Spin()
    {
        _isSpin = true;

        float startZ = transform.eulerAngles.z;
        float endZ = startZ + 360f;

        StartCoroutine(Shoot("Spin"));
        // 한 바퀴 회전
        transform.DORotate(new Vector3(0, 0, endZ), 1f, RotateMode.FastBeyond360).OnComplete(() =>
        {
            endZ = startZ - 360f;
            transform.DORotate(new Vector3(0, 0, endZ), 1f, RotateMode.FastBeyond360).OnComplete(() =>
            {
                Vector3 dir = _target.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // 플레이어를 바라보는 각도로 0.2초 동안 회전
                transform.DORotate(new Vector3(0, 0, angle - 90f), 0.2f).SetEase(Ease.InOutSine).OnComplete(() => _isSpin = false);
            });
        });
    }

    #endregion

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Bullet"))
        {
            
        }
        else if (collision.transform.CompareTag("Player"))
        {

        }
    }*/

    private void Spown(H_EnemyDataSO data)
    {
        GameObject enemy = H_PoolManager.Instance.PoolPop(data);


        enemy.transform.position = FirePos.transform.position;
        H_Enemy script = enemy.GetComponent<H_Enemy>();

        script.Data = data;

        script.SetData();
    }
}
