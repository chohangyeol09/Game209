using UnityEngine;

public struct ZoneInfo //���� ������ ���� �ϳ��� ���ο� �ڷ������� �����ϴ� ��� = struct
{
    public Vector2 center;
    public float radius;
    public int phase;
    public bool isShrinking;
    public bool isWaiting;
    public float damagePerSecond;
}