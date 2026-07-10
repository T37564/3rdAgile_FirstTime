// -----------------------------------------------------------------------------------
// ゲーム内のUIを管理するクラス
// InGameUIController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
    [Header("スコアUI")]
    [SerializeField] private ScoreUI scoreUI = null;

    [Header("スコア表示用のUIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    // HPのUI
    private VisualElement[] hartUIs = null;

    /// <summary>
    /// 使用するUIを取得する
    /// </summary>
    private void OnEnable()
    {
        // UXML内からHPのVisualElementを取得
        VisualElement root = uiDocument.rootVisualElement;
        VisualElement hp = root.Q<VisualElement>("HP");

        // 取得したHPの子要素をすべて取得
        hartUIs = hp.Children().ToArray();
    }

    /// <summary>
    /// プレイヤーHP表示更新処理
    /// </summary>
    public void PlayerHPDisplay(int playerHP)
    {
        for (int i = 0; i < hartUIs.Length; i++)
        {
            // 現在のHP以内のUIは表示する
            if (i < playerHP)
            {
                // UIを表示
                hartUIs[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                // UIを非表示にする
                hartUIs[i].style.display = DisplayStyle.None;
            }
        }
    }


    /// <summary>
    /// スコアUIを表示する
    /// </summary>
    public void ShowScoreUI()
    {
        scoreUI.enabled = true;
    }
}
