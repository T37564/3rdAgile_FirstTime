// -----------------------------------------------------------------------------------
// タイトル画面のボタンを制御するクラス
// TitleButtonController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Linq;
using UnityEngine;

public class TitleButtonController : MonoBehaviour
{
    // プレイヤー最低人数
    private readonly int MINIMUM_NUMBER_OF_PEOPLE = 2;

    // ゲームシーン名
    private readonly string GAME_SCENE_NAME = "PlayerSpawnTestScenes 1";

    // 仮のチーム名
    private const string DEFAULT_TEAM_NAME = "test";

    [Header("NetworkGameStarterの参照")]
    [SerializeField] private NetworkGameStarter networkGameStarter = null;

    /// <summary>
    ///  ホストとしてルームを作成する
    /// </summary>
    public void OnClickCreateRoomButton()
    {
        // タイトルUI非表示
        UIReferences.Instance.TitleUI.SetActive(false);

        HostModeStartButton(DEFAULT_TEAM_NAME);
    }

    /// <summary>
    /// 指定したPINでルームに参加する
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
            // 人数不足メッセージを表示
            LobbyUI lobbyUI = UIReferences.Instance.LobbyUI.GetComponent<LobbyUI>();
            StartCoroutine(lobbyUI.ActiveLackOfPersonnel());
            return;
        }

        // Fusionのシーン同期機能を使ってゲームシーンへ移動
        networkGameStarter.networkRunner.LoadScene(GAME_SCENE_NAME);
    }
}
