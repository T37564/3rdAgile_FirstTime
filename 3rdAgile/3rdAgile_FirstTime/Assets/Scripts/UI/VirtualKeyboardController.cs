// -----------------------------------------------------------------------------------
// 仮想キーボードの入力、選択などの処理
// VirtualKeyboardController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class VirtualKeyboardController : MonoBehaviour
{
    // UI Toolkit のルート要素を参照するための UIDocument
    [SerializeField] private UIDocument uiDocument = null;

    // UXML の root
    private VisualElement root = null;

    // class="key" を持つ全てのキー（UI 要素）をまとめて格納
    private Button[] keys = null;

    private Label matchingNumbersText = null;

    // 現在選択中のキーのインデックス
    private int currentIndex = 0;

    private string matchingNumbers = "";

    // UI Document が有効になった瞬間に呼ばれる
    private void OnEnable()
    {
        // 仮想キーボードのVisualElementを探す
        root = uiDocument.rootVisualElement;

        // UXML 内で class="key" が付いた要素を全部取得して配列に変換
        keys = root.Query<Button>(className: "key").ToList().ToArray();
        matchingNumbersText = root.Query<Label>();
        // 初期キー　色変更
        Highlight(currentIndex);
    }


    private void Update()
    {
        // 左移動（←）
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(-1);
        }

        // 右移動（→）
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(+1);
        }

        // 上移動
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(-3);
        }

        // 下移動
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(+3);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            ClickCalculatorButton(currentIndex);
        }
    }


    /// <summary>
    /// 選択しているキーの変更処理
    /// </summary>
    private void Move(int dir)
    {
        keys[currentIndex].RemoveFromClassList("selected");

        currentIndex = (currentIndex + dir + keys.Length) % keys.Length;

        Highlight(currentIndex);

        string st = keys[currentIndex].text;
        Debug.Log(st);
    }


    /// <summary>
    /// 選択中のキー　強調させるため色を変更する処理
    /// </summary>
    private void Highlight(int index)
    {
        // USSの色に変更（グレー）
        keys[index].AddToClassList("selected");
    }

    /// <summary>
    /// 電卓のボタンが押されたときの処理
    /// </summary>
    private void ClickCalculatorButton(int index)
    {
        if (keys[index].text == "消")
        {
            if (0 < matchingNumbers.Length)
                DeleteButton();
        }
        else if (keys[index].text == "決")
        {
            if (0 < matchingNumbers.Length)
                DecisionButton();
        }
        else
        {
            if (matchingNumbers.Length < 6)
                matchingNumbers += keys[index].text;
        }

        MatchingNumbersTextChange();
    }


    /// <summary>
    /// 決定ボタンが押されたときの処理
    /// </summary>
    private void DecisionButton()
    {

    }

    /// <summary>
    /// 削除ボタンが押されたときの処理
    /// </summary>
    private void DeleteButton()
    {
        matchingNumbers = matchingNumbers.Substring(0, matchingNumbers.Length - 1);
    }

    private void MatchingNumbersTextChange()
    {
        matchingNumbersText.text = matchingNumbers.ToString();
    }
}
