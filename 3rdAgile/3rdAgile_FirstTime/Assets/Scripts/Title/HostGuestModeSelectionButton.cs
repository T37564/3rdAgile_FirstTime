// -----------------------------------------------------------------------------------
// ホストモード、ゲストモードそれぞれボタンを押したときの処理
// BasicSpawner.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Linq;
using UnityEngine;

public class HostGuestModeSelectionButton : MonoBehaviour
{
    [Header("")]
    [SerializeField] private NetworkGameStarter networkGameStarter = null;

    // true=ホストモード / false=ゲストモード
    public bool isHostMode = false;

    /// <summary>
    /// ホストモードのボタンが押されたとき
    /// </summary>
    public void NameInputDisplayAsHost()
    {
        isHostMode = true;
        NameInputDisplay();
    }

    /// <summary>
    /// ゲストモードのボタンが押されたとき
    /// </summary>
    public void NameInputDisplayAsGuest()
    {
        isHostMode = false;
        NameInputDisplay();
    }




    /// <summary>
    /// InputFieldを表示して、リスナーを一度だけ登録
    /// </summary>
    public void NameInputDisplay()
    {
        TitleCanvasDisplaySettings.Instance.roomNameInput.transform.parent.gameObject.SetActive(true);

        var input = TitleCanvasDisplaySettings.Instance.roomNameInput;
        input.onSubmit.RemoveAllListeners(); // 以前のリスナーを削除

        input.onSubmit.AddListener(OnEnterPressed);
    }


    /// <summary>
    /// エンターが押されたとき
    /// </summary>

    private void OnEnterPressed(string roomName)
    {
        // 空白だけ、または何も入力されていない場合は何もしない
        if (string.IsNullOrWhiteSpace(roomName)) return;

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
    /// ホストモードのUIのスタートボタンにアタッチするメソッド
    /// </summary>
    private void HostModeStartButton(string roomName)
    {
        // ホストとしてルーム作成
        networkGameStarter.CreateHostRoom(roomName);

        TitleCanvasDisplaySettings.Instance.roomNameInput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// ゲストモードのUIのスタートボタンにアタッチするメソッド
    /// </summary>
    private void GuestModeStartButton(string roomName)
    {
        // ゲストとしてルームに入る
        networkGameStarter.JoinHostRoom(roomName);

        TitleCanvasDisplaySettings.Instance.roomNameInput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// スタートボタンが押されたときの処理
    /// ゲームシーンに移動する
    /// </summary>
    public void ClickStartButton()
    {
        // runnerがちゃんと存在するか確認
        if (networkGameStarter == null || networkGameStarter.networkRunner == null) return;
        
        // ルーム内の人数を取得
        int playerCount = networkGameStarter.networkRunner.ActivePlayers.Count();
        
        // 人数チェック（例：4人揃うまで開始しない）
        if (playerCount != 2)
        {
            CoroutineRunner.Instance.StartCoroutine(TitleCanvasDisplaySettings.Instance.ErrorTextDisplay(false,"We don't have enough people.",2));
            return;
        }
        
        // シーン遷移
        networkGameStarter.networkRunner.LoadScene("PlayerSpawnTestScenes 1");
    }
}
