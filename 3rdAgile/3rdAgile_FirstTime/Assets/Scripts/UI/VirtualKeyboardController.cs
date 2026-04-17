// -----------------------------------------------------------------------------------
// 仮想キーボードの入力、選択などの処理
// VirtualKeyboardController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
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

    // 現在選択中のキーのインデックス
    private int currentIndex = 0;


    // UI Document が有効になった瞬間に呼ばれる
    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        // UXML 内で class="key" が付いた要素を全部取得して配列に変換
        keys = root.Query<Button>(className: "key").ToList().ToArray();

        if (keys == null)
        {
            Debug.LogWarning("ボタンなし");
        }

        // 初期状態として最初のキーをハイライトする
        Highlight(currentIndex);

       

        string key = keys[currentIndex].text;
        Debug.Log(key);
    }


    private void Update()
    {
        // 左移動（←）
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(-1);
            SelectKey(currentIndex);   // 見た目の更新
        }

        // 右移動（→）
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(+1);
            SelectKey(currentIndex);   // 見た目の更新
        }
    }


    // -------------------------------
    // キーの選択移動処理
    // -------------------------------
    private void Move(int dir)
    {
        // 今のキーから selected を外す
        keys[currentIndex].RemoveFromClassList("selected");

        // インデックスを進める
        currentIndex += dir;

        // 範囲を超えないようにクランプ
        currentIndex = Mathf.Clamp(currentIndex, 0, keys.Length - 1);

        // 新しいキーを強調表示
        Highlight(currentIndex);

        string key=keys[currentIndex].text;

        Debug.Log(key);
    }


    // 選択中のキーをハイライトする（selected を付与）
    private void Highlight(int index)
    {
        keys[index].AddToClassList("selected");
    }



    // 配列内のキーの見た目を一括で更新する
    void SelectKey(int index)
    {
        // 全キーから selected を外す
        foreach (var key in keys)
            key.RemoveFromClassList("selected");

        // 指定されたキーに selected を付ける
        keys[index].AddToClassList("selected");
    }
}
