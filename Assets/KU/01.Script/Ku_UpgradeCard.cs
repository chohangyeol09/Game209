using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Ku_UpgradeCard : MonoBehaviour
{
    public Ku_UpgradeCardSO upgradeCardSO;

    [SerializeField] private Image visual;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI coment;

    private void Update()
    {
        visual.sprite = upgradeCardSO.visual;
        name.text = upgradeCardSO.name;
        coment.text = upgradeCardSO.coment;
    }

    public void ChooseButton()
    {
        Time.timeScale = 1;
        if (upgradeCardSO.name == "Blood")
        {
            GameObject.Find("UpgradePanel").GetComponent<Ku_UpgradePanel>().SpecialUpgrade(9);
        }
        GameObject.Find("expManager").GetComponent<Ku_PlayerUpgradeManager>().OnUpgradeComplete(upgradeCardSO.upgradeType);
    }
}
