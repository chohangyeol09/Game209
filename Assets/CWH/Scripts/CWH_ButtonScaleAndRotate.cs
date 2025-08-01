using UnityEngine;
using UnityEngine.EventSystems;

public class CWH_ButtonScaleAndRotate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 targetScale = new Vector3(1.1f, 1.1f, 1f); // 확대 크기
    public float scaleSpeed = 5f;
    public float rotateSpeed = 180f; // 초당 회전 속도 (도 단위)
    public float rotationResetSpeed = 5f; // 회전 복원 속도

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine scaleRoutine;
    private Coroutine rotateResetRoutine;
    private bool isHovering = false;

    void Awake()
    {
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (isHovering)
        {
            transform.Rotate(Vector3.forward * rotateSpeed * Time.unscaledDeltaTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (rotateResetRoutine != null)
        {
            StopCoroutine(rotateResetRoutine);
            rotateResetRoutine = null;
        }

        StartScaling(targetScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        StartScaling(originalScale);

        rotateResetRoutine = StartCoroutine(RotateToOriginal());
    }

    void StartScaling(Vector3 target)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleTo(target));
    }

    System.Collections.IEnumerator ScaleTo(Vector3 target)
    {
        while (Vector3.Distance(transform.localScale, target) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * scaleSpeed);
            yield return null;
        }

        transform.localScale = target;
        scaleRoutine = null;
    }

    System.Collections.IEnumerator RotateToOriginal()
    {
        Quaternion startRot = transform.rotation;
        float t = 0f;
        float duration = 1f / rotationResetSpeed;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            transform.rotation = Quaternion.Lerp(startRot, originalRotation, t);
            yield return null;
        }

        transform.rotation = originalRotation;
        rotateResetRoutine = null;
    }

    void OnDisable()
    {
        isHovering = false;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        if (rotateResetRoutine != null) StopCoroutine(rotateResetRoutine);

        scaleRoutine = null;
        rotateResetRoutine = null;
    }
}
