using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private SafeZone zoneController;
    [SerializeField] private float zoomOutWhenNearEdge = 2f;

    private Camera cam;
    private float baseOrthographicSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseOrthographicSize = cam.orthographicSize;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 위치 추적
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 안전지대 가장자리에 가까워지면 줌 아웃
        if (zoneController != null && cam != null)
        {
            float distanceFromEdge = zoneController.GetDistanceFromEdge(target.position);

            if (distanceFromEdge < 10f && distanceFromEdge > 0)
            {
                float targetSize = Mathf.Lerp(baseOrthographicSize * zoomOutWhenNearEdge,
                    baseOrthographicSize, distanceFromEdge / 10f);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize,
                    smoothSpeed * Time.deltaTime);
            }
            else
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize,
                    baseOrthographicSize, smoothSpeed * Time.deltaTime);
            }
        }
    }
}