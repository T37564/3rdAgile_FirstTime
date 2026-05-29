using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    [Header("UIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("VisualTreeAsset 一覧")]
    [SerializeField] private UIAssetData uIAssetData = null;

    [Header("タイトルボタン参照用")]
    [SerializeField] private TitleButtonController titleButtonController = null;




    private Button gameStartButton = null;

    public Label playerCount = null;

    public Label roomPIN = null;

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

        // イベント登録
        gameStartButton.clicked += titleButtonController.ClickStartButton;

        networkGameStarter = NetworkGameStarter.Instance;

        networkGameStarter.networkLobbyUI.DisplayPINNumber(networkGameStarter.networkRunner, this);
    }

    /// <summary>
    /// イベント登録解除
    /// </summary>
    private void OnDisable()
    {
        UIReferences.Instance.TitleUI.SetActive(true);

        if (gameStartButton != null)
        {
            gameStartButton.clicked -= titleButtonController.ClickStartButton;
        }
    }
}
