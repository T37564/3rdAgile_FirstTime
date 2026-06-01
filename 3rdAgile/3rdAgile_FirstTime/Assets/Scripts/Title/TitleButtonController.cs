using System.Linq;
using UnityEngine;

public class TitleButtonController : MonoBehaviour
{
    // プレイヤー最低人数
    private readonly int MINIMUM_NUMBER_OF_PEOPLE = 2;

    [Header("NetworkGameStarterの参照")]
    [SerializeField] private NetworkGameStarter networkGameStarter = null;

    /// <summary>
    /// CreateRoomボタンが押された時に呼ばれる
    /// </summary>
    public void OnClickCreateRoomButton()
    {
        // タイトルUI非表示
        UIReferences.Instance.TitleUI.SetActive(false);

        HostModeStartButton("test");
    }

    /// <summary>
    /// EnterRoomボタンが押されたとき
    /// </summary>
    public void OnClickEnterRoomButton()
    {
        // タイトルUI非表示
        UIReferences.Instance.TitleUI.SetActive(false);

        // 暗証番号を入力させる
        UIReferences.Instance.VirtualKeyboardUI.SetActive(true);
    }







    /// <summary>
    /// CreateRoomボタン押下時に呼ばれる
    /// </summary>
    private void HostModeStartButton(string teamName)
    {
        // ホストとしてルーム作成
        networkGameStarter.CreateHostRoom(teamName);
    }

    /// <summary>
    /// EnterRoomボタン押下時に呼ばれる
    /// </summary>
    public void GuestModeStartButton(string pin)
    {
        // ゲストとしてルームに入る
        networkGameStarter.JoinHostRoom(pin);
    }

    /// <summary>
    /// スタートボタンが押されたときの処理
    /// 人数確認後、ゲームシーンへ移動する
    /// </summary>
    public void ClickStartButton()
    {
        // NetworkRunnerが存在するか確認
        if (networkGameStarter == null || networkGameStarter.networkRunner == null) return;

        // ルーム内の人数を取得
        int playerCount = networkGameStarter.networkRunner.ActivePlayers.Count();

        // 2人未満の場合はゲーム開始できない
        if (playerCount < MINIMUM_NUMBER_OF_PEOPLE)
        {
            // 人数不足エラーを表示
            LobbyUI lobbyUI = UIReferences.Instance.LobbyUI.GetComponent<LobbyUI>();

            StartCoroutine(lobbyUI.ActiveLackOfPersonnel());

            return;
        }

        // Fusionのシーン同期機能を使ってゲームシーンへ移動
        networkGameStarter.networkRunner.LoadScene("PlayerSpawnTestScenes 1");
    }



    //チーム名入力が欲しい場合使用

    ///// <summary>
    ///// InputFieldを表示し、Submitイベントを登録し直す
    ///// </summary>
    //public void OpenTeamNameInputUI()
    //{
    //    // チーム名入力UIを表示する
    //    TitleCanvasDisplaySettings.Instance.roomNameInput.transform.parent.gameObject.SetActive(true);

    //    // チーム名入力用InputFieldを取得
    //    TMP_InputField input = TitleCanvasDisplaySettings.Instance.roomNameInput;
    //    // 以前のリスナーを削除
    //    input.onSubmit.RemoveAllListeners();

    //    // Enterキー押下時にOnEnterPressedを呼ぶ
    //    input.onSubmit.AddListener(OnTeamNameSubmitted);
    //}


    ///// <summary>
    ///// Enterキー押下時に呼ばれる
    ///// </summary>
    //private void OnTeamNameSubmitted(string teamName)
    //{
    //    // 空白だけ、または何も入力されていない場合は何もしない
    //    if (string.IsNullOrWhiteSpace(teamName)) return;

    //    HostModeStartButton(teamName);
    //}
}
