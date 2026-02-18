// -----------------------------------------------------------------------------------
// ホストモード、ゲストモードそれぞれボタンを押したときの処理
// BasicSpawner.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using TMPro;
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
        TitleUIManager.Instance.roomNameInput.transform.parent.gameObject.SetActive(true);

        var input = TitleUIManager.Instance.roomNameInput;
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

        Debug.Log("lll");

        // ホストとしてルーム作成
        networkGameStarter.CreateHostRoom(roomName);

        TitleUIManager.Instance.roomNameInput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// ゲストモードのUIのスタートボタンにアタッチするメソッド
    /// </summary>
    private void GuestModeStartButton(string roomName)
    {

        Debug.Log("mmm");
        // ゲストとしてルームに入る
        networkGameStarter.JoinHostRoom(roomName);

        TitleUIManager.Instance.roomNameInput.transform.parent.gameObject.SetActive(false);
    }

}
