using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class MoneyManager : SingletonNetworkBehaviour<MoneyManager>
{
    //[SerializeField] public int currentMoney = 0;

    [Networked, OnChangedRender(nameof(OnTotalMoneyChanged))] public int totalMoney { get; set; }
    //[SerializeField] public TextMeshPro moneyText;

    // InGameUIController参照用
    private InGameUIController uiController = null;

    public override void Spawned()
    {
        // Spawn時点の金額も表示に反映
        UpdateMoneyUI();
    }

    /// <summary>
    /// 同期された金額が変更されたときに呼ばれる
    /// </summary>
    private void OnTotalMoneyChanged()
    {
        UpdateMoneyUI();
    }

    /// <summary>
    /// 各端末のスコアUIを更新する
    /// </summary>
    private void UpdateMoneyUI()
    {
        if (uiController == null) 
        {
            uiController = FindAnyObjectByType<InGameUIController>();
        }

        uiController.UpdateAmountText();
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
