using UnityEngine;

public class CWH_RotateObstacleParent : MonoBehaviour
{
    public float rotateSpeed = 60f;
    public CWH_ObstacleSpawner spawner;

    private float accumulatedRotation = 0f;

    void Update()
    {
        float deltaRotation = rotateSpeed * Time.deltaTime;
        transform.Rotate(0, 0, deltaRotation);
        accumulatedRotation += deltaRotation;

        if (accumulatedRotation >=360f)
        {
            accumulatedRotation -= 360f;

            // 도형의 -1개 만큼 장애물 생성
            spawner.SpawnPattern();
        }
    }
}
