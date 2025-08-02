using UnityEngine;

[CreateAssetMenu(fileName = "H_EnemyDataSO", menuName = "SO/Enemy/EnemyData")]
public class H_EnemyDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public GameObject EnemyPrefab;
    public int Id;
    public string Name;
    public Sprite Sprite;
    public Color color = Color.white;

    [Header("Combat Stats")]
    public int Damage;
    public float Speed;
    public int MaxHealth;
    public int Exp;

    [Header("Spawn Settings")]
    public float SpawnStartTime;
    public int SpawnStage;

    [Header("Enemy Type Info")]
    [TextArea(3, 5)]
    public string Description;

    // Inspector���� Id�� ������ �����ִ� Ŀ���� �����Ϳ�
    private void OnValidate()
    {
        switch (Id)
        {
            case 1:
                Description = "�⺻ ������ Enemy\n- �÷��̾ �Ѿƴٴϸ� ���� �� ������\n- �������� ����";
                break;
            case 2:
                Description = "Ǫ���� Enemy\n- �÷��̾ �о�� Ư�� ����\n- ���� �̼�, ���� ������ ����\n- Push ������ H_Enemy ������Ʈ���� ����";
                break;
            case 8:
                Description = "������ Enemy\n- �÷��̾� ��ó���� �����Ͽ� ū ������\n- ���� ü��, ���� �̼� ����\n- ���� ������ H_Enemy ������Ʈ���� ����";
                break;
            default:
                Description = "��Ÿ Enemy Ÿ��\n- Id�� ���� Ư�� �ൿ ����";
                break;
        }
    }
}