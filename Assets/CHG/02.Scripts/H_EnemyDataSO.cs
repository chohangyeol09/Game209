using UnityEngine;

[CreateAssetMenu(fileName = "H_EnemyDataSO", menuName = "SO/Enemy/EnemyData")]
public class H_EnemyDataSO : ScriptableObject
{
    public GameObject EnemyPrefab;
    public int Id;
    public string Name;
    public Sprite Sprite;
    public int Damage;
    public float Speed;
    public int Exp;
    public float SpawnStartTime;
    public Color color;
    public int MaxHealth;

    public int SpawnStage;
}
