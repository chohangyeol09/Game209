using UnityEngine;

[System.Serializable]
public struct BlueZoneInfo
{
    public int phase;
    public bool isWaiting;
    public bool isShrinking;
    public float timer;
    public Vector2 blueZoneCenter;
    public float blueZoneRadius;
    public Vector2 safeZoneCenter;
    public float safeZoneRadius;
    public float damagePerSecond;
}