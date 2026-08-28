// -----------------------------------------------------------------------------------
// タイトル画面のボタンを制御するクラス
// TitleButtonController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
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

    [Header("NetworkGameStarterの参照用")]
    [SerializeField] private NetworkGameStarter networkGameStarter = null;

    [Header("SEManagerの参照用")]
    [SerializeField] private SEManager seManager = null;

    /// <summary>
    /// チームメンバーを募集するボタンを押したとき
    /// </summary>
    public void OnClickCreateRoomButton()
    {
        // SEを鳴らす
        seManager.SEPlayOneShot(seManager.SEList.teamButtonSE);

        // タイトルUI非表示
        UIReferences.Instance.TitleUI.SetActive(false);

        HostModeStartButton(DEFAULT_TEAM_NAME);
    }

    /// <summary>
    /// チームに入るボタンを押したとき
    /// </summary>
    public void OnClickEnterRoomButton()
    {
        // SEを鳴らす
        seManager.SEPlayOneShot(seManager.SEList.teamButtonSE);

        // タイトルUI非表示
        UIReferences.Instance.TitleUI.SetActive(false);

        // 暗証番号を入力させる
        UIReferences.Instance.VirtualKeyboardUI.SetActive(true);
    }

    /// <summary>
    /// チーム名を使用してルーム作成
    /// </summary>
    private void HostModeStartButton(string teamName)
    {
        // ホストとしてルーム作成
        networkGameStarter.CreateHostRoom(teamName);
    }

    /// <summary>
    /// 暗証番号入力時に呼ばれる
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

        StartCoroutine(LoadSceneCorutine());
    }

    private IEnumerator LoadSceneCorutine()
    {
        seManager.SEPlayOneShot(seManager.SEList.gameStartSE);

        yield return new WaitForSeconds(1.0f);

        // Fusionのシーン同期機能を使ってゲームシーンへ移動
        networkGameStarter.networkRunner.LoadScene(GAME_SCENE_NAME);
    }
}
