using DG.Tweening;
using UnityEngine;

public class Ku_PlayerWeaponRotate : MonoBehaviour
{
    [SerializeField] private GameObject playerWeapon; // 회전할 무기
    [SerializeField] private float rotationDuration = 0.2f; // 회전 시간
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        playerWeapon.SetActive(false);
    }

    private void OnEnable()
    {
        float startZ = playerTransform.eulerAngles.z;
        playerWeapon.transform.rotation = Quaternion.Euler(0, 0, startZ);

        // 360도 한 바퀴 회전
        playerWeapon.transform.DOKill();
        playerWeapon.transform
            .DORotate(new Vector3(0, 0, startZ + 360f), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutSine)
            .OnComplete(() => DeactivateWeapon());
    }

    private void DeactivateWeapon()
    {
        playerWeapon.SetActive(false);
    }
}
