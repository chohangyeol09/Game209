using UnityEngine;

[CreateAssetMenu(fileName = "H_EnemyDataSO", menuName = "SO/Enemy/EnemyData")]
public class H_EnemyDataSO : ScriptableObject
{
    public int Id;
    public string Name;
    public Sprite Sprite;
    public float Damage;
    public float Speed;
    public int Exp;
}
