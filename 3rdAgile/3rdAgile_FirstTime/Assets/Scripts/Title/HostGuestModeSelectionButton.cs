// -----------------------------------------------------------------------------------
// ホストモード、ゲストモードそれぞれボタンを押したときの処理
// HostGuestModeSelectionButton.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Linq;
using TMPro;
using UnityEngine;

public class HostGuestModeSelectionButton : MonoBehaviour
{
    // プレイヤー最低人数
    private readonly int MINIMUM_NUMBER_OF_PEOPLE = 2;

    [Header("NetworkGameStarterの参照")]
    [SerializeField] private NetworkGameStarter networkGameStarter = null;

    // true=ホストモード / false=ゲストモード
    public bool isHostMode = false;

    /// <summary>
    /// CreateRoomボタンが押された時に呼ばれる
    /// </summary>
    public void NameInputDisplayAsHost()
    {
        // ホストモードに切り替える
        isHostMode = true;
        NameInputDisplay();
    }

    /// <summary>
    /// EnterRoomボタンが押されたとき
    /// </summary>
    public void NameInputDisplayAsGuest()
    {
        // ゲストモードに切り替える
        isHostMode = false;
        NameInputDisplay();
    }




    /// <summary>
    /// InputFieldを表示し、Submitイベントを登録し直す
    /// </summary>
    public void NameInputDisplay()
    {
        // ルーム名入力UIを表示する
        TitleCanvasDisplaySettings.Instance.roomNameInput.transform.parent.gameObject.SetActive(true);

        // ルーム名入力用InputFieldを取得
        TMP_InputField input = TitleCanvasDisplaySettings.Instance.roomNameInput;
        // 以前のリスナーを削除
        input.onSubmit.RemoveAllListeners();

        // Enterキー押下時にOnEnterPressedを呼ぶ
        input.onSubmit.AddListener(OnEnterPressed);
    }


    /// <summary>
    /// Enterキー押下時に呼ばれる
    /// </summary>
    private void OnEnterPressed(string roomName)
    {
        // 名前照合ミスを減らすため前後空白削除
        roomName = roomName.Trim();

        // 空白だけ、または何も入力されていない場合は何もしない
        if (string.IsNullOrWhiteSpace(roomName)) return;

        // モードに応じてルーム参加処理を分岐
        if (isHostMode)
        {
            HostModeStartButton(roomName);
        }
        else
        {
            GuestModeStartButton(roomName);
        }
    }



    /// <summary>
    /// CreateRoomボタン押下時に呼ばれる
    /// </summary>
    private void HostModeStartButton(string roomName)
    {
        // ホストとしてルーム作成
        networkGameStarter.CreateHostRoom(roomName);

        // ルーム名入力UIを非表示にする
        TitleCanvasDisplaySettings.Instance.roomNameInput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// EnterRoomボタン押下時に呼ばれる
    /// </summary>
    private void GuestModeStartButton(string roomName)
    {
        // ゲストとしてルームに入る
        networkGameStarter.JoinHostRoom(roomName);

        // ルーム名入力UIを非表示にする
        TitleCanvasDisplaySettings.Instance.roomNameInput.transform.parent.gameObject.SetActive(false);
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
            CoroutineRunner.Instance.StartCoroutine(TitleCanvasDisplaySettings.Instance.ShowErrorMessage(false, "We don't have enough people.", 2));
            return;
        }

        // Fusionのシーン同期機能を使ってゲームシーンへ移動
        networkGameStarter.networkRunner.LoadScene("PlayerSpawnTestScenes 1");
    }
}
