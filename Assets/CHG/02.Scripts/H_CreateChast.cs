using UnityEngine;

public class H_CreateChast : MonoBehaviour
{
    [SerializeField] private GameObject Chast;

    public void SpawnInsideCameraView()
    {
        // 1. 카메라 기준 0~1 사이의 뷰포트 랜덤 좌표
        float randomX = Random.Range(0.1f, 0.9f); // 화면의 10%~90% 사이
        float randomY = Random.Range(0.1f, 0.9f);

        // 2. 뷰포트 좌표 → 월드 좌표
        Vector3 viewportPos = new Vector3(randomX, randomY, Camera.main.nearClipPlane);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        worldPos.z = 0; // 2D 게임이라면 Z 값 고정

        // 3. 오브젝트 생성
        Instantiate(Chast, worldPos, Quaternion.identity);
    }
}
