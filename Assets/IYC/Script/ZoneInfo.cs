using UnityEngine;

public struct ZoneInfo //여러 변수를 묶어 하나의 새로운 자료형으로 정의하는 방법 = struct
{
    public Vector2 center;
    public float radius;
    public int phase;
    public bool isShrinking;
    public bool isWaiting;
    public float damagePerSecond;
}