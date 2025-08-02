using UnityEngine;
using System.Collections;

public class Ku_PlayerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject playerWeapon;
    [SerializeField] private float rotationDuration = 0.2f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float AttackAngle = 360f;
    [SerializeField] private float AttackStartAngle = 90f;

    private void Start()
    {
        playerWeapon.SetActive(false);
    }

    private void OnEnable()
    {
        playerWeapon.SetActive(true);
        StartCoroutine(RotateWeapon());
    }

    private IEnumerator RotateWeapon()
    {
        float elapsed = 0f;
        float startAngle = playerTransform.eulerAngles.z - AttackStartAngle;
        float endAngle = startAngle + AttackAngle;

        while (elapsed < rotationDuration)
        {
            float t = elapsed / rotationDuration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            playerWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerWeapon.transform.rotation = Quaternion.Euler(0, 0, endAngle);
        DeleteWeapon();
    }

    private void DeleteWeapon()
    {
        playerWeapon.SetActive(false);
    }
}
