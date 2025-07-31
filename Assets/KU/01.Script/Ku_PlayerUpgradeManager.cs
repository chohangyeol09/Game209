using TMPro;
using UnityEngine;

public class Ku_PlayerUpgradeManager : MonoBehaviour
{
    public int nowExp = 0;
    private int limitExp = 10;
    private int level = 1;
    private int otherExp = 0;

    public bool isUpgrade = false;
    [SerializeField] private Iyc_PlayerController movement;
    [SerializeField] private Ku_PlayerWeaponAttack attack;

    [SerializeField] private TextMeshProUGUI testMesh;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject weapon;

    private float targetScale = 3;
    private void Update()
    {
        transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        testMesh.text = $"nowExp : {nowExp}, limitExp : {limitExp}, level : {level}";

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

    public void OnUpgradeComplete(int num)
    {
        isUpgrade = false;
        upgradePanel.SetActive(false);

        while (otherExp > 0 && !isUpgrade)
        {
            int expNeeded = limitExp - nowExp;

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
        switch (num)
        {
            case 0:
                movement.speed += 0.5f;
                return;
            case 1:
                attack.damage += 4;
                return;
            case 2:
                movement.LowHealthPlayer(movement.maxHp / 2);
                attack.damage += 5;
                return;
            case 3:
                movement.HealPlayer(movement.maxHp / 5);
                return;
            case 4:
                attack.damage += 6;
                return;
            case 5:
                movement.MaxHPPlayer(40);
                return;
            case 6:
                movement.MaxHPPlayer(20);
                attack.damage += 2;
                return;
            case 7:
                targetScale++;
                return;
            case 8:
                movement.cooldown -= 0.2f;
                return;
            case 9:
                return;
            case 10: attack.pushDistance += 0.25f;
                return;
            case 11: weapon.transform.localScale = new Vector3(1, weapon.transform.localScale.y + 0.25f, 1);
                return;
            default: return;
        }
    }

    private void LevelUp()
    {
        level++;
        nowExp -= limitExp;
        limitExp = level * 10;
        upgradePanel.SetActive(true);
    }
}
