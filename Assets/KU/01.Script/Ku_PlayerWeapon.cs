using DG.Tweening;
using UnityEngine;

public class Ku_PlayerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject playerWeapon;
    [SerializeField] private float rotationDuration = 0.2f;
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        playerWeapon.SetActive(false);
    }

    private void OnEnable()
    {
        float startZ = playerTransform.eulerAngles.z;
        playerWeapon.transform.rotation = Quaternion.Euler(0, 0, startZ);

        playerWeapon.transform.DOKill();
        playerWeapon.transform
            .DORotate(new Vector3(0, 0, startZ + 360f), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutSine)
            .OnComplete(() => DeleteWeapon());
    }

    private void DeleteWeapon()
    {
        playerWeapon.SetActive(false);
    }
}
