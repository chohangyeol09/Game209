using UnityEngine;

public class CWH_ObstacleSpawner : MonoBehaviour
{
    public GameObject wallPrefab;
    public Transform wallParent;
    public float spawnRadius = 5f;

    [Range(3, 12)]
    public int sectorCount = 6;

    public void SpawnPattern()
    {
        int emptyIndex = Random.Range(0, sectorCount); // 비워둘 섹터

        for (int i = 0; i < sectorCount; i++)
        {
            if (i == emptyIndex) continue;
            SpawnWallAtSectorIndex(i);
        }
    }

    private void SpawnWallAtSectorIndex(int index)
    {
        float anglePerSector = 360f / sectorCount;

        // 모서리 중앙 각도 계산 (꼭짓점 X)
        float angleDeg = (index + 0.5f) * anglePerSector;
        float rad = angleDeg * Mathf.Deg2Rad;

        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Vector3 spawnPos = transform.position + dir * spawnRadius;

        // 장애물 회전: 중심(반대 방향)으로 향하도록 설정
        float wallAngle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, wallAngle + 90f);

        // 소환
        Instantiate(wallPrefab, spawnPos, rotation, wallParent);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (sectorCount < 3) return;

        float anglePerSector = 360f / sectorCount;

        Gizmos.color = Color.red;

        for (int i = 0; i < sectorCount; i++)
        {
            float angleDeg = (i + 0.5f) * anglePerSector;
            float rad = angleDeg * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Vector3 spawnPos = transform.position + dir * spawnRadius;

            Gizmos.DrawWireSphere(spawnPos, 0.2f);
            Gizmos.DrawLine(transform.position, spawnPos);
        }

        // 원형 스폰 반지름 시각화
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, spawnRadius);
    }
#endif
}
