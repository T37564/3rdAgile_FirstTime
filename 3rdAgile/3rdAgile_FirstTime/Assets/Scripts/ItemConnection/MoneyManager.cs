using Fusion;
using TMPro;
using UnityEngine;

public class MoneyManager : NetworkBehaviour
{
    //[SerializeField] public int currentMoney = 0;

    [Networked] public int totalMoney { get; set; }
    //[SerializeField] public TextMeshPro moneyText;

    // オブジェクトが有効になったとき呼ばれる
    private void OnEnable()
    {
        // ActionにAddAmountメソッドを登録
        ItemGroundChecker.OnGroundedStateChanged += AddAmount;
    }

    // オブジェクトが無効になったとき呼ばれる
    private void OnDisable()
    {
        ItemGroundChecker.OnGroundedStateChanged -= AddAmount;
    }

    /// <summary>
    /// アイテムを納品した合計の売却値を更新するメソッド
    /// </summary>
    private void AddAmount(int amount)
    {
        if(!Object.HasStateAuthority) return;

        totalMoney += amount;
        Debug.Log("現在の所持金: " + totalMoney);
    }

}
