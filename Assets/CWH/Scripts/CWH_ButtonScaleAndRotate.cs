using UnityEngine;
using UnityEngine.EventSystems;

public class CWH_ButtonScaleAndRotate : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 targetScale = new Vector3(1.1f, 1.1f, 1f); // Ȯ�� ũ��
    public float scaleSpeed = 5f;
    public float rotateSpeed = 180f; // �ʴ� ȸ�� �ӵ� (�� ����)
    public float rotationResetSpeed = 5f; // ȸ�� ���� �ӵ�

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
            transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        // �ߺ� �ڷ�ƾ ����
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

        // ȸ�� ���� ����
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
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * scaleSpeed);
            yield return null;
        }

        transform.localScale = target;
        scaleRoutine = null;
    }

    System.Collections.IEnumerator RotateToOriginal()
    {
        Quaternion startRot = transform.rotation;
        float t = 0f;

        while (Quaternion.Angle(transform.rotation, originalRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(startRot, originalRotation, t);
            t += Time.deltaTime * rotationResetSpeed;
            yield return null;
        }

        transform.rotation = originalRotation;
        rotateResetRoutine = null;
    }
}
