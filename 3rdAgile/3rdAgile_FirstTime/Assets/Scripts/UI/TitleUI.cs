// -----------------------------------------------------------------------------------
// タイトルのUIを切り替えるクラス
// ScoreUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UIElements;

public class TitleUI : MonoBehaviour
{
    [Header("タイトルのUIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("タイトルで使用するVisualTreeAsset")]
    [SerializeField] private UIAssetData uIAssetData = null;

    [Header("タイトルボタン参照用")]
    [SerializeField] private TitleButtonController titleButtonController = null;

    private Button createRoom = null;
    private Button enterRoom = null;

    private void OnEnable()
    {
        // タイトルのUIに切り替える
        uiDocument.rootVisualElement.Clear();
        uIAssetData.titleUI.CloneTree(uiDocument.rootVisualElement);

        VisualElement root = uiDocument.rootVisualElement;

        // CreateRoomボタンとEnterRoomボタンを取得
        createRoom = root.Q<Button>("CreateRoom");
        enterRoom = root.Q<Button>("EnterRoom");

        // イベント登録
        createRoom.clicked += titleButtonController.OnClickCreateRoomButton;
        enterRoom.clicked += titleButtonController.OnClickEnterRoomButton;
    }

    /// <summary>
    /// イベント登録解除
    /// </summary>
    private void OnDisable()
    {
        if (uiDocument.rootVisualElement == null) return;
        uiDocument.rootVisualElement.Clear();

        if (createRoom != null)
        {
            createRoom.clicked -= titleButtonController.OnClickCreateRoomButton;
        }
        if (enterRoom != null)
        {
            enterRoom.clicked -= titleButtonController.OnClickEnterRoomButton;
        }
    }
}
