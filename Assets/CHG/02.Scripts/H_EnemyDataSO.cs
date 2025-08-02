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

    // Inspector에서 Id별 설명을 보여주는 커스텀 에디터용
    private void OnValidate()
    {
        switch (Id)
        {
            case 1:
                Description = "기본 추적형 Enemy\n- 플레이어를 쫓아다니며 접촉 시 데미지\n- 균형잡힌 스탯";
                break;
            case 2:
                Description = "푸시형 Enemy\n- 플레이어를 밀어내는 특수 공격\n- 빠른 이속, 낮은 데미지 권장\n- Push 설정은 H_Enemy 컴포넌트에서 조절";
                break;
            case 8:
                Description = "자폭형 Enemy\n- 플레이어 근처에서 자폭하여 큰 데미지\n- 낮은 체력, 보통 이속 권장\n- 폭발 설정은 H_Enemy 컴포넌트에서 조절";
                break;
            default:
                Description = "기타 Enemy 타입\n- Id에 따른 특수 행동 패턴";
                break;
        }
    }
}