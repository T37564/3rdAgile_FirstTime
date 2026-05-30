using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] public int currentMoney = 0;

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
        currentMoney += amount;
        Debug.Log("現在の所持金: " + currentMoney);
    }

}
