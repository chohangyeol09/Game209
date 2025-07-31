using UnityEngine;

public class ObstacleWall : MonoBehaviour
{
    public float shrinkSpeed = 5f;
    public float scaleShrinkDuration = 1.5f;

    private Vector3 initialScale;
    private float shrinkTimer = 0f;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // ��ġ ���
        Vector3 dirToCenter = -transform.position.normalized;
        transform.position += dirToCenter * shrinkSpeed * Time.deltaTime;

        // ������ ��� (x�ุ ���̱�)
        shrinkTimer += Time.deltaTime;
        float t = shrinkTimer / scaleShrinkDuration;
        float newX = Mathf.Lerp(initialScale.x, 0.8f, t);
        transform.localScale = new Vector3(newX, initialScale.y, initialScale.z);  // y, z ����

        // ���� ����
        if (transform.position.magnitude < 0.5f || t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
