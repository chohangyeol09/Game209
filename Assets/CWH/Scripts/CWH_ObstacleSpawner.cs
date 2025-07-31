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
}
