using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Ku_UpgradePanel : MonoBehaviour
{
    [SerializeField] private GameObject upgradeCard;
    [SerializeField] private List<Ku_UpgradeCardSO> upgradeCardSOList = new List<Ku_UpgradeCardSO>();
    private List<GameObject> cardList= new List<GameObject>();
    private void OnEnable()
    {
        if (cardList != null)
        {
            cardList.Clear();
        }
        for (int i = 1; i <= 3; i++)
        {
            Vector3 position = new Vector3(400 + (-580 + 580 * i), 500, 0);

            GameObject clone = Instantiate(upgradeCard, position, Quaternion.identity, transform);
            cardList.Add(clone);
            clone.GetComponent<Ku_UpgradeCard>().upgradeCardSO = upgradeCardSOList[Random.Range(0, upgradeCardSOList.Count)];
        }
    }
}
