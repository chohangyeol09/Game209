using UnityEngine;
using System.Collections.Generic;

public class Ku_UpgradePanel : MonoBehaviour
{
    [SerializeField] private GameObject upgradeCard;
    [SerializeField] private List<Ku_UpgradeCardSO> upgradeCardSOList = new List<Ku_UpgradeCardSO>();

    private List<GameObject> cardList = new List<GameObject>();
    private int _count = 0;
    private void OnEnable()
    {
        _count = 0;

        if (cardList != null)
        {
            cardList.Clear();
        }

        while (_count <= 3)
        {
            Vector3 position = new Vector3(400 + (-580 + 580 * (_count + 1)), 500, 0);
            GameObject clone = Instantiate(upgradeCard, position, Quaternion.identity, transform);

            Ku_UpgradeCardSO randomSO = upgradeCardSOList[Random.Range(0, upgradeCardSOList.Count)];
            if (!(Ku_PlayerUpgradeManager.Instance._upgradeType[randomSO.upgradeType] >=
                Ku_PlayerUpgradeManager.Instance._maxupgrade[randomSO.upgradeType]))
            {
                clone.GetComponent<Ku_UpgradeCard>().upgradeCardSO = randomSO;

                cardList.Add(clone);
                _count++;
            }
        }

        
    }

    private void Update()
    {
        if (cardList != null)
        {
            bool allNull = true;

            foreach (var card in cardList)
            {
                if (card != null)
                {
                    allNull = false;
                    break;
                }
            }

            if (allNull)
            {
                Debug.Log("false");
                gameObject.SetActive(false);
            }
        }
    }
}
