using TMPro;
using UnityEngine;

public class Ku_PlayerUpgradeManager : MonoBehaviour
{
    public int nowExp = 0;
    private int limitExp = 10;
    private int level = 1;
    private int otherExp = 0;

    public bool isUpgrade = false;

    [SerializeField] private TextMeshProUGUI testMesh;
    [SerializeField] private GameObject upgradePanel;

    private void Update()
    {
        testMesh.text = $"nowExp : {nowExp}, limitExpt : {limitExp}, level : {level}";
        if(limitExp <= nowExp)
        {
            isUpgrade = true;
            level++;
            int exp = nowExp - limitExp;
            nowExp = exp;
            limitExp = level * 10;
            upgradePanel.SetActive(true);
        }
    }

    public void GetExp(int exp)
    {
        nowExp += exp;
    }
}
