// -----------------------------------------------------------------------------------
// 仮想キーボードの入力、選択などの処理
// VirtualKeyboardController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class VirtualKeyboardController : MonoBehaviour
{
    // ボタン移動最大値
    private readonly int KEYBOARD_COLUMN_COUNT = 3;

    // 暗証番号の最大入力桁数
    private readonly int MAXIMUM_NUMBER_OF_DIGITS = 6;

    // 消 ボタンの番号
    private readonly int DELETE_BUTTON_INDEX = 9;

    // 決 ボタンの番号
    private readonly int DECISION_BUTTON_INDEX = 11;


    // 連続移動を制御する時間
    private readonly float MOVE_COOLDOWN = 0.2f;

    // ハイライト時の待ち時間
    private readonly float HIGHLIGHT_WAIT_SECOND = 0.3f;

    // スティックの傾き度合い
    private readonly float INPUT_THRESHOLD = 0.5f;

    // 桁数制限テキスト
    private readonly string INSUFFICIENT_NUMBER_OF_DIGITS = "桁数が不足しています";
    private readonly string EXCESSIVE_NUMBER_OF_DIGITS = "これ以上追加することはできません！";




    [Header("PlayerInput 参照")]
    [SerializeField] private PlayerInput playerInput = null;

    [Header("UI Toolkit のルート要素を参照するための UIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("TitleButtonController 参照")]
    [SerializeField] private TitleButtonController titleButtonController = null;

    [Header("UI Asset Data 参照")]
    [SerializeField] private UIAssetData uiAssetData = null;

    // UXML の root
    private VisualElement root = null;

    // class="key" を持つ全てのキー（UI 要素）をまとめて格納
    private Button[] keys = null;

    // UIToolkitの暗証番号を入れるLabel
    private Label matchingNumbersText = null;

    // 数字をこれ以上追加できないことを知らせるテキスト
    private Label restrictionText = null;

    // 各ボタンに登録するクリックイベント
    private Action[] clickActions;

    // ゲームパッドの入力方向
    private Vector2 moveInput;

    // 現在選択中のキーのインデックス
    private int currentIndex = 0;

    // 次に移動可能になる時間
    private float nextMoveTime;

    // 入力中の暗証番号
    private string matchingNumbers = "";

    // 連続で決定ボタンを押さないようにするフラグ
    private bool isDuplicateMonitoring = false;

    /// <summary>
    /// オブジェクト有効化時にUI要素と入力イベントを登録する
    /// </summary>
    private void OnEnable()
    {
        // 暗証番号入力UIに変更
        uiDocument.rootVisualElement.Clear();
        uiAssetData.VirtualKeyboardUI.CloneTree(uiDocument.rootVisualElement);

        // 仮想キーボードのVisualElementを探す
        root = uiDocument.rootVisualElement;

        // UXML 内で class="key" が付いた要素を全部取得して配列に変換
        keys = root.Query<Button>(className: "key").ToList().ToArray();
        matchingNumbersText = root.Q<Label>("InputNumber");

        // UXML 内で Label "Restriction" を見つけて入れる
        restrictionText = root.Q<Label>("Restriction");

        // 桁数制限テキストを非表示にする
        restrictionText.style.display = DisplayStyle.None;

        // キーボード入力イベントを登録
        playerInput.actions["NumberUI"].performed += OnNumberUI;

        // ゲームパッド入力イベントを登録
        playerInput.actions["MoveSelectNumber"].performed += OnMoveNumberUI;
        playerInput.actions["MoveSelectNumber"].canceled += OnStopMoveNumberUI;

        // マウスクリックイベントを登録
        playerInput.actions["NumberAssignment"].performed += ClickCalculatorButton;

        // UIDocumentのボタンが押されたときのイベントを新しく作る
        clickActions = new Action[keys.Length];

        // 各キーにクリックイベントを登録する
        for (int i = 0; i < keys.Length; i++)
        {
            int index = i;
            clickActions[i] = () => OnKeyClicked(index);
            keys[i].clicked += clickActions[i];
        }

        // ゲームパッドが接続されている場合
        if (Gamepad.current != null)
        {
            // キーを選択状態にする
            GamepadHighlight(0);
        }

        // 入力状態を初期化
        matchingNumbers = "";
        isDuplicateMonitoring = false;
    }

    /// <summary>
    /// オブジェクト無効化時に入力イベントを解除する
    /// </summary>
    private void OnDisable()
    {
        if (uiDocument.rootVisualElement == null) return;
        uiDocument.rootVisualElement.Clear();

        // 入力イベントを解除
        playerInput.actions["NumberUI"].performed -= OnNumberUI;
        playerInput.actions["MoveSelectNumber"].performed -= OnMoveNumberUI;
        playerInput.actions["MoveSelectNumber"].canceled -= OnStopMoveNumberUI;
        playerInput.actions["NumberAssignment"].performed -= ClickCalculatorButton;

        // クリックイベントを解除
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].clicked -= clickActions[i];
        }
    }


    /// <summary>
    /// ゲームパッド入力に応じて選択キーを移動する
    /// </summary>
    private void Update()
    {
        // クールタイム中は移動しない
        if (Time.time < nextMoveTime)
            return;

        // ゲームパッドの傾きが少ないとき
        if (moveInput.magnitude < INPUT_THRESHOLD)
            return;

        // 入力方向に応じて選択キーを移動
        if (INPUT_THRESHOLD < moveInput.y)
        {
            Move(-KEYBOARD_COLUMN_COUNT);
        }
        else if (moveInput.y < -INPUT_THRESHOLD)
        {
            Move(+KEYBOARD_COLUMN_COUNT);
        }
        else if (INPUT_THRESHOLD < moveInput.x)
        {
            Move(+1);
        }
        else if (moveInput.x < -INPUT_THRESHOLD)
        {
            Move(-1);
        }

        // 次に移動可能になる時間を更新
        nextMoveTime = Time.time + MOVE_COOLDOWN;
    }


    /// <summary>
    /// ゲームパッドの入力方向を取得する
    /// </summary>
    private void OnMoveNumberUI(InputAction.CallbackContext context)
    {
        // ゲームパッドの入力方向を取得
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// ゲームパッドの入力が消滅したとき
    /// </summary>
    private void OnStopMoveNumberUI(InputAction.CallbackContext context)
    {
        // 入力方向をリセットする
        moveInput = Vector2.zero;
    }


    /// <summary>
    /// マウスでクリックされたときの処理
    /// </summary>
    private void OnKeyClicked(int index)
    {
        // クリックされたボタンの表示文字を取得
        string text = keys[index].text;

        // ボタンの種類に応じて処理を分岐
        if (text == "消")
        {
            // 入力中の数字を削除する
            DeleteButton();
        }
        else if (text == "決")
        {
            // 決定ボタン処理
            DecisionButton();
        }
        else
        {
            // 入力した数字を追加する
            AddNumericalInputFromKeyboard(int.Parse(text));
        }

        // 指定された数字をハイライトにする処理
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
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(0);
                break;

            // 1
            case "/Keyboard/numpad1":
            case "/Keyboard/1":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(1);
                break;

            // 2
            case "/Keyboard/numpad2":
            case "/Keyboard/2":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(2);
                break;

            // 3
            case "/Keyboard/numpad3":
            case "/Keyboard/3":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(3);
                break;

            // 4
            case "/Keyboard/numpad4":
            case "/Keyboard/4":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(4);
                break;

            // 5
            case "/Keyboard/numpad5":
            case "/Keyboard/5":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(5);
                break;

            // 6
            case "/Keyboard/numpad6":
            case "/Keyboard/6":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(6);
                break;

            // 7
            case "/Keyboard/numpad7":
            case "/Keyboard/7":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(7);
                break;

            // 8
            case "/Keyboard/numpad8":
            case "/Keyboard/8":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(8);
                break;

            // 9
            case "/Keyboard/numpad9":
            case "/Keyboard/9":
                // 入力した数字を追加する
                AddNumericalInputFromKeyboard(9);
                break;

            // Enter
            case "/Keyboard/numpadEnter":
            case "/Keyboard/Enter":
                // 決定ボタン処理
                DecisionButton();
                break;


            //Del
            case "/Keyboard/numpadPeriod":
            case "/Keyboard/backspace":
                // 入力中の数字を削除する
                DeleteButton();
                break;

            default:
                Debug.LogWarning("未対応のキー入力");
                return;

        }
    }


    /// <summary>
    /// 数字を入力し、UI表示を更新する（最大桁数制限あり）
    /// </summary>
    private void AddNumericalInputFromKeyboard(int index)
    {
        // 選択したUIの色を変える
        int highlightIndex = GetButtonIndex(index);
        if (highlightIndex >= 0)
            StartCoroutine(Highlight(highlightIndex));

        // 暗証番号の最大桁数より小さいとき
        if (matchingNumbers.Length < MAXIMUM_NUMBER_OF_DIGITS)
        {
            // 入力した数字を追加する
            matchingNumbers += index.ToString();
        }
        else
        {
            // 桁数制限テキストを表示する
            restrictionText.text = EXCESSIVE_NUMBER_OF_DIGITS;
            restrictionText.style.display = DisplayStyle.Flex;
        }

        // 暗証番号の数値が書かれたUIの更新
        MatchingNumbersTextChange();
    }


    /// <summary>
    /// 決定ボタンが押されたときの処理
    /// </summary>
    private void DecisionButton()
    {
        // selectedクラスを追加
        StartCoroutine(Highlight(DECISION_BUTTON_INDEX));

        // 暗証番号が入力されているとき
        if (6 == matchingNumbers.Length)
        {
            if (isDuplicateMonitoring) return;

            // 二重押し防止のフラグを立てる
            isDuplicateMonitoring = true;

            // 仮想キーボードを非表示にする
            gameObject.SetActive(false);

            // 入力された暗証番号を使ってルームに入る
            titleButtonController.GuestModeStartButton(matchingNumbers);

            isDuplicateMonitoring = false;
        }
        else
        {
            // 桁数が少ないことを知らせるテキストを表示する
            restrictionText.text = INSUFFICIENT_NUMBER_OF_DIGITS;
            restrictionText.style.display = DisplayStyle.Flex;
        }
    }


    /// <summary>
    /// 削除ボタンが押されたときの処理
    /// </summary>
    private void DeleteButton()
    {
        // 桁数制限テキストを非表示にする
        restrictionText.style.display = DisplayStyle.None;

        StartCoroutine(Highlight(DELETE_BUTTON_INDEX));

        // 文字が１文字以上あるとき
        if (0 < matchingNumbers.Length)
            // 入力文字列の最後の文字を削除
            matchingNumbers = matchingNumbers.Substring(0, matchingNumbers.Length - 1);

        // 暗証番号の数値が書かれたUIの更新
        MatchingNumbersTextChange();
    }


    /// <summary>
    /// 暗証番号表示テキストを更新する
    /// </summary>
    private void MatchingNumbersTextChange()
    {
        matchingNumbersText.text = matchingNumbers;
    }


    /// <summary>
    /// 指定したボタンを一時的にハイライト表示する（視覚フィードバック用）
    /// </summary>
    private IEnumerator Highlight(int index)
    {
        // selectedクラスを追加
        keys[index].AddToClassList("selected");

        yield return new WaitForSeconds(HIGHLIGHT_WAIT_SECOND);

        // selectedクラスを削除
        keys[index].RemoveFromClassList("selected");
    }

    /// <summary>
    /// 選択中のキーを移動する
    /// </summary>
    private void Move(int dir)
    {
        // selectedクラスを削除
        keys[currentIndex].RemoveFromClassList("selected");

        // キーの移動計算処理
        currentIndex = (currentIndex + dir + keys.Length) % keys.Length;

        // 移動後のキーを選択状態にする
        GamepadHighlight(currentIndex);
    }


    /// <summary>
    /// 選択中のキーを強調表示する
    /// </summary>
    private void GamepadHighlight(int index)
    {
        // selectedクラスを追加して見た目を変更 (灰色)
        keys[index].AddToClassList("selected");
    }

    /// <summary>
    /// 電卓のボタンが押されたときの処理
    /// </summary>
    private void ClickCalculatorButton(InputAction.CallbackContext context)
    {
        if (keys[currentIndex].text == "消" && 0 < matchingNumbers.Length)
        {
            // 入力中の数字を削除する
            DeleteButton();
        }
        else if (keys[currentIndex].text == "決" && 0 < matchingNumbers.Length)
        {
            // 決定ボタン処理
            DecisionButton();
        }
        else
        {
            // 暗証番号の最大桁数より小さいとき
            if (matchingNumbers.Length < MAXIMUM_NUMBER_OF_DIGITS)
            {
                // 入力した数字を追加する
                matchingNumbers += keys[currentIndex].text;
            }
            else
            {
                // 桁数制限テキストを表示する
                restrictionText.text = EXCESSIVE_NUMBER_OF_DIGITS;
                restrictionText.style.display = DisplayStyle.Flex;
            }
        }

        // 暗証番号表示UIを更新
        MatchingNumbersTextChange();
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
}
