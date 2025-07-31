using UnityEngine;

public class Ku_EnemyTest : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float speed;

    private void Update()
    {
        Vector3 direction = (targetTransform.position - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;
    }

}