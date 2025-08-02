using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ku_PlayerUpgradeManager : MonoBehaviour
{
    public static Ku_PlayerUpgradeManager Instance;


    public float nowExp = 0;
    private float limitExp = 10;
    private int level = 1;
    private float otherExp = 0;

    public bool isUpgrade = false;
    [SerializeField] private Ku_PlayerMovement movement;
    [SerializeField] private Ku_PlayerWeaponAttack attack;

    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject weapon;
    [SerializeField] private Scrollbar expBar;
    [SerializeField] private TextMeshProUGUI lvText;
    [SerializeField] private TextMeshProUGUI nowExpText;

    private float targetScale = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        expBar.size = nowExp / limitExp;
        lvText.text = $"Lv: {level}";
        nowExpText.text = $"{(int)(nowExp * 100 / limitExp)}%";

        transform.localScale = new Vector3(targetScale, targetScale, targetScale);

        if (!isUpgrade && nowExp >= limitExp)
        {
            isUpgrade = true;
            LevelUp();
        }
    }

    public void GetExp(int exp)
    {
        if (isUpgrade)
        {
            otherExp += exp;
        }
        else
        {
            nowExp += exp;

            if (nowExp >= limitExp)
            {
                isUpgrade = true;
                LevelUp();
            }
        }
    }

    public Dictionary<UpgradeType, int> _upgradeType = new Dictionary<UpgradeType, int>() // 자원 저장
    {
        {UpgradeType.Band, 0},
        {UpgradeType.Blood, 0},
        {UpgradeType.DemonPlane, 0},
        {UpgradeType.DoubleSwords, 0},
        {UpgradeType.FastAttack, 0},
        {UpgradeType.FastMove,0},
        {UpgradeType.LongSword,0},
        {UpgradeType.Magnetic,0},
        {UpgradeType.MaxHealth,0},
        {UpgradeType.PushPower,0},
        {UpgradeType.StrongPower,0},
        {UpgradeType.Viking,0}
    };  //_upgradeType[UpgradeType.(타입)] 으로 사용, 강화횟수에 따라 스크립트에서 수치 조정
    public Dictionary<UpgradeType, int> _maxupgrade = new Dictionary<UpgradeType, int>() // 자원 저장
    {
        {UpgradeType.Band, 0},
        {UpgradeType.Blood, 1},
        {UpgradeType.DemonPlane, 0},
        {UpgradeType.DoubleSwords, 1},
        {UpgradeType.FastAttack, 2},
        {UpgradeType.FastMove,2},
        {UpgradeType.LongSword,2},
        {UpgradeType.Magnetic,1},
        {UpgradeType.MaxHealth,2},
        {UpgradeType.PushPower,1},
        {UpgradeType.StrongPower,2},
        {UpgradeType.Viking,2}
    };  //업그레이드 한번만 가능한건 0으로 표시

    public void OnUpgradeComplete(UpgradeType type)
    {
        isUpgrade = false;
        upgradePanel.SetActive(false);

        while (otherExp > 0 && !isUpgrade)
        {
            float expNeeded = limitExp - nowExp;

            if (otherExp >= expNeeded)
            {
                nowExp += expNeeded;
                otherExp -= expNeeded;

                isUpgrade = true;
                LevelUp();
            }
            else
            {
                nowExp += otherExp;
                otherExp = 0;
            }
        }
        switch (type)
        {
            case UpgradeType.FastMove:
                movement.speed += 0.7f;
                _upgradeType[UpgradeType.FastMove]++;
                return;
            case UpgradeType.StrongPower:
                attack.damage += 3;
                _upgradeType[UpgradeType.StrongPower]++;
                return;
            case UpgradeType.DemonPlane:
                movement.LowHealthPlayer(movement.maxHp / 2);
                attack.damage += 5;
                _upgradeType[UpgradeType.DemonPlane]++;
                return;
            case UpgradeType.Band:
                movement.HealPlayer(movement.maxHp / 5);
                _upgradeType[UpgradeType.Band]++;
                return;
            case UpgradeType.DoubleSwords:
                attack.damage += 4;
                _upgradeType[UpgradeType.DoubleSwords]++;
                return;
            case UpgradeType.MaxHealth:
                movement.MaxHPPlayer(40);
                _upgradeType[UpgradeType.MaxHealth]++;
                return;
            case UpgradeType.Viking:
                movement.MaxHPPlayer(20);
                attack.damage += 2;
                _upgradeType[UpgradeType.Viking]++;
                return;
            case UpgradeType.Magnetic:
                targetScale++;
                return;
            case UpgradeType.FastAttack:
                movement.cooldown -= 0.2f;
                _upgradeType[UpgradeType.FastAttack]++;
                return;
            case UpgradeType.Blood://아직 수정중
                _upgradeType[UpgradeType.Blood]++;
                return;
            case UpgradeType.PushPower:
                attack.pushDistance += 0.5f;
                _upgradeType[UpgradeType.PushPower]++;
                return;
            case UpgradeType.LongSword:
                weapon.transform.localScale = new Vector3(1, weapon.transform.localScale.y + 0.5f, 1);
                _upgradeType[UpgradeType.LongSword]++;
                return;
            default: return;
        }
    }

    private void LevelUp()
    {
        Time.timeScale = 0;
        level++;
        nowExp -= limitExp;
        limitExp = level * 10;
        upgradePanel.SetActive(true);
    }
}
