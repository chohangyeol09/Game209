using UnityEngine;

public class H_CreateChast : MonoBehaviour
{
    [SerializeField] private GameObject Chast;

    public void SpawnInsideCameraView()
    {
        // 1. ī�޶� ���� 0~1 ������ ����Ʈ ���� ��ǥ
        float randomX = Random.Range(0.1f, 0.9f); // ȭ���� 10%~90% ����
        float randomY = Random.Range(0.1f, 0.9f);

        // 2. ����Ʈ ��ǥ �� ���� ��ǥ
        Vector3 viewportPos = new Vector3(randomX, randomY, Camera.main.nearClipPlane);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        worldPos.z = 0; // 2D �����̶�� Z �� ����

        // 3. ������Ʈ ����
        Instantiate(Chast, worldPos, Quaternion.identity);
    }
}
