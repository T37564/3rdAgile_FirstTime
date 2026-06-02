using Fusion;
using TMPro;
using UnityEngine;

public class MoneyManager : NetworkBehaviour
{
    //[SerializeField] public int currentMoney = 0;

    [Networked] public int totalMoney { get; set; }
    //[SerializeField] public TextMeshPro moneyText;

    private void OnEnable()
    {
        ItemGroundChecker.OnGroundedStateChanged += AddAmount;
    }

    private void OnDisable()
    {
        ItemGroundChecker.OnGroundedStateChanged -= AddAmount;
    }


    private void AddAmount(int amount)
    {
        if(!Object.HasStateAuthority) return;

        totalMoney += amount;
        Debug.Log("現在の所持金: " + totalMoney);
    }

}
