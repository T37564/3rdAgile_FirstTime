using Fusion;
using TMPro;
using UnityEngine;

public class MoneyManager : SingletonNetworkBehaviour<MoneyManager>
{
    //[SerializeField] public int currentMoney = 0;

    [Networked] public int totalMoney { get; set; }
    //[SerializeField] public TextMeshPro moneyText;

    // オブジェクトが有効になったとき呼ばれる
    private void OnEnable()
    {
        // ActionにAddAmountメソッドを登録
        //ItemGroundChecker.OnGroundedStateChanged += AddAmount;
    }

    // オブジェクトが無効になったとき呼ばれる
    private void OnDisable()
    {
        //ItemGroundChecker.OnGroundedStateChanged -= AddAmount;
    }

    /// <summary>
    /// アイテムを納品した合計の売却値を更新するメソッド
    /// </summary>
    public void AddAmount(int amount, int requiredPeople)
    {
        if (!Object.HasStateAuthority) return;

        totalMoney += (int)(amount * BonusCheck(requiredPeople));
        Debug.Log("現在の所持金: " + totalMoney);
    }

    private float BonusCheck(int requiredPeople)
    {
        return requiredPeople switch
        {
            1 => 1.0f,
            2 => 1.5f,
            3 => 2.0f,
            4 => 2.5f,
            _ => 1.0f
        };
    }
}
