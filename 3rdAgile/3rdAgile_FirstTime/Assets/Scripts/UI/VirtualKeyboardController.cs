// -----------------------------------------------------------------------------------
// 仮想キーボードの入力、選択などの処理
// VirtualKeyboardController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class VirtualKeyboardController : MonoBehaviour
{
    private readonly int MAXIMUM_NUMBER_OF_DIGITS = 6;



    [SerializeField] private PlayerInput playerInput = null;

    // UI Toolkit のルート要素を参照するための UIDocument
    [SerializeField] private UIDocument uiDocument = null;



    // UXML の root
    private VisualElement root = null;

    // class="key" を持つ全てのキー（UI 要素）をまとめて格納
    private Button[] keys = null;

    // UIToolkitの暗証番号を入れるテキスト
    private Label matchingNumbersText = null;

    private Label restrictionText = null;

    private System.Action[] clickActions;

    // 暗証番号を入れる変数
    private string matchingNumbers = "";

    // 現在選択中のキーのインデックス
    private int currentIndex = 0;

    private float moveCooldown = 0.2f;

    private Vector2 moveInput;

    private float nextMoveTime;

    // UI Document が有効になった瞬間に呼ばれる
    private void OnEnable()
    {
        // 仮想キーボードのVisualElementを探す
        root = uiDocument.rootVisualElement;

        // UXML 内で class="key" が付いた要素を全部取得して配列に変換
        keys = root.Query<Button>(className: "key").ToList().ToArray();
        matchingNumbersText = root.Query<Label>("InputNumber");

        restrictionText = root.Query<Label>("Restriction");
        restrictionText.style.display = DisplayStyle.None;

        // ボタンの登録
        playerInput.actions["NumberUI"].performed += OnNumberUI;

        // コントローラーの登録
        playerInput.actions["MoveSelectNumber"].performed += MoveNmberUI;
        playerInput.actions["MoveSelectNumber"].canceled += StopMoveNumberUI;

        playerInput.actions["NumberAssignment"].performed += ClickCalculatorButton;

        clickActions = new System.Action[keys.Length];

        for (int i = 0; i < keys.Length; i++)
        {
            int index = i;
            clickActions[i] = () => OnKeyClicked(index);
            keys[i].clicked += clickActions[i];
        }

        // ゲームパッド接続中
        if (Gamepad.current != null)
        {
            GamepadHighlight(0);
        }
    }


    private void OnDisable()
    {
        // ボタンの登録解除
        playerInput.actions["NumberUI"].performed -= OnNumberUI;

        // クリックの登録
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].clicked -= clickActions[i];
        }
    }


    /// <summary>
    /// 数字（0～9）をUI上のボタンインデックスに変換する
    /// </summary>
    private int GetButtonIndex(int number)
    {
        switch (number)
        {
            case 7: return 0;
            case 8: return 1;
            case 9: return 2;

            case 4: return 3;
            case 5: return 4;
            case 6: return 5;

            case 1: return 6;
            case 2: return 7;
            case 3: return 8;

            case 0: return 10;

            default: return -1;
        }
    }



    ///// <summary>
    ///// ゲームパッドの入力が検知されたとき
    ///// </summary>
    private void MoveNmberUI(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// ゲームパッドの入力が消滅したとき
    /// </summary>
    private void StopMoveNumberUI(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    /// <summary>
    /// ゲームパッドの入力を反映
    /// </summary>
    private void Update()
    {
        if (Time.time < nextMoveTime)
            return;
        if (moveInput.magnitude < 0.5f)
            return;

        if (moveInput.y > 0.5f)
        {
            Move(-3);
        }
        else if (moveInput.y < -0.5f)
        {
            Move(+3);
        }
        else if (moveInput.x > 0.5f)
        {
            Move(+1);
        }
        else if (moveInput.x < -0.5f)
        {
            Move(-1);
        }

        nextMoveTime = Time.time + moveCooldown;
    }

    /// <summary>
    /// マウスでクリックされたときの処理
    /// </summary>
    private void OnKeyClicked(int index)
    {
        string text = keys[index].text;

        if (text == "消")
        {
            DeleteButton();
        }
        else if (text == "決")
        {
            DecisionButton();
        }
        else
        {
            AddnNumericalInputFromKeyboard(int.Parse(text));
        }

        StartCoroutine(Highlight(index));
    }

    /// <summary>
    /// キーボード入力を受け取り、対応する処理（数字入力・削除・決定）を実行する
    /// </summary>
    private void OnNumberUI(InputAction.CallbackContext callbackContext)
    {
        switch (callbackContext.control.path)
        {
            // 0
            case "/Keyboard/numpad0":
            case "/Keyboard/0":
                AddnNumericalInputFromKeyboard(0);
                break;

            // 1
            case "/Keyboard/numpad1":
            case "/Keyboard/1":
                AddnNumericalInputFromKeyboard(1);
                break;

            // 2
            case "/Keyboard/numpad2":
            case "/Keyboard/2":
                AddnNumericalInputFromKeyboard(2);
                break;

            // 3
            case "/Keyboard/numpad3":
            case "/Keyboard/3":
                AddnNumericalInputFromKeyboard(3);
                break;

            // 4
            case "/Keyboard/numpad4":
            case "/Keyboard/4":
                AddnNumericalInputFromKeyboard(4);
                break;

            // 5
            case "/Keyboard/numpad5":
            case "/Keyboard/5":
                AddnNumericalInputFromKeyboard(5);
                break;

            // 6
            case "/Keyboard/numpad6":
            case "/Keyboard/6":
                AddnNumericalInputFromKeyboard(6);
                break;

            // 7
            case "/Keyboard/numpad7":
            case "/Keyboard/7":
                AddnNumericalInputFromKeyboard(7);
                break;

            // 8
            case "/Keyboard/numpad8":
            case "/Keyboard/8":
                AddnNumericalInputFromKeyboard(8);
                break;

            // 9
            case "/Keyboard/numpad9":
            case "/Keyboard/9":
                AddnNumericalInputFromKeyboard(9);
                break;

            // Enter
            case "/Keyboard/numpadEnter":
            case "/Keyboard/Enter":
                DecisionButton();
                break;


            //Del
            case "/Keyboard/numpadPeriod":
            case "/Keyboard/backspace":
                DeleteButton();
                break;

            default:
                Debug.Log("例外");
                return;

        }
    }


    /// <summary>
    /// 数字を入力し、UI表示を更新する（最大桁数制限あり）
    /// </summary>
    private void AddnNumericalInputFromKeyboard(int index)
    {
        // 選択したUIの色を変える
        int highlightIndex = GetButtonIndex(index);
        if (highlightIndex >= 0)
            StartCoroutine(Highlight(highlightIndex));

        // 暗証番号の最大桁数より小さいとき
        if (matchingNumbers.Length < MAXIMUM_NUMBER_OF_DIGITS)
            // 入力された番号を追加する
            matchingNumbers += index.ToString();
        else
            restrictionText.style.display = DisplayStyle.Flex;

        // 暗証番号の数値が書かれたUIの更新
        MatchingNumbersTextChange();
    }


    /// <summary>
    /// 決定ボタンが押されたときの処理
    /// </summary>
    private void DecisionButton()
    {
        StartCoroutine(Highlight(11));
        Debug.Log("確定");
    }


    /// <summary>
    /// 削除ボタンが押されたときの処理
    /// </summary>
    private void DeleteButton()
    {
        restrictionText.style.display = DisplayStyle.None;

        StartCoroutine(Highlight(9));

        if (matchingNumbers.Length > 0)
            matchingNumbers = matchingNumbers.Substring(0, matchingNumbers.Length - 1);

        MatchingNumbersTextChange();
    }


    /// <summary>
    /// 暗証番号が書かれたテキストの更新
    /// </summary>
    private void MatchingNumbersTextChange()
    {
        matchingNumbersText.text = matchingNumbers.ToString();
    }


    /// <summary>
    /// 指定したボタンを一時的にハイライト表示する（視覚フィードバック用）
    /// </summary>
    private IEnumerator Highlight(int index)
    {
        keys[index].AddToClassList("selected");

        yield return new WaitForSeconds(0.3f);

        keys[index].RemoveFromClassList("selected");
    }

    /// <summary>
    /// 選択しているキーの変更処理
    /// </summary>
    private void Move(int dir)
    {
        keys[currentIndex].RemoveFromClassList("selected");

        currentIndex = (currentIndex + dir + keys.Length) % keys.Length;

        GamepadHighlight(currentIndex);

        string st = keys[currentIndex].text;
    }


    /// <summary>
    /// 選択中のキー　強調させるため色を変更する処理
    /// </summary>
    private void GamepadHighlight(int index)
    {
        // USSの色に変更（グレー）
        keys[index].AddToClassList("selected");
    }

    /// <summary>
    /// 電卓のボタンが押されたときの処理
    /// </summary>
    private void ClickCalculatorButton(InputAction.CallbackContext context)
    {
        if (keys[currentIndex].text == "消")
        {
            if (0 < matchingNumbers.Length)
                DeleteButton();
        }
        else if (keys[currentIndex].text == "決")
        {
            if (0 < matchingNumbers.Length)
                DecisionButton();
        }
        else
        {
            // 暗証番号の最大桁数より小さいとき
            if (matchingNumbers.Length < MAXIMUM_NUMBER_OF_DIGITS)
                // 入力された番号を追加する
                matchingNumbers += keys[currentIndex].text.ToString();
            else
                restrictionText.style.display = DisplayStyle.Flex;
        }

        MatchingNumbersTextChange();
    }
}
