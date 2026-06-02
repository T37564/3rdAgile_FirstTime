using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    private const float DISPLAY_COUNT = 1.0f;
    [Header("UIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("VisualTreeAsset 一覧")]
    [SerializeField] private UIAssetData uIAssetData = null;

    [Header("タイトルボタン参照用")]
    [SerializeField] private TitleButtonController titleButtonController = null;




    public Button gameStartButton = null;

    public Label playerCount = null;

    public Label roomPIN = null;

    public Label lackOfPersonnel = null;

    private NetworkGameStarter networkGameStarter = null;


    private void OnEnable()
    {
        // ロビーのUIに切り替える
        uiDocument.rootVisualElement.Clear();
        uIAssetData.lobbyUI.CloneTree(uiDocument.rootVisualElement);

        VisualElement root = uiDocument.rootVisualElement;

        gameStartButton = root.Q<Button>("StartButton");
        playerCount = root.Q<Label>("CountText");
        roomPIN = root.Q<Label>("PINText");
        lackOfPersonnel = root.Q<Label>("LackOfPersonnel");

        lackOfPersonnel.style.display = DisplayStyle.None;

        // イベント登録
        gameStartButton.clicked += titleButtonController.ClickStartButton;

        networkGameStarter = NetworkGameStarter.Instance;

        networkGameStarter.networkLobbyUI.DisplayHostUI(networkGameStarter.networkRunner, this);
    }

    /// <summary>
    /// イベント登録解除
    /// </summary>
    private void OnDisable()
    {
        if (gameStartButton != null)
        {
            gameStartButton.clicked -= titleButtonController.ClickStartButton;
        }
    }

    /// <summary>
    /// プレイヤーが不足していることを一定時間表示させるコルーチン
    /// </summary>
    /// <returns></returns>
    public IEnumerator ActiveLackOfPersonnel()
    {
        lackOfPersonnel.style.display = DisplayStyle.Flex;

        yield return new WaitForSecondsRealtime(DISPLAY_COUNT);

        lackOfPersonnel.style.display = DisplayStyle.None;
    }
}
