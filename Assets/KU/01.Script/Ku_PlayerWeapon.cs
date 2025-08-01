using DG.Tweening;
using UnityEngine;

public class Ku_PlayerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject playerWeapon;
    [SerializeField] private float rotationDuration = 0.2f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float AttackAngle;
    [SerializeField] private float AttackStartAngle;
    private void Start()
    {
        playerWeapon.SetActive(false);
    }


    private void OnEnable()
    {
        playerWeapon.SetActive(true);

        float startZ = playerTransform.eulerAngles.z;

        // ���� ���� ���� ���� (�⺻ ���� ����)
        float weaponStartAngle = startZ - AttackStartAngle;
        float weaponEndAngle = weaponStartAngle + AttackAngle;

        playerWeapon.transform.DOKill();
        playerWeapon.transform.rotation = Quaternion.Euler(0, 0, weaponStartAngle); // ȸ�� �ʱ�ȭ

        playerWeapon.transform
            .DORotate(new Vector3(0, 0, weaponEndAngle), rotationDuration, RotateMode.Fast)
            .SetEase(Ease.OutSine)
            .OnComplete(() => DeleteWeapon());
    }

    /*private void OnEnable()
    {
        float startZ = playerTransform.eulerAngles.z;
        playerWeapon.transform.rotation = Quaternion.Euler(0, 0, startZ - AttackStartAngle);

        //playerWeapon.transform.DOKill();
        //playerWeapon.transform
        //    .DORotate(new Vector3(0, 0, startZ + 360f), rotationDuration, RotateMode.FastBeyond360)
        //    .SetEase(Ease.OutSine)
        //    .OnComplete(() => DeleteWeapon());

        playerWeapon.transform.DOKill();
        playerWeapon.transform
            .DORotate(new Vector3(0, 0, startZ + AttackAngle), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutSine)
            .OnComplete(() => DeleteWeapon());
    }*/

    private void DeleteWeapon()
    {
        playerWeapon.SetActive(false);
    }
}
